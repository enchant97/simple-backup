using System.Windows;
using SimpleBackup.Core.Configuration;

namespace SimpleBackup.InterfaceWpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App() : base()
        {
            QuickConfig.Read();
        }
    }
}
