using KickbackKingdom.API.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace KickbackKingdomLauncher.Models.Software
{
    public class SoftwareManager : ReactiveObject
    {
        private static readonly Lazy<SoftwareManager> _instance = new(() => new SoftwareManager());
        public static SoftwareManager Instance => _instance.Value;

        public ObservableCollection<SoftwareEntry> AllSoftware { get; } = new();

        public IEnumerable<SoftwareEntry> Installed => AllSoftware.Where(s => s.IsInstalled);
        public IEnumerable<SoftwareEntry> NotInstalled => AllSoftware.Where(s => !s.IsInstalled);
        public IEnumerable<SoftwareEntry> Games => AllSoftware.Where(s => s.Type == SoftwareType.Game);
        public IEnumerable<SoftwareEntry> Tools => AllSoftware.Where(s => s.Type == SoftwareType.Tool);
        public IEnumerable<SoftwareEntry> Servers => AllSoftware.Where(s => s.Type == SoftwareType.Server);

        private readonly Dictionary<SoftwareEntry, IDisposable> _subscriptions = new();

        private SoftwareManager()
        {
            AllSoftware.CollectionChanged += (_, __) =>
            {
                this.RaisePropertyChanged(nameof(Installed));
                this.RaisePropertyChanged(nameof(NotInstalled));
                this.RaisePropertyChanged(nameof(Games));
                this.RaisePropertyChanged(nameof(Tools));
                this.RaisePropertyChanged(nameof(Servers));
            };
        }

        public void Add(SoftwareEntry software)
        {
            if (!AllSoftware.Contains(software))
            {
                AllSoftware.Add(software);
                HookPropertyChanges(software);
            }
        }

        public void Remove(SoftwareEntry software)
        {
            if (AllSoftware.Remove(software))
            {
                UnhookPropertyChanges(software);
            }
        }

        public void Clear()
        {
            foreach (var software in AllSoftware.ToList())
                UnhookPropertyChanges(software);

            AllSoftware.Clear();
        }

        private void HookPropertyChanges(SoftwareEntry software)
        {
            var subscription = software.Changed.Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(Installed));
                this.RaisePropertyChanged(nameof(NotInstalled));
                this.RaisePropertyChanged(nameof(Games));
                this.RaisePropertyChanged(nameof(Tools));
                this.RaisePropertyChanged(nameof(Servers));
            });

            _subscriptions[software] = subscription;
        }

        private void UnhookPropertyChanges(SoftwareEntry software)
        {
            if (_subscriptions.TryGetValue(software, out var subscription))
            {
                subscription.Dispose();
                _subscriptions.Remove(software);
            }
        }
    }
}
