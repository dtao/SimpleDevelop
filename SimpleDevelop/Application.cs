using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;

using SimpleDevelop.Core;

namespace SimpleDevelop
{
    public class Application
    {
        HttpListener server;
        CSharpCodeExecutor compiler;
        ManualResetEvent exitEvent;

        public event EventHandler Stopped;

        public Application()
        {
            this.server = new HttpListener();
            this.server.Prefixes.Add("http://localhost:9999/");
            this.compiler = new CSharpCodeExecutor();
            this.exitEvent = new ManualResetEvent(false);
        }
        
        public void Start()
        {
            this.server.Start();
            ListenForRequest();
        }

        public void Stop()
        {
            this.server.Close();
            this.exitEvent.Set();
            
            EventHandler stoppedHandler = this.Stopped;
            if (stoppedHandler != null)
            {
                stoppedHandler(this, EventArgs.Empty);
            }
        }

        public void WaitForExit()
        {
            this.exitEvent.WaitOne();
        }

        void ListenForRequest()
        {
            this.server.BeginGetContext(result => {
                try
                {
                    HttpListenerContext context = this.server.EndGetContext(result);
                    ListenForRequest();
                    HandleRequest(context);
                }
                catch (HttpListenerException) { }
                catch (ObjectDisposedException) { }
            }, null);
        }

        void HandleRequest(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            NameValueCollection parameters = ParseRequest(request);

            switch (request.HttpMethod + ":" + request.Url.AbsolutePath)
            {
            case "POST:/compile":
                this.compiler.ExecuteWithCallback(parameters["code"], output =>
                {
                    context.SendTextResponse(output, null);
                });
                break;
            case "POST:/exit":
                Stop();
                break;
            default:
                context.SendFile(GetFileName(context.Request.Url.AbsolutePath));
                break;
            }
        }

        string ReadRequestBody(HttpListenerRequest request)
        {
            var stringBuilder = new StringBuilder();
            using (var reader = new StreamReader(request.InputStream))
            {
                while (!reader.EndOfStream)
                {
                    stringBuilder.AppendLine(reader.ReadLine());
                }
            }
            return stringBuilder.ToString();
        }

        NameValueCollection ParseRequest(HttpListenerRequest request)
        {
            if (request.HttpMethod == "GET")
            {
                return request.QueryString;
            }
            else
            {
                return HttpUtility.ParseQueryString(ReadRequestBody(request));
            }
        }

        string GetFileName(string pathFromUrl)
        {
            if (pathFromUrl.StartsWith("/"))
            {
                return pathFromUrl.TrimStart('/');
            }

            return pathFromUrl;
        }
    }
}
