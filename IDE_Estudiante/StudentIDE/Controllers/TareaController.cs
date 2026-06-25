using StudentIDE.Models;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;


namespace StudentIDE.Controllers
{
    public class TareaController
    {
        private readonly HttpClient _http;
        public TareaController()
        {
            _http = new HttpClient();
        }
        public async Task<List<Tarea>> ObtenerTareasAsync()
        {
            string json = await _http.GetStringAsync("https://localhost/backend-api/tasks");
            return JsonSerializer.Deserialize<List<Tarea>>(json)!;
        }
    }
}
