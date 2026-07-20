using BankAppWPF.Models;
using BankAppWPF.Services;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Windows;

namespace BankAppWPF.Views
{
    public partial class BillFormWindow : Window
    {
        private readonly ApiClient _apiClient;
        private readonly int? _billId;

        public BillFormWindow(ApiClient apiClient, int? billId = null)
        {
            InitializeComponent();
            _apiClient = apiClient;
            _billId = billId;

            ConfigureMode();
            Loaded += BillFormWindow_Loaded;
        }

        private void ConfigureMode()
        {
            if (_billId.HasValue)
            {
                Title = "Edit Bill";
                FormTitle.Text = "Edit Bill";
                FormSubtitle.Text = $"Bill #{_billId.Value}";
                SaveButton.Content = "Save Changes";
                return;
            }

            Title = "Create Bill";
            FormTitle.Text = "Create Bill";
            FormSubtitle.Text = "Enter the new bill information.";
            SaveButton.Content = "Create";
            DueDatePicker.SelectedDate = DateTime.Today;
        }

        private async void BillFormWindow_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            SaveButton.IsEnabled = false;
            StatusMessage.Text = "Loading form options...";

            try
            {
                var customersTask = _apiClient
                    .GetAsync<List<CustomerListItem>>("api/customers");
                var currenciesTask = _apiClient
                    .GetAsync<List<CurrencyListItem>>("api/currencies");

                await Task.WhenAll(customersTask, currenciesTask);

                var customersResult = await customersTask;
                var currenciesResult = await currenciesTask;

                if (customersResult?.Success != true ||
                    customersResult.Data is null ||
                    currenciesResult?.Success != true ||
                    currenciesResult.Data is null)
                {
                    StatusMessage.Text =
                        "Customer or currency options could not be loaded.";
                    return;
                }

                CustomerComboBox.ItemsSource = customersResult.Data;
                CurrencyComboBox.ItemsSource = currenciesResult.Data;

                if (!_billId.HasValue)
                {
                    StatusMessage.Text = string.Empty;
                    return;
                }

                StatusMessage.Text = "Loading bill...";

                var result = await _apiClient
                    .GetAsync<BillListItem>($"api/bills/{_billId}");

                if (result?.Success != true || result.Data is null)
                {
                    StatusMessage.Text =
                        result?.Message ?? "Bill could not be loaded.";
                    return;
                }

                CustomerComboBox.SelectedValue =
                    result.Data.CustomerId;
                BillTypeTextBox.Text = result.Data.BillType;
                AmountTextBox.Text = result.Data.Amount.ToString("N2");
                CurrencyComboBox.SelectedValue =
                    result.Data.CurrencyCode;
                DueDatePicker.SelectedDate = result.Data.DueDate;
                PaidCheckBox.IsChecked = result.Data.IsPaid;
                PaidDatePicker.SelectedDate = result.Data.PaidDate;
                UpdatePaidDateState();
                StatusMessage.Text = string.Empty;
            }
            catch (HttpRequestException exception)
            {
                Debug.WriteLine(
                    $"[HTTP] Bill-form request failed: {exception}");
                StatusMessage.Text = "BankApp API could not be reached.";
            }
            catch (Exception exception)
            {
                Debug.WriteLine(
                    $"[HTTP] Bill-form loading error: {exception}");
                StatusMessage.Text =
                    "An unexpected error occurred while loading the bill form.";
            }
            finally
            {
                SaveButton.IsEnabled = true;
            }
        }

        private void PaidCheckBox_Click(object sender, RoutedEventArgs e)
        {
            UpdatePaidDateState();
        }

        private void UpdatePaidDateState()
        {
            var isPaid = PaidCheckBox.IsChecked == true;
            PaidDatePicker.IsEnabled = isPaid;

            if (isPaid && !PaidDatePicker.SelectedDate.HasValue)
            {
                PaidDatePicker.SelectedDate = DateTime.Today;
            }

            if (!isPaid)
            {
                PaidDatePicker.SelectedDate = null;
            }
        }

        private async void SaveButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (CustomerComboBox.SelectedValue is not int customerId)
            {
                StatusMessage.Text = "Select a customer.";
                return;
            }

            var billType = BillTypeTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(billType))
            {
                StatusMessage.Text = "Bill type is required.";
                return;
            }

            if (!decimal.TryParse(AmountTextBox.Text, out var amount) ||
                amount <= 0)
            {
                StatusMessage.Text =
                    "Amount must be a valid number greater than zero.";
                return;
            }

            if (CurrencyComboBox.SelectedValue is not string currencyCode)
            {
                StatusMessage.Text = "Select a currency.";
                return;
            }

            if (!DueDatePicker.SelectedDate.HasValue)
            {
                StatusMessage.Text = "Select a due date.";
                return;
            }

            var isPaid = PaidCheckBox.IsChecked == true;
            var paidDate = isPaid
                ? PaidDatePicker.SelectedDate
                : null;

            SaveButton.IsEnabled = false;
            StatusMessage.Text = _billId.HasValue
                ? "Saving changes..."
                : "Creating bill...";

            try
            {
                ApiResult<object>? result;

                if (_billId.HasValue)
                {
                    var request = new BillUpdateRequest
                    {
                        BillId = _billId.Value,
                        CustomerId = customerId,
                        BillType = billType,
                        Amount = amount,
                        CurrencyCode = currencyCode,
                        DueDate = DueDatePicker.SelectedDate.Value,
                        IsPaid = isPaid,
                        PaidDate = paidDate
                    };

                    result = await _apiClient.PutAsync(
                        $"api/bills/{_billId.Value}",
                        request);
                }
                else
                {
                    var request = new BillCreateRequest
                    {
                        CustomerId = customerId,
                        BillType = billType,
                        Amount = amount,
                        CurrencyCode = currencyCode,
                        DueDate = DueDatePicker.SelectedDate.Value,
                        IsPaid = isPaid,
                        PaidDate = paidDate
                    };

                    result = await _apiClient.PostAsync(
                        "api/bills",
                        request);
                }

                if (result?.Success != true)
                {
                    StatusMessage.Text =
                        result?.Message ?? "Bill could not be saved.";
                    return;
                }

                DialogResult = true;
            }
            catch (HttpRequestException exception)
            {
                Debug.WriteLine(
                    $"[HTTP] Save-bill request failed: {exception}");
                StatusMessage.Text = "BankApp API could not be reached.";
            }
            catch (Exception exception)
            {
                Debug.WriteLine(
                    $"[HTTP] Save-bill error: {exception}");
                StatusMessage.Text =
                    "An unexpected error occurred while saving the bill.";
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
