using Gtk.Extensions.Widgets;
using System;
using System.Linq;

namespace Gtk.Extensions.Popup
{
    public class ListManager : Dialog
    {
        #region Properties
        private readonly ListBox listBox;
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
        #endregion
        #region Initialisers
        public ListManager(Window parent, string title, string caption, string[] choices) : base(title, parent, 0)
        {
            Label captionLabel = new(caption);
            ContentArea.PackStart(captionLabel, false, false, 10);

            ScrolledWindow scrolled = new();
            listBox = new();
            listBox.SelectionMode = SelectionMode.None;
            scrolled.Add(listBox);
            ContentArea.PackStart(scrolled, true, true, 0);
            AddChoice(choices);

            Button addBnt = new("Add");
            addBnt.Clicked += OnAddClick;
            ContentArea.PackStart(addBnt, true, false, 0);

            AddButton(Stock.Cancel, ResponseType.Cancel);
            AddButton(Stock.Ok, ResponseType.Ok);

            ShowAll();
        }
        #endregion
        #region Methods
        public void AddChoice(string choice)
        {
            Widgets.ListBoxWithString row = new();
            row.DataString = choice;
            HBox rowBox = new();

            Label choiceLabel = new(choice);
            Button choiceDelete = new("X");
            choiceDelete.Clicked += (sender, args) => OnRemoveClick(sender, args, row);
            rowBox.PackStart(choiceLabel, true, false, 0);
            rowBox.PackEnd(choiceDelete, false, false, 0);
            row.Add(rowBox);
            listBox.Add(row);
            listBox.ShowAll();
        }
        public void AddChoice(string[] choices)
        {
            foreach (var choice in choices)
            {
                AddChoice(choice);
            }
        }
        public void RemoveChoice(ListBoxWithString rowWidget)
        {
            listBox.Remove(rowWidget);
        }
        #endregion
        #region Events
        /// <summary>
        /// Method that is triggered on row remove,
        /// defaults to just removing the row
        /// </summary>
        public virtual void OnRemoveClick(object obj, EventArgs args, ListBoxWithString rowWidget)
        {
            RemoveChoice(rowWidget);
        }
        /// <summary>
        /// Method that is triggered on the add button being clicked,
        /// defaults to spawing a AskTextInput dialog
        /// </summary>
        public virtual void OnAddClick(object obj, EventArgs args)
        {
            AskTextInput dialog = new(
                this,
                "Add Value",
                "Enter A New Value"
            );
            var response = dialog.Run();
            if (response == ((int)ResponseType.Ok))
            {
                AddChoice(dialog.Input);
            }
            dialog.Destroy();
        }
        #endregion
    }
}
