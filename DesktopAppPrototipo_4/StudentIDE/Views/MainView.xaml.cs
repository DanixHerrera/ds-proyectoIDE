using System.Windows;
using System.Windows.Input;
using StudentIDE.ViewModels;

namespace StudentIDE.Views
{
    /// <summary>
    /// Code-behind de la ventana principal.
    /// En MVVM este archivo se mantiene lo más vacío posible.
    /// Toda la lógica vive en MainViewModel.
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();
            DataContext = new MainViewModel();

            // RF-11: Ctrl+S activo desde cualquier parte de la ventana,
            // incluso cuando el foco está dentro del TextBox del editor.
            InputBindings.Add(new KeyBinding(
                ((MainViewModel)DataContext).GuardarCommand,
                new KeyGesture(Key.S, ModifierKeys.Control)
            ));
        }
    }
}
