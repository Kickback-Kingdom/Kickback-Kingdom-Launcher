using KickbackKingdom.API.Models;
using KickbackKingdom.API.Services;
using KickbackKingdomLauncher.Models;
using KickbackKingdomLauncher.Models.Software;
using KickbackKingdomLauncher.Models.Tasks;
using System;
using System.IO;
using System.Threading.Tasks;

namespace KickbackKingdomLauncher.Services
{
    public class SoftwareInstaller
    {
        private readonly DownloadService _downloadService;
        private readonly IntegrityService _integrityService;
        private readonly InstallerService _installerService;

        public SoftwareInstaller()
        {
            _downloadService = new DownloadService();
            _integrityService = new IntegrityService();
            _installerService = new InstallerService();
        }
        public async Task<SoftwareManifest> GetManifestAsync(SoftwareEntry software)
        {
            if (string.IsNullOrWhiteSpace(software.Software.Locator))
                throw new ArgumentException("Software ID is required to fetch manifest.");

            return await LibraryService.GetManifestAsync(software.Software.Locator);
        }

        public async Task<bool> InstallSoftwareAsync(SoftwareEntry softwareEntry, TaskProgress taskProgress)
        {
            SoftwareManifest manifest = await GetManifestAsync(softwareEntry);

            return await InstallSoftwareFromManifestAsync(softwareEntry, manifest, softwareEntry.InstallPath!, taskProgress);
        }

        public async Task<bool> InstallSoftwareFromManifestAsync(
            SoftwareEntry software,
            SoftwareManifest manifest,
            string installPath,
            TaskProgress taskProgress)
        {
            var progress = new Progress<double>(value => taskProgress.Progress = value);

            taskProgress.Software = software;
            taskProgress.Type = TaskProgress.TaskType.Install;
            taskProgress.Progress = 0;
            taskProgress.IsFailed = false;

            var success = await _installerService.InstallFromManifestAsync(manifest, installPath, progress);
            if (!success)
            {
                taskProgress.IsFailed = true;
                return false;
            }

            software.IsInstalled = true;
            taskProgress.Progress = 100;

            return true;
        }

    }
}
