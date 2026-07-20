namespace BankAppWPF.Models
{
    public class CurrencyListItem
    {
        public string CurrencyCode { get; set; } = string.Empty;
        public string CurrencyName { get; set; } = string.Empty;

        public string DisplayName =>
            $"{CurrencyCode} - {CurrencyName}";
    }
}
