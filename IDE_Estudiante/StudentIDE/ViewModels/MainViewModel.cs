using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using StudentIDE.Controllers;
using StudentIDE.Models;
using StudentIDE.Utils;

namespace StudentIDE.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ArchivoController _fileService;
        private readonly InterpretePythonController _pythonRunnerService;
        private TareaController _taskService;
        private readonly FirmaDigitalController _signService;

        public ObservableCollection<Tarea> Tareas { get; } = new();
        public ObservableCollection<ProjectItem> ProjectRoots { get; } = new();
        public ObservableCollection<EditorTab> OpenTabs { get; } = new();

        public MainViewModel()
        {
            _taskService = new TareaController();
            _fileService = new ArchivoController();
            _pythonRunnerService = new InterpretePythonController();
            _signService = new FirmaDigitalController();

            GuardarCommand = new AtajoTeclado(Guardar);
            AbrirCommand = new AtajoTeclado(Abrir);
            EjecutarCommand = new AtajoTeclado(Ejecutar);
            EnviarTerminalCommand = new AtajoTeclado(EnviarTerminal);
            CerrarDetalleCommand = new AtajoTeclado(CerrarDetalle);
            DescargarArchivoCommand = new AtajoTeclado(DescargarArchivo);
            DescargarEntregaCommand = new AtajoTeclado(DescargarEntrega);
            SubirSolucionCommand = new AtajoTeclado(SubirSolucion);
            AbrirProyectoCommand = new AtajoTeclado(AbrirProyecto);
            CerrarProyectoCommand = new AtajoTeclado(CerrarProyecto);
            NuevoArchivoCommand = new AtajoTeclado(NuevoArchivo);
            NuevaCarpetaCommand = new AtajoTeclado(NuevaCarpeta);
            MensajeEstado = "Listo";
        }

        // ── Editor tabs ──────────────────────────────────────

        private EditorTab? _selectedTab;
        public EditorTab? SelectedTab
        {
            get => _selectedTab;
            set { _selectedTab = value; OnPropertyChanged(); OnPropertyChanged(nameof(TieneTabActivo)); }
        }

        public bool TieneTabActivo => SelectedTab != null;

        public EditorTab? FindOpenTab(string path) =>
            OpenTabs.FirstOrDefault(t => t.FilePath.Equals(path, StringComparison.OrdinalIgnoreCase));

        public EditorTab? AbrirArchivo(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return null;

            var existing = FindOpenTab(path);
            if (existing != null)
            {
                SelectedTab = existing;
                return existing;
            }

            if (!_signService.VerificarFirma(path, File.ReadAllText(path)))
            {
                MessageBox.Show(
                    "No se pudo abrir el archivo porque las firmas no coinciden.\n\n" +
                    "El archivo ha sido modificado fuera del IDE.",
                    "Error de firma digital",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                MensajeEstado = $"Error de firma: {path}";
                return null;
            }

            var content = File.ReadAllText(path);
            var newTab = new EditorTab { FilePath = path, Content = content };
            OpenTabs.Add(newTab);
            SelectedTab = newTab;
            MensajeEstado = $"Abierto: {path}";
            return newTab;
        }

        public void CerrarTab(EditorTab tab)
        {
            if (tab == null) return;

            if (tab.IsModified)
            {
                var result = MessageBox.Show(
                    $"¿Guardar cambios en {tab.FileName}?",
                    "Archivo modificado",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);
                if (result == MessageBoxResult.Cancel) return;
                if (result == MessageBoxResult.Yes)
                {
                    _fileService.Guardar(tab.FilePath, tab.Content);
                    tab.IsModified = false;
                }
            }

            var idx = OpenTabs.IndexOf(tab);
            OpenTabs.Remove(tab);

            if (OpenTabs.Count > 0)
                SelectedTab = OpenTabs[Math.Min(idx, OpenTabs.Count - 1)];
            else
                SelectedTab = null;
        }

        // ── Project explorer ──────────────────────────────────

        public ICommand AbrirProyectoCommand { get; }
        public ICommand CerrarProyectoCommand { get; }

        private void AbrirProyecto()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Seleccionar carpeta del proyecto"
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                CargarProyecto(dialog.SelectedPath);
        }

        private void CargarProyecto(string folderPath)
        {
            ProjectRoots.Clear();
            var root = new ProjectItem
            {
                Name = Path.GetFileName(folderPath),
                FullPath = folderPath,
                IsDirectory = true
            };
            ScanDirectory(root);
            ProjectRoots.Add(root);
            MensajeEstado = $"Proyecto: {folderPath}";

            // Abrir todos los .py de la raiz automaticamente
            foreach (var child in root.Children)
            {
                if (!child.IsDirectory)
                    AbrirArchivo(child.FullPath);
            }
        }

        private static void ScanDirectory(ProjectItem dir)
        {
            try
            {
                foreach (var d in Directory.GetDirectories(dir.FullPath))
                {
                    var item = new ProjectItem
                    {
                        Name = Path.GetFileName(d),
                        FullPath = d,
                        IsDirectory = true
                    };
                    ScanDirectory(item);
                    dir.Children.Add(item);
                }
                foreach (var f in Directory.GetFiles(dir.FullPath, "*.py"))
                {
                    dir.Children.Add(new ProjectItem
                    {
                        Name = Path.GetFileName(f),
                        FullPath = f,
                        IsDirectory = false
                    });
                }
            }
            catch { }
        }

        public ICommand NuevoArchivoCommand { get; }
        public ICommand NuevaCarpetaCommand { get; }

        private string? ProjectRootPath
        {
            get
            {
                var root = ProjectRoots.FirstOrDefault();
                return root?.FullPath;
            }
        }

        private void NuevoArchivo()
        {
            try
            {
                var rootPath = ProjectRootPath;
                if (rootPath == null) { MensajeEstado = "Abre un proyecto primero."; return; }

                var name = Prompt("Nombre del archivo:", "Nuevo archivo", "nuevo.py");
                if (name == null) return;
                if (!name.EndsWith(".py")) name += ".py";

                var path = Path.Combine(rootPath, name);
                if (File.Exists(path)) { MensajeEstado = "El archivo ya existe."; return; }

                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                File.WriteAllText(path, "# " + name);
                _fileService.Guardar(path, "# " + name);
                RecargarProyecto();
                AbrirArchivo(path);
                MensajeEstado = $"Creado: {path}";
            }
            catch (Exception ex)
            {
                MensajeEstado = $"Error al crear archivo: {ex.Message}";
            }
        }

        private void NuevaCarpeta()
        {
            try
            {
                var rootPath = ProjectRootPath;
                if (rootPath == null) { MensajeEstado = "Abre un proyecto primero."; return; }

                var name = Prompt("Nombre de la carpeta:", "Nueva carpeta", "nueva_carpeta");
                if (name == null) return;

                var path = Path.Combine(rootPath, name);
                if (Directory.Exists(path)) { MensajeEstado = "La carpeta ya existe."; return; }

                Directory.CreateDirectory(path);
                RecargarProyecto();
                MensajeEstado = $"Carpeta creada: {path}";
            }
            catch (Exception ex)
            {
                MensajeEstado = $"Error al crear carpeta: {ex.Message}";
            }
        }

        private void RecargarProyecto()
        {
            var root = ProjectRoots.FirstOrDefault();
            if (root == null) return;
            var path = root.FullPath;
            ProjectRoots.Clear();
            var newRoot = new ProjectItem { Name = Path.GetFileName(path), FullPath = path, IsDirectory = true };
            ScanDirectory(newRoot);
            ProjectRoots.Add(newRoot);
        }

        private void CerrarProyecto()
        {
            ProjectRoots.Clear();
            OpenTabs.Clear();
            MensajeEstado = "Proyecto cerrado.";
        }

        private static string? Prompt(string message, string title, string defaultValue)
        {
            var window = new Window
            {
                Title = title,
                Width = 380,
                Height = 160,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.ToolWindow,
                Background = new SolidColorBrush(Color.FromRgb(0x2D, 0x2D, 0x2D)),
                Foreground = Brushes.White,
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI")
            };

            var stack = new StackPanel { Margin = new Thickness(16) };
            stack.Children.Add(new TextBlock
            {
                Text = message,
                Margin = new Thickness(0, 0, 0, 10),
                FontSize = 13
            });

            var textBox = new TextBox
            {
                Text = defaultValue,
                FontSize = 13,
                Padding = new Thickness(4),
                Background = new SolidColorBrush(Color.FromRgb(0x3C, 0x3C, 0x3C)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55)),
                BorderThickness = new Thickness(1),
                SelectionBrush = new SolidColorBrush(Color.FromRgb(0x00, 0x7A, 0xCC))
            };
            textBox.SelectAll();
            stack.Children.Add(textBox);

            var btnPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 12, 0, 0)
            };

            var okBtn = new Button
            {
                Content = "Aceptar",
                Width = 80,
                Height = 28,
                Margin = new Thickness(0, 0, 8, 0),
                Background = new SolidColorBrush(Color.FromRgb(0x3C, 0x3C, 0x3C)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55)),
                BorderThickness = new Thickness(1),
                Cursor = Cursors.Hand,
                IsDefault = true
            };
            var cancelBtn = new Button
            {
                Content = "Cancelar",
                Width = 80,
                Height = 28,
                Background = new SolidColorBrush(Color.FromRgb(0x3C, 0x3C, 0x3C)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55)),
                BorderThickness = new Thickness(1),
                Cursor = Cursors.Hand,
                IsCancel = true
            };

            okBtn.Click += (_, _) => { window.DialogResult = true; window.Close(); };
            cancelBtn.Click += (_, _) => { window.DialogResult = false; window.Close(); };
            textBox.KeyDown += (_, e) => { if (e.Key == Key.Enter) { window.DialogResult = true; window.Close(); } };

            btnPanel.Children.Add(okBtn);
            btnPanel.Children.Add(cancelBtn);
            stack.Children.Add(btnPanel);
            window.Content = stack;
            window.Loaded += (_, _) => textBox.Focus();

            return window.ShowDialog() == true ? textBox.Text.Trim() : null;
        }

        // ── File commands ─────────────────────────────────────

        public ICommand GuardarCommand { get; }
        public ICommand AbrirCommand { get; }
        public ICommand EjecutarCommand { get; }

        private void Guardar()
        {
            if (SelectedTab == null)
            {
                MensajeEstado = "No hay archivo abierto para guardar.";
                return;
            }

            if (_fileService.Guardar(SelectedTab.FilePath, SelectedTab.Content))
            {
                SelectedTab.IsModified = false;
                MensajeEstado = $"Archivo guardado | {SelectedTab.FilePath}";
            }
            else
            {
                MensajeEstado = "Error: no se pudo guardar el archivo.";
            }
        }

        private void Abrir()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Abrir archivo",
                DefaultExt = ".py",
                Filter = "Archivos Python (*.py)|*.py|Todos los archivos (*.*)|*.*",
                FilterIndex = 1
            };

            if (dialog.ShowDialog() == true)
                AbrirArchivo(dialog.FileName);
        }

        // ── Execution ─────────────────────────────────────────

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
            File.WriteAllText(tempFile, SelectedTab?.Content ?? "");

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

        // ── Terminal ──────────────────────────────────────────

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

        public ICommand EnviarTerminalCommand { get; }

        private void EnviarTerminal()
        {
            if (!string.IsNullOrEmpty(InputTerminal))
            {
                SalidaTerminal += InputTerminal + Environment.NewLine;
                _pythonRunnerService.EscribirInput(InputTerminal);
                InputTerminal = "";
            }
        }

        // ── Status bar ────────────────────────────────────────

        private string _mensajeEstado = "Listo";
        public string MensajeEstado
        {
            get => _mensajeEstado;
            set { _mensajeEstado = value; OnPropertyChanged(); }
        }

        // ── Task detail ───────────────────────────────────────

        private Tarea? _tareaSeleccionada;
        public Tarea? TareaSeleccionada
        {
            get => _tareaSeleccionada;
            set
            {
                _tareaSeleccionada = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MostrarDetalleTarea));
            }
        }

        public bool MostrarDetalleTarea => TareaSeleccionada != null;

        public ICommand CerrarDetalleCommand { get; }
        public ICommand DescargarArchivoCommand { get; }
        public ICommand DescargarEntregaCommand { get; }
        public ICommand SubirSolucionCommand { get; }

        private void CerrarDetalle()
        {
            TareaSeleccionada = null;
        }

        private async void DescargarArchivo()
        {
            if (TareaSeleccionada == null || !TareaSeleccionada.TieneArchivo) return;
            try
            {
                MensajeEstado = "Descargando archivo adjunto...";
                await _taskService.DescargarArchivoAsync(TareaSeleccionada);
                MensajeEstado = "Archivo descargado correctamente.";
            }
            catch (Exception ex)
            {
                MensajeEstado = $"Error al descargar: {ex.Message}";
            }
        }

        private async void DescargarEntrega()
        {
            if (TareaSeleccionada == null || !TareaSeleccionada.TieneEntrega) return;
            try
            {
                MensajeEstado = "Descargando solución...";
                await _taskService.DescargarEntregaAsync(TareaSeleccionada);
                MensajeEstado = "Solución descargada correctamente.";
            }
            catch (Exception ex)
            {
                MensajeEstado = $"Error al descargar: {ex.Message}";
            }
        }

        private async void SubirSolucion()
        {
            if (TareaSeleccionada == null) return;

            var dialog = new OpenFileDialog
            {
                Title = "Seleccionar archivo de solución",
                DefaultExt = ".py",
                Filter = "Archivos Python (*.py)|*.py|Todos los archivos (*.*)|*.*",
                FilterIndex = 1,
            };

            if (dialog.ShowDialog() != true) return;

            var confirm = MessageBox.Show(
                $"¿Estás seguro de que deseas entregar \"{Path.GetFileName(dialog.FileName)}\" como solución de \"{TareaSeleccionada.Titulo}\"?\n\n" +
                $"La entrega puede realizarse múltiples veces; se guardará un historial de versiones.",
                "Confirmar entrega",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                MensajeEstado = "Subiendo solución...";
                var tareaId = int.Parse(TareaSeleccionada.Id);
                var exito = await _taskService.SubirSolucionAsync(tareaId, dialog.FileName);

                if (exito)
                {
                    MensajeEstado = "Solución entregada correctamente.";
                    var idTareaReSeleccion = TareaSeleccionada.Id;
                    await CargarTareasAsync();
                    TareaSeleccionada = Tareas.FirstOrDefault(t => t.Id == idTareaReSeleccion);
                }
                else
                {
                    MensajeEstado = "Error al subir la solución.";
                }
            }
            catch (Exception ex)
            {
                MensajeEstado = $"Error al subir solución: {ex.Message}";
            }
        }

        // ── Task list ─────────────────────────────────────────

        public async Task CargarTareasAsync()
        {
            var tareas = await _taskService.ObtenerTareasAsync();
            Tareas.Clear();
            foreach (var tarea in tareas)
                Tareas.Add(tarea);
        }

        // ── INotifyPropertyChanged ─────────────────────────────

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
