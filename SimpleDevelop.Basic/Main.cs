using System;
using System.Diagnostics;

namespace SimpleDevelop.Basic
{
    class MainClass
    {
        public static void Main (string[] args)
        {
            var eng = new Engine();
            eng.Start();

            Process.Start("http://localhost:9999/index.html");

            eng.WaitForExit();

            Console.WriteLine("Finished!");
        }
    }
}
