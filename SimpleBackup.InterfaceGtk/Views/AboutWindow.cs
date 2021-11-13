using Gtk;

namespace SimpleBackup.InterfaceGtk.Views
{
    public class AboutWindow : AboutDialog
    {
        public AboutWindow() : this(new Builder("SimpleBackup.InterfaceGtk.Assets.AboutWindow.glade")) { }
        private AboutWindow(Builder builder) : base(builder.GetRawOwnedObject("AboutWindow"))
        {
            builder.Autoconnect(this);
            AddButton(Stock.Close, ResponseType.Close);
        }
    }
}
