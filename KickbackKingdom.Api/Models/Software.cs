namespace KickbackKingdom.API.Models
{
    public enum SoftwareType
    {
        Unknown = -1,
        Game = 0,
        Tool = 1,
        Server = 2,
    }
    public class Software : RecordId
    {
        public string Locator { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string BuildVersion { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public SoftwareType Type { get; set; } = SoftwareType.Game;
        public string? IconUrl { get; set; }
        public string? BannerUrl { get; set; }
    }
}
