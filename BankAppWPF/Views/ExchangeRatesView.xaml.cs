using BankAppWPF.Models;
using BankAppWPF.Services;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;

namespace BankAppWPF.Views
{
    public partial class ExchangeRatesView : UserControl
    {
        private readonly ApiClient _apiClient;

        public ExchangeRatesView(ApiClient apiClient)
        {
            InitializeComponent();
            _apiClient = apiClient;
            Loaded += ExchangeRatesView_Loaded;
        }

        private async void ExchangeRatesView_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            await LoadExchangeRatesAsync();
        }

        private async Task LoadExchangeRatesAsync()
        {
            StatusMessage.Text = "Loading exchange rates...";

            try
            {
                var result = await _apiClient
                    .GetAsync<List<ExchangeRateListItem>>(
                        "api/exchangerates");

                if (result?.Success != true || result.Data is null)
                {
                    StatusMessage.Text =
                        result?.Message ?? "Exchange rates could not be loaded.";
                    return;
                }

                ExchangeRatesGrid.ItemsSource = result.Data;
                StatusMessage.Text =
                    $"{result.Data.Count} exchange rates loaded.";
            }
            catch (HttpRequestException exception)
            {
                Debug.WriteLine(
                    $"[HTTP] Exchange-rate request failed: {exception}");
                StatusMessage.Text = "BankApp API could not be reached.";
            }
            catch (Exception exception)
            {
                Debug.WriteLine(
                    $"[HTTP] Exchange-rate loading error: {exception}");
                StatusMessage.Text =
                    "An unexpected error occurred while loading exchange rates.";
            }
        }

        private void ViewRateDetailsButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (ExchangeRatesGrid.SelectedItem is not
                ExchangeRateListItem selectedRate)
            {
                StatusMessage.Text =
                    "Select an exchange-rate row before viewing details.";
                return;
            }

            var detailWindow = new ExchangeRateDetailWindow(
                _apiClient,
                selectedRate.RateId)
            {
                Owner = Window.GetWindow(this)
            };

            detailWindow.ShowDialog();
        }

        private async void CreateRateButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            var createWindow = new CreateExchangeRateWindow(_apiClient)
            {
                Owner = Window.GetWindow(this)
            };

            if (createWindow.ShowDialog() == true)
            {
                await LoadExchangeRatesAsync();
            }
        }
    }
}
