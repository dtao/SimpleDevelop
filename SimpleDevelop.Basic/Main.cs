using System;
using System.Diagnostics;

namespace SimpleDevelop.Basic
{
    class MainClass
    {
        public static void Main (string[] args)
        {
            var app = new Application();
            app.Start();

            Process.Start("http://localhost:9999/index.html");

            app.WaitForExit();

            Console.WriteLine("Finished!");
        }
    }
}
