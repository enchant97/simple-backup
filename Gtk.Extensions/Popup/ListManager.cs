using System;
using System.Linq;
using Gtk.Extensions.Widgets;

namespace Gtk.Extensions.Popup
{
    public class ListManager : Dialog
    {
        private readonly ListBox listBox;
        public ListManager(Window parent, string title, string caption, string[] choices) : base(title, parent, 0)
        {
            Label captionLabel = new(caption);
            ContentArea.PackStart(captionLabel, false, false, 10);

            ScrolledWindow scrolled = new();
            listBox = new();
            listBox.SelectionMode = SelectionMode.None;
            scrolled.Add(listBox);
            ContentArea.PackStart(scrolled, true, true, 0);
            LoadNewChoices(choices);

            Button addBnt = new("Add");
            addBnt.Clicked += OnAddClick;
            ContentArea.PackStart(addBnt, true, false, 0);

            AddButton(Stock.Cancel, ResponseType.Cancel);
            AddButton(Stock.Ok, ResponseType.Ok);

            ShowAll();
        }
        private void AddChoiceRow(string choice)
        {
            Widgets.ListBoxWithString row = new();
            row.DataString = choice;
            HBox rowBox = new();

            Label choiceLabel = new(choice);
            Button choiceDelete = new("X");
            choiceDelete.Clicked += (sender, args) => OnChoiceRemoveClick(sender, args, row);
            rowBox.PackStart(choiceLabel, true, false, 0);
            rowBox.PackEnd(choiceDelete, false, false, 0);
            row.Add(rowBox);
            listBox.Add(row);
            listBox.ShowAll();
        }
        private void LoadNewChoices(string[] choices)
        {
            foreach (var choice in choices)
            {
                AddChoiceRow(choice);
            }
        }
        private void OnChoiceRemoveClick(object obj, EventArgs args, ListBoxWithString rowWidget)
        {
            listBox.Remove(rowWidget);
        }
        private void OnAddClick(object obj, EventArgs args)
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
                AddChoiceRow(dialog.Filename);
            }
            dialog.Destroy();
        }
        public string[] Choices
        {
            get
            {
                return listBox.Children.Select(widget =>
                {
                    var lb = (ListBoxWithString)widget;
                    return lb.DataString;
                }).ToArray();
            }
        }
    }
}
