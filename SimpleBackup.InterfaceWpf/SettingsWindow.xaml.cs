using System;
using System.Collections.Generic;
using System.Linq;
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
            VistaFolderBrowserDialog dialog = new()
            {
                Multiselect = false
            };
            if (dialog.ShowDialog() == true)
            {
                string[] includedPaths = QuickConfig.AppConfig.BackupConfigs[ConfigToEditCB.SelectedIndex].IncludedPaths;
                Array.Resize(ref includedPaths, includedPaths.Length + 1);
                includedPaths[^1] = dialog.SelectedPath;
                QuickConfig.AppConfig.BackupConfigs[ConfigToEditCB.SelectedIndex].IncludedPaths = includedPaths;
                IncludedPathsLB.ItemsSource = includedPaths;
            }
        }

        private void DeleteIncludedPathBnt_Click(object sender, RoutedEventArgs e)
        {
            if (IncludedPathsLB.SelectedItem != null)
            {
                List<string> includedPaths = QuickConfig.AppConfig.BackupConfigs[ConfigToEditCB.SelectedIndex].IncludedPaths.ToList();
                includedPaths.RemoveAt(IncludedPathsLB.SelectedIndex);
                QuickConfig.AppConfig.BackupConfigs[ConfigToEditCB.SelectedIndex].IncludedPaths = includedPaths.ToArray();
                IncludedPathsLB.ItemsSource = includedPaths;
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
                string[] excludedPaths = QuickConfig.AppConfig.BackupConfigs[ConfigToEditCB.SelectedIndex].ExcludedPaths;
                Array.Resize(ref excludedPaths, excludedPaths.Length + 1);
                excludedPaths[^1] = dialog.SelectedPath;
                QuickConfig.AppConfig.BackupConfigs[ConfigToEditCB.SelectedIndex].ExcludedPaths = excludedPaths;
                ExcludedPathsLB.ItemsSource = excludedPaths;
            }
        }

        private void DeleteExcludedPathBnt_Click(object sender, RoutedEventArgs e)
        {
            if (ExcludedPathsLB.SelectedItem != null)
            {
                List<string> excludedPaths = QuickConfig.AppConfig.BackupConfigs[ConfigToEditCB.SelectedIndex].ExcludedPaths.ToList();
                excludedPaths.RemoveAt(ExcludedPathsLB.SelectedIndex);
                QuickConfig.AppConfig.BackupConfigs[ConfigToEditCB.SelectedIndex].ExcludedPaths = excludedPaths.ToArray();
                ExcludedPathsLB.ItemsSource = excludedPaths;
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
    }
}
