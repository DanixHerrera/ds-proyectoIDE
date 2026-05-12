namespace AcademicIDE.Models
{
    /// <summary>
    /// Representa al usuario autenticado en la sesión actual.
    /// RF-01: Ingreso de estudiantes
    /// Almacena el token JWT activo para las llamadas a la API.
    /// </summary>
    public class Usuario
    {
        public string Carne { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string TokenJWT { get; set; } = string.Empty;
        public RolUsuario Rol { get; set; } = RolUsuario.Estudiante;
    }

    public enum RolUsuario
    {
        Estudiante,
        Profesor
    }
}
