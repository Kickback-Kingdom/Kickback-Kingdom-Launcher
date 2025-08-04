using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using KickbackKingdomLauncher.ViewModels.Pages;
using KickbackKingdomLauncher.ViewModels.Software;
using KickbackKingdomLauncher.ViewModels.Windows;

namespace KickbackKingdomLauncher.Views.Windows
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnFooterClicked(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                // If already showing Task page, go back to default (or do nothing)
                if (vm.CurrentPage is TaskPageViewModel)
                {
                    // Example fallback: go back to selected game
                    if (vm.SelectedListItem is SoftwareItemEntry entry)
                        vm.CurrentPage = new GamePageViewModel(entry.Software);
                    else if (vm.SelectedListItem is SoftwareGroupHeader header)
                        vm.CurrentPage = new GroupSoftwarePageViewModel(header.Title);
                }
                else
                {
                    vm.CurrentPage = new TaskPageViewModel();
                }
            }
        }

    }
}
