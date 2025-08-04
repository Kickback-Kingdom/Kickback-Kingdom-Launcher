using KickbackKingdom.API.Core;
using KickbackKingdom.API.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace KickbackKingdom.API.Services
{
    public static class AuthService
    {
        public class LoginRequest
        {
            public string Email { get; set; } = "";
            public string Pwd { get; set; } = "";
            public string ServiceKey { get; set; } = APIClient.ServiceKey;
        }

        public class LoginResponse : APIResponse<Account> { }

        public static async Task<APIResponse<Account>> LoginAsync(string email, string password)
        {
            var request = new LoginRequest
            {
                Email = email,
                Pwd = password
            };

            return await APIClient.Call<LoginRequest, LoginResponse>(
                "account/login.php",
                HttpMethod.Post,
                request,
                null,
                APIClient.BodyType.Form
            );
        }
    }
}
