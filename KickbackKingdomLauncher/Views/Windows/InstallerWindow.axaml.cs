using Avalonia.Controls;
using Avalonia.Platform.Storage;
using KickbackKingdomLauncher.Helpers;
using KickbackKingdomLauncher.ViewModels.Windows;
using System.Linq;
using System.Reactive;
using System.Diagnostics;

namespace KickbackKingdomLauncher.Views.Windows;

public partial class InstallerWindow : WindowBase
{
    public InstallerWindow()
    {
        InitializeComponent();

        this.Opened += (_, _) =>
        {
            if (DataContext is not InstallerViewModel vm || StorageProvider is null)
                return;

            Debug.WriteLine("InstallerWindow opened, binding interactions...");

            vm.PickFolderInteraction.RegisterHandler(async interaction =>
            {
                var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = "Choose Install Location",
                    AllowMultiple = false
                });

                var folder = folders?.FirstOrDefault();
                interaction.SetOutput(folder?.Path.LocalPath);
            });

            vm.ShowErrorDialog.RegisterHandler(async interaction =>
            {
                await ErrorDialogHelper.ShowErrorAsync(this, interaction.Input);
                interaction.SetOutput(Unit.Default);
            });
        };
    }
}
