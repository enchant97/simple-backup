using Gtk;

namespace SimpleBackup.InterfaceGtk.Views
{
    class AskChoice : Dialog
    {
        private readonly ComboBox comboBox;
        public AskChoice(Window parent, string title, string caption, string[] choices) : base(title, parent, 0)
        {
            Label captionLabel = new(caption);
            ContentArea.PackStart(captionLabel, false, false, 10);

            comboBox = new(choices);
            ContentArea.PackStart(comboBox, true, false, 0);

            AddButton(Stock.Cancel, ResponseType.Cancel);
            AddButton(Stock.Ok, ResponseType.Ok);

            ShowAll();
        }
        public int SelectedI { get => comboBox.Active; }
    }
}
