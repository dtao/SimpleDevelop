using System;
using System.IO;
using System.Net;

namespace SimpleDevelop
{
    public class Application
    {
        static readonly string DocumentRoot;
        
        HttpListener server;
        
        static Application()
        {
            DocumentRoot = Environment.GetEnvironmentVariable("SIMPLE_DEVELOP_ROOT");
        }
        
        public Application()
        {
            this.server = new HttpListener();
            this.server.Prefixes.Add("http://localhost:8888/");
        }
        
        public void Start()
        {
            this.server.Start();
            this.server.BeginGetContext(HandleRequestFromContext, null);
        }
        
        void HandleRequestFromContext(IAsyncResult result)
        {
            HttpListenerContext context = this.server.EndGetContext(result);
            HandleRequest(context.Request, context.Response);
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
                    return;
                }
            }
            
            string filename = Path.Combine(DocumentRoot, request.Url.AbsolutePath);
            SendFile(filename, response);
        }

        string ReadRequest(HttpListenerRequest request)
        {
            using (var reader = new StreamReader(request.InputStream))
            {
                return reader.ReadToEnd();
            }
        }
        
        void SendFile(string filename, HttpListenerResponse response)
        {
            string extension = Path.GetExtension(filename);
            
            response.ContentType = GetContentType(extension);
            using (var reader = new StreamReader(File.OpenRead(filename)))
            using (var writer = new StreamWriter(response.OutputStream))
            {
                while (!reader.EndOfStream)
                {
                    writer.WriteLine(reader.ReadLine());
                }
            }
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

