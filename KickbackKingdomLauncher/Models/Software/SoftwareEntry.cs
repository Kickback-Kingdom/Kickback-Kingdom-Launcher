using Avalonia.Controls;
using KickbackKingdom.API.Models;
using KickbackKingdomLauncher.Models.Tasks;
using KickbackKingdomLauncher.Models.Vault;
using ReactiveUI;
using Splat;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace KickbackKingdomLauncher.Models.Software
{
    public class SoftwareEntry : ReactiveObject
    {
        public SoftwareEntry(KickbackKingdom.API.Models.Software software)
        {
            Software = software;
            TaskManager.Instance.ActiveTasks.CollectionChanged += (s, e) =>
            {
                // Only raise change if a task related to this software is involved
                bool affected = false;

                if (e.NewItems != null)
                    affected = affected || e.NewItems.Cast<TaskProgress>().Any(t => t.Software == this);

                if (e.OldItems != null)
                    affected = affected || e.OldItems.Cast<TaskProgress>().Any(t => t.Software == this);

                if (affected)
                {
                    this.RaisePropertyChanged(nameof(IsInstalling));
                }
            };
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
            set
            {
                if (this.RaiseAndSetIfChanged(ref _isInstalled, value))
                {
                    this.RaisePropertyChanged(nameof(NotInstalled));
                    this.RaisePropertyChanged(nameof(GroupLabel));
                }
            }
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
        public bool IsInstalling =>
    TaskManager.Instance.ActiveTasks.Any(t => t.Software == this && !t.IsComplete);

        public RecordId Id => Software;
        public string Title => Software.Title;
        public string Description => Software.Description;
        public string BuildVersion => Software.BuildVersion;
        public SoftwareType Type => Software.Type;
        public string? IconUrl => Software.IconUrl;
        public string? BannerUrl => Software.BannerUrl;
        public string ExecutablePath => Software.ExecutablePath;
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
        public bool Uninstall()
        {
            try
            {
                if (Directory.Exists(InstallPath))
                    Directory.Delete(InstallPath, true);

                IsInstalled = false;


                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Uninstall failed: {ex.Message}");
                return false;
            }
        }

        public bool Play()
        {
            if (string.IsNullOrWhiteSpace(Software.ExecutablePath))
                return false;

            var exePath = Path.Combine(InstallPath, Software.ExecutablePath);

            if (!File.Exists(exePath))
            {
                Console.WriteLine("Executable not found: " + exePath);
                return false;
            }

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    WorkingDirectory = InstallPath,
                    UseShellExecute = true
                };

                Process.Start(startInfo);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to launch: {ex.Message}");
                return false;
            }
        }
    }
}
