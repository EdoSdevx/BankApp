using BankAppWPF.Models;
using BankAppWPF.Services;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;

namespace BankAppWPF.Views
{
    public partial class DashboardView : UserControl
    {
        private readonly ApiClient _apiClient;
        private bool _hasLoaded;

        public DashboardView(ApiClient apiClient)
        {
            InitializeComponent();
            _apiClient = apiClient;
            Loaded += DashboardView_Loaded;
        }

        private async void DashboardView_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            if (_hasLoaded)
            {
                return;
            }

            _hasLoaded = true;
            StatusMessage.Text = "Loading dashboard...";

            try
            {
                var customersTask = _apiClient
                    .GetAsync<List<CustomerListItem>>("api/customers");
                var employeesTask = _apiClient
                    .GetAsync<List<EmployeeListItem>>("api/employees");
                var ratesTask = _apiClient
                    .GetAsync<List<ExchangeRateListItem>>(
                        "api/exchangerates");

                await Task.WhenAll(
                    customersTask,
                    employeesTask,
                    ratesTask);

                var customersResult = await customersTask;
                var employeesResult = await employeesTask;
                var ratesResult = await ratesTask;

                CustomerCountText.Text =
                    customersResult?.Data?.Count.ToString() ?? "—";
                EmployeeCountText.Text =
                    employeesResult?.Data?.Count.ToString() ?? "—";
                ExchangeRateCountText.Text =
                    ratesResult?.Data?.Count.ToString() ?? "—";

                var allSucceeded =
                    customersResult?.Success == true &&
                    employeesResult?.Success == true &&
                    ratesResult?.Success == true;

                StatusMessage.Text = allSucceeded
                    ? "Dashboard overview loaded."
                    : "Some dashboard information could not be loaded.";
            }
            catch (HttpRequestException exception)
            {
                Debug.WriteLine(
                    $"[HTTP] Dashboard request failed: {exception}");
                StatusMessage.Text = "BankApp API could not be reached.";
            }
            catch (Exception exception)
            {
                Debug.WriteLine(
                    $"[HTTP] Dashboard loading error: {exception}");
                StatusMessage.Text =
                    "An unexpected error occurred while loading the dashboard.";
            }
        }
    }
}
