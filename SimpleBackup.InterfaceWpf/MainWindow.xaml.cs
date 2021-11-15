using System.Windows;

namespace SimpleBackup.InterfaceWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MenuExitBnt_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuPreferencesBnt_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuGetStartedBnt_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuAboutBnt_Click(object sender, RoutedEventArgs e)
        {

        }

        private void StartBackupBnt_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ShowErrorsBnt_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
