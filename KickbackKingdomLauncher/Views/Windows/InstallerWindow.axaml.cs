using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using KickbackKingdomLauncher.Helpers;
using KickbackKingdomLauncher.ViewModels.Windows;
using ReactiveUI;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;

namespace KickbackKingdomLauncher.Views;

public partial class InstallerWindow : ReactiveWindow<InstallerViewModel>
{
    public InstallerWindow()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            if (ViewModel is null || StorageProvider is null)
                return;

            ViewModel.PickFolderInteraction
                .RegisterHandler(async interaction =>
                {
                    var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                    {
                        Title = "Choose Install Location",
                        AllowMultiple = false
                    });

                    var folder = folders?.FirstOrDefault();
                    interaction.SetOutput(folder?.Path.LocalPath);
                })
                .DisposeWith(disposables);


            ViewModel?.ShowErrorDialog.RegisterHandler(async interaction =>
            {
                await ErrorDialogHelper.ShowErrorAsync(this, interaction.Input);
                interaction.SetOutput(Unit.Default);
            }).DisposeWith(disposables);
        });
    }
}
