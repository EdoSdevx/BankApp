namespace TcmbSimulator.Configuration;

public class BankAuthenticationOptions
{
    public const string SectionName = "BankAuthentication";

    public int AllowedClockSkewSeconds { get; set; } = 300;
    public Dictionary<string, string> SharedSecrets { get; set; } = new();
}
