using KickbackKingdom.API.Models;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace KickbackKingdomLauncher.Services
{
    public class DownloadService
    {
        private readonly HttpClient _httpClient = new();
        private CancellationTokenSource? _cts;
        private bool _isPaused;
        private readonly IntegrityService _integrityService = new IntegrityService();


        public bool IsDownloading { get; private set; }
        public bool IsPaused => _isPaused;

        public void Pause()
        {
            _isPaused = true;
        }

        public void Resume()
        {
            _isPaused = false;
        }

        public void Cancel()
        {
            _cts?.Cancel();
            _isPaused = false;
        }
        public async Task<bool> DownloadManifestFilesAsync(
    SoftwareManifest manifest,
    string installRoot,
    IProgress<double>? overallProgress = null,
    CancellationToken cancellationToken = default)
        {
            long totalBytes = manifest.Files.Sum(f => f.Size);
            long downloadedBytes = 0;

            foreach (var file in manifest.Files)
            {
                var destPath = Path.Combine(installRoot, file.RelativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);

                var isValid = _integrityService.VerifySha256(destPath, file.Sha256);
                if (File.Exists(destPath) && isValid)
                {
                    downloadedBytes += file.Size;
                    overallProgress?.Report((double)downloadedBytes / totalBytes * 100);
                    continue;
                }
                var success = await DownloadToFileAsync(manifest.GetFileUrl(file), destPath, null, cancellationToken);
                if (!success)
                    return false;

                isValid = _integrityService.VerifySha256(destPath, file.Sha256);
                if (!isValid)
                {
                    File.Delete(destPath);
                    return false;
                }

                downloadedBytes += file.Size;
                overallProgress?.Report((double)downloadedBytes / totalBytes * 100);
            }

            return true;
        }

        public async Task<bool> DownloadToFileAsync(string url, string destinationPath, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            IsDownloading = true;
            _isPaused = false;
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            try
            {
                long existingLength = File.Exists(destinationPath) ? new FileInfo(destinationPath).Length : 0;

                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                if (existingLength > 0)
                    request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(existingLength, null);

                using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, _cts.Token);
                response.EnsureSuccessStatusCode();

                var totalBytes = (response.Content.Headers.ContentRange?.Length ?? response.Content.Headers.ContentLength) ?? -1;
                var buffer = new byte[8192];
                var isContentLengthKnown = totalBytes > 0;

                using var contentStream = await response.Content.ReadAsStreamAsync(_cts.Token);
                using var fileStream = new FileStream(destinationPath, FileMode.Append, FileAccess.Write, FileShare.None, 8192, true);

                long bytesDownloaded = existingLength;

                while (true)
                {
                    if (_cts.Token.IsCancellationRequested)
                        return false;

                    if (_isPaused)
                    {
                        await Task.Delay(100, _cts.Token);
                        continue;
                    }

                    var bytesRead = await contentStream.ReadAsync(buffer.AsMemory(0, buffer.Length), _cts.Token);
                    if (bytesRead == 0)
                        break;

                    await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), _cts.Token);
                    bytesDownloaded += bytesRead;

                    if (isContentLengthKnown && progress != null)
                    {
                        double percent = (double)bytesDownloaded / totalBytes * 100;
                        progress.Report(percent);
                    }
                }

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Download failed for {url}: {ex.Message}");
                return false;
            }
            finally
            {
                IsDownloading = false;
            }
        }

    }
}
