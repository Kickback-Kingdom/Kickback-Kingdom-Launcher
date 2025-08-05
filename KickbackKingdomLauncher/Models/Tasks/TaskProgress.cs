using KickbackKingdomLauncher.Models.Software;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KickbackKingdomLauncher.Models.Tasks
{
    public class TaskProgress : ReactiveObject
    {
        public enum TaskType { Download, Install, Update, BuildManifest }

        public TaskType Type { get; set; } = TaskType.Download;

        public bool IsComplete => Progress >= 100;
        public bool IsFailed { get; set; } = false;
        public bool IsActive => Progress > 0 && Progress < 100 && !IsFailed;

        public string TaskName { get; set; } = "Unknown Task";

        private double _progress;
        public double Progress
        {
            get => _progress;
            set
            {
                this.RaiseAndSetIfChanged(ref _progress, value);
                this.RaisePropertyChanged(nameof(IsComplete));
                this.RaisePropertyChanged(nameof(StatusMessage));
            }
        }
        private SoftwareEntry? _software;
        public SoftwareEntry? Software
        {
            get => _software;
            set
            {
                this.RaiseAndSetIfChanged(ref _software, value);
                if (_software != null)
                    TaskName = _software.Title;
            }
        }
        public string StatusMessage =>
            IsFailed ? "Failed" :
            Progress == 0 ? "Queued" :
            Progress < 25 ? Type switch
            {
                TaskType.Download => "Connecting...",
                TaskType.Install => "Preparing install...",
                TaskType.Update => "Checking files...",
                _ => "Starting..."
            } :
            Progress < 70 ? Type switch
            {
                TaskType.Download => "Downloading...",
                TaskType.Install => "Installing...",
                TaskType.Update => "Applying patch...",
                _ => "Working..."
            } :
            Progress < 100 ? "Finalizing..." :
            "Complete";

        private TimeSpan? _estimatedTimeRemaining;
        public TimeSpan? EstimatedTimeRemaining
        {
            get => _estimatedTimeRemaining;
            set => this.RaiseAndSetIfChanged(ref _estimatedTimeRemaining, value);
        }

        public  void RunAsync(Func<Action<double>, Task> work)
        {
            Task.Run(async () =>
            {
                try
                {
                    await work(progress =>
                    {
                        // Clamp to 0–100
                        Progress = Math.Max(0, Math.Min(100, progress * 100));
                    });

                    Progress = 100;
                }
                catch
                {
                    IsFailed = true;
                }
            });
        }
    }


}
