using System.Collections.Generic;
using System.Linq;
using KickbackKingdomLauncher.Models.Software;
using KickbackKingdomLauncher.ViewModels.Base;

namespace KickbackKingdomLauncher.ViewModels.Pages
{
    public class GroupSoftwarePageViewModel : ViewModelBase
    {
        public string GroupTitle { get; }

        public IEnumerable<SoftwareEntry> GroupSoftware { get; }

        public GroupSoftwarePageViewModel(string groupTitle)
        {
            GroupTitle = groupTitle;

            GroupSoftware = SoftwareManager.Instance.AllSoftware
                .Where(s => s.GroupLabel == groupTitle)
                .ToList();
        }
    }
}
