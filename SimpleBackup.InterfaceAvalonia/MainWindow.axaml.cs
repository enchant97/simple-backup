using Avalonia.Controls;
using Avalonia.Interactivity;

namespace SimpleBackup.InterfaceAvalonia
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void OnClickMenuAbout(object sender, RoutedEventArgs e)
        {
            AboutWindow window = new();
            window.ShowDialog(this);
        }
    }
}
