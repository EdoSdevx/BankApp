namespace TcmbSimulator.Contracts.Payments;

public class SubmitPaymentResponse
{
    public string CentralReference { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime AcceptedAtUtc { get; set; }
}
