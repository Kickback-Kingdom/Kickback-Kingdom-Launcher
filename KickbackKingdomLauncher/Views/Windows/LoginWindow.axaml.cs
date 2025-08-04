using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using KickbackKingdomLauncher.ViewModels.Windows;

namespace KickbackKingdomLauncher.Views.Windows;

public partial class LoginWindow : WindowBase
{
    public LoginWindow()
    {
        InitializeComponent();
    }

    private void OnMinimizeClicked(object? sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void OnToggleMaximizeClicked(object? sender, RoutedEventArgs e) =>
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    private void OnCloseClicked(object? sender, RoutedEventArgs e) => Close();


    private void OnForgotPasswordLinkClicked(object? sender, PointerPressedEventArgs e)
    {
        (DataContext as LoginViewModel)?.OpenLinkCommand.Execute("https://kickback-kingdom.com/forgot");
    }

    private void OnRegisterLinkClicked(object? sender, PointerPressedEventArgs e)
    {
        (DataContext as LoginViewModel)?.OpenLinkCommand.Execute("https://kickback-kingdom.com/register");
    }
}
