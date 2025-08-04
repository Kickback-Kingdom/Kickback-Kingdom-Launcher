using KickbackKingdomLauncher.Models.Vault;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KickbackKingdomLauncher.Models.Settings
{
    public class AppSettings
    {
        public List<VaultInfo> Vaults { get; set; } = new();
        public Guid? DefaultVaultId { get; set; }

        // You can add more fields later:
        // public string Theme { get; set; }
        // public bool AutoLaunchOnStartup { get; set; }
    }
}
