namespace BankApp2.Configuration;

public class SwitchAuthenticationOptions
{
    public const string SectionName = "SwitchAuthentication";

    public string SwitchCode { get; set; } = "TCMB";
    public int AllowedClockSkewSeconds { get; set; } = 300;
    public string SharedSecret { get; set; } = string.Empty;
}
