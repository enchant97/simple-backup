using Gtk;

namespace SimpleBackup.InterfaceGtk.Views
{
    class AskTextInput : Dialog
    {
        private readonly Entry entry;
        public AskTextInput(Window parent, string title, string caption, string placeholder = "") : base(title, parent, 0)
        {
            Label captionLabel = new(caption);
            ContentArea.PackStart(captionLabel, false, false, 10);

            entry = new();
            entry.PlaceholderText = placeholder;
            ContentArea.PackStart(entry, true, false, 0);

            AddButton(Stock.Cancel, ResponseType.Cancel);
            AddButton(Stock.Ok, ResponseType.Ok);

            ShowAll();
        }
        public string Input { get => entry.Text; }
    }
}
