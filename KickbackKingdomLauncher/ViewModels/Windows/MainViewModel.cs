using KickbackKingdom.API.Core;
using KickbackKingdom.API.Models;
using KickbackKingdom.API.Services;
using KickbackKingdomLauncher.Models.Software;
using KickbackKingdomLauncher.Models.Tasks;
using KickbackKingdomLauncher.ViewModels.Base;
using KickbackKingdomLauncher.ViewModels.Pages;
using KickbackKingdomLauncher.ViewModels.Software;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;


namespace KickbackKingdomLauncher.ViewModels.Windows
{
    public class MainViewModel : ViewModelBase
    {
        // Managers
        public SoftwareManager SoftwareManager => SoftwareManager.Instance;
        public TaskManager TaskManager => TaskManager.Instance; 
        public Account? CurrentAccount => SessionManager.CurrentAccount;


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
        public bool HasActiveTasks => TaskManager.HasActiveTasks;

        public bool IsViewingGroup => SelectedListItem is SoftwareGroupHeader;

        // Constructor
        public MainViewModel()
        {
            Debug.WriteLine("MainViewModel constructor started.");

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

                this.RaisePropertyChanged(nameof(HasActiveTasks));
                this.RaisePropertyChanged(nameof(TaskManager.CombinedProgress));
                this.RaisePropertyChanged(nameof(TaskManager.ActiveTaskTypeLabel));
            };

            try
            {
                Debug.WriteLine("Assigning SelectedListItem...");
                SelectedListItem = SoftwareManager.AllSoftware
                    .Select(s => new SoftwareItemEntry { Software = s })
                    .FirstOrDefault();
                Debug.WriteLine("SelectedListItem assignment complete.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception assigning SelectedListItem: " + ex);
            }

            Debug.WriteLine("MainViewModel constructor complete.");
            LoadLibraryAsync();
        }

        public void CleanCompletedTasks()
        {
            return;
            foreach (var task in TaskManager.ActiveTasks.Where(t => t.IsComplete).ToList())
                TaskManager.ActiveTasks.Remove(task);
        }

        private async void LoadLibraryAsync()
        {
            try
            {
                var apiList = await LibraryService.GetAccountLibraryAsync();

                SoftwareManager.Clear();

                foreach (var software in apiList)
                {
                    var entry = SoftwareEntry.FromApiSoftware(software);
                    SoftwareManager.Add(entry);
                }

                // Subscribe to installed changes after loading
                foreach (var software in SoftwareManager.AllSoftware)
                {
                    software.WhenAnyValue(x => x.IsInstalled)
                        .Subscribe(_ =>
                        {
                            this.RaisePropertyChanged(nameof(GroupedSoftwareFlat));
                            this.RaisePropertyChanged(nameof(SelectedGroupSoftware));
                        });
                }

                this.RaisePropertyChanged(nameof(GroupedSoftwareFlat));

                SelectedListItem = SoftwareManager.AllSoftware
                    .Select(s => new SoftwareItemEntry { Software = s })
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load library: {ex.Message}");
            }
        }


    }
}
