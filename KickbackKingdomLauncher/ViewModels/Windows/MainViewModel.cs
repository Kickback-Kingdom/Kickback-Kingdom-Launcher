using KickbackKingdomLauncher.Models.Software;
using KickbackKingdomLauncher.Models.Tasks;
using KickbackKingdomLauncher.ViewModels.Base;
using KickbackKingdomLauncher.ViewModels.Pages;
using KickbackKingdomLauncher.ViewModels.Software;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;

namespace KickbackKingdomLauncher.ViewModels.Windows
{
    public class MainViewModel : ViewModelBase
    {
        // Managers
        public SoftwareManager SoftwareManager => SoftwareManager.Instance;
        public TaskManager TaskManager => TaskManager.Instance;

        // Commands
        public ReactiveCommand<SoftwareEntry, Unit> SelectGameCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowTaskPageCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowStorePageCommand { get; }

        // Modular Page View
        private ViewModelBase? _currentPage;
        public ViewModelBase? CurrentPage
        {
            get => _currentPage;
            set => this.RaiseAndSetIfChanged(ref _currentPage, value);
        }

        // List Selection
        private SoftwareListItem? _selectedListItem;
        public SoftwareListItem? SelectedListItem
        {
            get => _selectedListItem;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedListItem, value);
                this.RaisePropertyChanged(nameof(SelectedGroupSoftware));
                this.RaisePropertyChanged(nameof(IsViewingGroup));

                if (value is SoftwareItemEntry entry)
                {
                    CurrentPage = new GamePageViewModel(entry.Software);
                }
                else if (value is SoftwareGroupHeader header)
                {
                    CurrentPage = new GroupSoftwarePageViewModel(header.Title);
                }
            }
        }

        // Group Logic
        public IEnumerable<SoftwareListItem> GroupedSoftwareFlat =>
            SoftwareManager.AllSoftware
                .GroupBy(s => s.GroupLabel)
                .SelectMany(group => new SoftwareListItem[] { new SoftwareGroupHeader { Title = group.Key } }
                .Concat(group.Select(s => new SoftwareItemEntry { Software = s })));

        public IEnumerable<SoftwareEntry> SelectedGroupSoftware =>
            SelectedListItem is SoftwareGroupHeader header
                ? SoftwareManager.AllSoftware.Where(s => s.GroupLabel == header.Title)
                : Enumerable.Empty<SoftwareEntry>();

        public bool IsViewingGroup => SelectedListItem is SoftwareGroupHeader;

        // Constructor
        public MainViewModel()
        {
            SelectGameCommand = ReactiveCommand.Create<SoftwareEntry>(software =>
                SelectedListItem = new SoftwareItemEntry { Software = software });

            ShowTaskPageCommand = ReactiveCommand.Create(() =>
            {
                CurrentPage = new TaskPageViewModel();
                return Unit.Default;
            });

            ShowStorePageCommand = ReactiveCommand.Create(() =>
            {
                CurrentPage = new StorePageViewModel();
                return Unit.Default;
            });


            TaskManager.ActiveTasks.CollectionChanged += (_, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (TaskProgress task in e.NewItems)
                    {
                        task.WhenAnyValue(x => x.Progress)
                            .Subscribe(_ => CleanCompletedTasks());
                    }
                }

                this.RaisePropertyChanged(nameof(TaskManager.HasActiveTasks));
                this.RaisePropertyChanged(nameof(TaskManager.CombinedProgress));
                this.RaisePropertyChanged(nameof(TaskManager.ActiveTaskTypeLabel));
            };

            AddTestData();

            // Default View
            SelectedListItem = SoftwareManager.AllSoftware
                .Select(s => new SoftwareItemEntry { Software = s })
                .FirstOrDefault();
        }

        private void CleanCompletedTasks()
        {
            var finished = TaskManager.ActiveTasks.Where(t => t.Progress >= 100).ToList();
            foreach (var task in finished)
                TaskManager.ActiveTasks.Remove(task);
        }

        private void AddTestData()
        {
            SoftwareManager.Clear();

            var emberwood = new SoftwareEntry
            {
                Title = "Emberwood Trading Co.",
                BuildVersion = "1.2.4",
                Description = "Explore dungeons, trade across galaxies, and uncover lost Nebi Gems in this fantasy-sci-fi crossover adventure.",
                IsInstalled = false,
                Type = SoftwareType.Game
            };

            var atlas = new SoftwareEntry
            {
                Title = "Atlas Odyssey",
                BuildVersion = "0.9.1",
                Description = "Race starships, steal artifacts, and uncover cosmic conspiracies.",
                IsInstalled = true,
                Type = SoftwareType.Game
            };

            var craftsmen = new SoftwareEntry
            {
                Title = "Craftsmen Simulator",
                BuildVersion = "2.0.0",
                Description = "Build, repair, and innovate in the world of high fantasy industry.",
                IsInstalled = true,
                Type = SoftwareType.Game
            };

            SoftwareManager.Add(emberwood);
            SoftwareManager.Add(atlas);
            SoftwareManager.Add(craftsmen);

            TaskManager.ActiveTasks.Clear();
            TaskManager.ActiveTasks.Add(new TaskProgress { Software = emberwood, Progress = 30, Type = TaskProgress.TaskType.Download });
            TaskManager.ActiveTasks.Add(new TaskProgress { Software = atlas, Progress = 65, Type = TaskProgress.TaskType.Install });
            TaskManager.ActiveTasks.Add(new TaskProgress { Software = craftsmen, Progress = 10, Type = TaskProgress.TaskType.Update });
        }
    }
}
