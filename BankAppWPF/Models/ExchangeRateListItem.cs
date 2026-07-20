namespace BankAppWPF.Models
{
    public class ExchangeRateListItem
    {
        public int RateId { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public DateTime RateDate { get; set; }
        public string Source { get; set; } = string.Empty;
    }
}
