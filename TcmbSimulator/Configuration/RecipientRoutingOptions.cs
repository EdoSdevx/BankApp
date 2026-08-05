namespace TcmbSimulator.Configuration;

public class RecipientRoutingOptions
{
    public const string SectionName = "RecipientRouting";

    public string SwitchCode { get; set; } = "TCMB";
    public int PollIntervalSeconds { get; set; } = 5;
    public int MaxAttempts { get; set; } = 5;
    public Dictionary<string, string> SharedSecrets { get; set; } = new();
}
