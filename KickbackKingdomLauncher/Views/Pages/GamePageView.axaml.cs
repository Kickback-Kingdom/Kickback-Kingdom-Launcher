using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using KickbackKingdomLauncher.Models.Software;
using KickbackKingdomLauncher.ViewModels;
using KickbackKingdomLauncher.ViewModels.Pages;
using KickbackKingdomLauncher.ViewModels.Windows;
using KickbackKingdomLauncher.Views.Windows;
using System.Threading.Tasks;

namespace KickbackKingdomLauncher.Views.Pages
{
    public partial class GamePageView : UserControl
    {
        public GamePageView()
        {
            InitializeComponent();
        }

        public GamePageView(SoftwareEntry software) : this()
        {
            DataContext = new GamePageViewModel(software);
        }


        private async void OnInstallClicked(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not GamePageViewModel vm || vm.Software is null)
                return;

            await ShowInstallerAsync(vm.Software);
        }

        public async Task ShowInstallerAsync(SoftwareEntry software)
        {
            var vm = new InstallerViewModel(software);
            var installerWindow = new InstallerWindow
            {
                DataContext = vm
            };

            var owner = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

            if (owner != null)
                await installerWindow.ShowDialog(owner);
            else
                installerWindow.Show(); // fallback if no owner available
        }
    }
}
