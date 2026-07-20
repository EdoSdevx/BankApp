using BankAppWPF.Models;
using BankAppWPF.Services;
using System.Diagnostics;
using System.Net.Http;
using System.Windows;

namespace BankAppWPF.Views
{
    public partial class BillDetailWindow : Window
    {
        private readonly ApiClient _apiClient;
        private readonly int _billId;

        public BillDetailWindow(ApiClient apiClient, int billId)
        {
            InitializeComponent();
            _apiClient = apiClient;
            _billId = billId;

            BillIdText.Text = $"Bill #{_billId}";
            Loaded += BillDetailWindow_Loaded;
        }

        private async void BillDetailWindow_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            StatusMessage.Text = "Loading bill details...";

            try
            {
                var result = await _apiClient
                    .GetAsync<BillListItem>($"api/bills/{_billId}");

                if (result?.Success != true || result.Data is null)
                {
                    StatusMessage.Text =
                        result?.Message ?? "Bill details could not be loaded.";
                    return;
                }

                CustomerIdText.Text =
                    result.Data.CustomerId.ToString();
                BillTypeText.Text = result.Data.BillType;
                AmountText.Text =
                    $"{result.Data.Amount:N2} {result.Data.CurrencyCode}";
                DueDateText.Text =
                    result.Data.DueDate.ToString("dd.MM.yyyy");
                PaidStatusText.Text =
                    result.Data.IsPaid ? "Paid" : "Unpaid";
                PaidDateText.Text = result.Data.PaidDate.HasValue
                    ? result.Data.PaidDate.Value.ToString("dd.MM.yyyy HH:mm")
                    : "Not paid";
                StatusMessage.Text = string.Empty;
            }
            catch (HttpRequestException exception)
            {
                Debug.WriteLine(
                    $"[HTTP] Bill-detail request failed: {exception}");
                StatusMessage.Text = "BankApp API could not be reached.";
            }
            catch (Exception exception)
            {
                Debug.WriteLine(
                    $"[HTTP] Bill-detail loading error: {exception}");
                StatusMessage.Text =
                    "An unexpected error occurred while loading bill details.";
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
