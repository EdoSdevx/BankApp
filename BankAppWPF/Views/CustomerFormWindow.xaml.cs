using BankAppWPF.Models;
using BankAppWPF.Services;
using System.Diagnostics;
using System.Net.Http;
using System.Windows;

namespace BankAppWPF.Views
{
    public partial class CustomerFormWindow : Window
    {
        private readonly ApiClient _apiClient;
        private readonly int? _customerId;

        public CustomerFormWindow(
            ApiClient apiClient,
            int? customerId = null)
        {
            InitializeComponent();
            _apiClient = apiClient;
            _customerId = customerId;

            ConfigureMode();
        }

        private void ConfigureMode()
        {
            if (_customerId.HasValue)
            {
                Title = "Edit Customer";
                FormTitle.Text = "Edit Customer";
                FormSubtitle.Text =
                    "Update the selected customer's information.";
                PasswordLabel.Text =
                    "New Password (leave blank to keep the current password)";
                SaveButton.Content = "Save Changes";
                Loaded += CustomerFormWindow_Loaded;
                return;
            }

            Title = "Create Customer";
            FormTitle.Text = "Create Customer";
            FormSubtitle.Text =
                "Enter the customer's account information.";
            SaveButton.Content = "Create";
            ActiveCheckBox.Visibility = Visibility.Collapsed;
        }

        private async void CustomerFormWindow_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            SaveButton.IsEnabled = false;
            StatusMessage.Text = "Loading customer...";

            try
            {
                var result = await _apiClient
                    .GetAsync<CustomerDetail>(
                        $"api/customers/{_customerId}");

                if (result?.Success != true || result.Data is null)
                {
                    StatusMessage.Text =
                        result?.Message ?? "Customer could not be loaded.";
                    return;
                }

                FirstNameTextBox.Text = result.Data.FirstName;
                LastNameTextBox.Text = result.Data.LastName;
                EmailTextBox.Text = result.Data.Email;
                PhoneTextBox.Text = result.Data.Phone ?? string.Empty;
                AddressTextBox.Text = result.Data.Address;
                ActiveCheckBox.IsChecked = result.Data.IsActive;
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
                    "An unexpected error occurred while loading the customer.";
            }
            finally
            {
                SaveButton.IsEnabled = true;
            }
        }

        private async void SaveButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            var firstName = FirstNameTextBox.Text.Trim();
            var lastName = LastNameTextBox.Text.Trim();
            var email = EmailTextBox.Text.Trim();
            var phone = string.IsNullOrWhiteSpace(PhoneTextBox.Text)
                ? null
                : PhoneTextBox.Text.Trim();
            var address = AddressTextBox.Text.Trim();
            var password = PasswordInput.Password;

            if (string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(address))
            {
                StatusMessage.Text =
                    "First name, last name, email and address are required.";
                return;
            }

            if (!_customerId.HasValue &&
                string.IsNullOrWhiteSpace(password))
            {
                StatusMessage.Text =
                    "Password is required when creating a customer.";
                return;
            }

            if (!string.IsNullOrEmpty(password) && password.Length < 6)
            {
                StatusMessage.Text =
                    "Password must be at least 6 characters.";
                return;
            }

            SaveButton.IsEnabled = false;
            StatusMessage.Text = _customerId.HasValue
                ? "Saving changes..."
                : "Creating customer...";

            try
            {
                ApiResult<object>? result;

                if (_customerId.HasValue)
                {
                    var request = new CustomerUpdateRequest
                    {
                        CustomerId = _customerId.Value,
                        FirstName = firstName,
                        LastName = lastName,
                        Email = email,
                        Phone = phone,
                        Address = address,
                        IsActive = ActiveCheckBox.IsChecked == true,
                        Password = string.IsNullOrWhiteSpace(password)
                            ? null
                            : password
                    };

                    result = await _apiClient.PutAsync(
                        $"api/customers/{_customerId.Value}",
                        request);
                }
                else
                {
                    var request = new CustomerCreateRequest
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        Email = email,
                        Phone = phone,
                        Address = address,
                        Password = password
                    };

                    result = await _apiClient.PostAsync(
                        "api/customers",
                        request);
                }

                if (result?.Success != true)
                {
                    StatusMessage.Text =
                        result?.Message ?? "Customer could not be saved.";
                    return;
                }

                DialogResult = true;
            }
            catch (HttpRequestException exception)
            {
                Debug.WriteLine(
                    $"[HTTP] Save-customer request failed: {exception}");
                StatusMessage.Text = "BankApp API could not be reached.";
            }
            catch (Exception exception)
            {
                Debug.WriteLine(
                    $"[HTTP] Save-customer error: {exception}");
                StatusMessage.Text =
                    "An unexpected error occurred while saving the customer.";
            }
            finally
            {
                SaveButton.IsEnabled = true;
            }
        }

        private void CancelButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
