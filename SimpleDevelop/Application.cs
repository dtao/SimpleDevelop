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
        static readonly string DocumentRoot;

        static Application()
        {
            DocumentRoot = Environment.GetEnvironmentVariable("SIMPLE_DEVELOP_ROOT", EnvironmentVariableTarget.User);
        }

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
                    HandleRequest(context.Request, context.Response);
                }
                catch (HttpListenerException) { }
                catch (ObjectDisposedException) { }
            }, null);
        }

        void HandleRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            if (request.HttpMethod == "POST")
            {
                switch (request.Url.AbsolutePath)
                {
                case "/files":
                    string body = ReadRequest(request);
                    File.WriteAllText(Path.Combine(DocumentRoot, "test.txt"), body);
                    response.Close();
                    return;
                case "/compile":
                    NameValueCollection parameters = ParseRequest(request);
                    string output = this.compiler.CompileAndRun(parameters["code"]);
                    SendTextResponse(output, response);
                    return;
                case "/exit":
                    response.Close();
                    Stop();
                    return;
                }
            }

            string filename = GetFileName(request.Url.AbsolutePath);
            string filepath = Path.Combine(DocumentRoot, filename);
            SendFile(filepath, response);
        }

        string ReadRequest(HttpListenerRequest request)
        {
            using (var reader = new StreamReader(request.InputStream))
            {
                return reader.ReadToEnd();
            }
        }

        NameValueCollection ParseRequest(HttpListenerRequest request)
        {
            return HttpUtility.ParseQueryString(ReadRequest(request));
        }

        void SendFile(string filepath, HttpListenerResponse response)
        {
            string extension = Path.GetExtension(filepath);
            response.ContentType = GetContentType(extension);

            if (File.Exists(filepath))
            {
                SendTextResponse(File.ReadAllText(filepath), response);
            }
            else
            {
                response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Close();
            }
        }

        void SendTextResponse(string text, HttpListenerResponse response)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(text);
                response.ContentLength64 = data.LongLength;

                using (Stream outputStream = response.OutputStream)
                {
                    outputStream.Write(data, 0, data.Length);
                }
            }
            finally
            {
                response.Close();
            }
        }

        string GetFileName(string pathFromUrl)
        {
            if (pathFromUrl.StartsWith("/"))
            {
                return pathFromUrl.Substring(1);
            }

            return pathFromUrl;
        }

        string GetContentType(string extension)
        {
            switch (extension)
            {
                case ".css": return "text/css";
                case ".js": return "text/javascript";
                default: return "text/html";
            }
        }
    }
}
