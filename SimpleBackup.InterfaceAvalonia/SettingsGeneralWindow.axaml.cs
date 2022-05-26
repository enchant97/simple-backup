using Avalonia.Controls;
using Avalonia.Interactivity;
using MessageBox.Avalonia;
using MessageBox.Avalonia.BaseWindows.Base;
using MessageBox.Avalonia.Enums;
using SimpleBackup.Core.Configuration;

namespace SimpleBackup.InterfaceAvalonia
{
    public partial class SettingsGeneralWindow : Window
    {
        public SettingsGeneralWindow()
        {
            InitializeComponent();
            LoadFormValues();
        }
        private void LoadFormValues()
        {
            ShowHelpCB.IsChecked = QuickConfig.AppConfig.ShowHelp;
            DefaultConfigCB.Items = QuickConfig.AppConfig.BackupConfigs;
            DefaultConfigCB.SelectedIndex = QuickConfig.AppConfig.DefaultConfigI;
        }
        private async void OnClickResetApp(object sender, RoutedEventArgs e)
        {
            IMsBoxWindow<ButtonResult> prompt = MessageBoxManager.GetMessageBoxStandardWindow(
                "Confirm",
                "This will reset the app config there will be no going back",
                ButtonEnum.OkCancel,
                MessageBox.Avalonia.Enums.Icon.Warning
            );
            ButtonResult promptResult = await prompt.Show();
            if (promptResult == ButtonResult.Ok)
            {
                QuickConfig.Reset();
                LoadFormValues();
            }
        }
        private void OnClickCancel(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void OnClickSave(object sender, RoutedEventArgs e)
        {
            if (ShowHelpCB.IsChecked == true)
                QuickConfig.AppConfig.ShowHelp = true;
            else
                QuickConfig.AppConfig.ShowHelp = false;

            QuickConfig.AppConfig.DefaultConfigI = DefaultConfigCB.SelectedIndex;
            QuickConfig.Write();

            Close();
        }
    }
}
