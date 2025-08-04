using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace KickbackKingdomLauncher.Views.Windows;

public partial class WindowBase : Window
{
    public WindowBase()
    {
        InitializeComponent();
    }

    protected void OnDragWindow(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }

    private void OnMinimizeClicked(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void OnToggleMaximizeClicked(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void OnCloseClicked(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
