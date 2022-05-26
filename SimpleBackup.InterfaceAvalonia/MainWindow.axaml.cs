using Avalonia.Controls;
using Avalonia.Interactivity;
using MessageBox.Avalonia;
using MessageBox.Avalonia.Enums;
using MessageBox.Avalonia.BaseWindows.Base;
using SimpleBackup.Core.Configuration;
using System;
using SimpleBackup.Core.Configuration.Types;
using System.Collections.ObjectModel;
using System.Threading;
using SimpleBackup.Core.Backup;
using SimpleBackup.Core;
using Avalonia.Threading;
using SimpleBackup.Core.Paths;

namespace SimpleBackup.InterfaceAvalonia
{
    public partial class MainWindow : Window
    {
        #region Fields
        private Thread? backupThread;
        private BackupHandler? backupHandler;
        private readonly ObservableCollection<string> loggedErrors;
        public int FoundCount { get; private set; }
        public int CopiedCount { get; private set; }
        #endregion
        public MainWindow()
        {
            loggedErrors = new();

            Opened += (sender, evt) => OnWindowLoad();
            InitializeComponent();

            LoggedErrorsLB.Items = loggedErrors;
            ShowBackupConfigs();
            ShowSelectedConfigOnUI();
        }
        #region UI Helpers
        public static string ExceptionToString(BackupHandlerErrorEventArgs args)
        {
            string errorMsg = args.ErrorType switch
            {
                Constants.ErrorTypes.NO_PERMISSION => "No permission at",
                Constants.ErrorTypes.NOT_COPYABLE_TYPE => "Not copyable type at",
                Constants.ErrorTypes.NOT_FOUND => "Not found at",
                _ => "Unhandled error at"

            };
            return string.Format("{0} '{1}'", errorMsg, args.FullPath);
        }
        private void ShowBackupConfigs()
        {
            CurrConfigCB.Items = QuickConfig.AppConfig.BackupConfigs;
            CurrConfigCB.SelectedIndex = QuickConfig.AppConfig.DefaultConfigI;
        }
        private void ShowSelectedConfigOnUI()
        {
            if (CurrConfigCB.SelectedItem != null)
            {
                BackupConfig backupConfig = (BackupConfig)CurrConfigCB.SelectedItem;
                LastBackupLabel.Content = backupConfig.LastBackup;
                DestinationLabel.Content = backupConfig.DestinationPath;
                TypeLabel.Content = Enum.GetName(backupConfig.BackupType);
            }
        }
        private void BackupRunningUi(bool backupRunning)
        {
            CurrConfigCB.IsEnabled = !backupRunning;
            MenuImportConfigBnt.IsEnabled = !backupRunning;
            MenuPreferencesBnt.IsEnabled = !backupRunning;

            if (backupRunning)
                StartBackupBnt.Content = "Stop Backup";
            else
                StartBackupBnt.Content = "Start Backup";
        }
        #endregion
        #region Backup Handlers
        private void HandleBackupDiscovery()
        {
            FoundCount++;
            MainStatus.Content = string.Format("Found {0}", FoundCount);
        }
        private void HandleBackupCopied()
        {
            CopiedCount++;
            MainStatus.Content = string.Format("Copied {0}/{1}", CopiedCount, FoundCount);
        }
        private void HandleBackupException(BackupHandlerErrorEventArgs args)
        {
            loggedErrors.Add(ExceptionToString(args));
        }
        private void HandleBackupClean()
        {
            MainStatus.Content = "Removing old backups";
        }
        private void HandleBackupFinished()
        {
            QuickConfig.AppConfig.BackupConfigs[CurrConfigCB.SelectedIndex].LastBackup = DateTime.UtcNow;
            QuickConfig.Write();

            if (loggedErrors.Count > 0)
                MainStatus.Content = "Backup Finished (With Errors)";
            else
                MainStatus.Content = "Backup Finished";
            backupThread = null;
            backupHandler = null;
            BackupRunningUi(false);
        }
        #endregion
        #region Backup Control
        private async void StartBackup(BackupConfig backupConfig)
        {
            await Dispatcher.UIThread.InvokeAsync(
                () =>
                {
                    BackupRunningUi(true);
                    loggedErrors.Clear();
                    FoundCount = 0;
                    CopiedCount = 0;
                    MainStatus.Content = "Backup Starting";
                },
                DispatcherPriority.Background
            );

            string backupDstPath = Generation.GenerateBackupName(
                backupConfig.DestinationPath,
                backupConfig.BackupType
            );

            backupHandler = new BackupHandler(
                backupDstPath,
                backupConfig.IncludedPaths.ToArray(),
                backupConfig.ExcludedPaths.ToArray(),
                QuickConfig.AppConfig.ExcludedFilenames,
                backupConfig.BackupType,
                false
            );

            // setup events
            backupHandler.DiscoveryEvent += (object? sender, BackupHandlerEventArgs args) =>
            {
                Dispatcher.UIThread.Post(HandleBackupDiscovery);
            };
            backupHandler.CopyEvent += (object? sender, BackupHandlerEventArgs args) =>
            {
                Dispatcher.UIThread.Post(HandleBackupCopied);
            };
            backupHandler.ExceptionDiscoveringEvent += (object? sender, BackupHandlerErrorEventArgs args) =>
            {
                Dispatcher.UIThread.Post(() => HandleBackupException(args));
            };
            backupHandler.ExceptionCopyEvent += (object? sender, BackupHandlerErrorEventArgs args) =>
            {
                Dispatcher.UIThread.Post(() => HandleBackupException(args));
            };
            backupHandler.FinishedEvent += async (object? sender, EventArgs args) =>
            {
                if (backupConfig.VersionsToKeep > 0)
                {
                    await Dispatcher.UIThread.InvokeAsync(HandleBackupClean);
                    int backupsRemoved = Cleaning.RemovePreviousBackups(
                        backupConfig.VersionsToKeep,
                        backupConfig.DestinationPath
                    );
                }
                Dispatcher.UIThread.Post(HandleBackupFinished);
            };

            // start backup
            backupHandler.Start();
        }
        #endregion
        #region Events
        private void OnWindowLoad()
        {
            // show inital help dialog if needed
            if (QuickConfig.AppConfig.ShowHelp)
            {
                GettingStartedWindow window = new();
                window.ShowDialog(this);

                QuickConfig.AppConfig.ShowHelp = false;
                QuickConfig.Write();
            }
        }
        private async void OnClickMenuImportConfig(object sender, RoutedEventArgs e)
        {
            IMsBoxWindow<ButtonResult> prompt = MessageBoxManager.GetMessageBoxStandardWindow(
                "Confirm",
                "Importing a new config will overwrite your current one.",
                ButtonEnum.OkCancel,
                MessageBox.Avalonia.Enums.Icon.Warning
            );
            ButtonResult promptResult = await prompt.Show();
            if (promptResult == ButtonResult.Ok)
            {
                OpenFileDialog dialog = new()
                {
                    AllowMultiple = false,
                    Filters = {
                        new() {
                        Extensions = {".xml"},
                        Name = "XML Files"
                        }
                    }
                };
                string[]? filepaths = await dialog.ShowAsync(this);
                if (filepaths != null)
                {
                    string filepath = filepaths[0];
                    MainStatus.Content = "Importing new config";
                    QuickConfig.Read(filepath);
                    QuickConfig.Write();
                    // FIXME: refresh UI here
                    MainStatus.Content = "Imported new config";
                }
            }
        }
        private async void OnClickMenuExportConfig(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new()
            {
                DefaultExtension = ".xml",
                Filters =
                {
                    new() {
                        Extensions = {".xml"},
                        Name = "XML Files"
                    }
                }
            };
            string? filepath = await dialog.ShowAsync(this);
            if (filepath != null)
            {
                MainStatus.Content = "Exporting config";
                QuickConfig.Write(filepath);
                MainStatus.Content = "Done Exporting config";
            }
        }
        private void OnClickMenuExit(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private async void OnClickMenuSettings(object sender, RoutedEventArgs e)
        {
            SettingsWindow window = new();
            await window.ShowDialog(this);

            ShowBackupConfigs();
            ShowSelectedConfigOnUI();
        }
        private void OnClickMenuGettingStarted(object sender, RoutedEventArgs e)
        {
            GettingStartedWindow window = new();
            window.ShowDialog(this);
        }
        private void OnClickMenuAbout(object sender, RoutedEventArgs e)
        {
            AboutWindow window = new();
            window.ShowDialog(this);
        }
        private void OnSelectionCurrConfigCB(object sender, SelectionChangedEventArgs e)
        {
            ShowSelectedConfigOnUI();
        }
        private void OnClickStartStopBackup(object sender, RoutedEventArgs e)
        {
            if (backupThread != null)
            {
                if (backupHandler != null)
                {
                    loggedErrors.Add("Backup stopped by user...");
                    backupHandler.Stop();
                }
                return;
            }
            BackupConfig backupConfig = QuickConfig.AppConfig.BackupConfigs[CurrConfigCB.SelectedIndex];
            if (string.IsNullOrEmpty(backupConfig.DestinationPath))
            {
                IMsBoxWindow<ButtonResult> prompt = MessageBoxManager.GetMessageBoxStandardWindow(
                    "Cannot Start Backup",
                    "No backup destination path has been set",
                    ButtonEnum.Ok,
                    MessageBox.Avalonia.Enums.Icon.Warning
                );
                prompt.ShowDialog(this);
                return;
            }
            backupThread = new(() => StartBackup(backupConfig));
            backupThread.Start();
        }
        #endregion
    }
}
