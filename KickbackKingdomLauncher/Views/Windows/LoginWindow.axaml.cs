using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using KickbackKingdomLauncher.ViewModels.Windows;
using System;

namespace KickbackKingdomLauncher.Views.Windows;

public partial class LoginWindow : WindowBase
{
    public LoginWindow()
    {
        InitializeComponent();
        this.Opened += (_, _) => emailBox.Focus();
        this.KeyDown += OnKeyDown;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is LoginViewModel vm)
        {
            vm.LoginCommand.Execute().Subscribe();
            e.Handled = true;
        }
    }


    private void OnMinimizeClicked(object? sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void OnToggleMaximizeClicked(object? sender, RoutedEventArgs e) =>
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    private void OnCloseClicked(object? sender, RoutedEventArgs e) => Close();


    private void OnForgotPasswordLinkClicked(object? sender, PointerPressedEventArgs e)
    {
        (DataContext as LoginViewModel)?.OpenLinkCommand.Execute("https://kickback-kingdom.com/forgot-password.php").Subscribe();
    }

    private void OnRegisterLinkClicked(object? sender, PointerPressedEventArgs e)
    {
        (DataContext as LoginViewModel)?.OpenLinkCommand.Execute("https://kickback-kingdom.com/register.php").Subscribe();
    }
}
