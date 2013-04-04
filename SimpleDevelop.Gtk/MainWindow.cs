using System;
using Gtk;
using WebKit;

using SimpleDevelop;

public partial class MainWindow: Gtk.Window
{
    SimpleDevelop.Application app;
    WebView webView;

    public MainWindow(): base (Gtk.WindowType.Toplevel)
    {
        Build();

        this.app = new SimpleDevelop.Application();
        this.app.Start();
        this.app.Stopped += HandleApphandleStopped;

        this.webView = new WebView();
        this.mainContainer.Add(this.webView);
        this.webView.Show();
        this.webView.LoadUri("http://localhost:9999/index.html");
    }

    protected void OnDeleteEvent(object sender, DeleteEventArgs a)
    {
        Gtk.Application.Quit();
        a.RetVal = true;
    }

    void HandleApphandleStopped (object sender, EventArgs e)
    {
        Gtk.Application.Quit();
    }
}
