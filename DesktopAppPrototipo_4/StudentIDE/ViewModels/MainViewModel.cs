using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using StudentIDE.Helpers;
using StudentIDE.Services;

namespace StudentIDE.ViewModels
{
    /// <summary>
    /// ViewModel principal de la ventana de trabajo.
    ///
    /// Implementado:
    ///   - RF-05: CodigoActual enlazado al editor
    ///   - RF-11: GuardarCommand (Ctrl+S y botón Guardar)
    ///            Abre diálogo de Windows para elegir ubicación,
    ///            muestra MessageBox de éxito o error,
    ///            actualiza barra de estado con la ruta guardada.
    ///
    /// Pendiente:
    ///   - RF-12: comando Ejecutar script
    ///   - RF-19: lógica de bloqueo de pegado
    ///   - RF-06: lista de tareas del estudiante
    ///   - RF-22: comando Entregar Tarea
    ///   - RF-14: verificación de firma digital
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly FileService _fileService;

        public MainViewModel()
        {
            _fileService = new FileService();
            GuardarCommand = new RelayCommand(Guardar);
            MensajeEstado = "Listo";
        }

        // ── RF-05: Contenido del editor ────────────────────────────────

        private string _codigoActual = "# Bienvenido a StudentIDE\n# Abre o crea un archivo para comenzar";
        public string CodigoActual
        {
            get => _codigoActual;
            set { _codigoActual = value; OnPropertyChanged(); }
        }

        // ── RF-11: Barra de estado ─────────────────────────────────────

        private string _mensajeEstado = "Listo";
        public string MensajeEstado
        {
            get => _mensajeEstado;
            set { _mensajeEstado = value; OnPropertyChanged(); }
        }

        // ── RF-11: Comando Guardar ─────────────────────────────────────

        public ICommand GuardarCommand { get; }

        private void Guardar()
        {
            // 1. Abrir diálogo de guardado de Windows
            var dialogo = new SaveFileDialog
            {
                Title       = "Guardar archivo",
                FileName    = "archivo",          // nombre sugerido
                DefaultExt  = ".tmp",             // TODO RF-08: cambiar a .py al oficializar
                Filter      = "Archivo temporal (*.tmp)|*.tmp|Todos los archivos (*.*)|*.*",
                FilterIndex = 1
            };

            // 2. Si el usuario cancela el diálogo, no hacer nada
            bool? resultado = dialogo.ShowDialog();
            if (resultado != true)
                return;

            string rutaElegida = dialogo.FileName;

            // 3. Intentar guardar en la ruta elegida
            bool exitoso = _fileService.Guardar(rutaElegida, CodigoActual);

            // 4. Actualizar barra de estado
            MensajeEstado = exitoso
                ? $"Archivo guardado  |  {rutaElegida}"
                : "Error: no se pudo guardar el archivo.";

            // 5. Mostrar diálogo de confirmación
            if (exitoso)
            {
                MessageBox.Show(
                    $"El archivo fue guardado correctamente en:\n\n{rutaElegida}",
                    "Guardado exitoso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            else
            {
                MessageBox.Show(
                    $"No se pudo guardar el archivo en:\n\n{rutaElegida}\n\n" +
                    "Verifique que tenga permisos de escritura en esa ubicación.",
                    "Error al guardar",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // TODO RF-06: Lista de tareas
        // public ObservableCollection<Tarea> Tareas { get; set; } = new();

        // ── INotifyPropertyChanged ──────────────────────────────────────
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? nombre = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombre));
    }
}
