using Avalonia.Controls;
using Avalonia.VisualTree;
using Avalonia;
using KickbackKingdomLauncher.ViewModels;
using KickbackKingdomLauncher.Views;
using System;
using System.Threading.Tasks;
using KickbackKingdomLauncher.ViewModels.Dialogs;

namespace KickbackKingdomLauncher.Helpers
{
    public static class ErrorDialogHelper
    {
        public static async Task ShowErrorAsync(Control owner, Exception exception)
        {
            var topLevel = owner.GetVisualRoot() as TopLevel;
            var clipboard = topLevel?.Clipboard;

            var vm = new ErrorDialogViewModel(
                message: exception.Message,
                details: exception.ToString(),
                clipboard: clipboard!
            );

            var dialog = new ErrorDialog
            {
                DataContext = vm
            };

            if (topLevel is Window window)
            {
                await dialog.ShowDialog(window);
            }
            else
            {
                dialog.Show();
            }
        }
    }
}
