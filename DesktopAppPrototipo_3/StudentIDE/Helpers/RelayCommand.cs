<<<<<<< HEAD
using System.Windows.Input;

namespace StudentIDE.Helpers
{
    /// <summary>
    /// Implementación genérica de ICommand para usar en ViewModels (patrón MVVM).
    /// Permite enlazar botones y atajos de teclado de la View a métodos del ViewModel
    /// sin código en el code-behind.
    ///
    /// Uso en ViewModel:
    ///   public ICommand GuardarCommand { get; }
    ///   GuardarCommand = new RelayCommand(Guardar);
    ///
    /// Uso en XAML:
    ///   Command="{Binding GuardarCommand}"
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _ejecutar;
        private readonly Func<bool>? _puedeEjecutar;

        public RelayCommand(Action ejecutar, Func<bool>? puedeEjecutar = null)
        {
            _ejecutar = ejecutar;
            _puedeEjecutar = puedeEjecutar;
        }

        // WPF llama esto para habilitar/deshabilitar el botón automáticamente
        public bool CanExecute(object? parameter)
            => _puedeEjecutar?.Invoke() ?? true;

        public void Execute(object? parameter)
            => _ejecutar();

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
=======
using System.Windows.Input;

namespace StudentIDE.Helpers
{
    /// <summary>
    /// Implementación genérica de ICommand para usar en ViewModels (patrón MVVM).
    /// Permite enlazar botones y atajos de teclado de la View a métodos del ViewModel
    /// sin código en el code-behind.
    ///
    /// Uso en ViewModel:
    ///   public ICommand GuardarCommand { get; }
    ///   GuardarCommand = new RelayCommand(Guardar);
    ///
    /// Uso en XAML:
    ///   Command="{Binding GuardarCommand}"
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _ejecutar;
        private readonly Func<bool>? _puedeEjecutar;

        public RelayCommand(Action ejecutar, Func<bool>? puedeEjecutar = null)
        {
            _ejecutar = ejecutar;
            _puedeEjecutar = puedeEjecutar;
        }

        // WPF llama esto para habilitar/deshabilitar el botón automáticamente
        public bool CanExecute(object? parameter)
            => _puedeEjecutar?.Invoke() ?? true;

        public void Execute(object? parameter)
            => _ejecutar();

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
>>>>>>> funcion: guardar archivo .tmp con dialogo windows
