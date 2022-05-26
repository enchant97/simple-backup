using Avalonia.Controls;
using SimpleBackup.Resources;

namespace SimpleBackup.InterfaceAvalonia
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            LicenseTB.Text = Resource.ResourceManager.GetString("ThirdPartyLicences");
        }
    }
}
