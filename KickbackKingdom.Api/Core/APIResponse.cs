using System;
using System.Collections.Generic;
using System.Text;

namespace KickbackKingdom.API.Core
{
    public class APIResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";

        public static T Error<T>(string message) where T : APIResponse, new()
        {
            return new T { Success = false, Message = message };
        }
    }

    public class APIResponse<T> : APIResponse
    {
        public T? Data { get; set; }
    }
}
