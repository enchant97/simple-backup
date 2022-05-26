using Avalonia.Controls;
using Avalonia.Interactivity;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using SimpleBackup.Core.Configuration;
using SimpleBackup.Core.Configuration.Types;

namespace SimpleBackup.InterfaceAvalonia
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }
        private async void OnClickNewConfig(object sender, RoutedEventArgs e)
        {
            var inputDialog = MessageBoxManager.GetMessageBoxInputWindow(
               new MessageBoxInputParams()
               {
                   ContentTitle = "New Config",
                   ContentMessage = "Enter a new config name",
                   WatermarkText = "Name here...",
                   WindowStartupLocation = WindowStartupLocation.CenterOwner,
               }
            );
            var dialogResult = await inputDialog.ShowDialog(this);
            if (dialogResult.Button == "Confirm" && dialogResult.Message != null)
            {
                string textInput = dialogResult.Message.Trim();
                if (textInput.Length > 0)
                {
                    BackupConfig backupConfig = new() { Name = textInput };
                    QuickConfig.AppConfig.BackupConfigs.Add(backupConfig);
                    QuickConfig.Write();
                }
            }
        }
        private void OnClickGeneral(object sender, RoutedEventArgs e)
        {
            SettingsGeneralWindow dialog = new();
            dialog.ShowDialog(this);
        }
        private void OnClickEditConfig(object sender, RoutedEventArgs e)
        {
            SettingsEditConfigWindow dialog = new();
            dialog.ShowDialog(this);
        }
    }
}
