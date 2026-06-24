using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using StudentIDE.Services;
using StudentIDE.Views;

namespace StudentIDE
{
    public partial class App : Application
    {
        public static ApiService Api { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var config = ApiConfig.Load();
            Api = new ApiService(config.ApiBaseUrl, config.Timeout);

            var token = CargarTokenGuardado();
            if (!string.IsNullOrEmpty(token))
            {
                Api.SetToken(token);
                var mainView = new MainView();
                mainView.Show();
            }
            else
            {
                var welcomeView = new WelcomeView();
                welcomeView.Show();
            }
        }

        public static void GuardarToken(string token)
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "StudentIDE");
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, "session.json"),
                JsonSerializer.Serialize(new { token }));
        }

        public static string? CargarTokenGuardado()
        {
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "StudentIDE", "session.json");
            if (!File.Exists(path)) return null;
            try
            {
                var doc = JsonDocument.Parse(File.ReadAllText(path));
                return doc.RootElement.TryGetProperty("token", out var t)
                    ? t.GetString() : null;
            }
            catch
            {
                return null;
            }
        }

        public static void LimpiarToken()
        {
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "StudentIDE", "session.json");
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
