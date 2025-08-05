using KickbackKingdom.API.Core;
using KickbackKingdom.API.Models;
using KickbackKingdom.API.Services;
using ReactiveUI;
using System;
using System.Net.Http;
using System.Reactive;
using System.Security.Principal;
using System.Threading.Tasks;

namespace KickbackKingdomLauncher.ViewModels.Windows;

public class LoginViewModel : ReactiveObject
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string ErrorMessage { get; set; } = "";

    public ReactiveCommand<Unit, Unit> LoginCommand { get; }
    public ReactiveCommand<string, Unit> OpenLinkCommand { get; }


    public event Action<Account>? OnLoginSuccess;

    public LoginViewModel()
    {
        LoginCommand = ReactiveCommand.CreateFromTask(LoginAsync);
        OpenLinkCommand = ReactiveCommand.Create<string>(OpenLink);

    }
    private void OpenLink(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return;

        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true // crucial: lets OS open the link with default browser
            };

            System.Diagnostics.Process.Start(psi);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Unable to open link: {ex.Message}";
            this.RaisePropertyChanged(nameof(ErrorMessage));
        }
    }


    private async Task LoginAsync()
    {
        ErrorMessage = "";

        var response = await AuthService.LoginAsync(Email, Password);

        if (response.Success)
        {
            SessionManager.Save(response.Data!);
            OnLoginSuccess?.Invoke(response.Data!);
        }
        else
        {
            ErrorMessage = response.Message ?? "Login failed.";
            this.RaisePropertyChanged(nameof(ErrorMessage));
        }
    }

}
