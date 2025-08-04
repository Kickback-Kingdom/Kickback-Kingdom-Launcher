using KickbackKingdomLauncher.Models.Software;
using ReactiveUI;

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
        }
        public string Title => Software.Title;
        public string BuildVersion => Software.BuildVersion;
        public string Description => Software.Description;
        public bool IsInstalled => Software.IsInstalled;
        public bool NotInstalled => Software.NotInstalled;
    }
}
