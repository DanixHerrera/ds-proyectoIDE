namespace StudentIDE.Models
{
    public class Usuario
    {
        public int Id { get; set; }
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
