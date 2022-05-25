using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MessageBox.Avalonia;
using MessageBox.Avalonia.BaseWindows.Base;
using MessageBox.Avalonia.Enums;
using SimpleBackup.Core.Configuration;

namespace SimpleBackup.InterfaceAvalonia
{
    public partial class SettingsGeneralWindow : Window
    {
        private readonly CheckBox showHelpCB;
        private readonly ComboBox defaultConfigCB;
        public SettingsGeneralWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            defaultConfigCB = this.FindControl<ComboBox>("DefaultConfigCB");
            showHelpCB = this.FindControl<CheckBox>("ShowHelpCB");

            LoadFormValues();
        }
        private void LoadFormValues()
        {
            showHelpCB.IsChecked = QuickConfig.AppConfig.ShowHelp;
            defaultConfigCB.Items = QuickConfig.AppConfig.BackupConfigs;
            defaultConfigCB.SelectedIndex = QuickConfig.AppConfig.DefaultConfigI;
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        private async void OnClickResetApp(object sender, RoutedEventArgs e)
        {
            IMsBoxWindow<ButtonResult> prompt = MessageBoxManager.GetMessageBoxStandardWindow(
                "Confirm",
                "This will reset the app config\nthere will be no going back",
                ButtonEnum.OkCancel
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
            if (showHelpCB.IsChecked == true)
                QuickConfig.AppConfig.ShowHelp = true;
            else
                QuickConfig.AppConfig.ShowHelp = false;

            QuickConfig.AppConfig.DefaultConfigI = defaultConfigCB.SelectedIndex;
            QuickConfig.Write();

            Close();
        }
    }
}
