using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using StudentIDE.Utils;
using StudentIDE.Controllers;

namespace StudentIDE.ViewModels
{

    // ViewModel principal de la ventana de trabajo.

    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ArchivoController _fileService;
        private readonly InterpretePythonController _pythonRunnerService;

        private readonly FirmaDigitalController _signService;

        public MainViewModel()
        {
            _fileService = new ArchivoController();
            _pythonRunnerService = new InterpretePythonController();
            GuardarCommand = new AtajoTeclado(Guardar);
            AbrirCommand = new AtajoTeclado(Abrir);
            EjecutarCommand = new AtajoTeclado(Ejecutar);
            EnviarTerminalCommand = new AtajoTeclado(EnviarTerminal);
            MensajeEstado = "Listo";
            _signService = new FirmaDigitalController();
        }

        // Contenido del editor ────────────────────────────────

        private string _codigoActual = "# Bienvenido a Crystal IDE\n# Abre o crea un archivo para comenzar";
        public string CodigoActual
        {
            get => _codigoActual;
            set { _codigoActual = value; OnPropertyChanged(); }
        }

        //Barra de estado ─────────────────────────────────────

        private string _mensajeEstado = "Listo";
        public string MensajeEstado
        {
            get => _mensajeEstado;
            set { _mensajeEstado = value; OnPropertyChanged(); }
        }

        // Comando Guardar ─────────────────────────────────────

        public ICommand GuardarCommand { get; }
        public ICommand AbrirCommand { get; }
        public ICommand EjecutarCommand { get; }
        public ICommand EnviarTerminalCommand { get; }

        private void Guardar()
        {
          
            var dialogo = new SaveFileDialog
        {
            Title       = "Guardar archivo",
            FileName    = "archivo",        
            DefaultExt  = ".py",
            Filter      = "Archivo de Python (*.py)|*.py|Todos los archivos (*.*)|*.*",
            FilterIndex = 1
        };

         
            bool? resultado = dialogo.ShowDialog();
            if (resultado != true)
                return;

            string rutaElegida = dialogo.FileName;


            bool exitoso = _fileService.Guardar(rutaElegida, CodigoActual);

   
            MensajeEstado = exitoso
                ? $"Archivo guardado  |  {rutaElegida}"
                : "Error: no se pudo guardar el archivo.";


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

        private void Abrir()
        {
            var dialogo = new OpenFileDialog
            {
                Title = "Abrir archivo",
                DefaultExt = ".py",
                Filter = "Archivos Python (*.py)|*.py|Todos los archivos (*.*)|*.*",
                FilterIndex = 1
            };

            if (dialogo.ShowDialog() == true)
            {
                string ruta = dialogo.FileName;
                CodigoActual = File.ReadAllText(ruta);
                if (_signService.VerificarFirma(ruta, CodigoActual)) {
                    MensajeEstado = $"Archivo abierto | {ruta}";
                } else {
                    CodigoActual = "# Bienvenido a Crystal IDE\n# Abre o crea un archivo para comenzar";
                    MessageBox.Show(
                    $"No se pudo abrir el archivo porque las firmas no coinciden",
                    "Error al abrir archivo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                
                );
                }
            }

        }

        private async void Ejecutar()
        {
            if (MensajeEstado == "Ejecutando...") 
            {
                _pythonRunnerService.Detener();
                MensajeEstado = "Ejecución detenida.";
                return;
            }

            MensajeEstado = "Ejecutando...";
            SalidaTerminal = "";

            string tempFile = Path.Combine(Path.GetTempPath(), "studentIDE_temp.py");
            File.WriteAllText(tempFile, CodigoActual);

            await _pythonRunnerService.IniciarInteractivaAsync(
                tempFile,
                output => 
                {
                    Application.Current.Dispatcher.Invoke(() => 
                    {
                        SalidaTerminal += output;
                    });
                },
                exitCode => 
                {
                    Application.Current.Dispatcher.Invoke(() => 
                    {
                        if (exitCode == 0)
                            MensajeEstado = "Ejecución finalizada con éxito.";
                        else
                            MensajeEstado = $"Error de ejecución (Código {exitCode})";
                    });
                }
            );
        }

        private string _salidaTerminal = "";
        public string SalidaTerminal
        {
            get => _salidaTerminal;
            set { _salidaTerminal = value; OnPropertyChanged(); }
        }

        private string _inputTerminal = "";
        public string InputTerminal
        {
            get => _inputTerminal;
            set { _inputTerminal = value; OnPropertyChanged(); }
        }

        private void EnviarTerminal()
        {
            if (!string.IsNullOrEmpty(InputTerminal))
            {
                SalidaTerminal += InputTerminal + Environment.NewLine;
                _pythonRunnerService.EscribirInput(InputTerminal);
                InputTerminal = "";
            }
        }

        //Lista de tareas
        // public ObservableCollection<Tarea> Tareas{
        // get;
        // set; } = new();

        // ── INotifyPropertyChanged ──────────────────────────────────────
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? nombre = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombre));
    }
}
