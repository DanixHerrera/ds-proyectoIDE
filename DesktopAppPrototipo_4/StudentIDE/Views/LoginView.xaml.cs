using System.Windows;

namespace StudentIDE.Views
{
    // RF-01: Se implementará en la siguiente iteración
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainView m = new MainView();
            m.Show();
            this.Close();
        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void OlvideContrasena_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
