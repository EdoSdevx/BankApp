using BankAppWPF.Services;
using System.Diagnostics;
using System.Net.Http;
using System.Windows;

namespace BankAppWPF.Views
{
    public partial class LoginWindow : Window
    {
        private readonly ApiClient _apiClient = new();

        public LoginWindow()
        {
            InitializeComponent();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var email = EmailTextBox.Text.Trim();
            var password = PasswordInput.Password;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                LoginMessage.Text = "Email and password are required.";
                return;
            }

            LoginButton.IsEnabled = false;
            LoginMessage.Text = "Signing in...";

            try
            {
                var result = await _apiClient.LoginAsync(email, password);

                if (result?.Success != true || result.Data is null)
                {
                    LoginMessage.Text = result?.Message ?? "Login failed.";
                    return;
                }

                if (result.Data.Role is not ("Employee" or "Admin"))
                {
                    LoginMessage.Text =
                        "This operations portal is available only to employees and administrators.";
                    return;
                }

                var dashboard = new MainWindow(_apiClient);
                dashboard.Show();
                Close();
            }
            catch (HttpRequestException exception)
            {
                Debug.WriteLine($"[HTTP] Login request failed: {exception}");
                LoginMessage.Text = "BankApp API could not be reached.";
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"[HTTP] Login error: {exception}");
                LoginMessage.Text = "An unexpected error occurred while signing in.";
            }
            finally
            {
                LoginButton.IsEnabled = true;
            }
        }

        private void ForgotPasswordButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            var forgotPasswordWindow =
                new ForgotPasswordWindow(_apiClient)
                {
                    Owner = this
                };

            forgotPasswordWindow.ShowDialog();
        }
    }
}
