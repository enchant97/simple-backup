using Gtk;

namespace SimpleBackup.InterfaceGtk
{
    class Program
    {
        static void Main(string[] args)
        {
            Application.Init();
            Views.MainWindow mainWindow = new();
            mainWindow.Show();
            Application.Run();
        }
    }
}
