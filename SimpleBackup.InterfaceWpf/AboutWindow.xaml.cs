using System.Windows;

namespace SimpleBackup.InterfaceWpf
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        private void LicencesBnt_Click(object sender, RoutedEventArgs e)
        {
            LicencesWindow licencesWindow = new() { Owner = this };
            licencesWindow.ShowDialog();
        }
    }
}
