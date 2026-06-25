using System;
using System.Windows;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;
using StudentIDE.ViewModels;
using StudentIDE.Utils;

namespace StudentIDE.Views
{
  
    public partial class MainView : Window
    {
        private readonly MainViewModel _viewModel;

        public MainView()
        {
            InitializeComponent();

            _viewModel = new MainViewModel();
            DataContext = _viewModel;
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;

            //ctrl+S activo a nivel de ventana,

            InputBindings.Add(new KeyBinding(
                _viewModel.GuardarCommand,
                new KeyGesture(Key.S, ModifierKeys.Control)
            ));

            InputBindings.Add(new KeyBinding(
                _viewModel.AbrirCommand,
                new KeyGesture(Key.O, ModifierKeys.Control)
            ));

            InputBindings.Add(new KeyBinding(
                _viewModel.EjecutarCommand,
                new KeyGesture(Key.F5)
            ));

            CodeEditor.Text = _viewModel.CodigoActual;


            CodeEditor.TextChanged += OnEditorTextChanged;

            BloqueadorClipboard.Attach(CodeEditor);

            Loaded += async (s, e) =>
            {
                try
                {
                    await _viewModel.CargarTareasAsync();
                    _viewModel.MensajeEstado = $"{_viewModel.Tareas.Count} tarea(s) cargada(s)";
                }
                catch
                {
                    _viewModel.MensajeEstado = "Error al cargar tareas";
                }
            };
        }

        //Mantiene CodigoActual en el ViewModel sincronizado con lo que el usuario escribe en el editor AvalonEdit.
        private void OnEditorTextChanged(object? sender, EventArgs e)
        {
            if (_viewModel.CodigoActual != CodeEditor.Text)
                _viewModel.CodigoActual = CodeEditor.Text;
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            App.LimpiarToken();
            App.Api.ClearToken();
            var welcome = new WelcomeView();
            welcome.Show();
            Close();
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.CodigoActual))
            {
                if (CodeEditor.Text != _viewModel.CodigoActual)
                {
                    CodeEditor.Text = _viewModel.CodigoActual;
                }
            }
        }

    }
}
