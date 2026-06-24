using System.Windows;

namespace StudentIDE.Views
{
    public partial class WelcomeView : Window
    {
        public WelcomeView()
        {
            InitializeComponent();
        }

        private void RegistrarseBtn_Click(object sender, RoutedEventArgs e)
        {
            var r = new RegisterView(App.Api);
            r.ShowDialog();
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            var l = new LoginView(App.Api);
            if (l.ShowDialog() == true && l.LoginExitoso)
            {
                App.GuardarToken(l.UsuarioLogueado!.TokenJWT);
                var main = new MainView();
                main.Show();
                Close();
            }
        }
    }
}
