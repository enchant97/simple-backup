using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using SimpleBackup.Core;
using SimpleBackup.Core.Configuration;
using SimpleBackup.Core.Configuration.Types;

namespace SimpleBackup.InterfaceGtk.Views
{
    class MainWindow : Window
    {
        #region Fields
        private int currConfigI;
        private readonly Entry configName;
        private readonly Label configLastBackup;
        private readonly SpinButton configVersionsToKeepSpinner;
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
            configName = new();
            configName.TextDeleted += OnConfigNameChange;
            configName.TextInserted += OnConfigNameChange;
            configLastBackup = new();
            Label configVersionsToKeepLabel = new("Version To Keep");
            configVersionsToKeepSpinner = new(0, 100, 1);
            configVersionsToKeepSpinner.ValueChanged += OnVersionsToKeepChange;

            mainBox.PackStart(menuBar, false, false, 0);
            mainBox.PackStart(title, false, false, 14);
            mainBox.PackStart(configName, false, false, 0);
            mainBox.PackStart(configLastBackup, false, false, 0);
            mainBox.PackStart(configVersionsToKeepLabel, false, false, 0);
            mainBox.PackStart(configVersionsToKeepSpinner, false, false, 0);
            Add(mainBox);

            LoadConfigWidgets(QuickConfig.AppConfig.DefaultConfigI);
        }
        private void LoadConfigWidgets(int configIndex)
        {
            currConfigI = configIndex;
            var loadedConfig = QuickConfig.AppConfig.BackupConfigs[configIndex];
            configName.Text = loadedConfig.Name;
            configLastBackup.Text = loadedConfig.LastBackup.ToString();
            configVersionsToKeepSpinner.Value = loadedConfig.VersionsToKeep;
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
            QuickConfig.AppConfig.BackupConfigs[currConfigI].Name = configName.Text;
            QuickConfig.Write();
        }
        private void OnVersionsToKeepChange(object obj, EventArgs args)
        {
            QuickConfig.AppConfig.BackupConfigs[currConfigI].VersionsToKeep = configVersionsToKeepSpinner.ValueAsInt;
            QuickConfig.Write();
        }
        #endregion
    }
}
