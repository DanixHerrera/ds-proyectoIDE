using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudentIDE.Services
{
    public class ApiService
    {
        private readonly HttpClient _client;

        public string? Token { get; private set; }

        public ApiService(string baseUrl, int timeoutSeconds = 30)
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
            _client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            _client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public void SetToken(string token)
        {
            Token = token;
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        public void ClearToken()
        {
            Token = null;
            _client.DefaultRequestHeaders.Authorization = null;
        }

        public async Task<JsonDocument> PostAsync(string endpoint, object body)
        {
            var json = JsonSerializer.Serialize(body, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(endpoint, content);
            await EnsureSuccess(response);
            return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        }

        public async Task<JsonDocument> GetAsync(string endpoint)
        {
            var response = await _client.GetAsync(endpoint);
            await EnsureSuccess(response);
            return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        }

        public async Task<JsonDocument> PutAsync(string endpoint, object body)
        {
            var json = JsonSerializer.Serialize(body, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PutAsync(endpoint, content);
            await EnsureSuccess(response);
            return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        }

        public async Task DeleteAsync(string endpoint)
        {
            var response = await _client.DeleteAsync(endpoint);
            await EnsureSuccess(response);
        }

        private static async Task EnsureSuccess(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                string mensaje;
                try
                {
                    var doc = JsonDocument.Parse(body);
                    mensaje = doc.RootElement.TryGetProperty("mensaje", out var m)
                        ? m.GetString() ?? "Error del servidor"
                        : $"HTTP {(int)response.StatusCode}";
                }
                catch
                {
                    mensaje = $"HTTP {(int)response.StatusCode}";
                }
                throw new ApiException(mensaje, (int)response.StatusCode);
            }
        }
    }

    public class ApiException : Exception
    {
        public int StatusCode { get; }
        public ApiException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
