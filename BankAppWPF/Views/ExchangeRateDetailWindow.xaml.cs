using BankAppWPF.Models;
using BankAppWPF.Services;
using System.Diagnostics;
using System.Net.Http;
using System.Windows;

namespace BankAppWPF.Views
{
    public partial class ExchangeRateDetailWindow : Window
    {
        private readonly ApiClient _apiClient;
        private readonly int _rateId;

        public ExchangeRateDetailWindow(ApiClient apiClient, int rateId)
        {
            InitializeComponent();
            _apiClient = apiClient;
            _rateId = rateId;

            RateIdText.Text = $"Rate #{_rateId}";
            Loaded += ExchangeRateDetailWindow_Loaded;
        }

        private async void ExchangeRateDetailWindow_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            StatusMessage.Text = "Loading exchange-rate details...";

            try
            {
                var result = await _apiClient
                    .GetAsync<ExchangeRateListItem>(
                        $"api/exchangerates/{_rateId}");

                if (result?.Success != true || result.Data is null)
                {
                    StatusMessage.Text =
                        result?.Message ?? "Exchange-rate details could not be loaded.";
                    return;
                }

                CurrencyCodeText.Text = result.Data.CurrencyCode;
                RateText.Text = result.Data.Rate.ToString("N4");
                RateDateText.Text =
                    result.Data.RateDate.ToString("dd.MM.yyyy HH:mm");
                SourceText.Text = result.Data.Source;
                StatusMessage.Text = string.Empty;
            }
            catch (HttpRequestException exception)
            {
                Debug.WriteLine(
                    $"[HTTP] Exchange-rate detail failed: {exception}");
                StatusMessage.Text = "BankApp API could not be reached.";
            }
            catch (Exception exception)
            {
                Debug.WriteLine(
                    $"[HTTP] Exchange-rate detail error: {exception}");
                StatusMessage.Text =
                    "An unexpected error occurred while loading exchange-rate details.";
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
