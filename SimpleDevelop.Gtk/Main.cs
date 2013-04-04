using System;
using Gtk;

namespace SimpleDevelop.Gtk
{
    class MainClass
    {
        public static void Main (string[] args)
        {
            global::Gtk.Application.Init ();
            MainWindow win = new MainWindow ();
            win.Show ();
            global::Gtk.Application.Run ();
        }
    }
}
