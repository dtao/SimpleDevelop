using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;

using Newtonsoft.Json;
using SimpleDevelop.Core;
using SimpleDevelop.CodeCompletion;

namespace SimpleDevelop
{
    public class Engine
    {
        INativeInterface ui;
        HttpListener server;
        CSharpCodeExecutor compiler;
        CodeCompletionHelper completionHelper;
        ManualResetEvent exitEvent;

        public event EventHandler Stopped;

        public Engine(INativeInterface ui = null)
        {
            this.ui = ui ?? new NullInterface();

            this.server = new HttpListener();
            this.server.Prefixes.Add("http://localhost:9999/");
            this.compiler = new CSharpCodeExecutor();

            this.completionHelper = new CodeCompletionHelper();
            this.completionHelper.AddReference(typeof(System.Object));

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
            try
            {
                HttpListenerRequest request = context.Request;
                NameValueCollection parameters = ParseRequest(request);
                
                switch (request.HttpMethod + ":" + request.Url.AbsolutePath)
                {
                case "GET:/":
                    context.SendFile("index.html");
                    break;
                case "POST:/open":
                    FileInfo[] files = new DirectoryInfo(parameters["directory"].Trim()).GetFiles();
                    context.SendFile("partials/files.html", new { Files = files });
                    break;
                case "GET:/open":
                    context.SendFile(parameters["filepath"]);
                    this.ui.Title = "SimpleDevelop - " + Path.GetFileName(parameters["filepath"]);
                    break;
                case "POST:/save":
                    File.WriteAllText(parameters["filepath"], parameters["code"]);
                    this.ui.Title = "SimpleDevelop - " + Path.GetFileName(parameters["filepath"]);
                    context.Ok();
                    break;
                case "POST:/compile":
                    this.compiler.ExecuteWithCallback(parameters["code"], output =>
                    {
                        context.SendTextResponse(output, null);
                    });
                    break;
                case "POST:/complete":
                    SendCompletionData(context, parameters);
                    break;
                case "POST:/parse":
                    this.completionHelper.ProcessCode(parameters["code"]);
                    if (!this.ui.Title.EndsWith("*"))
                    {
                        this.ui.Title += "*";
                    }
                    context.Ok();
                    break;
                case "POST:/exit":
                    Stop();
                    break;
                default:
                    context.SendAsset(GetFileName(context.Request.Url.AbsolutePath));
                    break;
                }
            }
            catch (Exception ex)
            {
                context.SendJsonResponse(new { Error = ex.ToString() });
            }
        }

        void SendCompletionData(HttpListenerContext context, NameValueCollection parameters)
        {
            dynamic data = JsonConvert.DeserializeObject(parameters["data"]);
            IList<CompletionData> completionData = this.completionHelper.GetCompletionData((string)data.token, (int)data.line, (int)data.col);
            context.SendJsonResponse(new { Items = completionData });
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
