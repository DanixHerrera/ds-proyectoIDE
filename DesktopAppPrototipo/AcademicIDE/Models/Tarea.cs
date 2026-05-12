namespace AcademicIDE.Models
{
    /// <summary>
    /// Representa una tarea asignada al estudiante.
    /// RF-06: Visualización de tareas
    /// RF-07: Ver detalles de tarea
    /// RF-27: Estado de tarea (Pendiente, Entregada, Vencida)
    /// </summary>
    public class Tarea
    {
        public string Id { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Instrucciones { get; set; } = string.Empty;
        public DateTime FechaLimite { get; set; }
        public EstadoTarea Estado { get; set; } = EstadoTarea.Pendiente;
    }

    public enum EstadoTarea
    {
        Pendiente,
        Entregada,
        Vencida
    }
}
