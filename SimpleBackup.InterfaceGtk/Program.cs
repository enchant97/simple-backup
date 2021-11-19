using System;
using Gtk;

namespace SimpleBackup.InterfaceGtk
{
    class Program
    {
        public static Gdk.Pixbuf SharedAppIcon = new(System.Reflection.Assembly.Load("SimpleBackup.Resources"), "SimpleBackup.Resources.Icon.svg", 250, 250);
        public static Application App;
        public static Window MainWin;
        [STAThread]
        static void Main(string[] args)
        {
            Application.Init();

            App = new("enchant97.SimpleBackup", GLib.ApplicationFlags.None);
            App.Register(GLib.Cancellable.Current);

            MainWin = new Views.MainWindow();
            App.AddWindow(MainWin);

            MainWin.ShowAll();
            Application.Run();
        }
    }
}
