// LoginWindow.cs
// This is the first window the user sees. They can log in or register.
// After registering, new users go through an onboarding setup.
// After logging in, existing users who haven't done onboarding are prompted too.

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
                // Check if the user still needs to do onboarding
                if (!ShowOnboardingIfNeeded(username))
                    return; // user closed onboarding without finishing

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
                // After registering, go straight to onboarding and then the main app
                if (!ShowOnboardingIfNeeded(username))
                    return; // user closed onboarding without finishing

                var main = new MainWindow(username);
                main.Show();
                Close();
            }
        }

        // Shows the onboarding window if the user hasn't completed it yet.
        // Returns true if onboarding is done (or was already done).
        // Returns false if the user closed the onboarding window without finishing.
        private static bool ShowOnboardingIfNeeded(string username)
        {
            var settingsService = new SettingsFileService();
            var settings = settingsService.Load(username);

            if (settings.IsOnboardingComplete)
                return true; // already done, skip it

            var onboarding = new OnboardingWindow(settingsService, username);
            bool? result = onboarding.ShowDialog();

            return result == true;
        }
    }
}
