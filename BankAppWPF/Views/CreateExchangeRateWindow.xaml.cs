using BankAppWPF.Models;
using BankAppWPF.Services;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Windows;

namespace BankAppWPF.Views
{
    public partial class CreateExchangeRateWindow : Window
    {
        private readonly ApiClient _apiClient;

        public CreateExchangeRateWindow(ApiClient apiClient)
        {
            InitializeComponent();
            _apiClient = apiClient;
            Loaded += CreateExchangeRateWindow_Loaded;
        }

        private async void CreateExchangeRateWindow_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            SaveButton.IsEnabled = false;
            StatusMessage.Text = "Loading currencies...";

            try
            {
                var result = await _apiClient
                    .GetAsync<List<CurrencyListItem>>("api/currencies");

                if (result?.Success != true || result.Data is null)
                {
                    StatusMessage.Text =
                        result?.Message ?? "Currencies could not be loaded.";
                    return;
                }

                CurrencyComboBox.ItemsSource = result.Data;
                StatusMessage.Text = string.Empty;
            }
            catch (HttpRequestException exception)
            {
                Debug.WriteLine(
                    $"[HTTP] Currency request failed: {exception}");
                StatusMessage.Text = "BankApp API could not be reached.";
            }
            catch (Exception exception)
            {
                Debug.WriteLine(
                    $"[HTTP] Currency loading error: {exception}");
                StatusMessage.Text =
                    "An unexpected error occurred while loading currencies.";
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
            if (CurrencyComboBox.SelectedValue is not string currencyCode)
            {
                StatusMessage.Text = "Select a currency.";
                return;
            }

            if (!decimal.TryParse(RateTextBox.Text, out var rate) ||
                rate <= 0)
            {
                StatusMessage.Text =
                    "Rate must be a valid number greater than zero.";
                return;
            }

            var source = SourceTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(source))
            {
                StatusMessage.Text = "Source is required.";
                return;
            }

            var request = new ExchangeRateCreateRequest
            {
                CurrencyCode = currencyCode,
                Rate = rate,
                Source = source
            };

            SaveButton.IsEnabled = false;
            StatusMessage.Text = "Creating exchange rate...";

            try
            {
                var result = await _apiClient.PostAsync(
                    "api/exchangerates",
                    request);

                if (result?.Success != true)
                {
                    StatusMessage.Text =
                        result?.Message ?? "Exchange rate could not be created.";
                    return;
                }

                DialogResult = true;
            }
            catch (HttpRequestException exception)
            {
                Debug.WriteLine(
                    $"[HTTP] Create-rate request failed: {exception}");
                StatusMessage.Text = "BankApp API could not be reached.";
            }
            catch (Exception exception)
            {
                Debug.WriteLine(
                    $"[HTTP] Create-rate error: {exception}");
                StatusMessage.Text =
                    "An unexpected error occurred while creating the exchange rate.";
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
