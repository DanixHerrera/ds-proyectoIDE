using System.IO;
using System.Text.Json;

namespace StudentIDE.Services
{
    public class ApiConfig
    {
        public string ApiBaseUrl { get; set; } = "http://localhost:5000/api";
        public int Timeout { get; set; } = 30;

        public static ApiConfig Load()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (!File.Exists(path))
                return new ApiConfig();
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<ApiConfig>(json) ?? new ApiConfig();
        }
    }
}
