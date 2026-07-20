using BankAppWPF.Models;
using BankAppWPF.Services;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;

namespace BankAppWPF.Views
{
    public partial class AccountsView : UserControl
    {
        private readonly ApiClient _apiClient;

        public AccountsView(ApiClient apiClient)
        {
            InitializeComponent();
            _apiClient = apiClient;
            Loaded += AccountsView_Loaded;
        }

        private async void AccountsView_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            await LoadAccountsAsync();
        }

        private async Task LoadAccountsAsync()
        {
            StatusMessage.Text = "Loading accounts...";

            try
            {
                var result = await _apiClient
                    .GetAsync<List<AccountListItem>>("api/accounts");

                if (result?.Success != true || result.Data is null)
                {
                    StatusMessage.Text =
                        result?.Message ?? "Accounts could not be loaded.";
                    return;
                }

                AccountsGrid.ItemsSource = result.Data;
                StatusMessage.Text =
                    $"{result.Data.Count} accounts loaded.";
            }
            catch (HttpRequestException exception)
            {
                Debug.WriteLine(
                    $"[HTTP] Account request failed: {exception}");
                StatusMessage.Text = "BankApp API could not be reached.";
            }
            catch (Exception exception)
            {
                Debug.WriteLine(
                    $"[HTTP] Account loading error: {exception}");
                StatusMessage.Text =
                    "An unexpected error occurred while loading accounts.";
            }
        }

        private async void EditAccountButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (AccountsGrid.SelectedItem is not
                AccountListItem selectedAccount)
            {
                StatusMessage.Text =
                    "Select an account row before editing.";
                return;
            }

            var editWindow = new AccountFormWindow(
                _apiClient,
                selectedAccount.AccountId)
            {
                Owner = Window.GetWindow(this)
            };

            var accountUpdated = editWindow.ShowDialog() == true;

            if (accountUpdated)
            {
                await LoadAccountsAsync();
            }
        }

        private async void CreateAccountButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            var createWindow = new AccountFormWindow(_apiClient)
            {
                Owner = Window.GetWindow(this)
            };

            var accountCreated = createWindow.ShowDialog() == true;

            if (accountCreated)
            {
                await LoadAccountsAsync();
            }
        }

        private void ViewAccountDetailsButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (AccountsGrid.SelectedItem is not
                AccountListItem selectedAccount)
            {
                StatusMessage.Text =
                    "Select an account row before viewing details.";
                return;
            }

            var detailWindow = new AccountDetailWindow(
                _apiClient,
                selectedAccount.AccountId)
            {
                Owner = Window.GetWindow(this)
            };

            detailWindow.ShowDialog();
        }
    }
}
