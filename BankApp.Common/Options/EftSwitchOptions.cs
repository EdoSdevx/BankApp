namespace BankApp.BankApp.Common.Options;

public class EftSwitchOptions
{
    public const string SectionName = "EftSwitch";

    public string BaseUrl { get; set; } = string.Empty;
    public string BankCode { get; set; } = string.Empty;
    public string SharedSecret { get; set; } = string.Empty;
    public int PollIntervalSeconds { get; set; } = 5;
    public int MaxAttempts { get; set; } = 5;
}
