namespace BankAppWPF.Models
{
    public class ExchangeRateCreateRequest
    {
        public string CurrencyCode { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public string Source { get; set; } = string.Empty;
    }
}
