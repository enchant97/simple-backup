using Gtk;

namespace SimpleBackup.InterfaceGtk.Views
{
    public class HelpWindow : Dialog
    {
        public HelpWindow() : this(new Builder("SimpleBackup.InterfaceGtk.Assets.HelpWindow.glade")) { }
        private HelpWindow(Builder builder) : base(builder.GetRawOwnedObject("HelpWindow"))
        {
            builder.Autoconnect(this);
            AddButton(Stock.Close, ResponseType.Close);
        }
    }
}
