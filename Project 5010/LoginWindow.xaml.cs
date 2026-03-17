using System.Windows;
using Project_5010.Services;

namespace Project_5010
{
    public partial class LoginWindow : Window
    {
        private readonly AuthService authService = new();

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text ?? "";
            string password = PasswordBox.Password ?? "";

            if (authService.Login(username, password, out string message))
            {
                // Open main app for this user
                var main = new MainWindow(username);
                main.Show();
                Close();
            }
            else
            {
                StatusText.Text = message;
            }
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text ?? "";
            string password = PasswordBox.Password ?? "";

            bool ok = authService.Register(username, password, out string message);
            StatusText.Text = message;

            if (ok)
            {
                // optional: clear password
                PasswordBox.Clear();
            }
        }
    }
}
