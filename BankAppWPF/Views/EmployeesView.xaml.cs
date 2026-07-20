using BankAppWPF.Models;
using BankAppWPF.Services;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;

namespace BankAppWPF.Views
{
    public partial class EmployeesView : UserControl
    {
        private readonly ApiClient _apiClient;

        public EmployeesView(ApiClient apiClient)
        {
            InitializeComponent();
            _apiClient = apiClient;
            Loaded += EmployeesView_Loaded;
        }

        private async void EmployeesView_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            await LoadEmployeesAsync();
        }

        private async Task LoadEmployeesAsync()
        {
            StatusMessage.Text = "Loading employees...";

            try
            {
                var result = await _apiClient
                    .GetAsync<List<EmployeeListItem>>("api/employees");

                if (result?.Success != true || result.Data is null)
                {
                    StatusMessage.Text =
                        result?.Message ?? "Employees could not be loaded.";
                    return;
                }

                EmployeesGrid.ItemsSource = result.Data;
                StatusMessage.Text =
                    $"{result.Data.Count} employees loaded.";
            }
            catch (HttpRequestException exception)
            {
                Debug.WriteLine(
                    $"[HTTP] Employee request failed: {exception}");
                StatusMessage.Text = "BankApp API could not be reached.";
            }
            catch (Exception exception)
            {
                Debug.WriteLine(
                    $"[HTTP] Employee loading error: {exception}");
                StatusMessage.Text =
                    "An unexpected error occurred while loading employees.";
            }
        }

        private void ViewEmployeeDetailsButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (EmployeesGrid.SelectedItem is not
                EmployeeListItem selectedEmployee)
            {
                StatusMessage.Text =
                    "Select an employee row before viewing details.";
                return;
            }

            var detailWindow = new EmployeeDetailWindow(
                _apiClient,
                selectedEmployee.EmployeeId)
            {
                Owner = Window.GetWindow(this)
            };

            detailWindow.ShowDialog();
        }

        private async void CreateEmployeeButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            var createWindow = new EmployeeFormWindow(_apiClient)
            {
                Owner = Window.GetWindow(this)
            };

            if (createWindow.ShowDialog() == true)
            {
                await LoadEmployeesAsync();
            }
        }

        private async void EditEmployeeButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (EmployeesGrid.SelectedItem is not
                EmployeeListItem selectedEmployee)
            {
                StatusMessage.Text =
                    "Select an employee row before editing.";
                return;
            }

            var editWindow = new EmployeeFormWindow(
                _apiClient,
                selectedEmployee.EmployeeId)
            {
                Owner = Window.GetWindow(this)
            };

            if (editWindow.ShowDialog() == true)
            {
                await LoadEmployeesAsync();
            }
        }
    }
}
