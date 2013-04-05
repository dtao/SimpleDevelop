using System;
using Gtk;
using WebKit;

using SimpleDevelop;

public partial class MainWindow : Gtk.Window
{
    Engine eng;
    WebView webView;

    public MainWindow() : base(Gtk.WindowType.Toplevel)
    {
        Build();

        this.eng = new Engine();
        this.eng.Start();
        this.eng.Stopped += HandleApphandleStopped;

        this.webView = new WebView();
        this.mainContainer.Add(this.webView);
        this.webView.Show();
        this.webView.LoadUri("http://localhost:9999/index.html");
    }

    protected void OnDeleteEvent(object sender, DeleteEventArgs a)
    {
        Application.Quit();
        a.RetVal = true;
    }

    void HandleApphandleStopped(object sender, EventArgs e)
    {
        Application.Quit();
    }
}
