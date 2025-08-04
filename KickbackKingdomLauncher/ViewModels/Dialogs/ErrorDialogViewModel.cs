using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;

namespace KickbackKingdomLauncher.ViewModels.Dialogs;

public class ErrorDialogViewModel : ReactiveObject
{
    private string _message;
    private string _details;
    private bool _showDetails;

    private readonly IClipboard _clipboard;
    public ErrorDialogViewModel(Exception ex, IClipboard clipboard)
    : this(ex.Message, ex.ToString(), clipboard)
    {
    }

    public ErrorDialogViewModel(string message, string details, IClipboard clipboard)
    {
        _message = message;
        _details = details;
        _clipboard = clipboard;

        CopyToClipboardCommand = ReactiveCommand.CreateFromTask(CopyToClipboardAsync);
        CloseCommand = ReactiveCommand.Create(() => { });
        ToggleDetailsCommand = ReactiveCommand.Create(() =>
        {
            ShowDetails = !ShowDetails;
            return Unit.Default;
        });

    }

    public string Message
    {
        get => _message;
        set => this.RaiseAndSetIfChanged(ref _message, value);
    }

    public string Details
    {
        get => _details;
        set => this.RaiseAndSetIfChanged(ref _details, value);
    }

    public bool ShowDetails
    {
        get => _showDetails;
        set => this.RaiseAndSetIfChanged(ref _showDetails, value);
    }

    public ReactiveCommand<Unit, Unit> CopyToClipboardCommand { get; }
    public ReactiveCommand<Unit, Unit> CloseCommand { get; }
    public ReactiveCommand<Unit, Unit> ToggleDetailsCommand { get; }

    private async Task CopyToClipboardAsync()
    {
        if (_clipboard != null)
        {
            await _clipboard.SetTextAsync($"{Message}\n\n{Details}");
        }
    }
}
