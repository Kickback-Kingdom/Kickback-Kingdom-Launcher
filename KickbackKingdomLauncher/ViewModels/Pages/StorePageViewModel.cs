using System.Collections.Generic;
using System.Linq;
using KickbackKingdomLauncher.Models.Software;
using KickbackKingdomLauncher.ViewModels.Base;

namespace KickbackKingdomLauncher.ViewModels.Pages
{
    public class StorePageViewModel : ViewModelBase
    {
        public IEnumerable<SoftwareEntry> StoreItems { get; }

        public string Title => "Emberwood Trading Co. Store";

        public StorePageViewModel()
        {
            // For now, show all software not yet installed as store items
            StoreItems = SoftwareManager.Instance.AllSoftware
                .Where(s => !s.IsInstalled)
                .OrderBy(s => s.Title)
                .ToList();
        }
    }
}
