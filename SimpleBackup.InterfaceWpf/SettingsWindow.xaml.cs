using System;
using System.Windows;
using System.Windows.Controls;
using Ookii.Dialogs.Wpf;
using SimpleBackup.Core;
using SimpleBackup.Core.Configuration;
using SimpleBackup.Core.Configuration.Types;

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
            BackupTypeCB.ItemsSource = Enum.GetNames(typeof(Constants.BackupType));
            RefreshCurrBackupConfig();
        }

        private void RefreshCurrBackupConfig()
        {
            BackupConfig currConfig = QuickConfig.AppConfig.BackupConfigs[ConfigToEditCB.SelectedIndex];
            CurrConfigNameTB.Text = currConfig.Name;
            ConfigDestination.Content = currConfig.DestinationPath;
            IncludedPathsLB.ItemsSource = currConfig.IncludedPaths;
            ExcludedPathsLB.ItemsSource = currConfig.ExcludedPaths;
            VersionsToKeepTB.Text = currConfig.VersionsToKeep.ToString();
            BackupTypeCB.SelectedItem = Enum.GetName(currConfig.BackupType);
        }

        private void ResetBnt_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
            "Reseting the app will delete all configs", "Confirm",
            MessageBoxButton.OKCancel,
            MessageBoxImage.Question
            );
            if (result == MessageBoxResult.OK)
            {
                QuickConfig.Reset();
            }
            Close();
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
            QuickConfig.AppConfig.BackupConfigs.Add(backupConfig);

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
            VistaFolderBrowserDialog dialog = new()
            {
                Multiselect = false
            };
            if (dialog.ShowDialog() == true)
            {
                QuickConfig.AppConfig.BackupConfigs[ConfigToEditCB.SelectedIndex].IncludedPaths.Add(dialog.SelectedPath);
                IncludedPathsLB.Items.Refresh();
            }
        }

        private void DeleteIncludedPathBnt_Click(object sender, RoutedEventArgs e)
        {
            if (IncludedPathsLB.SelectedItem != null)
            {
                QuickConfig.AppConfig.BackupConfigs[ConfigToEditCB.SelectedIndex].IncludedPaths.RemoveAt(IncludedPathsLB.SelectedIndex);
                IncludedPathsLB.Items.Refresh();
            }
        }

        private void AddExcludedPathBnt_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog dialog = new()
            {
                Multiselect = false
            };
            if (dialog.ShowDialog() == true)
            {
                QuickConfig.AppConfig.BackupConfigs[ConfigToEditCB.SelectedIndex].ExcludedPaths.Add(dialog.SelectedPath);
                ExcludedPathsLB.Items.Refresh();
            }
        }

        private void DeleteExcludedPathBnt_Click(object sender, RoutedEventArgs e)
        {
            if (ExcludedPathsLB.SelectedItem != null)
            {
                QuickConfig.AppConfig.BackupConfigs[ConfigToEditCB.SelectedIndex].ExcludedPaths.RemoveAt(ExcludedPathsLB.SelectedIndex);
                ExcludedPathsLB.Items.Refresh();
            }
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
            RefreshCurrBackupConfig();
        }

        private void CurrConfigNameTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            QuickConfig.AppConfig.BackupConfigs[ConfigToEditCB.SelectedIndex].Name = CurrConfigNameTB.Text;
        }

        private void VersionsToKeepTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool isValid = int.TryParse(VersionsToKeepTB.Text, out int newVersion);
            if (isValid)
            {
                QuickConfig.AppConfig.BackupConfigs[ConfigToEditCB.SelectedIndex].VersionsToKeep = newVersion;
            }
        }

        private void BackupTypeCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Constants.BackupType backupType = Enum.Parse<Constants.BackupType>((string)BackupTypeCB.SelectedItem);
            QuickConfig.AppConfig.BackupConfigs[ConfigToEditCB.SelectedIndex].BackupType = backupType;
        }

        private void DeleteConfigBnt_Click(object sender, RoutedEventArgs e)
        {
            if (QuickConfig.AppConfig.BackupConfigs.Count <= 1)
            {
                QuickConfig.AppConfig.BackupConfigs[0] = new BackupConfig();
                QuickConfig.AppConfig.DefaultConfigI = 0;

                ConfigToEditCB.SelectedIndex = QuickConfig.AppConfig.DefaultConfigI;               
            }
            else if (ConfigToEditCB.SelectedIndex != -1)
            {
                QuickConfig.AppConfig.BackupConfigs.RemoveAt(ConfigToEditCB.SelectedIndex);

                if (ConfigToEditCB.SelectedIndex == QuickConfig.AppConfig.DefaultConfigI)
                    QuickConfig.AppConfig.DefaultConfigI = 0;

                ConfigToEditCB.SelectedIndex = QuickConfig.AppConfig.DefaultConfigI;
            }
            RefreshCurrBackupConfig();
        }
    }
}
