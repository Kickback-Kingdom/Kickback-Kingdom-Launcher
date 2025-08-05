using System.Collections.Generic;

namespace KickbackKingdomLauncher.Models.Software
{
    public class InstallState
    {
        public string SoftwareId { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public HashSet<string> CompletedFiles { get; set; } = new(); // Store RelativePath of completed files
    }
}
