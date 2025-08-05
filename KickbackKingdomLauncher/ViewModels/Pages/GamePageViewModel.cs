using DynamicData.Binding;
using KickbackKingdomLauncher.Models.Software;
using ReactiveUI;
using System;

namespace KickbackKingdomLauncher.ViewModels.Pages
{
    public class GamePageViewModel : Base.ViewModelBase
    {
        private SoftwareEntry _software;
        public SoftwareEntry Software
        {
            get => _software;
            set => this.RaiseAndSetIfChanged(ref _software, value);
        }

        public GamePageViewModel(SoftwareEntry software)
        {
            _software = software;

            Software.WhenAnyPropertyChanged()
                       .Subscribe(_ =>
                       {
                           this.RaisePropertyChanged(nameof(IsInstalled));
                           this.RaisePropertyChanged(nameof(NotInstalled));
                           this.RaisePropertyChanged(nameof(IsInstalling));
                           this.RaisePropertyChanged(nameof(ShowInstall));
                       });
        }
        public bool IsInstalling => Software.IsInstalling;
        public bool ShowInstall => !IsInstalled && !IsInstalling;

        public string Title => Software.Title;
        public string BuildVersion => Software.BuildVersion;
        public string Description => Software.Description;

        // These are now reactive to Software.IsInstalled
        public bool IsInstalled => Software.IsInstalled;
        public bool NotInstalled => Software.NotInstalled;
    }
}
