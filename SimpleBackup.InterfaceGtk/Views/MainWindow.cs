using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Gtk;
using Gtk.Extensions.Popup;
using SimpleBackup.Core;
using SimpleBackup.Core.Backup;
using SimpleBackup.Core.Configuration;
using SimpleBackup.Core.Configuration.Types;
using SimpleBackup.Core.Paths;
using UI = Gtk.Builder.ObjectAttribute;

namespace SimpleBackup.InterfaceGtk.Views
{
    class MainWindow : Window
    {
        #region Fields
        private int currConfigI;
        private Thread backupThread;
        [UI] protected readonly Label configNameLabel;
        [UI] protected readonly Label configLastBackup;
        [UI] protected readonly SpinButton configVersionsToKeepSpinner;
        [UI] protected readonly Button includedPathsBnt;
        [UI] protected readonly Button excludedPathsBnt;
        [UI] protected readonly Label destPathLabel;
        [UI] protected readonly Button destPathBnt;
        [UI] protected readonly Button changeBackupTypeBnt;
        [UI] protected readonly Button startBackup;
        [UI] protected readonly Button showErrorsBnt;
        [UI] protected readonly ProgressBar progressBar;
        [UI] protected readonly Statusbar statusBar;
        private readonly List<string> raisedErrors = new();
        public int FoundCount { get; private set; }
        public int CopiedCount { get; private set; }
        #endregion
        public MainWindow() : this(new Builder("SimpleBackup.InterfaceGtk.Assets.MainWindow.glade")) { }
        private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);
            Icon = Program.SharedAppIcon;
            SetPosition(WindowPosition.Center);
            DeleteEvent += OnQuit;

