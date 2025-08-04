using System;
using System.Collections.Generic;
using System.Text;

namespace KickbackKingdom.API.Models
{
    public class Account
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public string SessionToken { get; set; } = "";
        public string Email { get; set; } = "";
        public string AvatarMedia { get; set; } = "";
        public int Level { get; set; }

        public List<object> Characters { get; set; } = new();

        public bool IsSessionValid() => !string.IsNullOrEmpty(SessionToken);
    }
}
