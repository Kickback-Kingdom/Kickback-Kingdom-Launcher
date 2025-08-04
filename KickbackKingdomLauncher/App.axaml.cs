using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using KickbackKingdom.API.Core;
using KickbackKingdomLauncher.Models.Vault;
using KickbackKingdomLauncher.ViewModels;
using KickbackKingdomLauncher.ViewModels.Dialogs;
using KickbackKingdomLauncher.ViewModels.Windows;
using KickbackKingdomLauncher.Views;
using KickbackKingdomLauncher.Views.Windows;
using System;
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
            Exception? earlyException = null;

            try
            {
                // Always load vaults early
                VaultManager.Instance.LoadVaults();

                if (!VaultManager.Instance.HasVaults)
                {
                    string path = VaultManager.GetDefaultVaultPath();

                    var vault = new VaultInfo
                    {
                        Name = "Main Vault",
                        Path = path
                    };

                    VaultManager.Instance.AddVault(vault, setAsDefault: true);
                }

                // Load session (from KickbackKingdom.API)
                SessionManager.Load();

                if (SessionManager.IsLoggedIn())
                {
                    // User is logged in — go straight to MainWindow
                    var mainWindow = new MainWindow();

                    desktop.MainWindow = mainWindow;
                    mainWindow.Show();
                }
                else
                {
                    // User is not logged in — show LoginWindow first
                    var loginWindow = new LoginWindow();
                    var loginVM = new LoginViewModel();

                    loginWindow.DataContext = loginVM;

                    loginVM.OnLoginSuccess += account =>
                    {
                        SessionManager.Save(account);

                        var mainWindow = new MainWindow();

                        loginWindow.Close();
                        desktop.MainWindow = mainWindow;
                        mainWindow.Show();
                    };

                    loginWindow.Show();
                }
            }
            catch (Exception ex)
            {
                earlyException = ex;
            }

            // Set up global error handlers
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

            base.OnFrameworkInitializationCompleted();

            if (earlyException != null)
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(async () =>
                {
                    if (desktop.MainWindow?.IsVisible == true)
                        await ShowErrorAsync(desktop.MainWindow, earlyException);
                    else
                        await ShowErrorAsync(null, earlyException);
                });
            }
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
