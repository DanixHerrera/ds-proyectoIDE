using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AcademicIDE.ViewModels
{
    /// <summary>
    /// ViewModel principal de la ventana de trabajo.
    /// 
    /// Aquí se conectarán progresivamente:
    ///   - RF-05: estado del editor (archivo activo, contenido)
    ///   - RF-11: comando Guardar
    ///   - RF-12: comando Ejecutar script
    ///   - RF-19: lógica de bloqueo de pegado
    ///   - RF-06: lista de tareas del estudiante
    ///   - RF-22: comando Entregar Tarea
    ///   - RF-14: verificación de firma digital
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        // ── Propiedades enlazadas a la View ────────────────────────────

        private string _tituloVentana = "AcademicIDE";
        public string TituloVentana
        {
            get => _tituloVentana;
            set { _tituloVentana = value; OnPropertyChanged(); }
        }

        // TODO RF-05: Contenido del editor
        // private string _codigoActual = string.Empty;
        // public string CodigoActual { get => _codigoActual; set { _codigoActual = value; OnPropertyChanged(); } }

        // TODO RF-06: Lista de tareas
        // public ObservableCollection<Tarea> Tareas { get; set; } = new();

        // ── INotifyPropertyChanged ──────────────────────────────────────
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? nombre = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombre));
    }
}
