using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KickbackKingdom.API.Models;
using KickbackKingdomLauncher.Models.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;

namespace KickbackKingdomLauncher.ViewModels.Windows;

public partial class ManifestBuilderViewModel : ObservableObject
{
    [ObservableProperty] private string selectedFolder = string.Empty;
    [ObservableProperty] private string softwareId = string.Empty;
    [ObservableProperty] private string version = string.Empty;
    [ObservableProperty] private string outputPath = string.Empty;
    [ObservableProperty] private string statusMessage = string.Empty;

    public async Task<string?> BrowseFolder(Window parent)
    {
        var dialog = new OpenFolderDialog();
        return await dialog.ShowAsync(parent);
    }


    [RelayCommand]
    public void GenerateManifest()
    {
        if (string.IsNullOrWhiteSpace(SelectedFolder) ||
            string.IsNullOrWhiteSpace(SoftwareId) ||
            string.IsNullOrWhiteSpace(Version))
        {
            StatusMessage = "Please fill in all fields.";
            return;
        }

        var task = new TaskProgress
        {
            TaskName = $"Building Manifest: {SoftwareId}",
            Type = TaskProgress.TaskType.BuildManifest,
            Progress = 0
        };

        task.RunAsync(async reportProgress =>
        {
            StatusMessage = "Generating manifest...";

            var files = await ScanFilesAsync(SelectedFolder, reportProgress);

            var manifest = new SoftwareManifest
            {
                SoftwareId = SoftwareId,
                Version = Version,
                Files = files
            };

            var manifestJson = JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true });
            var path = Path.Combine(SelectedFolder, "manifest.json");
            await File.WriteAllTextAsync(path, manifestJson);

            OutputPath = path;
            StatusMessage = "Manifest successfully generated.";
        });

        TaskManager.Instance.AddTask(task);
    }


    private async Task<List<SoftwareFile>> ScanFilesAsync(string root, Action<double>? reportProgress = null)
    {
        var result = new List<SoftwareFile>();
        var filePaths = Directory.GetFiles(root, "*", SearchOption.AllDirectories);
        int total = filePaths.Length;

        for (int i = 0; i < total; i++)
        {
            string path = filePaths[i];
            string rel = Path.GetRelativePath(root, path).Replace("\\", "/");

            using var stream = File.OpenRead(path);
            using var sha256 = SHA256.Create();
            var hash = await Task.Run(() => sha256.ComputeHash(stream));
            string hex = Convert.ToHexString(hash).ToLowerInvariant();

            result.Add(new SoftwareFile
            {
                RelativePath = rel,
                Size = new FileInfo(path).Length,
                Sha256 = hex
            });

            reportProgress?.Invoke((i + 1) / (double)total);
        }

        return result;
    }

}
