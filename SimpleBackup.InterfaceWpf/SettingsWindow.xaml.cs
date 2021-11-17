using SimpleBackup.Core.Configuration;
using SimpleBackup.Core.Configuration.Types;
using System;
using System.Windows;
using System.Windows.Controls;
using Ookii.Dialogs.Wpf;

namespace SimpleBackup.InterfaceWpf
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();

            ShowHelpCB.IsChecked = QuickConfig.AppConfig.ShowHelp;
            DefaultBackupConfigCB.ItemsSource = QuickConfig.AppConfig.BackupConfigs;
            DefaultBackupConfigCB.SelectedIndex = QuickConfig.AppConfig.DefaultConfigI;
            ConfigToEditCB.ItemsSource = QuickConfig.AppConfig.BackupConfigs;
            ConfigToEditCB.SelectedIndex = QuickConfig.AppConfig.DefaultConfigI;
            RefreshCurrBackupConfig();
        }

        private void RefreshCurrBackupConfig()
        {
            BackupConfig currConfig = QuickConfig.AppConfig.BackupConfigs[ConfigToEditCB.SelectedIndex];
            CurrConfigNameTB.Text = currConfig.Name;
            ConfigDestination.Content = currConfig.DestinationPath;
        }

        private void NewConfigBnt_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NewConfigNameTB.Text))
            {
                _ = MessageBox.Show(
                    this,
                    "No name input", "Creation Error",
                    MessageBoxButton.OK, MessageBoxImage.Error
                );
                return;
            }
            BackupConfig backupConfig = new() { Name = NewConfigNameTB.Text };
            Array.Resize(ref QuickConfig.AppConfig.BackupConfigs, QuickConfig.AppConfig.BackupConfigs.Length + 1);
            QuickConfig.AppConfig.BackupConfigs[^1] = backupConfig;

            NewConfigNameTB.Clear();
            DefaultBackupConfigCB.Items.Refresh();
            ConfigToEditCB.Items.Refresh();
        }

        private void SetDestinationBnt_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog dialog = new()
            {
                Multiselect = false
            };
            if (dialog.ShowDialog() == true)
            {
                ConfigDestination.Content = dialog.SelectedPath;
                QuickConfig.AppConfig.BackupConfigs[ConfigToEditCB.SelectedIndex].DestinationPath = dialog.SelectedPath;
            }
        }

        private void AddIncludedPathBnt_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteIncludedPathBnt_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddExcludedPathBnt_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteExcludedPathBnt_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ShowHelpCB_Click(object sender, RoutedEventArgs e)
        {
            if (ShowHelpCB.IsChecked == true)
                QuickConfig.AppConfig.ShowHelp = true;
            else
                QuickConfig.AppConfig.ShowHelp = false;
        }

        private void DefaultBackupConfigCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            QuickConfig.AppConfig.DefaultConfigI = DefaultBackupConfigCB.SelectedIndex;
        }

        private void ConfigToEditCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void CurrConfigNameTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        private void VersionsToKeepTB_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
