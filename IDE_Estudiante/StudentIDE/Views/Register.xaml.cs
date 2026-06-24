using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using StudentIDE.Controllers;
using StudentIDE.Models;
using StudentIDE.Services;

namespace StudentIDE.Views
{
    public partial class RegisterView : Window
    {
        private readonly AuthController _authController;
        public bool RegistroExitoso { get; private set; }

        public RegisterView(ApiService api)
        {
            InitializeComponent();
            _authController = new AuthController(api);
        }

        private async void Registrarse_Click(object sender, RoutedEventArgs e)
        {
            var nombre = NombreBox.Text.Trim();
            var email = EmailBox.Text.Trim();
            var password = PasswordBox.Password;
            var carne = CarneBox.Text.Trim();

            if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MostrarError("Debe completar nombre, correo y contraseña");
                return;
            }

            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MostrarError("Correo electrónico no válido");
                return;
            }

            if (password.Length < 8)
            {
                MostrarError("La contraseña debe tener al menos 8 caracteres");
                return;
            }

            RegistrarseBtn.IsEnabled = false;
            RegistrarseBtn.Content = "Registrando...";
            OcultarMensajes();

            try
            {
                await _authController.RegisterAsync(nombre, email, password, carne, "estudiante");
                MostrarExito("Cuenta creada exitosamente. Ahora puede iniciar sesión.");
                RegistrarseBtn.Content = "¡Cuenta creada!";
            }
            catch (ApiException ex)
            {
                MostrarError(ex.Message);
                RegistrarseBtn.IsEnabled = true;
                RegistrarseBtn.Content = "Registrarse";
            }
            catch (Exception ex)
            {
                MostrarError("Error de conexión: " + ex.Message);
                RegistrarseBtn.IsEnabled = true;
                RegistrarseBtn.Content = "Registrarse";
            }
        }

        private void Volver_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void MostrarError(string mensaje)
        {
            MensajeError.Text = mensaje;
            MensajeError.Visibility = Visibility.Visible;
            MensajeExito.Visibility = Visibility.Collapsed;
        }

        private void MostrarExito(string mensaje)
        {
            MensajeExito.Text = mensaje;
            MensajeExito.Visibility = Visibility.Visible;
            MensajeError.Visibility = Visibility.Collapsed;
        }

        private void OcultarMensajes()
        {
            MensajeError.Visibility = Visibility.Collapsed;
            MensajeExito.Visibility = Visibility.Collapsed;
        }
    }
}
