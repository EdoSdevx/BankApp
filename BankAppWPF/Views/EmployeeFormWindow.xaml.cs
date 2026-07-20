using BankAppWPF.Models;
using BankAppWPF.Services;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Windows;

namespace BankAppWPF.Views
{
    public partial class EmployeeFormWindow : Window
    {
        private readonly ApiClient _apiClient;
        private readonly int? _employeeId;

        public EmployeeFormWindow(
            ApiClient apiClient,
            int? employeeId = null)
        {
            InitializeComponent();
            _apiClient = apiClient;
            _employeeId = employeeId;

            AuthRoleComboBox.ItemsSource =
                new List<string> { "Employee", "Admin" };
            AuthRoleComboBox.SelectedItem = "Employee";

            ConfigureMode();
            Loaded += EmployeeFormWindow_Loaded;
        }

        private void ConfigureMode()
        {
            if (_employeeId.HasValue)
            {
                Title = "Edit Employee";
                FormTitle.Text = "Edit Employee";
                FormSubtitle.Text = $"Employee #{_employeeId.Value}";
                PasswordLabel.Text =
                    "New Password (leave blank to keep the current password)";
                SaveButton.Content = "Save Changes";
                return;
            }

            Title = "Create Employee";
            FormTitle.Text = "Create Employee";
            FormSubtitle.Text = "Enter the new employee information.";
            SaveButton.Content = "Create";
        }

        private async void EmployeeFormWindow_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            SaveButton.IsEnabled = false;
            StatusMessage.Text = "Loading form options...";

            try
            {
                var branchesTask = _apiClient
                    .GetAsync<List<BranchListItem>>("api/branches");
                var rolesTask = _apiClient
                    .GetAsync<List<RoleListItem>>("api/roles");

                await Task.WhenAll(branchesTask, rolesTask);

                var branchesResult = await branchesTask;
                var rolesResult = await rolesTask;

                if (branchesResult?.Success != true ||
                    branchesResult.Data is null ||
                    rolesResult?.Success != true ||
                    rolesResult.Data is null)
                {
                    StatusMessage.Text =
                        "Branch or role options could not be loaded.";
                    return;
                }

                BranchComboBox.ItemsSource = branchesResult.Data;
                RoleComboBox.ItemsSource = rolesResult.Data;

                if (!_employeeId.HasValue)
                {
                    StatusMessage.Text = string.Empty;
                    return;
                }

                StatusMessage.Text = "Loading employee...";

                var result = await _apiClient
                    .GetAsync<EmployeeDetail>(
                        $"api/employees/{_employeeId}");

                if (result?.Success != true || result.Data is null)
                {
                    StatusMessage.Text =
                        result?.Message ?? "Employee could not be loaded.";
                    return;
                }

                FirstNameTextBox.Text = result.Data.FirstName;
                LastNameTextBox.Text = result.Data.LastName;
                EmailTextBox.Text = result.Data.Email;
                PhoneTextBox.Text = result.Data.Phone;
                BranchComboBox.SelectedValue = result.Data.BranchId;
                RoleComboBox.SelectedValue = result.Data.RoleId;
                AuthRoleComboBox.SelectedItem = result.Data.AuthRole;
                StatusMessage.Text = string.Empty;
            }
            catch (HttpRequestException exception)
            {
                Debug.WriteLine(
                    $"[HTTP] Employee-form request failed: {exception}");
                StatusMessage.Text = "BankApp API could not be reached.";
            }
            catch (Exception exception)
            {
                Debug.WriteLine(
                    $"[HTTP] Employee-form loading error: {exception}");
                StatusMessage.Text =
                    "An unexpected error occurred while loading the employee form.";
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
            var phone = PhoneTextBox.Text.Trim();
            var password = PasswordInput.Password;

            if (string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(phone))
            {
                StatusMessage.Text =
                    "First name, last name, email and phone are required.";
                return;
            }

            if (BranchComboBox.SelectedValue is not int branchId)
            {
                StatusMessage.Text = "Select a branch.";
                return;
            }

            if (RoleComboBox.SelectedValue is not int roleId)
            {
                StatusMessage.Text = "Select a role.";
                return;
            }

            if (AuthRoleComboBox.SelectedItem is not string authRole)
            {
                StatusMessage.Text = "Select an authorization role.";
                return;
            }

            if (!_employeeId.HasValue &&
                string.IsNullOrWhiteSpace(password))
            {
                StatusMessage.Text =
                    "Password is required when creating an employee.";
                return;
            }

            if (!string.IsNullOrEmpty(password) && password.Length < 6)
            {
                StatusMessage.Text =
                    "Password must be at least 6 characters.";
                return;
            }

            SaveButton.IsEnabled = false;
            StatusMessage.Text = _employeeId.HasValue
                ? "Saving changes..."
                : "Creating employee...";

            try
            {
                ApiResult<object>? result;

                if (_employeeId.HasValue)
                {
                    var request = new EmployeeUpdateRequest
                    {
                        EmployeeId = _employeeId.Value,
                        BranchId = branchId,
                        RoleId = roleId,
                        FirstName = firstName,
                        LastName = lastName,
                        Email = email,
                        Phone = phone,
                        Password = string.IsNullOrWhiteSpace(password)
                            ? null
                            : password,
                        AuthRole = authRole
                    };

                    result = await _apiClient.PutAsync(
                        $"api/employees/{_employeeId.Value}",
                        request);
                }
                else
                {
                    var request = new EmployeeCreateRequest
                    {
                        BranchId = branchId,
                        RoleId = roleId,
                        FirstName = firstName,
                        LastName = lastName,
                        Email = email,
                        Phone = phone,
                        Password = password,
                        AuthRole = authRole
                    };

                    result = await _apiClient.PostAsync(
                        "api/employees",
                        request);
                }

                if (result?.Success != true)
                {
                    StatusMessage.Text =
                        result?.Message ?? "Employee could not be saved.";
                    return;
                }

                DialogResult = true;
            }
            catch (HttpRequestException exception)
            {
                Debug.WriteLine(
                    $"[HTTP] Save-employee request failed: {exception}");
                StatusMessage.Text = "BankApp API could not be reached.";
            }
            catch (Exception exception)
            {
                Debug.WriteLine(
                    $"[HTTP] Save-employee error: {exception}");
                StatusMessage.Text =
                    "An unexpected error occurred while saving the employee.";
            }
            finally
            {
                SaveButton.IsEnabled = true;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
