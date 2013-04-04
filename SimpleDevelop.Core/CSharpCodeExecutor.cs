using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Text;

using Microsoft.CSharp;

namespace SimpleDevelop.Core
{
    public class CSharpCodeExecutor : CodeDomExecutor
    {
        public string CompileAndRun(string code)
        {
            string tempPath = Path.GetTempPath();
            string filePath = Path.Combine(tempPath, Path.GetRandomFileName() + ".cs");
            File.WriteAllText(filePath, code);
            
            var output = new StringBuilder(Execute("gmcs", "-debug- " + filePath));
            
            string execPath = ChangeExtension(filePath, ".exe");
            if (File.Exists(execPath))
            {
                output.AppendLine(Execute("mono", execPath));
            }
            else
            {
                output.AppendLine("Compilation didn't succeed.");
            }
            
            return output.ToString();
        }

        protected override CodeDomProvider CreateProvider()
        {
            return new CSharpCodeProvider();
        }

        string Execute(string fileName, string arguments)
        {
            var output = new StringBuilder();
            var processInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WorkingDirectory = Path.GetDirectoryName(fileName)
            };
            
            using (Process p = Process.Start(processInfo))
            using (var writer = new StringWriter(output))
            {
                p.WaitForExit();
                
                while (!p.StandardError.EndOfStream)
                {
                    output.AppendLine("STDERR: " + p.StandardError.ReadLine());
                }
                while (!p.StandardOutput.EndOfStream)
                {
                    output.AppendLine("STDOUT: " + p.StandardOutput.ReadLine());
                }
                
                if (p.HasExited)
                {
                    writer.WriteLine("{0} exit code: {1}", fileName, p.ExitCode);
                }
            }

            return output.ToString();
        }
        
        string ChangeExtension(string filePath, string ext)
        {
            string basePath = Path.GetDirectoryName(filePath);
            string baseName = Path.GetFileNameWithoutExtension(filePath);
            return Path.Combine(basePath, baseName + ext);
        }
    }
}
