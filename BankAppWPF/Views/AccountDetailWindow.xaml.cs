using BankAppWPF.Models;
using BankAppWPF.Services;
using System.Diagnostics;
using System.Net.Http;
using System.Windows;

namespace BankAppWPF.Views
{
    public partial class AccountDetailWindow : Window
    {
        private readonly ApiClient _apiClient;
        private readonly int _accountId;

        public AccountDetailWindow(ApiClient apiClient, int accountId)
        {
            InitializeComponent();
            _apiClient = apiClient;
            _accountId = accountId;

            AccountIdText.Text = $"Account #{_accountId}";
            Loaded += AccountDetailWindow_Loaded;
        }

        private async void AccountDetailWindow_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            StatusMessage.Text = "Loading account details...";

            try
            {
                var result = await _apiClient
                    .GetAsync<AccountDetail>(
                        $"api/accounts/{_accountId}");

                if (result?.Success != true || result.Data is null)
                {
                    StatusMessage.Text =
                        result?.Message ?? "Account details could not be loaded.";
                    return;
                }

                CustomerIdText.Text =
                    result.Data.CustomerId.ToString();
                BranchIdText.Text =
                    result.Data.BranchId.ToString();
                CurrencyCodeText.Text = result.Data.CurrencyCode;
                BalanceText.Text =
                    $"{result.Data.Balance:N2} {result.Data.CurrencyCode}";
                CreatedDateText.Text =
                    result.Data.CreatedDate.ToString("dd.MM.yyyy HH:mm");
                ActiveStatusText.Text =
                    result.Data.IsActive ? "Active" : "Inactive";
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
                    "An unexpected error occurred while loading account details.";
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
