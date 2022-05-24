using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SimpleBackup.Resources;

namespace SimpleBackup.InterfaceAvalonia
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            var licenseTB = this.FindControl<TextBox>("LicenseTB");
            licenseTB.Text = Resource.ResourceManager.GetString("ThirdPartyLicences");

        }
    }
}
