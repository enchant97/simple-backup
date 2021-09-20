using System;
using Gtk;

namespace SimpleBackup.InterfaceGtk
{
    class Program
    {
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


            GLib.Menu menu = new();

            GLib.MenuItem fileItem = new("File", "app.file");
            GLib.Menu fileMenu = new();
            fileMenu.AppendItem(new GLib.MenuItem("Quit", "app.quit"));
            fileItem.Submenu = fileMenu;

            GLib.MenuItem helpItem = new("Help", "app.help");
            GLib.Menu helpMenu = new();
            helpMenu.AppendItem(new GLib.MenuItem("About", "app.about"));
            helpItem.Submenu = helpMenu;

            menu.AppendItem(fileItem);
            menu.AppendItem(helpItem);

            App.AppMenu = menu;

            GLib.SimpleAction quitAction = new("quit", null);
            quitAction.Activated += QuitEvent;
            App.AddAction(quitAction);

            GLib.SimpleAction aboutAction = new("about", null);
            aboutAction.Activated += AboutEvent;
            App.AddAction(aboutAction);

            MainWin.ShowAll();
            Application.Run();
        }
        private static void QuitEvent(object sender, EventArgs e)
        {
            Application.Quit();
        }
        private static void AboutEvent(object sender, EventArgs e)
        {

        }
    }
}
