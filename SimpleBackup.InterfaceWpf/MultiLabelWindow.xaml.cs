using System.Windows;

namespace SimpleBackup.InterfaceWpf
{
    /// <summary>
    /// Interaction logic for MultiLabelWindow.xaml
    /// </summary>
    public partial class MultiLabelWindow : Window
    {
        public MultiLabelWindow(string[] messages, string title)
        {
            InitializeComponent();
            Title = title;
        }
    }
}
