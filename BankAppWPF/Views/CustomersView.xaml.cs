using BankAppWPF.Models;
using BankAppWPF.Services;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;

namespace BankAppWPF.Views
{
    public partial class CustomersView : UserControl
    {
        private readonly ApiClient _apiClient;

        public CustomersView(ApiClient apiClient)
        {
            InitializeComponent();
            _apiClient = apiClient;
            Loaded += CustomersView_Loaded;
        }

        private async void CustomersView_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            await LoadCustomersAsync();
        }

        private async Task LoadCustomersAsync()
        {
            StatusMessage.Text = "Loading customers...";

            try
            {
                var result = await _apiClient
                    .GetAsync<List<CustomerListItem>>("api/customers");

                if (result?.Success != true || result.Data is null)
                {
                    StatusMessage.Text = result?.Message ?? "Customers could not be loaded.";
                    return;
                }

                CustomersGrid.ItemsSource = result.Data;
                StatusMessage.Text = $"{result.Data.Count} customers loaded.";
            }
            catch (HttpRequestException exception)
            {
                Debug.WriteLine($"[HTTP] Customer request failed: {exception}");
                StatusMessage.Text = "BankApp API could not be reached.";
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"[HTTP] Customer loading error: {exception}");
                StatusMessage.Text = "An unexpected error occurred while loading customers.";
            }
        }

        private async void CreateCustomerButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            var createWindow = new CustomerFormWindow(_apiClient)
            {
                Owner = Window.GetWindow(this)
            };

            var customerCreated = createWindow.ShowDialog() == true;

            if (customerCreated)
            {
                await LoadCustomersAsync();
            }
        }

        private async void EditCustomerButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (CustomersGrid.SelectedItem is not
                CustomerListItem selectedCustomer)
            {
                StatusMessage.Text =
                    "Select a customer row before editing.";
                return;
            }

            var editWindow = new CustomerFormWindow(
                _apiClient,
                selectedCustomer.CustomerId)
            {
                Owner = Window.GetWindow(this)
            };

            var customerUpdated = editWindow.ShowDialog() == true;

            if (customerUpdated)
            {
                await LoadCustomersAsync();
            }
        }

        private void ViewCustomerDetailsButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (CustomersGrid.SelectedItem is not
                CustomerListItem selectedCustomer)
            {
                StatusMessage.Text =
                    "Select a customer row before viewing details.";
                return;
            }

            var detailWindow = new CustomerDetailWindow(
                _apiClient,
                selectedCustomer.CustomerId)
            {
                Owner = Window.GetWindow(this)
            };

            detailWindow.ShowDialog();
        }
    }
}
