using System.Windows;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;
using StudentIDE.ViewModels;

namespace StudentIDE.Views
{
    /// <summary>
    /// Code-behind de la ventana principal.
    ///
    /// AvalonEdit no expone su propiedad Text como DependencyProperty estándar
    /// de WPF, por lo que no admite {Binding} directo desde XAML.
    /// La sincronización entre el editor y el ViewModel se hace aquí:
    ///
    ///   - Al cargar la ventana: ViewModel → Editor (texto inicial)
    ///   - Al escribir en el editor: Editor → ViewModel (evento TextChanged)
    ///   - Al guardar: el ViewModel ya tiene el texto actualizado en CodigoActual
    /// </summary>
    public partial class MainView : Window
    {
        private readonly MainViewModel _viewModel;

        public MainView()
        {
            InitializeComponent();

            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            // RF-11: Ctrl+S activo a nivel de ventana,
            // incluso cuando el foco está dentro del editor AvalonEdit.
            InputBindings.Add(new KeyBinding(
                _viewModel.GuardarCommand,
                new KeyGesture(Key.S, ModifierKeys.Control)
            ));

            // Cargar texto inicial del ViewModel en el editor
            CodeEditor.Text = _viewModel.CodigoActual;

            // Cada vez que el usuario escriba en el editor,
            // actualizar CodigoActual en el ViewModel.
            CodeEditor.TextChanged += OnEditorTextChanged;
        }

        /// <summary>
        /// Mantiene CodigoActual en el ViewModel sincronizado
        /// con lo que el usuario escribe en el editor AvalonEdit.
        /// </summary>
        private void OnEditorTextChanged(object? sender, EventArgs e)
        {
            // Evitar ciclo: solo actualizar si el contenido realmente difiere
            if (_viewModel.CodigoActual != CodeEditor.Text)
                _viewModel.CodigoActual = CodeEditor.Text;
        }
    }
}
