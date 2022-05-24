using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SimpleBackup.InterfaceAvalonia
{
    public partial class GettingStartedWindow : Window
    {
        public GettingStartedWindow()
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
    }
}
