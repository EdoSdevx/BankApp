namespace BankApp.BankApp.Common.Dtos.Currencies;

public class CurrencyUpdateDto
{
    public string CurrencyCode { get; set; } = string.Empty;
    public string? CurrencyName { get; set; }
}
