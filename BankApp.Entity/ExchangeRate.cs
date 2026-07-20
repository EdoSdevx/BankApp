namespace BankApp.BankApp.Entity;

public class ExchangeRate
{
    public int RateId { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public DateTime RateDate { get; set; }
    public string Source { get; set; } = string.Empty;
}
