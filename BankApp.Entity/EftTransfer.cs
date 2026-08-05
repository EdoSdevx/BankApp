using BankApp.BankApp.Common.Constants;

namespace BankApp.BankApp.Entity;

public class EftTransfer
{
    public int EftTransferId { get; set; }
    public Guid RequestId { get; set; }
    public int CustomerId { get; set; }
    public int SourceAccountId { get; set; }
    public string ReceiverIban { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public string? Description { get; set; }
    public string SenderReference { get; set; } = string.Empty;
    public string? CentralReference { get; set; }
    public string Status { get; set; } = EftStatuses.PendingApproval;
    public DateTime CreatedAt { get; set; }
    public int? ApprovedByEmployeeId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? FailureReason { get; set; }
}
