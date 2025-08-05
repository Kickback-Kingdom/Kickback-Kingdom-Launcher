using System;
using System.Diagnostics;
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

                Secrets = JsonSerializer.Deserialize<Secrets>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new Secrets();

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SecretsManager] Failed to load secrets: {ex.Message}");
                Secrets = new Secrets(); // fallback to empty
            }
        }
    }
}
