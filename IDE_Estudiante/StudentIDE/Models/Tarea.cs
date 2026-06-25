namespace StudentIDE.Models
{
   
    public class Tarea
    {
        public string Id { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string CursoNombre { get; set; } = string.Empty;
        public string Instrucciones { get; set; } = string.Empty;
        public DateTime FechaLimite { get; set; }
        public EstadoTarea Estado { get; set; } = EstadoTarea.Pendiente;
        public bool TieneArchivo { get; set; }
        public string NombreArchivo { get; set; } = string.Empty;
        public long TamanoArchivo { get; set; }
    }

    public enum EstadoTarea
    {
        Pendiente,
        Entregada,
        Vencida
    }
}
