using System.Windows;
using SimpleBackup.Resources;

namespace SimpleBackup.InterfaceWpf
{
    /// <summary>
    /// Interaction logic for LicencesWindow.xaml
    /// </summary>
    public partial class LicencesWindow : Window
    {
        public LicencesWindow()
        {
            InitializeComponent();
            TextArea.Text = Resource.ResourceManager.GetString("ThirdPartyLicences");
        }
    }
}
