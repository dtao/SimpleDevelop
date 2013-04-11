using System;
using System.IO;
using System.Net;
using System.Text;

using Newtonsoft.Json;
using Nustache.Core;

namespace SimpleDevelop
{
    public static class HttpListenerContextExtensions
    {
        static readonly string DocumentRoot;

        static HttpListenerContextExtensions()
        {
            // This will work as long as all static assets are marked 'Copy to output directory'.
            DocumentRoot = Path.Combine(Path.GetDirectoryName(typeof(Engine).Assembly.Location), "www");
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

        public static void SendJsonResponse(this HttpListenerContext context, object data)
        {
            context.Response.ContentType = "text/json";
            string json = JsonConvert.SerializeObject(data);
            context.SendTextResponse(json);
        }

        public static void SendBinaryResponse(this HttpListenerContext context, byte[] bytes)
        {
            try
            {
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
            string filepath = GetFilePath(filename);

            if (File.Exists(filepath))
            {
                if (context.Response.ContentType == null || context.Response.ContentType.StartsWith("text"))
                {
                    context.SendTextResponse(File.ReadAllText(filepath), data ?? GetDataForFile(filepath));
                }
                else
                {
                    context.SendBinaryResponse(File.ReadAllBytes(filepath));
                }
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                context.Response.Close();
            }
        }

        public static void Ok(this HttpListenerContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.Close();
        }

        static string GetFilePath(string filename)
        {
            return Path.IsPathRooted(filename) ? filename : TranslatePathToSystemFormat(filename);
        }

        static string TranslatePathToSystemFormat(string filename)
        {
            string[] parts = filename.Split('/');
            return Path.Combine(DocumentRoot, Path.Combine(parts));
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
                case ".gif": return "image/gif";
                case ".jpg": return "image/jpeg";
                case ".png": return "image/png";
                case ".eot": return "application/vnd.ms-fontobject";
                case ".woff": return "application/x-font-woff";
                default: return "text/html";
            }
        }
    }
}
