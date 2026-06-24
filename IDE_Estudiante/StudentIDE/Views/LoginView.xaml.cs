using System;
using System.Threading.Tasks;
using System.Windows;
using StudentIDE.Controllers;
using StudentIDE.Models;
using StudentIDE.Services;

namespace StudentIDE.Views
{
    public partial class LoginView : Window
    {
        private readonly AuthController _authController;
        public Usuario? UsuarioLogueado { get; private set; }
        public bool LoginExitoso { get; private set; }

        public LoginView(ApiService api)
        {
            InitializeComponent();
            _authController = new AuthController(api);
        }

        private async void IniciarSesion_Click(object sender, RoutedEventArgs e)
        {
            var email = EmailBox.Text.Trim();
            var password = PasswordBox.Password;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MostrarError("Debe ingresar correo y contraseña");
                return;
            }

            IniciarSesionBtn.IsEnabled = false;
            IniciarSesionBtn.Content = "Conectando...";
            OcultarError();

            try
            {
                var usuario = await _authController.LoginAsync(email, password);
                UsuarioLogueado = usuario;
                LoginExitoso = true;
                DialogResult = true;
                Close();
            }
            catch (ApiException ex)
            {
                MostrarError(ex.Message);
            }
            catch (Exception ex)
            {
                MostrarError("Error de conexión: " + ex.Message);
            }
            finally
            {
                IniciarSesionBtn.IsEnabled = true;
                IniciarSesionBtn.Content = "Iniciar sesión";
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
        }

        private void OcultarError()
        {
            MensajeError.Visibility = Visibility.Collapsed;
        }
    }
}
