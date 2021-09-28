using System;

namespace Gtk.Extensions.Widgets
{
    public class ListBoxWithString : ListBoxRow
    {
        public string DataString;
        public ListBoxWithString() : base() { }
        public ListBoxWithString(string dataString) : base()
        {
            DataString = dataString;
        }
        public ListBoxWithString(IntPtr raw) : base(raw) { }
        public ListBoxWithString(IntPtr raw, string dataString) : base(raw)
        {
            DataString = dataString;
        }
    }
}
