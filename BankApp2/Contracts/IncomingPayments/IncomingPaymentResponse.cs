namespace BankApp2.Contracts.IncomingPayments;

public class IncomingPaymentResponse
{
    public string CentralReference { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime ProcessedAtUtc { get; set; }
    public string? FailureReason { get; set; }
}
