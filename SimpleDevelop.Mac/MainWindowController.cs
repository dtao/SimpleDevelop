using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace SimpleDevelop.Mac
{
    public partial class MainWindowController : MonoMac.AppKit.NSWindowController
    {
        BackgroundWorker backgroundWorker;
        
        #region Constructors
        
        // Called when created from unmanaged code
        public MainWindowController(IntPtr handle) : base(handle)
        {
            Initialize();
        }
        
        // Called when created directly from a XIB file
        [Export ("initWithCoder:")]
        public MainWindowController(NSCoder coder) : base(coder)
        {
            Initialize();
        }
        
        // Call to load from the XIB/NIB file
        public MainWindowController() : base("MainWindow")
        {
            Initialize();
        }
        
        // Shared initialization code
        void Initialize()
        {
            backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += HandleDoWork;
            backgroundWorker.RunWorkerCompleted += HandleRunWorkerCompleted;
        }
        
        #endregion
        
        //strongly typed window accessor
        public new MainWindow Window
        {
            get
            {
                return (MainWindow)base.Window;
            }
        }
        
        public override void WindowDidLoad()
        {
            // TODO: Get this to work properly with embedded resources.
            var directory = new FileInfo(typeof(MainWindowController).Assembly.Location).Directory;
            while (directory.Name != "Release" && directory.Name != "Debug")
            {
                directory = directory.Parent;
            }
            
            string resourcesDirectoryPath = Path.Combine(directory.FullName, "Resources");
            string html = File.ReadAllText(Path.Combine(resourcesDirectoryPath, "editor.html"));
            string css = File.ReadAllText(Path.Combine(resourcesDirectoryPath, "codemirror.css"));
            
            var javascripts = from filename in new[] { "codemirror.js", "ruby.js", "editor.js" }
                              select File.ReadAllText(Path.Combine(resourcesDirectoryPath, filename));
            
            string javascript = string.Join(Environment.NewLine, javascripts);
            
            webView.MainFrame.LoadHtmlString(html.Replace("{{css}}", css).Replace("{{javascript}}", javascript), null);
        }
        
        partial void clickedCompile(NSObject sender)
        {
            string code = webView.StringByEvaluatingJavaScriptFromString("return window.editor.getValue();");
            backgroundWorker.RunWorkerAsync(code);
            compileButton.Enabled = false;
            progressIndicator.Hidden = false;
        }

        void HandleDoWork(object sender, DoWorkEventArgs e)
        {
            string code = (string)e.Argument;
            string randomFileName = Path.GetTempFileName();
            File.WriteAllText(randomFileName, code);
            
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "ruby",
                Arguments = randomFileName,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            
            using (var ruby = Process.Start(processStartInfo))
            {
                var buffer = new StringBuilder();
                
                ruby.EnableRaisingEvents = true;
                ruby.OutputDataReceived += CreateDataReceivedEventHandler(buffer);
                ruby.BeginOutputReadLine();
                ruby.WaitForExit();
                e.Result = buffer.ToString();
            }
        }
        
        DataReceivedEventHandler CreateDataReceivedEventHandler(StringBuilder buffer)
        {
            return (sender, e) =>
            {
                buffer.AppendLine(e.Data);
            };
        }

        void HandleRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            string result = (string)e.Result;
            NSAlert alert = NSAlert.WithMessage(result, "OK", null, null, "");
            alert.BeginSheet(Window);
            progressIndicator.Hidden = true;
            compileButton.Enabled = true;
        }
    }
}
