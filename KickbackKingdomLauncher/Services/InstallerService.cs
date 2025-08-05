using KickbackKingdom.API.Models;
using KickbackKingdomLauncher.Services;
using System;
using System.IO;
using System.Threading.Tasks;

public class InstallerService
{
    private readonly DownloadService _downloadService = new();

    public async Task<bool> InstallFromManifestAsync(
        SoftwareManifest manifest,
        string installRoot,
        IProgress<double>? progress = null)
    {
        var success = await _downloadService.DownloadManifestFilesAsync(manifest, installRoot, progress);

        if (success)
        {
            var statePath = Path.Combine(installRoot, ".installstate.json");
            if (File.Exists(statePath))
                File.Delete(statePath);
        }

        return success;
    }
}
