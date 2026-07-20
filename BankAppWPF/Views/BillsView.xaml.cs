using BankAppWPF.Models;
using BankAppWPF.Services;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;

namespace BankAppWPF.Views
{
    public partial class BillsView : UserControl
    {
        private readonly ApiClient _apiClient;

        public BillsView(ApiClient apiClient)
        {
            InitializeComponent();
            _apiClient = apiClient;
            Loaded += BillsView_Loaded;
        }

        private async void BillsView_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            await LoadBillsAsync();
        }

        private async Task LoadBillsAsync()
        {
            StatusMessage.Text = "Loading bills...";

            try
            {
                var result = await _apiClient
                    .GetAsync<List<BillListItem>>("api/bills");

                if (result?.Success != true || result.Data is null)
                {
                    StatusMessage.Text =
                        result?.Message ?? "Bills could not be loaded.";
                    return;
                }

                var sortedBills = result.Data
                    .OrderBy(bill => bill.IsPaid)
                    .ThenBy(bill => bill.IsPaid
                        ? bill.PaidDate ?? bill.DueDate
                        : bill.DueDate)
                    .ToList();

                BillsGrid.ItemsSource = sortedBills;
                StatusMessage.Text =
                    $"{sortedBills.Count} bills loaded.";
            }
            catch (HttpRequestException exception)
            {
                Debug.WriteLine(
                    $"[HTTP] Bill request failed: {exception}");
                StatusMessage.Text = "BankApp API could not be reached.";
            }
            catch (Exception exception)
            {
                Debug.WriteLine(
                    $"[HTTP] Bill loading error: {exception}");
                StatusMessage.Text =
                    "An unexpected error occurred while loading bills.";
            }
        }

        private void ViewBillDetailsButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (BillsGrid.SelectedItem is not BillListItem selectedBill)
            {
                StatusMessage.Text =
                    "Select a bill row before viewing details.";
                return;
            }

            var detailWindow = new BillDetailWindow(
                _apiClient,
                selectedBill.BillId)
            {
                Owner = Window.GetWindow(this)
            };

            detailWindow.ShowDialog();
        }

        private async void CreateBillButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            var createWindow = new BillFormWindow(_apiClient)
            {
                Owner = Window.GetWindow(this)
            };

            if (createWindow.ShowDialog() == true)
            {
                await LoadBillsAsync();
            }
        }

        private async void EditBillButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (BillsGrid.SelectedItem is not BillListItem selectedBill)
            {
                StatusMessage.Text =
                    "Select a bill row before editing.";
                return;
            }

            var editWindow = new BillFormWindow(
                _apiClient,
                selectedBill.BillId)
            {
                Owner = Window.GetWindow(this)
            };

            if (editWindow.ShowDialog() == true)
            {
                await LoadBillsAsync();
            }
        }
    }
}
