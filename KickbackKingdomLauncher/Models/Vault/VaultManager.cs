using KickbackKingdomLauncher.Models.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KickbackKingdomLauncher.Models.Vault
{
    public class VaultManager
    {
        public static VaultManager Instance { get; } = new();

        public ObservableCollection<VaultInfo> Vaults { get; } = new();
        public IEnumerable<VaultInfo> ValidVaults =>
    Vaults.Where(v => Directory.Exists(v.Path));

        public VaultInfo? DefaultVault => SettingsManager.Settings.DefaultVaultId is Guid id
            ? Vaults.FirstOrDefault(v => v.Id == id)
            : null;

        private VaultManager() { }
        public static string GetDefaultVaultPath()
        {
            var baseDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "KickbackKingdom",
                "Vaults",
                "Main"
            );

            Directory.CreateDirectory(baseDir);
            return baseDir;
        }
        public void LoadVaults()
        {
            Vaults.Clear();
            foreach (var v in SettingsManager.Settings.Vaults)
                Vaults.Add(v);
        }

        public void SaveVaults()
        {
            SettingsManager.Settings.Vaults = Vaults.ToList();
            SettingsManager.Save();
        }

        public void SetDefault(VaultInfo vault)
        {
            SettingsManager.Settings.DefaultVaultId = vault.Id;
            SettingsManager.Save();
        }

        public void AddVault(VaultInfo vault, bool setAsDefault = false)
        {
            Vaults.Add(vault);
            if (setAsDefault || Vaults.Count == 1)
            {
                SetDefault(vault);
            }
            SaveVaults();
        }

        public void RemoveVault(VaultInfo vault)
        {
            Vaults.Remove(vault);
            if (SettingsManager.Settings.DefaultVaultId == vault.Id)
            {
                SettingsManager.Settings.DefaultVaultId = Vaults.FirstOrDefault()?.Id;
            }
            SaveVaults();
        }
        public bool HasVaults => Vaults.Any(v => Directory.Exists(v.Path));

    }
}
