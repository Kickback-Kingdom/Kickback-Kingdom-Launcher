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

        public async Task<bool> InstallSoftwareAsync(
            SoftwareEntry software,
            string downloadUrl,
            string installPath,
            string expectedHash,
            TaskProgress taskProgress)
        {
            var tempPath = Path.GetTempFileName();

            // Wrap TaskProgress in a Progress<double>
            var progress = new Progress<double>(value => taskProgress.Progress = value);

            taskProgress.Software = software;
            taskProgress.Type = TaskProgress.TaskType.Install;
            taskProgress.Progress = 0;
            taskProgress.IsFailed = false;

            var success = await _downloadService.DownloadToFileAsync(downloadUrl, tempPath, progress);
            if (!success)
            {
                taskProgress.IsFailed = true;
                return false;
            }

            if (!_integrityService.VerifySha256(tempPath, expectedHash))
            {
                taskProgress.IsFailed = true;
                return false;
            }

            await _installerService.Install(tempPath, installPath);
            software.IsInstalled = true;
            taskProgress.Progress = 100;

            return true;
        }
    }
}
