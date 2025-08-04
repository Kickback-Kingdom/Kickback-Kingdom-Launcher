using System;
using System.IO;
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

        public async Task<bool> DownloadToFileAsync(string url, string destinationPath, IProgress<double>? progress = null)
        {
            _cts = new CancellationTokenSource();
            IsDownloading = true;
            _isPaused = false;

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);

                using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, _cts.Token);
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                var buffer = new byte[8192];
                var isContentLengthKnown = totalBytes > 0;

                using var contentStream = await response.Content.ReadAsStreamAsync(_cts.Token);
                using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                long bytesDownloaded = 0;

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
            catch
            {
                if (File.Exists(destinationPath))
                    File.Delete(destinationPath);
                return false;
            }
            finally
            {
                IsDownloading = false;
            }
        }
    }
}
