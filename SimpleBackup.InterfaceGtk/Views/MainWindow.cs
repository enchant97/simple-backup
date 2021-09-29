using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using Gtk;
using Gtk.Extensions.Popup;
using SimpleBackup.Core;
using SimpleBackup.Core.Backup;
using SimpleBackup.Core.Configuration;
using SimpleBackup.Core.Configuration.Types;

namespace SimpleBackup.InterfaceGtk.Views
{
    class MainWindow : Window
    {
        #region Fields
        private int currConfigI;
        private Thread backupThread;
        private readonly Label configNameLabel;
        private readonly Label configLastBackup;
        private readonly SpinButton configVersionsToKeepSpinner;
        private readonly Button includedPathsBnt;
        private readonly Button excludedPathsBnt;
        private readonly Label destPathLabel;
        private readonly Button destPathBnt;
        private readonly Button startBackup;
        private readonly ProgressBar progressBar;
        private readonly Statusbar statusBar;
        public int FoundCount { get; private set; }
        public int CopiedCount { get; private set; }
        #endregion
        public MainWindow() : base(Constants.AppName + " - GUI Mode")
        {
            QuickConfig.Read();

            SetDefaultSize(400, 200);
            SetPosition(WindowPosition.Center);
            DeleteEvent += OnQuit;

            VBox mainBox = new(false, 2);

            MenuBar menuBar = new();
            Menu file = new();
            MenuItem mFile = new("File") { Submenu = file };
            MenuItem mQuit = new("Quit");
            mQuit.Activated += OnQuit;
            Menu config = new();
            MenuItem mConfig = new("Config") { Submenu = config };
            MenuItem mConfigNew = new("New");
            mConfigNew.Activated += OnConfigNew;
            MenuItem mConfigLoad = new("Load");
            mConfigLoad.Activated += OnConfigLoad;
            MenuItem mConfigChangeDefault = new("Change Default");
            mConfigChangeDefault.Activated += OnConfigChangeDefault;
            MenuItem mConfigRenameCurrent = new("Rename Current");
            mConfigRenameCurrent.Activated += OnConfigNameChange;
            MenuItem mConfigDeleteCurrent = new("Delete Current");
            mConfigDeleteCurrent.Activated += OnConfigDeleteCurrent;
            MenuItem mConfigResetAll = new("Reset All");
            mConfigResetAll.Activated += OnConfigResetAll;
            Menu help = new();
            MenuItem mHelp = new("Help") { Submenu = help };
            MenuItem mAbout = new("About");
            mAbout.Activated += OnAbout;

            file.Append(mQuit);
            config.Append(mConfigNew);
            config.Append(mConfigLoad);
            config.Append(mConfigChangeDefault);
            config.Append(mConfigRenameCurrent);
            config.Append(new SeparatorMenuItem());
            config.Append(mConfigDeleteCurrent);
            config.Append(mConfigResetAll);
            help.Append(mAbout);
            menuBar.Append(mFile);
            menuBar.Append(mConfig);
            menuBar.Append(mHelp);

            Label title = new(Constants.AppName + " - GUI MODE");
            configNameLabel = new();
            configLastBackup = new();
            Label configVersionsToKeepLabel = new("Version To Keep");
            configVersionsToKeepSpinner = new(0, 100, 1);
            configVersionsToKeepSpinner.ValueChanged += OnVersionsToKeepChange;
            includedPathsBnt = new("Included Paths");
            includedPathsBnt.Clicked += OnIncludeClick;
            excludedPathsBnt = new("Excluded Paths");
            excludedPathsBnt.Clicked += OnExcludeClick;
            destPathLabel = new();
            destPathBnt = new("Change Backup Destination");
            destPathBnt.Clicked += OnChangeDestClick;
            startBackup = new("Start");
            startBackup.Clicked += OnStartBackupClicked;
            progressBar = new();
            statusBar = new();

            mainBox.PackStart(menuBar, false, false, 0);
            mainBox.PackStart(title, false, false, 14);
            mainBox.PackStart(configNameLabel, false, false, 0);
            mainBox.PackStart(configLastBackup, false, false, 0);
            mainBox.PackStart(configVersionsToKeepLabel, false, false, 0);
            mainBox.PackStart(configVersionsToKeepSpinner, false, false, 0);
            mainBox.PackStart(includedPathsBnt, false, false, 0);
            mainBox.PackStart(excludedPathsBnt, false, false, 0);
            mainBox.PackStart(destPathLabel, false, false, 0);
            mainBox.PackStart(destPathBnt, false, false, 0);
            mainBox.PackStart(startBackup, false, false, 0);
            mainBox.PackEnd(progressBar, false, false, 0);
            mainBox.PackEnd(statusBar, false, false, 0);

            Add(mainBox);

            LoadConfigWidgets(QuickConfig.AppConfig.DefaultConfigI);
        }
        private void LoadConfigWidgets(int configIndex)
        {
            currConfigI = configIndex;
            var loadedConfig = QuickConfig.AppConfig.BackupConfigs[configIndex];
            configNameLabel.Text = string.Format("Config Name: {0}", loadedConfig.Name);
            configLastBackup.Text = string.Format("Last Known Backup: {0}", loadedConfig.LastBackup.ToString());
            configVersionsToKeepSpinner.Value = loadedConfig.VersionsToKeep;
            destPathLabel.Text = loadedConfig.DestinationPath;
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
            statusBar.Push(0, "Backup Finished");
            backupThread = null;
        }
        private void RunBackup()
        {
            // TODO handle finished event in BackupHandler
            // TODO handle error events in BackupHandler
            Application.Invoke(delegate { HandleBackupStart(); });
            BackupConfig currConfig = QuickConfig.AppConfig.BackupConfigs[currConfigI];
            string backupDstPath = Paths.GenerateBackupName(currConfig.DestinationPath);

            BackupHandler backupHandler = new(
                backupDstPath,
                currConfig.IncludedPaths,
                currConfig.ExcludedPaths,
                QuickConfig.AppConfig.ExcludedFilenames,
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

            backupHandler.Start();

            if (currConfig.VersionsToKeep > 0)
            {
                Application.Invoke(delegate { HandleBackupClean(); });
                int backupsRemoved = Cleaning.RemovePreviousBackups(
                    currConfig.VersionsToKeep,
                    currConfig.DestinationPath
                );
            }
            Application.Invoke(delegate { HandleBackupFinished(); });
        }
        #region Events
        private void OnQuit(object obj, EventArgs args)
        {
            Application.Quit();
        }
        private void OnAbout(object obj, EventArgs args)
        {
            AboutDialog aboutDialog = new();
            aboutDialog.ProgramName = Constants.AppName;
            aboutDialog.Copyright = "GPL-3.0 (c) Leo Spratt";
            aboutDialog.Comments = "Welcome to Simple Backup, here is some help to get you started:\n\n" +
                                   "- You can do full-backups\n" +
                                   "- You can include and exclude paths to backup\n" +
                                   "- You can set versions of backups to keep\n" +
                                   "- You can keep different configurations for backups\n" +
                                   "- Once everything is configured you can backup with one command\n" +
                                   "- All configs are stored in a xml file\n";
            aboutDialog.Run();
            aboutDialog.Destroy();
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
                    QuickConfig.AppConfig.BackupConfigs[QuickConfig.AppConfig.BackupConfigs.Length - 1] = new BackupConfig() { Name = dialog.Input };
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

            // start backup thread
            backupThread = new(RunBackup);
            backupThread.Start();
        }
        #endregion
    }
}
