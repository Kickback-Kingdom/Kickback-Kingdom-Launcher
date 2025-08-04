using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KickbackKingdomLauncher.Models.Vault
{
    public class VaultInfo
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "New Vault";
        public string Path { get; set; }

        public override string ToString() => $"{Name} ({Path})";
    }

}
