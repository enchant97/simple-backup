using System.Windows;
using SimpleBackup.Core.Configuration;
using SimpleBackup.Core.Configuration.Types;

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
            CurrConfigCB.ItemsSource = QuickConfig.AppConfig.BackupConfigs;
            CurrConfigCB.SelectedIndex = QuickConfig.AppConfig.DefaultConfigI;
        }

        private void MenuExitBnt_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuPreferencesBnt_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new() { Owner = this };
            _ = settingsWindow.ShowDialog();
        }

        private void MenuGetStartedBnt_Click(object sender, RoutedEventArgs e)
        {
            GetStartedWindow getStartedWindow = new() { Owner = this };
            getStartedWindow.ShowDialog();
        }

        private void MenuAboutBnt_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new() { Owner = this };
            aboutWindow.ShowDialog();
        }

        private void StartBackupBnt_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ShowErrorsBnt_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CurrConfigCB_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            BackupConfig backupConfig = (BackupConfig)CurrConfigCB.SelectedItem;
            LastBackupLabel.Content = backupConfig.LastBackup;
            DestinationLabel.Content = backupConfig.DestinationPath;
            TypeLabel.Content = backupConfig.BackupType.ToString();
        }
    }
}
