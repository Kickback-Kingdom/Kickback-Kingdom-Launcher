using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace KickbackKingdomLauncher.Views.Controls;

public partial class TitleBar : UserControl
{

    public TitleBar()
    {
        InitializeComponent();
    }

    private Window? GetHostWindow() => this.VisualRoot as Window;

    private void OnDragWindow(object? sender, PointerPressedEventArgs e)
    {
        GetHostWindow()?.BeginMoveDrag(e);
    }

    private void OnMinimizeClicked(object? sender, RoutedEventArgs e)
    {
        var window = GetHostWindow();
        if (window != null)
            window.WindowState = WindowState.Minimized;
    }

    private void OnToggleMaximizeClicked(object? sender, RoutedEventArgs e)
    {
        var window = GetHostWindow();
        if (window != null)
        {
            window.WindowState = window.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }
    }

    private void OnCloseClicked(object? sender, RoutedEventArgs e)
    {
        GetHostWindow()?.Close();
    }
}
