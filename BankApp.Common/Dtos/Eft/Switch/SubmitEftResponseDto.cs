namespace BankApp.BankApp.Common.Dtos.Eft.Switch;

public class SubmitEftResponseDto
{
    public string CentralReference { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime AcceptedAtUtc { get; set; }
}
