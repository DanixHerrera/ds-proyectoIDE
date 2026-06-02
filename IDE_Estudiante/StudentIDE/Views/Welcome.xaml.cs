using System.Windows;

namespace StudentIDE.Views
{
    public partial class WelcomeView : Window
    {
        public WelcomeView()
        {
            InitializeComponent();
        }

        private void RegistrarseBtn_Click(object sender, RoutedEventArgs e) {
            RegisterView r = new RegisterView();
            r.Show();
            this.Close();
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            LoginView l = new LoginView();
            l.Show();
            this.Close();
        }



    }
}
