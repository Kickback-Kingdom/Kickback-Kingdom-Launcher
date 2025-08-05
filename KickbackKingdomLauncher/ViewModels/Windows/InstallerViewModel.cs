using KickbackKingdomLauncher.Helpers;
using KickbackKingdomLauncher.Models.Software;
using KickbackKingdomLauncher.Models.Tasks;
using KickbackKingdomLauncher.Models.Vault;
using KickbackKingdomLauncher.Services;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace KickbackKingdomLauncher.ViewModels.Windows;

public class InstallerViewModel : ReactiveObject
{
    public SoftwareEntry Software { get; set; }

    private string _installPath = "";
    private long _availableSpace;
    public ObservableCollection<VaultInfo> Vaults { get; }
    private VaultInfo _selectedVault;
    public Interaction<Unit, Unit> RequestCloseWindow { get; } = new();

    private long _requiredSpace = 0L;
    public long RequiredSpace
    {
        get => _requiredSpace;
        private set
        {
            this.RaiseAndSetIfChanged(ref _requiredSpace, value);
            this.RaisePropertyChanged(nameof(HasEnoughSpace));
            this.RaisePropertyChanged(nameof(SpaceRequiredFormatted));
            this.RaisePropertyChanged(nameof(SpaceLeftAfterInstallFormatted));
        }
    }

    public VaultInfo SelectedVault
    {
        get => _selectedVault;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedVault, value);
            UpdateInstallPathFromVault();
        }
    }
    private void UpdateInstallPathFromVault()
    {
        if (SelectedVault != null)
        {
            // Install into a subdirectory of the vault, based on game name or ID
            InstallPath = Path.Combine(SelectedVault.Path, Software.Title); // customize this if needed
        }
        else
        {
            InstallPath = "";
        }
    }
    public void RefreshVaults()
    {
        Vaults.Clear();
        foreach (var v in VaultManager.Instance.ValidVaults)
            Vaults.Add(v);
    }

    public InstallerViewModel(SoftwareEntry software)
    {
        Software = software;
        RequiredSpace = software.Software.RequiredSpace;
        Vaults = new ObservableCollection<VaultInfo>();
        RefreshVaults();

        SelectedVault = VaultManager.Instance.DefaultVault ?? Vaults.FirstOrDefault();

        TriggerPickFolderCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var path = await PickFolderInteraction.Handle(Unit.Default);
            if (!string.IsNullOrWhiteSpace(path))
                InstallPath = path;
        }).WithGlobalErrorHandling();

        InstallCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            try
            {
                if (string.IsNullOrWhiteSpace(InstallPath) || !HasEnoughSpace)
                {
                    await ShowErrorDialog.Handle(new Exception("Invalid install path or not enough space."));
                    return;
                }

                Software.VaultId = SelectedVault.Id;

                var installer = new SoftwareInstaller();
                installer.InstallSoftwareAsync(Software);

                await RequestCloseWindow.Handle(Unit.Default);
            }
            catch (Exception ex)
            {
                await ShowErrorDialog.Handle(ex);
            }
        }).WithGlobalErrorHandling();

        CancelCommand = ReactiveCommand.Create(() =>
        {
            // Close window via interaction (if needed)
        }).WithGlobalErrorHandling();

        this.WhenAnyValue(x => x.InstallPath)
            .Subscribe(_ => UpdateAvailableSpace());
    }

    public string InstallPath
    {
        get => _installPath;
        set => this.RaiseAndSetIfChanged(ref _installPath, value);
    }


    public long AvailableSpace
    {
        get => _availableSpace;
        private set
        {
            this.RaiseAndSetIfChanged(ref _availableSpace, value);
            this.RaisePropertyChanged(nameof(HasEnoughSpace));
            this.RaisePropertyChanged(nameof(TotalSpaceFormatted));
            this.RaisePropertyChanged(nameof(FreeSpaceFormatted));
            this.RaisePropertyChanged(nameof(SpaceRequiredFormatted));
            this.RaisePropertyChanged(nameof(SpaceLeftAfterInstallFormatted));
        }
    }

    public bool HasEnoughSpace => AvailableSpace >= RequiredSpace;
    public bool CanInstall => HasEnoughSpace && !string.IsNullOrWhiteSpace(InstallPath);

    public string TotalSpaceFormatted => GetDrive()?.TotalSize is long t ? $"Total: {FormatSize(t)}" : "Total: N/A";
    public string FreeSpaceFormatted => $"Free: {FormatSize(AvailableSpace)}";
    public string SpaceRequiredFormatted => $"Required: {FormatSize(RequiredSpace)}";
    public string SpaceLeftAfterInstallFormatted => $"After install: {FormatSize(AvailableSpace - RequiredSpace)}";

    private DriveInfo? GetDrive()
    {
        try { return new DriveInfo(Path.GetPathRoot(InstallPath)!); }
        catch { return null; }
    }

    private void UpdateAvailableSpace()
    {
        AvailableSpace = GetDrive()?.AvailableFreeSpace ?? 0;
    }

    private static string FormatSize(long bytes)
    {
        var gb = bytes / (1024.0 * 1024 * 1024);
        return $"{gb:F2} GB";
    }

    public Interaction<Unit, string?> PickFolderInteraction { get; } = new();
    public Interaction<Exception, Unit> ShowErrorDialog { get; } = new();

    public ReactiveCommand<Unit, Unit> TriggerPickFolderCommand { get; }
    public ReactiveCommand<Unit, Unit> InstallCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }
}
