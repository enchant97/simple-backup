using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using SimpleBackup.Core;
using SimpleBackup.Core.Backup;
using SimpleBackup.Core.Configuration;
using SimpleBackup.Core.Configuration.Types;

namespace SimpleBackup.InterfaceWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Properties
        private Thread? backupThread;
        private readonly List<string> raisedErrors;
        public int FoundCount { get; private set; }
        public int CopiedCount { get; private set; }
        #endregion
        public MainWindow()
        {
            InitializeComponent();
            raisedErrors = new List<string>();
            CurrConfigCB.ItemsSource = QuickConfig.AppConfig.BackupConfigs;
            CurrConfigCB.SelectedIndex = QuickConfig.AppConfig.DefaultConfigI;
        }

        #region Helpers
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
        private void RefreshUi()
        {
            BackupConfig backupConfig = (BackupConfig)CurrConfigCB.SelectedItem;
            LastBackupLabel.Content = backupConfig.LastBackup;
            DestinationLabel.Content = backupConfig.DestinationPath;
            TypeLabel.Content = Enum.GetName(backupConfig.BackupType);
        }
        #endregion

        #region Handlers
        private void HandleBackupStart()
        {
            MainStatus.Content = "Backup Starting";
            FoundCount = 0;
            CopiedCount = 0;
        }

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
            raisedErrors.Add(ExceptionToString(args));
            ShowErrorsBnt.Content = string.Format("Show Errors ({0})", raisedErrors.Count);
        }

        private void HandleBackupClean()
        {
            MainStatus.Content = "Removing old backups";
        }

        private void HandleBackupFinished()
        {
            QuickConfig.AppConfig.BackupConfigs[CurrConfigCB.SelectedIndex].LastBackup = DateTime.UtcNow;
            QuickConfig.Write();

            if (raisedErrors.Count > 0)
                MainStatus.Content = "Backup Finished (With Errors)";
            else
                MainStatus.Content = "Backup Finished";
            backupThread = null;
        }
        #endregion

        private void RunBackup(BackupConfig currConfig)
        {
            Application.Current.Dispatcher.BeginInvoke(HandleBackupStart);

            string backupDstPath = Paths.GenerateBackupName(
                currConfig.DestinationPath,
                currConfig.BackupType
            );

            BackupHandler backupHandler = new(
                backupDstPath,
                currConfig.IncludedPaths,
                currConfig.ExcludedPaths,
                QuickConfig.AppConfig.ExcludedFilenames,
                currConfig.BackupType,
                false
            );

            // setup events
            backupHandler.DiscoveryEvent += (object sender, BackupHandlerEventArgs args) =>
            {
                Application.Current.Dispatcher.BeginInvoke(HandleBackupDiscovery);
            };
            backupHandler.CopyEvent += (object sender, BackupHandlerEventArgs args) =>
            {
                Application.Current.Dispatcher.BeginInvoke(HandleBackupCopied);
            };
            backupHandler.ExceptionDiscoveringEvent += (object sender, BackupHandlerErrorEventArgs args) =>
            {
                Application.Current.Dispatcher.BeginInvoke(HandleBackupException, args);
            };
            backupHandler.ExceptionCopyEvent += (object sender, BackupHandlerErrorEventArgs args) =>
            {
                Application.Current.Dispatcher.BeginInvoke(HandleBackupException, args);
            };
            backupHandler.FinishedEvent += (object sender, EventArgs args) =>
            {
                if (currConfig.VersionsToKeep > 0)
                {
                    Application.Current.Dispatcher.BeginInvoke(HandleBackupClean);
                    int backupsRemoved = Cleaning.RemovePreviousBackups(
                        currConfig.VersionsToKeep,
                        currConfig.DestinationPath
                    );
                }
                Application.Current.Dispatcher.BeginInvoke(HandleBackupFinished);
            };

            // start backup
            backupHandler.Start();
        }

        #region Event Handlers
        private void MenuExitBnt_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuPreferencesBnt_Click(object sender, RoutedEventArgs e)
        {
            if (backupThread != null)
            {
                _ = MessageBox.Show("Cannot open preferences while backup is running", "Backup Running", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            SettingsWindow settingsWindow = new() { Owner = this };
            _ = settingsWindow.ShowDialog();
            QuickConfig.Write();
            RefreshUi();
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
            if (backupThread != null)
            {
                _ = MessageBox.Show("Cannot start another backup while running", "Backup Running", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            BackupConfig currConfig = QuickConfig.AppConfig.BackupConfigs[CurrConfigCB.SelectedIndex];

            if (string.IsNullOrWhiteSpace(currConfig.DestinationPath))
            {
                _ = MessageBox.Show("Destination path has not set", "Backup Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            raisedErrors.Clear();

            // Start backup thread
            backupThread = new(() => RunBackup(currConfig));
            backupThread.Start();
        }

        private void ShowErrorsBnt_Click(object sender, RoutedEventArgs e)
        {
            if (raisedErrors.Count == 0)
            {
                _ = MessageBox.Show("No Errors Logged", "No Errors", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            MultiLabelWindow dialog = new(raisedErrors.ToArray(), "Logged Errors") { Owner = this };
            _ = dialog.ShowDialog();
        }

        private void CurrConfigCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshUi();
        }
        #endregion
    }
}
