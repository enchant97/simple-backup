using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
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
#if DEBUG
            this.AttachDevTools();
#endif
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
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
    }
}
