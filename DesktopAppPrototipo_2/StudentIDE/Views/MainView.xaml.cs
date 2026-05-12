using System.Windows;
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
        }
    }
}
