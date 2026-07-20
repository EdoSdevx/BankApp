namespace BankApp.BankApp.Common.Options;

public class ResetTokenOptions
{
    public string SecretKey { get; set; } = string.Empty;
    public int ExpiresMinutes { get; set; } = 15;
}
