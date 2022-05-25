using Avalonia.Controls;
using Avalonia.Interactivity;
using MessageBox.Avalonia;
using MessageBox.Avalonia.Enums;
using MessageBox.Avalonia.BaseWindows.Base;
using SimpleBackup.Core.Configuration;

namespace SimpleBackup.InterfaceAvalonia
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Initialized += (sender, evt) => OnWindowLoad();
            InitializeComponent();
        }
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
                "Importing a new config will\noverwrite your current one.",
                ButtonEnum.OkCancel
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
        #endregion
    }
}
