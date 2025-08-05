namespace KickbackKingdom.API.Models
{
    public class SoftwareFile
    {
        public string RelativePath { get; set; } = string.Empty;
        public long Size { get; set; }
        public string Sha256 { get; set; } = string.Empty;
    }
}
