using BankAppWPF.Models;
using BankAppWPF.Services;
using System.Diagnostics;
using System.Net.Http;
using System.Windows;
using System.Windows.Media;

namespace BankAppWPF.Views
{
    public partial class ForgotPasswordWindow : Window
    {
        private readonly ApiClient _apiClient;

        public ForgotPasswordWindow(ApiClient apiClient)
        {
            InitializeComponent();
            _apiClient = apiClient;
        }

        private async void SendButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            var email = EmailTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            {
                StatusMessage.Foreground = Brushes.Firebrick;
                StatusMessage.Text = "Enter a valid email address.";
                return;
            }

            var request = new ForgotPasswordRequest
            {
                Email = email
            };

            SendButton.IsEnabled = false;
            StatusMessage.Foreground = Brushes.SlateGray;
            StatusMessage.Text = "Sending reset link...";

            try
            {
                var result = await _apiClient.PostAsync(
                    "api/auth/forgot-password",
                    request);

                if (result?.Success != true)
                {
                    StatusMessage.Foreground = Brushes.Firebrick;
                    StatusMessage.Text =
                        result?.Message ?? "The reset request failed.";
                    return;
                }

                StatusMessage.Foreground = Brushes.ForestGreen;
                StatusMessage.Text = result.Message;
            }
            catch (HttpRequestException exception)
            {
                Debug.WriteLine(
                    $"[HTTP] Forgot-password request failed: {exception}");
                StatusMessage.Foreground = Brushes.Firebrick;
                StatusMessage.Text = "BankApp API could not be reached.";
            }
            catch (Exception exception)
            {
                Debug.WriteLine(
                    $"[HTTP] Forgot-password error: {exception}");
                StatusMessage.Foreground = Brushes.Firebrick;
                StatusMessage.Text =
                    "An unexpected error occurred while requesting a reset link.";
            }
            finally
            {
                SendButton.IsEnabled = true;
            }
        }

        private void BackButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }
    }
}
