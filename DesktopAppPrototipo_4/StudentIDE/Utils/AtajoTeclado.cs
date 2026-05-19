using System.Windows.Input;

namespace StudentIDE.Utils
{
   
    public class AtajoTeclado : ICommand
    {
        private readonly Action _ejecutar;
        private readonly Func<bool>? _puedeEjecutar;

        public AtajoTeclado(Action ejecutar, Func<bool>? puedeEjecutar = null)
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
