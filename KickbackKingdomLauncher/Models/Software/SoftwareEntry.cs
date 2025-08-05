using Avalonia.Controls;
using KickbackKingdom.API.Models;
using KickbackKingdomLauncher.Models.Vault;
using ReactiveUI;
using Splat;
using System;
using System.IO;

namespace KickbackKingdomLauncher.Models.Software
{
    public class SoftwareEntry : ReactiveObject
    {
        public SoftwareEntry(KickbackKingdom.API.Models.Software software)
        {
            Software = software;
        }

        public KickbackKingdom.API.Models.Software Software { get; }
        private Guid? _vaultId;
        public Guid? VaultId
        {
            get => _vaultId;
            set => this.RaiseAndSetIfChanged(ref _vaultId, value);
        }
        public VaultInfo? Vault => VaultId.HasValue
    ? VaultManager.Instance.FindById(VaultId.Value)
    : null;

        // Launcher-only fields
        private bool _isInstalled;
        private string? _customGroup;

        public bool IsInstalled
        {
            get => _isInstalled;
            set => this.RaiseAndSetIfChanged(ref _isInstalled, value);
        }

        public string? CustomGroup
        {
            get => _customGroup;
            set => this.RaiseAndSetIfChanged(ref _customGroup, value);
        }

        public string GroupLabel =>
            !string.IsNullOrWhiteSpace(CustomGroup)
                ? CustomGroup
                : IsInstalled ? "INSTALLED" : "NOT INSTALLED";

        public bool NotInstalled => !IsInstalled;

        // Convenience accessors from Software
        public RecordId Id => Software;
        public string Title => Software.Title;
        public string Description => Software.Description;
        public string BuildVersion => Software.BuildVersion;
        public SoftwareType Type => Software.Type;
        public string? IconUrl => Software.IconUrl;
        public string? BannerUrl => Software.BannerUrl;
        public string InstallPath
        {
            get
            {
                var vault = (VaultId != null)
                    ? VaultManager.Instance.FindById(VaultId.Value)
                    : null;

                vault ??= VaultManager.Instance.GetDefaultVault();

                return Path.Combine(vault.Path, Software.Locator);
            }
        }


        // Factory
        public static SoftwareEntry FromApiSoftware(KickbackKingdom.API.Models.Software api)
        {
            var vault = VaultManager.Instance.FindVaultContainingLocator(api.Locator);
            var isInstalled = vault != null;

            return new SoftwareEntry(api)
            {
                IsInstalled = isInstalled,
                VaultId = vault?.Id
            };
        }


        private bool CheckIfInstalled()
        {
            var vault = VaultManager.Instance.FindVaultContainingLocator(this.Software.Locator);
            return vault != null;
        }
    }
}