            QuickConfig.Read();
            LoadConfigWidgets(QuickConfig.AppConfig.DefaultConfigI);
        }
        private void LoadConfigWidgets(int configIndex)
        {
            currConfigI = configIndex;
            var loadedConfig = QuickConfig.AppConfig.BackupConfigs[configIndex];
            configNameLabel.Text = loadedConfig.Name;
            configLastBackup.Text = loadedConfig.LastBackup.ToString();
            configVersionsToKeepSpinner.Value = loadedConfig.VersionsToKeep;
            destPathLabel.Text = loadedConfig.DestinationPath;
            string backupTypeName = Enum.GetName(loadedConfig.BackupType);
            changeBackupTypeBnt.Label = string.Format("Change Backup Type ({0})", backupTypeName);
        }
        private void LockWidgets(bool locked = true)
        {
            locked = !locked;
            configVersionsToKeepSpinner.Sensitive = locked;
            includedPathsBnt.Sensitive = locked;
            excludedPathsBnt.Sensitive = locked;
            destPathBnt.Sensitive = locked;
            startBackup.Sensitive = locked;
        }
        private void HandleBackupStart()
        {
            LockWidgets();
            statusBar.Push(0, "Backup Starting");
            FoundCount = 0;
            CopiedCount = 0;
        }
        private void HandleBackupDiscovery()
        {
            FoundCount++;
            progressBar.Pulse();
            statusBar.Push(0, string.Format("Found: {0}", FoundCount));
        }
        private void HandleBackupCopied()
        {
            CopiedCount++;
            progressBar.Pulse();
            statusBar.Push(0, string.Format("Copied: {0}/{1}", CopiedCount, FoundCount));
        }
        private void HandleBackupClean()
        {
            progressBar.Pulse();
            statusBar.Push(0, "Removing old backups");
        }
        private void HandleBackupFinished()
        {
            // write backup timestamp
            QuickConfig.AppConfig.BackupConfigs[currConfigI].LastBackup = DateTime.UtcNow;
            QuickConfig.Write();
            LoadConfigWidgets(currConfigI);

            LockWidgets(false);
            progressBar.Fraction = 0;
            if (raisedErrors.Count > 0)
                statusBar.Push(0, "Backup Finished (With Errors)");
            else
                statusBar.Push(0, "Backup Finished");
            backupThread = null;
        }
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
        private void HandleBackupException(BackupHandlerErrorEventArgs args)
        {
            raisedErrors.Add(ExceptionToString(args));
            showErrorsBnt.Label = string.Format("Show Errors ({0})", raisedErrors.Count);
        }
        private void RunBackup()
        {
            Application.Invoke(delegate { HandleBackupStart(); });
            BackupConfig currConfig = QuickConfig.AppConfig.BackupConfigs[currConfigI];
            string backupDstPath = Generation.GenerateBackupName(
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
                Application.Invoke(delegate { HandleBackupDiscovery(); });
            };
            backupHandler.CopyEvent += (object sender, BackupHandlerEventArgs args) =>
            {
                Application.Invoke(delegate { HandleBackupCopied(); });
            };
            backupHandler.ExceptionDiscoveringEvent += (object sender, BackupHandlerErrorEventArgs args) =>
            {
                Application.Invoke(delegate { HandleBackupException(args); });
            };
            backupHandler.ExceptionCopyEvent += (object sender, BackupHandlerErrorEventArgs args) =>
            {
                Application.Invoke(delegate { HandleBackupException(args); });
            };
            backupHandler.FinishedEvent += (object sender, EventArgs args) =>
            {
                if (currConfig.VersionsToKeep > 0)
                {
                    Application.Invoke(delegate { HandleBackupClean(); });
                    int backupsRemoved = Cleaning.RemovePreviousBackups(
                        currConfig.VersionsToKeep,
                        currConfig.DestinationPath
                    );
                }
                Application.Invoke(delegate { HandleBackupFinished(); });
            };
            backupHandler.Start();
        }
        #region Events
        private void OnQuit(object obj, EventArgs args)
        {
            Application.Quit();
        }
        private void OnHelp(object obj, EventArgs args)
        {
            HelpWindow helpWindow = new() { AttachedTo = this };
            helpWindow.Run();
            helpWindow.Destroy();
        }
        private void OnAbout(object obj, EventArgs args)
        {
            AboutWindow aboutWindow = new() { AttachedTo = this };
            aboutWindow.Run();
            aboutWindow.Destroy();
        }
        private void OnConfigNew(object obj, EventArgs args)
        {
            AskTextInput dialog = new(this, "Create Config", "Enter A Name For The New Config");
            var response = dialog.Run();
            if (response == ((int)ResponseType.Ok))
            {
                if (!string.IsNullOrEmpty(dialog.Input))
                {
                    Array.Resize(ref QuickConfig.AppConfig.BackupConfigs, QuickConfig.AppConfig.BackupConfigs.Length + 1);
                    QuickConfig.AppConfig.BackupConfigs[^1] = new BackupConfig() { Name = dialog.Input };
                    QuickConfig.Write();
                }
            }
            dialog.Destroy();
        }
        private void OnConfigLoad(object obj, EventArgs args)
        {
            string[] configNames = QuickConfig.AppConfig.BackupConfigs.Select(config => config.Name).ToArray();
            AskChoice dialog = new(this, "Load Config", "Select A Config To Load", configNames);
            var response = dialog.Run();
            if (response == ((int)ResponseType.Ok))
            {
                if (dialog.SelectedI != -1)
                {
                    LoadConfigWidgets(dialog.SelectedI);
                }
            }
            dialog.Destroy();
        }
        private void OnConfigChangeDefault(object obj, EventArgs args)
        {
            string[] configNames = QuickConfig.AppConfig.BackupConfigs.Select(config => config.Name).ToArray();
            AskChoice dialog = new(this, "Change Default Config", "Select A Config To Be Default", configNames);
            var response = dialog.Run();
            if (response == ((int)ResponseType.Ok))
            {
                if (dialog.SelectedI != -1)
                {
                    QuickConfig.AppConfig.DefaultConfigI = dialog.SelectedI;
                    QuickConfig.Write();
                }
            }
            dialog.Destroy();
        }
        private void OnConfigDeleteCurrent(object obj, EventArgs args)
        {
            if (QuickConfig.AppConfig.BackupConfigs.Length <= 1)
                QuickConfig.AppConfig.BackupConfigs = new[] { new BackupConfig() };
            else
            {
                List<BackupConfig> configs = QuickConfig.AppConfig.BackupConfigs.ToList();
                configs.RemoveAt(currConfigI);
                QuickConfig.AppConfig.BackupConfigs = configs.ToArray();
                // reset default config as it's now out of range
                if (QuickConfig.AppConfig.DefaultConfigI >= QuickConfig.AppConfig.BackupConfigs.Length ||
                        QuickConfig.AppConfig.DefaultConfigI > currConfigI)
                    QuickConfig.AppConfig.DefaultConfigI = 0;
            }
            QuickConfig.Write();
            LoadConfigWidgets(QuickConfig.AppConfig.DefaultConfigI);
        }
        private void OnConfigResetAll(object obj, EventArgs args)
        {
            QuickConfig.Reset();
            LoadConfigWidgets(QuickConfig.AppConfig.DefaultConfigI);
        }
        private void OnConfigNameChange(object obj, EventArgs args)
        {
            AskTextInput dialog = new(this, "Rename Config", "Enter A New Name For The New Config");
            var response = dialog.Run();
            if (response == ((int)ResponseType.Ok))
            {
                if (!string.IsNullOrEmpty(dialog.Input))
                {
                    QuickConfig.AppConfig.BackupConfigs[currConfigI].Name = dialog.Input;
                    QuickConfig.Write();
                    LoadConfigWidgets(currConfigI);
                }
            }
            dialog.Destroy();
        }
        private void OnVersionsToKeepChange(object obj, EventArgs args)
        {
            QuickConfig.AppConfig.BackupConfigs[currConfigI].VersionsToKeep = configVersionsToKeepSpinner.ValueAsInt;
            QuickConfig.Write();
        }
        private void OnIncludeClick(object obj, EventArgs args)
        {
            DirectoryListManager dialog = new(
                this,
                "Included Paths",
                "Modify Or View The Included Paths",
                QuickConfig.AppConfig.BackupConfigs[currConfigI].IncludedPaths
            );
            var response = dialog.Run();
            if (response == ((int)ResponseType.Ok))
            {
                QuickConfig.AppConfig.BackupConfigs[currConfigI].IncludedPaths = dialog.Choices;
                QuickConfig.Write();
            }
            dialog.Destroy();
        }
        private void OnExcludeClick(object obj, EventArgs args)
        {
            DirectoryListManager dialog = new(
                this,
                "Excluded Paths",
                "Modify Or View The Excluded Paths",
                QuickConfig.AppConfig.BackupConfigs[currConfigI].ExcludedPaths
            );
            var response = dialog.Run();
            if (response == ((int)ResponseType.Ok))
            {
                QuickConfig.AppConfig.BackupConfigs[currConfigI].ExcludedPaths = dialog.Choices;
                QuickConfig.Write();
            }
            dialog.Destroy();
        }
        private void OnChangeDestClick(object obj, EventArgs args)
        {
            FileChooserDialog dialog = new(
                "Choice Backup Destination",
                this,
                FileChooserAction.SelectFolder,
                "Cancel", ResponseType.Cancel,
                "Select", ResponseType.Ok
            );
            var response = dialog.Run();
            if (response == ((int)ResponseType.Ok))
            {
                QuickConfig.AppConfig.BackupConfigs[currConfigI].DestinationPath = dialog.Filename;
                QuickConfig.Write();
                LoadConfigWidgets(currConfigI);
            }
            dialog.Destroy();
        }
        private void OnChangeBackupTypeClicked(object obj, EventArgs args)
        {
            string[] choices = Enum.GetNames<Constants.BackupType>();
            AskChoice dialog = new(this, "Choose Backup Type", "Select A Backup Type", choices);
            var response = dialog.Run();
            if (response == ((int)ResponseType.Ok))
            {
                if (dialog.SelectedI != -1)
                {
                    var newBackupTypeValue = Enum.Parse<Constants.BackupType>(choices[dialog.SelectedI]);
                    QuickConfig.AppConfig.BackupConfigs[currConfigI].BackupType = newBackupTypeValue;
                    QuickConfig.Write();
                    LoadConfigWidgets(currConfigI);
                }
            }
            dialog.Destroy();
        }
        private void OnStartBackupClicked(object obj, EventArgs args)
        {
            // TODO show error message instead of returning
            //  make sure no backup is running
            if (backupThread != null) return;

            // make sure the backup config has all the details
            BackupConfig currConfig = QuickConfig.AppConfig.BackupConfigs[currConfigI];

            if (string.IsNullOrWhiteSpace(currConfig.DestinationPath))
            {
                Alerts.ShowError(this, "Destination path has not been set!");
                return;
            }

            raisedErrors.Clear();

            // start backup thread
            backupThread = new(RunBackup);
            backupThread.Start();
        }
        private void OnShowErrorsClicked(object obj, EventArgs args)
        {
            if (raisedErrors.Count == 0)
            {
                Alerts.ShowInfo(this, "No Errors Logged");
                return;
            }
            ShowMessages dialog = new(this, "Logged Errors", "List of logged errors", raisedErrors.ToArray());
            dialog.Run();
            dialog.Destroy();
        }
        #endregion
    }
}
