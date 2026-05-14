namespace StudentIDE.Models
{
    // Representa al usuario autenticado en la sesión actual.
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
