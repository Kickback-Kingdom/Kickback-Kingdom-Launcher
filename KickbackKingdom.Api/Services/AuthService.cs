using KickbackKingdom.API.Core;
using KickbackKingdom.API.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace KickbackKingdom.API.Services
{
    public static class AuthService
    {
        public class LoginRequest
        {
            public string email { get; set; } = "";
            public string pwd { get; set; } = "";
            public string serviceKey { get; set; } = APIClient.ServiceKey;
        }

        public class LoginResponse : APIResponse<Account> { }

        public static async Task<APIResponse<Account>> LoginAsync(string email, string password)
        {
            var request = new LoginRequest
            {
                email = email,
                pwd = password
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
