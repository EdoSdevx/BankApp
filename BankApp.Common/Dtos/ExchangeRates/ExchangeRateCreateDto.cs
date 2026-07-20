namespace BankApp.BankApp.Common.Dtos.ExchangeRates;

public class ExchangeRateCreateDto
{
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public string Source { get; set; } = string.Empty;
}
