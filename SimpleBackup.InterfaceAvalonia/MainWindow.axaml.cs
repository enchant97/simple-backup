using Avalonia.Controls;
using Avalonia.Interactivity;
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
