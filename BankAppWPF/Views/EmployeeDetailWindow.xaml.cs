using BankAppWPF.Models;
using BankAppWPF.Services;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Windows;

namespace BankAppWPF.Views
{
    public partial class EmployeeDetailWindow : Window
    {
        private readonly ApiClient _apiClient;
        private readonly int _employeeId;

        public EmployeeDetailWindow(ApiClient apiClient, int employeeId)
        {
            InitializeComponent();
            _apiClient = apiClient;
            _employeeId = employeeId;

            EmployeeIdText.Text = $"Employee #{_employeeId}";
            Loaded += EmployeeDetailWindow_Loaded;
        }

        private async void EmployeeDetailWindow_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            StatusMessage.Text = "Loading employee details...";

            try
            {
                var employeeTask = _apiClient
                    .GetAsync<EmployeeDetail>(
                        $"api/employees/{_employeeId}");
                var branchesTask = _apiClient
                    .GetAsync<List<BranchListItem>>("api/branches");
                var rolesTask = _apiClient
                    .GetAsync<List<RoleListItem>>("api/roles");

                await Task.WhenAll(
                    employeeTask,
                    branchesTask,
                    rolesTask);

                var employeeResult = await employeeTask;
                var branchesResult = await branchesTask;
                var rolesResult = await rolesTask;

                if (employeeResult?.Success != true ||
                    employeeResult.Data is null)
                {
                    StatusMessage.Text =
                        employeeResult?.Message ??
                        "Employee details could not be loaded.";
                    return;
                }

                var employee = employeeResult.Data;
                var branch = branchesResult?.Data?
                    .FirstOrDefault(item =>
                        item.BranchId == employee.BranchId);
                var role = rolesResult?.Data?
                    .FirstOrDefault(item =>
                        item.RoleId == employee.RoleId);

                FullNameText.Text =
                    $"{employee.FirstName} {employee.LastName}";
                EmailText.Text = employee.Email;
                PhoneText.Text = employee.Phone;
                BranchText.Text =
                    branch?.DisplayName ?? $"Branch #{employee.BranchId}";
                RoleText.Text =
                    role?.DisplayName ?? $"Role #{employee.RoleId}";
                AuthRoleText.Text = employee.AuthRole;
                HireDateText.Text =
                    employee.HireDate.ToString("dd.MM.yyyy");
                StatusMessage.Text = string.Empty;
            }
            catch (HttpRequestException exception)
            {
                Debug.WriteLine(
                    $"[HTTP] Employee-detail request failed: {exception}");
                StatusMessage.Text = "BankApp API could not be reached.";
            }
            catch (Exception exception)
            {
                Debug.WriteLine(
                    $"[HTTP] Employee-detail loading error: {exception}");
                StatusMessage.Text =
                    "An unexpected error occurred while loading employee details.";
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
