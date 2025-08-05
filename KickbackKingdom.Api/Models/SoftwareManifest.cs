using System;
using System.Collections.Generic;

namespace KickbackKingdom.API.Models
{
    public class SoftwareManifest
    {
        public string SoftwareId { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public List<SoftwareFile> Files { get; set; } = new();

        public string GetFileUrl(SoftwareFile file)
        {
            return $"https://kickback-kingdom.com/testData/gamefiles/" +
           $"{Uri.EscapeDataString(SoftwareId)}/" +
           $"{Uri.EscapeDataString(file.RelativePath)}";

        }
    }
}