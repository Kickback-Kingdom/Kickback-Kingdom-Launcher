using System;
using System.IO;
using System.Security.Cryptography;

namespace KickbackKingdomLauncher.Services
{
    public class IntegrityService
    {
        /// <summary>
        /// Verifies the SHA256 hash of a file against an expected hash.
        /// </summary>
        /// <param name="filePath">The full path to the file to verify.</param>
        /// <param name="expectedHash">The expected SHA256 hash in lowercase hexadecimal.</param>
        /// <returns>True if the file is valid, false otherwise.</returns>
        public bool VerifySha256(string filePath, string expectedHash)
        {
            if (!File.Exists(filePath))
                return false;

            try
            {
                using var sha = SHA256.Create();
                using var stream = File.OpenRead(filePath);

                var hashBytes = sha.ComputeHash(stream);
                var actualHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

                return actualHash == expectedHash.ToLowerInvariant();
            }
            catch (Exception ex)
            {
                // You might want to log this with LoggingService
                Console.WriteLine($"Integrity check failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Computes the SHA256 hash of a file.
        /// </summary>
        /// <param name="filePath">The file to hash.</param>
        /// <returns>Hex-encoded hash string.</returns>
        public string ComputeSha256(string filePath)
        {
            using var sha = SHA256.Create();
            using var stream = File.OpenRead(filePath);

            var hashBytes = sha.ComputeHash(stream);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
