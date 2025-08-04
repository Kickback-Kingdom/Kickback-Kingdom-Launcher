using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace KickbackKingdomLauncher.Services
{
    public class InstallerService
    {
        /// <summary>
        /// Installs a software package from a downloaded file to a destination install directory.
        /// </summary>
        /// <param name="packagePath">Path to the downloaded installer file (e.g. .zip, .exe).</param>
        /// <param name="installDir">Target installation directory.</param>
        public async Task Install(string packagePath, string installDir)
        {
            if (!File.Exists(packagePath))
                throw new FileNotFoundException("Package file not found.", packagePath);

            Directory.CreateDirectory(installDir);

            var extension = Path.GetExtension(packagePath).ToLowerInvariant();

            if (extension == ".zip")
            {
                // Extract archive
                await Task.Run(() => ZipFile.ExtractToDirectory(packagePath, installDir, overwriteFiles: true));
            }
            else
            {
                // Simple copy/install
                var fileName = Path.GetFileName(packagePath);
                var destinationFile = Path.Combine(installDir, fileName);

                await Task.Run(() =>
                {
                    File.Copy(packagePath, destinationFile, overwrite: true);
                });
            }

            // Optionally clean up temp file
            // File.Delete(packagePath);
        }

        /// <summary>
        /// Unzips an archive to the target directory (can be used as helper).
        /// </summary>
        public async Task ExtractZipAsync(string zipFilePath, string extractToPath)
        {
            Directory.CreateDirectory(extractToPath);
            await Task.Run(() => ZipFile.ExtractToDirectory(zipFilePath, extractToPath, overwriteFiles: true));
        }
    }
}
