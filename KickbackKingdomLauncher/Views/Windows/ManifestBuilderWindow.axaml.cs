using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using KickbackKingdomLauncher.ViewModels.Windows;

namespace KickbackKingdomLauncher.Views.Windows
{
    public partial class ManifestBuilderWindow : WindowBase
    {
        public ManifestBuilderWindow()
        {
            InitializeComponent();
            DataContext = new ManifestBuilderViewModel();
        }
        private async void BrowseFolder_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is ManifestBuilderViewModel vm)
            {
                var folder = await vm.BrowseFolder(this);
                if (!string.IsNullOrEmpty(folder))
                    vm.SelectedFolder = folder;
            }
        }

    }
}