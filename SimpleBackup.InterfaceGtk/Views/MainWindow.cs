using System;
using Gtk;
using SimpleBackup.Core;
using SimpleBackup.Core.Configuration;

namespace SimpleBackup.InterfaceGtk.Views
{
    class MainWindow : Window
    {
        private int currConfigI;
        private readonly Label configName;
        private readonly Label configLastBackup;
        private readonly SpinButton configVersionsToKeepSpinner;
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
            Menu help = new();
            MenuItem mHelp = new("Help") { Submenu = help };
            MenuItem mAbout = new("About");
            mAbout.Activated += OnAbout;

            file.Append(mQuit);
            help.Append(mAbout);
            menuBar.Append(mFile);
            menuBar.Append(mHelp);
            menuBar.Append(file);

            Label title = new(Constants.AppName + " - GUI MODE");
            configName = new();
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
        private void OnVersionsToKeepChange(object obj, EventArgs args)
        {
            QuickConfig.AppConfig.BackupConfigs[currConfigI].VersionsToKeep = configVersionsToKeepSpinner.ValueAsInt;
            QuickConfig.Write();
        }
    }
}
