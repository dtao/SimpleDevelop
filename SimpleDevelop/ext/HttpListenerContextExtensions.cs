using System;
using System.IO;
using System.Net;
using System.Text;

using Nustache.Core;

namespace SimpleDevelop
{
    public static class HttpListenerContextExtensions
    {
        static readonly string DocumentRoot;

        static HttpListenerContextExtensions()
        {
            // This will work as long as all static assets are marked 'Copy to output directory'.
            DocumentRoot = Path.Combine(Path.GetDirectoryName(typeof(SimpleDevelop.Application).Assembly.Location), "www");
        }

        public static void SendTextResponse(this HttpListenerContext context, string text, object data = null)
        {
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(Render.StringToString(text, data));
                context.Response.ContentLength64 = bytes.LongLength;

                using (Stream outputStream = context.Response.OutputStream)
                {
                    outputStream.Write(bytes, 0, bytes.Length);
                }
            }
            finally
            {
                context.Response.Close();
            }
        }

        public static void SendAsset(this HttpListenerContext context, string filename, object data = null)
        {
            string extension = Path.GetExtension(filename);
            context.Response.ContentType = GetContentType(extension);
            context.SendFile(filename, data);
        }

        public static void SendFile(this HttpListenerContext context, string filename, object data = null)
        {
            string filepath = filename.StartsWith("/") ? filename : Path.Combine(DocumentRoot, filename);

            if (File.Exists(filepath))
            {
                context.SendTextResponse(File.ReadAllText(filepath), data ?? GetDataForFile(filepath));
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                context.Response.Close();
            }
        }

        static object GetDataForFile(string filepath)
        {
            string filename = Path.GetFileName(filepath);
            switch (filename)
            {
            case "index.html":
                return new
                {
                    ExampleCode = string.Join("\n", new string[] {
                        "using System;",
                        "",
                        "class Program",
                        "{",
                        "    public static void Main(string[] args)",
                        "    {",
                        "        Console.WriteLine(\"Hello, world!\");",
                        "    }",
                        "}"
                    })
                };
            }

            return null;
        }

        static string GetContentType(string extension)
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
