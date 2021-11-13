using System;
using Gtk;

namespace SimpleBackup.InterfaceGtk
{
    class Program
    {
        public static Gdk.Pixbuf SharedAppIcon = new(null, "SimpleBackup.InterfaceGtk.Assets.Icon.svg");
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
