using System;
using System.IO;
using System.Text.Json;

namespace KickbackKingdomLauncher.Models.Secrets
{
    public static class SecretsManager
    {
        public static Secrets Secrets { get; private set; }

        static SecretsManager()
        {
            try
            {
                var json = File.ReadAllText("KickbackSecrets.json");

                // Use System.Text.Json
                Secrets = JsonSerializer.Deserialize<Secrets>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true // allows "serviceKey" or "ServiceKey"
                }) ?? new Secrets();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SecretsManager] Failed to load secrets: {ex.Message}");
                Secrets = new Secrets(); // fallback to empty secrets
            }
        }
    }
}
