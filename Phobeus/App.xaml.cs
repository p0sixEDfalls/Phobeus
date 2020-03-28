using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Phobeus.ViewModels;
using Phobeus.Views;

namespace Phobeus
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                MainWindowViewModel mainWindowViewModel = new MainWindowViewModel();
                desktop.MainWindow = new MainWindow
                {
                    DataContext = mainWindowViewModel,
                };

                desktop.MainWindow.KeyDown += mainWindowViewModel.ProcessKeyInput;

                //desktop.MainWindow.Find<TextBox>("InputTextCurrency").KeyDown += mainWindowViewModel.ProcessKeyInputCurrency;
                desktop.MainWindow.Find<TabControl>("Tabs").SelectionChanged += mainWindowViewModel.ProcessKeyInputCurrency;
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
