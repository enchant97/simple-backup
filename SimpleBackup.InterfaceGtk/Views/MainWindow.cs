using Gtk;
using SimpleBackup.Core;

namespace SimpleBackup.InterfaceGtk.Views
{
    class MainWindow : Window
    {
        public MainWindow() : base(Constants.AppName + " - GUI Mode")
        {
            SetDefaultSize(400, 200);
            SetPosition(WindowPosition.Center);
            DeleteEvent += OnDelete;

            Label tempMessage = new(
                "SimpleBackup GUI is not currently implemented");

            Add(tempMessage);

            ShowAll();
        }
        private void OnDelete(object obj, DeleteEventArgs args)
        {
            Application.Quit();
        }
    }
}
