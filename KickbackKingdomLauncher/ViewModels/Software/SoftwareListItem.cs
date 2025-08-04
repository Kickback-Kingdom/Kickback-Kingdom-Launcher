using KickbackKingdomLauncher.Models.Software;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KickbackKingdomLauncher.ViewModels.Software
{
    public abstract class SoftwareListItem { }

    public class SoftwareGroupHeader : SoftwareListItem
    {
        public string Title { get; set; } = "";
    }

    public class SoftwareItemEntry : SoftwareListItem
    {
        public SoftwareEntry Software { get; set; } = null!;
    }
}
