using System;
using Gtk;
using Gtk.Extensions.Popup;

namespace SimpleBackup.InterfaceGtk.Views
{
    public class DirectoryListManager : ListManager
    {
        public DirectoryListManager(Window parent, string title, string caption, string[] choices) : base(parent, title, caption, choices)
        {

        }
        public override void OnAddClick(object obj, EventArgs args)
        {
            FileChooserDialog dialog = new(
                "Choice Path To Include",
                this,
                FileChooserAction.SelectFolder,
                "Cancel", ResponseType.Cancel,
                "Select", ResponseType.Ok
            );
            var response = dialog.Run();
            if (response == ((int)ResponseType.Ok))
            {
                AddChoice(dialog.Filename);
            }
            dialog.Destroy();
        }
    }
}
