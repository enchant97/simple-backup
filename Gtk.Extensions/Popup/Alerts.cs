namespace Gtk.Extensions.Popup
{
    /// <summary>
    /// Alert Dialog's to show
    /// </summary>
    public static class Alerts
    {
        private static void ShowAlert(Window parent, string text, MessageType messageType)
        {
            MessageDialog dialog = new(
                parent,
                0,
                messageType,
                ButtonsType.Ok,
                text
            );
            dialog.Run();
            dialog.Destroy();
        }
        /// <summary>
        /// Show an "info" alert
        /// </summary>
        /// <param name="parent">The parent window</param>
        /// <param name="text">The message to show</param>
        public static void ShowInfo(Window parent, string text)
        {
            ShowAlert(parent, text, MessageType.Info);
        }
        /// <summary>
        /// Show an "warning" alert
        /// </summary>
        /// <param name="parent">The parent window</param>
        /// <param name="text">The message to show</param>
        public static void ShowWarning(Window parent, string text)
        {
            ShowAlert(parent, text, MessageType.Warning);
        }
        /// <summary>
        /// Show an "error" alert
        /// </summary>
        /// <param name="parent">The parent window</param>
        /// <param name="text">The message to show</param>
        public static void ShowError(Window parent, string text)
        {
            ShowAlert(parent, text, MessageType.Error);
        }
        /// <summary>
        /// Asks the user user a question,
        /// they can either reply with Yes/No
        /// </summary>
        /// <param name="parent">The parent window</param>
        /// <param name="text">The message to show</param>
        /// <returns>What response was given</returns>
        public static ResponseType ShowQuestion(Window parent, string text)
        {
            MessageDialog dialog = new(
                parent,
                0,
                MessageType.Question,
                ButtonsType.YesNo,
                text
            );
            var response = dialog.Run();
            dialog.Destroy();
            return (ResponseType)response;
        }
    }
}
