using System.Collections.ObjectModel;
using System.Linq;
using ReactiveUI;
using System;

namespace KickbackKingdomLauncher.Models.Tasks;

public class TaskManager : ReactiveObject
{
    private static readonly Lazy<TaskManager> _instance = new(() => new TaskManager());
    public static TaskManager Instance => _instance.Value;

    public ObservableCollection<TaskProgress> ActiveTasks { get; } = new();

    public bool HasActiveTasks => ActiveTasks.Any();

    public double CombinedProgress => ActiveTasks.Count > 0
        ? ActiveTasks.Average(t => t.Progress)
        : 0;

    public string ActiveTaskTypeLabel =>
        ActiveTasks.Any(t => t.Type == TaskProgress.TaskType.Download) ? "Downloading" :
        ActiveTasks.Any(t => t.Type == TaskProgress.TaskType.Install) ? "Installing" :
        ActiveTasks.Any(t => t.Type == TaskProgress.TaskType.Update) ? "Updating" :
        "Working";

    private TaskManager()
    {
        ActiveTasks.CollectionChanged += (_, _) => this.RaisePropertyChanged(nameof(CombinedProgress));
    }

    public void AddTask(TaskProgress task)
    {
        ActiveTasks.Add(task);
        task.WhenAnyValue(t => t.Progress)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(CombinedProgress));
                this.RaisePropertyChanged(nameof(ActiveTaskTypeLabel));
            });
    }

    public void CleanCompleted()
    {
        foreach (var t in ActiveTasks.Where(t => t.IsComplete).ToList())
            ActiveTasks.Remove(t);
    }
}
