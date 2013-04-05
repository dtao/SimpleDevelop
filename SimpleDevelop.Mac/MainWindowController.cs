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
        Engine eng;
        
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
            this.eng = new Engine();
            this.eng.Start();
            this.eng.Stopped += HandleApplicationStopped;
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
            this.webView.MainFrameUrl = "http://localhost:9999/index.html";
        }
        
        void HandleApplicationStopped(object sender, EventArgs e)
        {
            Close();
        }
    }
}
