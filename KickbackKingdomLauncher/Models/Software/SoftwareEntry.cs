using ReactiveUI;
using System;

namespace KickbackKingdomLauncher.Models.Software
{
    public enum SoftwareType
    {
        Game,
        Tool,
        Server
    }
    public class SoftwareEntry : ReactiveObject
    {
        private string _title = "Unknown Software";
        private string _description = "No description provided.";
        private string _buildVersion = "0.0.0";
        private bool _isInstalled;
        private SoftwareType _type = SoftwareType.Game;
        public string GroupLabel => !string.IsNullOrWhiteSpace(CustomGroup)
    ? CustomGroup
    : IsInstalled ? "INSTALLED" : "NOT INSTALLED";

        private string? _customGroup;

        public string? CustomGroup
        {
            get => _customGroup;
            set => this.RaiseAndSetIfChanged(ref _customGroup, value);
        }

        public string Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        public string Description
        {
            get => _description;
            set => this.RaiseAndSetIfChanged(ref _description, value);
        }

        public string BuildVersion
        {
            get => _buildVersion;
            set => this.RaiseAndSetIfChanged(ref _buildVersion, value);
        }

        public bool IsInstalled
        {
            get => _isInstalled;
            set => this.RaiseAndSetIfChanged(ref _isInstalled, value);
        }

        public bool NotInstalled => !IsInstalled;

        public SoftwareType Type
        {
            get => _type;
            set => this.RaiseAndSetIfChanged(ref _type, value);
        }

    }
}
