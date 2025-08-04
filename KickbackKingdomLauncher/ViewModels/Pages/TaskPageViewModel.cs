using System.Collections.ObjectModel;
using KickbackKingdomLauncher.Models.Tasks;
using KickbackKingdomLauncher.ViewModels.Base;

namespace KickbackKingdomLauncher.ViewModels.Pages
{
    public class TaskPageViewModel : ViewModelBase
    {
        public ObservableCollection<TaskProgress> ActiveTasks => TaskManager.Instance.ActiveTasks;

        public string ActiveTaskTypeLabel => TaskManager.Instance.ActiveTaskTypeLabel;

        public double CombinedProgress => TaskManager.Instance.CombinedProgress;

        public bool HasActiveTasks => TaskManager.Instance.HasActiveTasks;

        public TaskPageViewModel()
        {
            // Optional: hook into task updates if needed
        }
    }
}
