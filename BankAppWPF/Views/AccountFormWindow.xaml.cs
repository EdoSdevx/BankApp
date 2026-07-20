using BankAppWPF.Models;
using BankAppWPF.Services;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Windows;
using System.Windows.Media;

namespace BankAppWPF.Views
{
    public partial class AccountFormWindow : Window
    {
        private readonly ApiClient _apiClient;
        private readonly int? _accountId;
        private decimal _balance;

        public AccountFormWindow(
            ApiClient apiClient,
            int? accountId = null)
        {
            InitializeComponent();
            _apiClient = apiClient;
            _accountId = accountId;

            ConfigureMode();
            Loaded += AccountFormWindow_Loaded;
        }

        private void ConfigureMode()
        {
            if (_accountId.HasValue)
            {
                Title = "Edit Account";
                FormTitle.Text = "Edit Account";
                FormSubtitle.Text = $"Account #{_accountId.Value}";
                SaveButton.Content = "Save Changes";
                BalanceTextBox.IsReadOnly = true;
                BalanceTextBox.Background =
                    new SolidColorBrush(Color.FromRgb(241, 245, 249));
                BalanceHint.Text =
                    "Balance is changed through account operations.";
                return;
            }

            Title = "Create Account";
            FormTitle.Text = "Create Account";
            FormSubtitle.Text =
                "Enter the new account information.";
            SaveButton.Content = "Create";
            BalanceTextBox.Text = "0";
            BalanceHint.Text =
                "Enter the account's initial balance.";
        }

        private async void AccountFormWindow_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            SaveButton.IsEnabled = false;
            StatusMessage.Text = "Loading form options...";

            try
            {
                var customersTask = _apiClient
                    .GetAsync<List<CustomerListItem>>("api/customers");
                var branchesTask = _apiClient
                    .GetAsync<List<BranchListItem>>("api/branches");

                await Task.WhenAll(customersTask, branchesTask);

                var customersResult = await customersTask;
                var branchesResult = await branchesTask;

                if (customersResult?.Success != true ||
                    customersResult.Data is null ||
                    branchesResult?.Success != true ||
                    branchesResult.Data is null)
                {
                    StatusMessage.Text =
                        "Customer or branch options could not be loaded.";
                    return;
                }

                CustomerComboBox.ItemsSource = customersResult.Data;
                BranchComboBox.ItemsSource = branchesResult.Data;

                if (!_accountId.HasValue)
                {
                    StatusMessage.Text = string.Empty;
                    return;
                }

                StatusMessage.Text = "Loading account...";

                var result = await _apiClient
                    .GetAsync<AccountDetail>(
                        $"api/accounts/{_accountId}");

                if (result?.Success != true || result.Data is null)
                {
                    StatusMessage.Text =
                        result?.Message ?? "Account could not be loaded.";
                    return;
                }

                CustomerComboBox.SelectedValue =
                    result.Data.CustomerId;
                BranchComboBox.SelectedValue =
                    result.Data.BranchId;
                CurrencyCodeTextBox.Text =
                    result.Data.CurrencyCode;

                _balance = result.Data.Balance;
                BalanceTextBox.Text = _balance.ToString("N2");
                StatusMessage.Text = string.Empty;
            }
            catch (HttpRequestException exception)
            {
                Debug.WriteLine(
                    $"[HTTP] Account-detail request failed: {exception}");
                StatusMessage.Text = "BankApp API could not be reached.";
            }
            catch (Exception exception)
            {
                Debug.WriteLine(
                    $"[HTTP] Account-detail loading error: {exception}");
                StatusMessage.Text =
                    "An unexpected error occurred while loading the account.";
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
            if (CustomerComboBox.SelectedValue is not int customerId)
            {
                StatusMessage.Text =
                    "Select a customer.";
                return;
            }

            if (BranchComboBox.SelectedValue is not int branchId)
            {
                StatusMessage.Text =
                    "Select a branch.";
                return;
            }

            var currencyCode =
                CurrencyCodeTextBox.Text.Trim().ToUpperInvariant();

            if (currencyCode.Length != 3)
            {
                StatusMessage.Text =
                    "Currency code must contain exactly 3 characters.";
                return;
            }

            var balance = _balance;

            if (!_accountId.HasValue &&
                (!decimal.TryParse(BalanceTextBox.Text, out balance) ||
                 balance < 0))
            {
                StatusMessage.Text =
                    "Initial balance must be a valid non-negative number.";
                return;
            }

            SaveButton.IsEnabled = false;
            StatusMessage.Text = _accountId.HasValue
                ? "Saving changes..."
                : "Creating account...";

            try
            {
                ApiResult<object>? result;

                if (_accountId.HasValue)
                {
                    var request = new AccountUpdateRequest
                    {
                        AccountId = _accountId.Value,
                        CustomerId = customerId,
                        BranchId = branchId,
                        CurrencyCode = currencyCode,
                        Balance = balance
                    };

                    result = await _apiClient.PutAsync(
                        $"api/accounts/{_accountId.Value}",
                        request);
                }
                else
                {
                    var request = new AccountCreateRequest
                    {
                        CustomerId = customerId,
                        BranchId = branchId,
                        CurrencyCode = currencyCode,
                        Balance = balance
                    };

                    result = await _apiClient.PostAsync(
                        "api/accounts",
                        request);
                }

                if (result?.Success != true)
                {
                    StatusMessage.Text =
                        result?.Message ?? "Account could not be saved.";
                    return;
                }

                DialogResult = true;
            }
            catch (HttpRequestException exception)
            {
                Debug.WriteLine(
                    $"[HTTP] Save-account request failed: {exception}");
                StatusMessage.Text = "BankApp API could not be reached.";
            }
            catch (Exception exception)
            {
                Debug.WriteLine(
                    $"[HTTP] Save-account error: {exception}");
                StatusMessage.Text =
                    "An unexpected error occurred while saving the account.";
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
