using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using KickbackKingdomLauncher.ViewModels.Pages;
using KickbackKingdomLauncher.ViewModels.Software;
using KickbackKingdomLauncher.ViewModels.Windows;
using System;
using System.Diagnostics;

namespace KickbackKingdomLauncher.Views.Windows
{
    public partial class MainWindow : WindowBase
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainViewModel();
            Debug.WriteLine("MainWindow initialized");
        }


        private void OnMinimizeClicked(object? sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void OnToggleMaximizeClicked(object? sender, RoutedEventArgs e) =>
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        private void OnCloseClicked(object? sender, RoutedEventArgs e) => Close();

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
        private void Upload_Click(object? sender, RoutedEventArgs e)
        {
            var window = new ManifestBuilderWindow();
            window.Show();
        }

    }
}
