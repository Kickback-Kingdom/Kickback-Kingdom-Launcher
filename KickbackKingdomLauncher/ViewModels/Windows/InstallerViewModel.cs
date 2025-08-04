using KickbackKingdomLauncher.Helpers;
using KickbackKingdomLauncher.Models.Software;
using KickbackKingdomLauncher.Models.Tasks;
using KickbackKingdomLauncher.Services;
using ReactiveUI;
using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace KickbackKingdomLauncher.ViewModels.Windows;

public class InstallerViewModel : ReactiveObject
{
    public SoftwareEntry Software { get; set; }

    private string _installPath = "C:\\Program Files\\Kickback Kingdom";
    private long _requiredSpace = 5L * 1024 * 1024 * 1024; // 5 GB
    private long _availableSpace;

    public InstallerViewModel(SoftwareEntry software)
    {
        Software = software;

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
                //Software.InstallPath = InstallPath;

                //var task = new TaskProgress
                //{
                //    Software = Software,
                //    Type = TaskProgress.TaskType.Install,
                //    Progress = 0
                //};

                //TaskManager.Instance.ActiveTasks.Add(task);

                //var installer = new SoftwareInstaller();

                //var success = await installer.InstallSoftwareAsync(
                //    Software,
                //    Software.DownloadUrl,
                //    InstallPath,
                //    Software.ExpectedHash,
                //    task
                //);

                //if (!success)
                //    task.IsFailed = true;

                //task.Progress = 100;
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

    public long RequiredSpace => _requiredSpace;

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
