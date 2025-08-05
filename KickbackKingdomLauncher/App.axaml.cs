using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using KickbackKingdom.API.Core;
using KickbackKingdomLauncher.Models.Secrets;
using KickbackKingdomLauncher.Models.Vault;
using KickbackKingdomLauncher.ViewModels;
using KickbackKingdomLauncher.ViewModels.Dialogs;
using KickbackKingdomLauncher.ViewModels.Windows;
using KickbackKingdomLauncher.Views;
using KickbackKingdomLauncher.Views.Windows;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace KickbackKingdomLauncher;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Provide a minimal placeholder window (invisible)
            var placeholderWindow = new LoginWindow();

            desktop.MainWindow = placeholderWindow;
            placeholderWindow.Show();


            // Global UI thread exception handler
            SynchronizationContext.SetSynchronizationContext(new UIExceptionCatchingSyncContext());

            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            {
                var exception = e.ExceptionObject as Exception ?? new Exception("Unknown unhandled exception.");
                _ = ShowErrorFromExceptionAsync(exception);
            };

            TaskScheduler.UnobservedTaskException += (_, e) =>
            {
                e.SetObserved();
                _ = ShowErrorFromExceptionAsync(e.Exception);
            };

            // Launch the startup logic async after UI is ready
            Avalonia.Threading.Dispatcher.UIThread.Post(async () =>
            {
                try
                {
                    await SafeStartupAsync(desktop);
                }
                catch (Exception ex)
                {
                    await ShowErrorFromExceptionAsync(ex);
                }
            });
        }

        base.OnFrameworkInitializationCompleted();
    }
    private async Task SafeStartupAsync(IClassicDesktopStyleApplicationLifetime desktop)
    {
        APIClient.ServiceKey = SecretsManager.Secrets.ServiceKey;
        VaultManager.Instance.LoadVaults();

        if (!VaultManager.Instance.HasVaults)
        {
            VaultManager.Instance.AddVault(new VaultInfo
            {
                Name = "Main Vault",
                Path = VaultManager.GetDefaultVaultPath()
            }, setAsDefault: true);
        }

        SessionManager.Load();

        if (SessionManager.IsLoggedIn())
        {
            var mainWindow = new MainWindow();

            mainWindow.DataContext = new MainViewModel();
            mainWindow.Show();       // Show the main window first

            if (desktop.MainWindow is LoginWindow loginWindow)
            {
                loginWindow.Close(); // Now safely close the login window
            }

            desktop.MainWindow = mainWindow; // Set as current main window after it's visible

            mainWindow.Activate();
            mainWindow.Focus();
        }
        else if (desktop.MainWindow is LoginWindow loginWindow)
        {
            var loginVM = new LoginViewModel();
            loginWindow.DataContext = loginVM;

            loginVM.OnLoginSuccess += account =>
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    try
                    {
                        SessionManager.Save(account);
                        var mainWindow = new MainWindow();
                        desktop.MainWindow = mainWindow;
                        mainWindow.Show();
                        loginWindow.Close();
                        mainWindow.Activate();
                        mainWindow.Focus();
                    }
                    catch (Exception ex)
                    {
                        _ = ShowErrorFromExceptionAsync(ex);
                    }
                });
            };
        }
    }




    /// <summary>
    /// Display an error dialog from an exception using the MainWindow as parent.
    /// </summary>
    public static async Task ShowErrorAsync(Window? parent, Exception ex)
    {
        var clipboard = parent?.Clipboard ?? TopLevel.GetTopLevel(parent)?.Clipboard;

        if (clipboard is null)
        {
            Console.Error.WriteLine("Clipboard not available.");
            return;
        }

        var dialog = new ErrorDialog
        {
            DataContext = new ErrorDialogViewModel(
                ex.Message,
                ex.ToString(),
                clipboard
            )
        };

        if (parent is { IsVisible: true })
            await dialog.ShowDialog(parent);
        else
            dialog.Show();
    }


    /// <summary>
    /// Helper for global exception hooks to get the top-level window and show the error.
    /// </summary>
    public static async Task ShowErrorFromExceptionAsync(Exception ex)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
            desktop.MainWindow is Window mainWindow)
        {
            await ShowErrorAsync(mainWindow, ex);
        }
        else
        {
            Console.Error.WriteLine("Unhandled exception: " + ex);
        }
    }
}

/// <summary>
/// Captures exceptions posted to the UI thread and routes them to the error dialog.
/// </summary>
public class UIExceptionCatchingSyncContext : SynchronizationContext
{
    public override void Post(SendOrPostCallback d, object? state)
    {
        try
        {
            base.Post(d, state);
        }
        catch (Exception ex)
        {
            _ = App.ShowErrorFromExceptionAsync(ex).ConfigureAwait(false);
        }
    }

    public override void Send(SendOrPostCallback d, object? state)
    {
        try
        {
            base.Send(d, state);
        }
        catch (Exception ex)
        {
            _ = App.ShowErrorFromExceptionAsync(ex).ConfigureAwait(false);
        }
    }
}
