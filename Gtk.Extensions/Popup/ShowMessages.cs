namespace Gtk.Extensions.Popup
{
    public class ShowMessages : Dialog
    {
        public ShowMessages(Window parent, string title, string caption, string[] messages) : base(title, parent, 0)
        {
            Label captionLabel = new(caption);
            ContentArea.PackStart(captionLabel, false, false, 10);
            ScrolledWindow scrolled = new();
            VBox messagesContainer = new();
            scrolled.Add(messagesContainer);
            ContentArea.PackStart(scrolled, true, true, 0);

            foreach (var message in messages)
            {
                Label messageLabel = new(message);
                messagesContainer.PackStart(messageLabel, true, false, 0);
            }

            AddButton(Stock.Close, ResponseType.Close);
            ShowAll();
        }
    }
}
