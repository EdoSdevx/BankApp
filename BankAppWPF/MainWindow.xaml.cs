using BankAppWPF.Views;
using BankAppWPF.Services;
using System.Windows;

namespace BankAppWPF
{
    public partial class MainWindow : Window
    {
        private readonly ApiClient _apiClient;

        public MainWindow(ApiClient apiClient)
        {
            InitializeComponent();
            _apiClient = apiClient;
            ShowDashboard();
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            ShowDashboard();
        }

        private void ShowDashboard()
        {
            ShowPage(
                "Dashboard",
                "Welcome to BankApp Operations",
                new DashboardView(_apiClient));
        }

        private void CustomersButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPage(
                "Customers",
                "View and manage BankApp customers",
                new CustomersView(_apiClient));
        }

        private void AccountsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPage(
                "Accounts",
                "View and manage customer bank accounts",
                new AccountsView(_apiClient));
        }

        private void EmployeesButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPage(
                "Employees",
                "View and manage BankApp employees",
                new EmployeesView(_apiClient));
        }

        private void ExchangeRatesButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPage(
                "Exchange Rates",
                "Review the latest currency rates",
                new ExchangeRatesView(_apiClient));
        }

        private void BillsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPage(
                "Bills",
                "Review customer bills and payment status",
                new BillsView(_apiClient));
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var confirmation = MessageBox.Show(
                "Are you sure you want to log out?",
                "Logout",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmation != MessageBoxResult.Yes)
            {
                return;
            }

            _apiClient.ClearAuthentication();

            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }

        private void ShowPage(string title, string subtitle, object content)
        {
            PageTitle.Text = title;
            PageSubtitle.Text = subtitle;
            PageContent.Content = content;
        }
    }
}
