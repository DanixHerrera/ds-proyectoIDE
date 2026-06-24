using System;
using System.Text.Json;
using System.Threading.Tasks;
using StudentIDE.Models;
using StudentIDE.Services;

namespace StudentIDE.Controllers
{
    public class AuthController
    {
        private readonly ApiService _api;

        public AuthController(ApiService api)
        {
            _api = api;
        }

        public async Task<Usuario> LoginAsync(string email, string password)
        {
            var result = await _api.PostAsync("auth/login", new
            {
                email,
                password
            });

            var root = result.RootElement;
            var token = root.GetProperty("token").GetString()
                ?? throw new Exception("No se recibió token");
            var usuario = root.GetProperty("usuario");
            var rol = root.GetProperty("rol").GetString() ?? "estudiante";

            var user = new Usuario
            {
                Id = usuario.GetProperty("id").GetInt32(),
                NombreCompleto = usuario.GetProperty("name").GetString() ?? "",
                Email = usuario.GetProperty("email").GetString() ?? "",
                Carne = usuario.TryGetProperty("carne", out var c) ? c.GetString() ?? "" : "",
                TokenJWT = token,
                Rol = rol == "profesor" ? RolUsuario.Profesor : RolUsuario.Estudiante
            };

            _api.SetToken(token);
            return user;
        }

        public async Task<Usuario> RegisterAsync(string name, string email, string password, string carne, string role = "estudiante")
        {
            var result = await _api.PostAsync("auth/registro", new
            {
                name,
                email,
                password,
                role,
                carne
            });

            var root = result.RootElement;

            var user = new Usuario
            {
                Id = root.GetProperty("id").GetInt32(),
                NombreCompleto = root.GetProperty("name").GetString() ?? "",
                Email = root.GetProperty("email").GetString() ?? "",
                Carne = root.TryGetProperty("carne", out var c) ? c.GetString() ?? "" : "",
                Rol = role == "profesor" ? RolUsuario.Profesor : RolUsuario.Estudiante
            };

            return user;
        }
    }
}
