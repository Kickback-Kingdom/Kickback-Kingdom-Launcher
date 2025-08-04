using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using KickbackKingdomLauncher.ViewModels.Dialogs;

namespace KickbackKingdomLauncher.Views;

public partial class ErrorDialog : ReactiveWindow<ErrorDialogViewModel>
{
    public ErrorDialog()
    {
        InitializeComponent();
    }


}
