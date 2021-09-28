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
        private readonly ToggleButton configLock;
        private Thread backupThread;
        private readonly Entry configName;
        private readonly Label configLastBackup;
        private readonly SpinButton configVersionsToKeepSpinner;
        private readonly Button includedPathsBnt;
        private readonly Button excludedPathsBnt;
        private readonly Button startBackup;
        private readonly Statusbar statusBar;
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
            config.Append(mConfigDeleteCurrent);
            config.Append(mConfigResetAll);
            help.Append(mAbout);
            menuBar.Append(mFile);
            menuBar.Append(mConfig);
            menuBar.Append(mHelp);

            Label title = new(Constants.AppName + " - GUI MODE");
            configLock = new("Readonly");
            configLock.Clicked += OnConfigLockToggle;
            configName = new();
            configName.TextDeleted += OnConfigNameChange;
            configName.TextInserted += OnConfigNameChange;
            configLastBackup = new();
            Label configVersionsToKeepLabel = new("Version To Keep");
            configVersionsToKeepSpinner = new(0, 100, 1);
            configVersionsToKeepSpinner.ValueChanged += OnVersionsToKeepChange;
            includedPathsBnt = new("Included Paths");
            includedPathsBnt.Clicked += OnIncludeClick;
            excludedPathsBnt = new("Excluded Paths");
            excludedPathsBnt.Clicked += OnExcludeClick;
            startBackup = new("Start");
            startBackup.Clicked += OnStartBackupClicked;
            statusBar = new();

            mainBox.PackStart(menuBar, false, false, 0);
            mainBox.PackStart(title, false, false, 14);
            mainBox.PackStart(configLock, false, false, 0);
            mainBox.PackStart(configName, false, false, 0);
            mainBox.PackStart(configLastBackup, false, false, 0);
            mainBox.PackStart(configVersionsToKeepLabel, false, false, 0);
            mainBox.PackStart(configVersionsToKeepSpinner, false, false, 0);
            mainBox.PackStart(includedPathsBnt, false, false, 0);
            mainBox.PackStart(excludedPathsBnt, false, false, 0);
            mainBox.PackStart(startBackup, false, false, 0);
            mainBox.PackEnd(statusBar, false, false, 0);

            Add(mainBox);

            LoadConfigWidgets(QuickConfig.AppConfig.DefaultConfigI);
            LockConfigWidgets();
        }
        private void LoadConfigWidgets(int configIndex)
        {
            currConfigI = configIndex;
            var loadedConfig = QuickConfig.AppConfig.BackupConfigs[configIndex];
            configName.Text = loadedConfig.Name;
            configLastBackup.Text = loadedConfig.LastBackup.ToString();
            configVersionsToKeepSpinner.Value = loadedConfig.VersionsToKeep;
        }
        private void LockConfigWidgets(bool locked = true)
        {
            locked = !locked;
            configName.Sensitive = locked;
            configVersionsToKeepSpinner.Sensitive = locked;
            includedPathsBnt.Sensitive = locked;
            excludedPathsBnt.Sensitive = locked;
            startBackup.Sensitive = !locked;
        }
        private void RunBackup()
        {
            // TODO move gui & event handlers outside method
            statusBar.Push(0, "Backup Starting");
            BackupConfig currConfig = QuickConfig.AppConfig.BackupConfigs[currConfigI];
            string backupDstPath = Paths.GenerateBackupName(currConfig.DestinationPath);
            int foundCount = 0;
            int copiedCount = 0;

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
                foundCount++;
                statusBar.Push(0, string.Format("Found: {0}", foundCount));
            };
            backupHandler.CopyEvent += (object sender, BackupHandlerEventArgs args) =>
            {
                copiedCount++;
                statusBar.Push(0, string.Format("Copied: {0}/{1}", copiedCount, foundCount));
            };

            backupHandler.Start();

            if (currConfig.VersionsToKeep > 0)
            {
                statusBar.Push(0, "Removing old backups");
                int backupsRemoved = Cleaning.RemovePreviousBackups(
                    currConfig.VersionsToKeep,
                    currConfig.DestinationPath
                );
            }

            statusBar.Push(0, "Backup Finished");
            backupThread = null;
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
        private void OnConfigLockToggle(object obj, EventArgs args)
        {
            bool toggled = configLock.Active;
            if (toggled)
            {
                configLock.Label = "Writeable";
                LockConfigWidgets(false);
            }
            else
            {
                configLock.Label = "Readonly";
                LockConfigWidgets();
            }
        }
        private void OnConfigNameChange(object obj, EventArgs args)
        {
            QuickConfig.AppConfig.BackupConfigs[currConfigI].Name = configName.Text;
            QuickConfig.Write();
        }
        private void OnVersionsToKeepChange(object obj, EventArgs args)
        {
            QuickConfig.AppConfig.BackupConfigs[currConfigI].VersionsToKeep = configVersionsToKeepSpinner.ValueAsInt;
            QuickConfig.Write();
        }
        private void OnIncludeClick(object obj, EventArgs args)
        {
            ListManager dialog = new(
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
            ListManager dialog = new(
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
        private void OnStartBackupClicked(object obj, EventArgs args)
        {
            // TODO show error message instead of returning
            //  make sure no backup is running
            if (backupThread != null) return;

            // make sure the backup config has all the details
            BackupConfig currConfig = QuickConfig.AppConfig.BackupConfigs[currConfigI];

            if (string.IsNullOrWhiteSpace(currConfig.DestinationPath)) return;

            // start backup thread
            backupThread = new(RunBackup);
            backupThread.Start();
        }
        #endregion
    }
}