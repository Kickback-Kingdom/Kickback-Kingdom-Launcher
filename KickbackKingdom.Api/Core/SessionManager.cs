using KickbackKingdom.API.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace KickbackKingdom.API.Core
{
    public static class SessionManager
    {
        private static readonly string SessionFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "KickbackKingdom", "session.json");

        public static Account? CurrentAccount { get; private set; }

        public static void Load()
        {
            if (!File.Exists(SessionFile)) return;

            try
            {
                var json = File.ReadAllText(SessionFile);
                CurrentAccount = JsonSerializer.Deserialize<Account>(json);

                if (CurrentAccount?.IsSessionValid() != true)
                {
                    CurrentAccount = null;
                    Logout();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Failed to load session: " + ex);
                CurrentAccount = null;
                Logout();
            }
        }


        public static void Save(Account account)
        {
            CurrentAccount = account;
            var dir = Path.GetDirectoryName(SessionFile)!;
            Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(account);
            File.WriteAllText(SessionFile, json);

            System.Diagnostics.Debug.WriteLine($"Session saved to {SessionFile}");
        }


        public static void Logout()
        {
            if (File.Exists(SessionFile))
                File.Delete(SessionFile);

            CurrentAccount = null;
        }

        public static bool IsLoggedIn() => CurrentAccount?.IsSessionValid() == true;
    }
}
