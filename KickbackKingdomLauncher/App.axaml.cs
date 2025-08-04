using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using KickbackKingdomLauncher.Models.Vault;
using KickbackKingdomLauncher.ViewModels;
using KickbackKingdomLauncher.ViewModels.Dialogs;
using KickbackKingdomLauncher.ViewModels.Windows;
using KickbackKingdomLauncher.Views;
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
            var window = new MainWindow();
            desktop.MainWindow = window;

            Exception? earlyException = null;

            try
            {
                // Try building the DataContext
                var viewModel = new MainViewModel();
                window.DataContext = viewModel;
            }
            catch (Exception ex)
            {
                earlyException = ex;
            }

            // Initialize Vaults
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

            // Set up UI-level error handling
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
                // Defer the dialog until window is fully loaded
                Avalonia.Threading.Dispatcher.UIThread.Post(async () =>
                {
                    await ShowErrorAsync(window, earlyException);
                });
            }
        }
    }


    /// <summary>
    /// Display an error dialog from an exception using the MainWindow as parent.
    /// </summary>
    public static async Task ShowErrorAsync(Window parent, Exception ex)
    {
        var clipboard = parent.Clipboard;
        if (clipboard is null)
            throw new InvalidOperationException("Clipboard not available on this window.");

        var dialog = new ErrorDialog
        {
            DataContext = new ErrorDialogViewModel(
                ex.Message,
                ex.ToString(),
                clipboard
            )
        };

        await dialog.ShowDialog(parent);
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
