using BankAppWPF.Models;
using BankAppWPF.Services;
using System.Diagnostics;
using System.Net.Http;
using System.Windows;

namespace BankAppWPF.Views
{
    public partial class CustomerDetailWindow : Window
    {
        private readonly ApiClient _apiClient;
        private readonly int _customerId;

        public CustomerDetailWindow(ApiClient apiClient, int customerId)
        {
            InitializeComponent();
            _apiClient = apiClient;
            _customerId = customerId;

            CustomerIdText.Text = $"Customer #{_customerId}";
            Loaded += CustomerDetailWindow_Loaded;
        }

        private async void CustomerDetailWindow_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            StatusMessage.Text = "Loading customer details...";

            try
            {
                var result = await _apiClient
                    .GetAsync<CustomerDetail>(
                        $"api/customers/{_customerId}");

                if (result?.Success != true || result.Data is null)
                {
                    StatusMessage.Text =
                        result?.Message ?? "Customer details could not be loaded.";
                    return;
                }

                FullNameText.Text =
                    $"{result.Data.FirstName} {result.Data.LastName}";
                EmailText.Text = result.Data.Email;
                PhoneText.Text = result.Data.Phone ?? "Not provided";
                AddressText.Text = result.Data.Address;
                CreatedDateText.Text =
                    result.Data.CreatedDate.ToString("dd.MM.yyyy HH:mm");
                ActiveStatusText.Text =
                    result.Data.IsActive ? "Active" : "Inactive";
                StatusMessage.Text = string.Empty;
            }
            catch (HttpRequestException exception)
            {
                Debug.WriteLine(
                    $"[HTTP] Customer-detail request failed: {exception}");
                StatusMessage.Text = "BankApp API could not be reached.";
            }
            catch (Exception exception)
            {
                Debug.WriteLine(
                    $"[HTTP] Customer-detail loading error: {exception}");
                StatusMessage.Text =
                    "An unexpected error occurred while loading customer details.";
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
