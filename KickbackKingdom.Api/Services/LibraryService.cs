using KickbackKingdom.API.Core;
using KickbackKingdom.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KickbackKingdom.API.Services
{
    public static class LibraryService
    {
        public static async Task<List<Software>> GetAccountLibraryAsync()
        {
            var response = await APIClient.GetAsync<List<Software>>("library.json", true);

            if (response.Success && response.Data != null)
                return response.Data;

            throw new Exception(response.Message ?? "Failed to retrieve software library.");
        }

        public static async Task<SoftwareManifest> GetManifestAsync(string softwareLocator)
        {
            var response = await APIClient.GetAsync<SoftwareManifest>($"manifests/{softwareLocator}.json", true);

            if (response.Success && response.Data != null)
                return response.Data;

            throw new Exception(response.Message ?? $"Failed to retrieve manifest for software ID '{softwareLocator}'.");
        }
    }
}
