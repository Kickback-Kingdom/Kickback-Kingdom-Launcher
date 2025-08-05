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
        public VaultInfo? FindById(Guid id)
        {
            return ValidVaults.FirstOrDefault(v => v.Id == id);
        }
        public VaultInfo? FindVaultContainingLocator(string locator)
        {
            foreach (var vault in ValidVaults)
            {
                var dir = Path.Combine(vault.Path, locator);
                if (Directory.Exists(dir))
                    return vault;
            }

            return null;
        }

        public static string GetInitialVaultPath()
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
        public VaultInfo GetDefaultVault()
        {
            return DefaultVault
                   ?? Vaults.FirstOrDefault()
                   ?? CreateDefaultVault();
        }


        public VaultInfo CreateVault(string path, string name)
        {
            // Ensure the directory exists
            Directory.CreateDirectory(path);

            // Create a new VaultInfo object
            var vault = new VaultInfo
            {
                Path = path,
                Name = name
            };

            // Add it to the list and save
            AddVault(vault);

            return vault;
        }
        public VaultInfo CreateDefaultVault()
        {
            var path = GetInitialVaultPath();
            var vault = CreateVault(path, "Main Vault");
            SetDefault(vault);
            return vault;
        }


        public bool HasVaults => Vaults.Any(v => Directory.Exists(v.Path));

    }
}
