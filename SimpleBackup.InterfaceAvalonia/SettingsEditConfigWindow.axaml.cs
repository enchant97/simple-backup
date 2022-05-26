using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using SimpleBackup.Core.Configuration;
using SimpleBackup.Core.Configuration.Types;
using static SimpleBackup.Core.Constants;

namespace SimpleBackup.InterfaceAvalonia
{
    public partial class SettingsEditConfigWindow : Window
    {
        private readonly ObservableCollection<string> currentIncludedPaths = new();
        private readonly ObservableCollection<string> currentExcludedPaths = new();
        public SettingsEditConfigWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            IncludedPaths.Items = currentIncludedPaths;
            ExcludedPaths.Items = currentExcludedPaths;

            CurrentBackupType.Items = Enum.GetNames(typeof(BackupType));
            SelectedConfig.Items = QuickConfig.AppConfig.BackupConfigs;
            SelectedConfig.SelectedIndex = QuickConfig.AppConfig.DefaultConfigI;
        }
        private void OnChangeSelectedConfig(object sender, SelectionChangedEventArgs e)
        {
            currentIncludedPaths.Clear();
            currentExcludedPaths.Clear();

            SelectedConfig.Items = QuickConfig.AppConfig.BackupConfigs;

            BackupConfig selectedConfigContent = QuickConfig.AppConfig.BackupConfigs[SelectedConfig.SelectedIndex];

            ConfigName.Text = selectedConfigContent.Name;
            selectedConfigContent.IncludedPaths.ForEach((string element) => { currentIncludedPaths.Add(element); });
            selectedConfigContent.ExcludedPaths.ForEach((string element) => { currentExcludedPaths.Add(element); });
            CurrentDestination.Content = selectedConfigContent.DestinationPath;
            VersionsToKeep.Value = selectedConfigContent.VersionsToKeep;
            CurrentBackupType.SelectedItem = Enum.GetName(selectedConfigContent.BackupType);
        }
        private async void OnClickAddIncludedPath(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog dialog = new();
            string? path = await dialog.ShowAsync(this);
            if (path != null)
            {
                currentIncludedPaths.Add(path);
            }
        }
        private void OnClickDeleteIncludedPath(object sender, RoutedEventArgs e)
        {
            if (IncludedPaths.SelectedIndex >= 0)
                currentIncludedPaths.RemoveAt(IncludedPaths.SelectedIndex);
        }
        private async void OnClickAddExcludedPath(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog dialog = new();
            string? path = await dialog.ShowAsync(this);
            if (path != null)
            {
                currentExcludedPaths.Add(path);
            }
        }
        private void OnClickDeleteExcludedPath(object sender, RoutedEventArgs e)
        {
            if (ExcludedPaths.SelectedIndex >= 0)
                currentExcludedPaths.RemoveAt(ExcludedPaths.SelectedIndex);
        }
        private async void OnClickSetDestination(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog dialog = new();
            string? path = await dialog.ShowAsync(this);
            if (path != null)
            {
                CurrentDestination.Content = path;
            }
        }
        private void OnClickDeleteConfig(object sender, RoutedEventArgs e)
        {
            // TODO add delete confirm dialog
            if (QuickConfig.AppConfig.BackupConfigs.Count <= 1)
            {
                QuickConfig.AppConfig.BackupConfigs[0] = new BackupConfig();
                QuickConfig.AppConfig.DefaultConfigI = 0;
            }
            else if (SelectedConfig.SelectedIndex != -1)
            {
                QuickConfig.AppConfig.BackupConfigs.RemoveAt(SelectedConfig.SelectedIndex);

                if (SelectedConfig.SelectedIndex == QuickConfig.AppConfig.DefaultConfigI)
                    QuickConfig.AppConfig.DefaultConfigI = 0;
            }
            QuickConfig.Write();
            Close();
        }
        private void OnClickCancel(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void OnClickSave(object sender, RoutedEventArgs e)
        {
            int selectedConfigI = SelectedConfig.SelectedIndex;

            string configName = ConfigName.Text.Trim();
            if (configName.Length == 0)
            {
                System.Diagnostics.Debug.WriteLine("ConfigName is empty");
                // TODO show user error here
                return;
            }
            QuickConfig.AppConfig.BackupConfigs[selectedConfigI].Name = configName;
            QuickConfig.AppConfig.BackupConfigs[selectedConfigI].IncludedPaths = currentIncludedPaths.ToList();
            QuickConfig.AppConfig.BackupConfigs[selectedConfigI].ExcludedPaths = currentExcludedPaths.ToList();
            QuickConfig.AppConfig.BackupConfigs[selectedConfigI].DestinationPath = CurrentDestination.Content.ToString();
            QuickConfig.AppConfig.BackupConfigs[selectedConfigI].VersionsToKeep = (int)VersionsToKeep.Value;
            if (CurrentBackupType.SelectedItem == null)
            {
                throw new Exception("BackupType not selected");
            }
            BackupType backupTypeValue = Enum.Parse<BackupType>((string)CurrentBackupType.SelectedItem);
            QuickConfig.AppConfig.BackupConfigs[selectedConfigI].BackupType = backupTypeValue;

            QuickConfig.Write();
            Close();
        }
    }
}
