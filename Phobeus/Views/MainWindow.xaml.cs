using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Phobeus.ViewModels;

namespace Phobeus.Views
{
    public class MainWindow : Window
    {
        public MainWindow()
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
