namespace BankApp.BankApp.Common.Dtos.ExchangeRates;

public class ExchangeRateUpdateDto
{
    public int RateId { get; set; }
    public string? CurrencyCode { get; set; }
    public decimal? Rate { get; set; }
    public string? Source { get; set; }
}
