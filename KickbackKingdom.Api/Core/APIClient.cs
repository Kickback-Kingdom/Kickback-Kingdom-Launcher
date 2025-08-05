using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KickbackKingdom.API.Core
{
    public static class APIClient
    {
        private const string BaseUrl = "https://kickback-kingdom.com/api/v1"; 
        public static string ServiceKey = "";

        private static readonly HttpClient client = new();

        public enum BodyType
        {
            None,
            Json,
            Form
        }

        private static Dictionary<string, string> ToFormDictionary<T>(T obj)
        {
            var dict = new Dictionary<string, string>();

            if (obj == null)
                return dict;

            foreach (var prop in typeof(T).GetProperties())
            {
                var value = prop.GetValue(obj)?.ToString();
                if (!string.IsNullOrWhiteSpace(prop.Name) && value != null)
                {
                    dict[prop.Name] = value;
                }
            }

            return dict;
        }

        public static async Task<TResponse> Call<TRequest, TResponse>(
            string endpoint,
            HttpMethod method,
            TRequest? requestBody,
            Dictionary<string, string>? headers = null,
            BodyType bodyType = BodyType.Json, bool testEndpoint = false) where TResponse : APIResponse, new()
        {
            string url = testEndpoint
                            ? $"https://kickback-kingdom.com/testData/{endpoint}"
                            : $"{BaseUrl}/{endpoint}";
            var request = new HttpRequestMessage(method, url);

            // Set up request content
            if (requestBody != null)
            {
                if (bodyType == BodyType.Json)
                {
                    var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                    });
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }
                else if (bodyType == BodyType.Form)
                {
                    var formDict = ToFormDictionary(requestBody);
                    request.Content = new FormUrlEncodedContent(formDict);
                }
            }

            // Set headers
            request.Headers.Add("Service-Key", ServiceKey); // standard header inclusion
            if (headers != null)
            {
                foreach (var kv in headers)
                    request.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
            }

            // Send and parse
            try
            {
                var response = await client.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<TResponse>(body, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new TResponse { Success = false, Message = "Invalid response format." };
            }
            catch (Exception ex)
            {
                return new TResponse { Success = false, Message = ex.Message };
            }
        }
        public static async Task<APIResponse<T>> GetAsync<T>(string endpoint, bool testEndpoint = false)
        {
            return await Call<object, APIResponse<T>>(endpoint, HttpMethod.Get, null, null, BodyType.None, testEndpoint);
        }

        public static async Task<APIResponse<T>> PostAsync<T>(string endpoint, object? requestBody, BodyType bodyType = BodyType.Json, bool testEndpoint = false)
        {
            return await Call<object, APIResponse<T>>(endpoint, HttpMethod.Post, requestBody, null, bodyType, testEndpoint);
        }
    }
}
