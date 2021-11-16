using SimpleBackup.Core.Configuration;
using System.Windows;

namespace SimpleBackup.InterfaceWpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App() :base()
        {
            QuickConfig.Read();
        }
    }
}
