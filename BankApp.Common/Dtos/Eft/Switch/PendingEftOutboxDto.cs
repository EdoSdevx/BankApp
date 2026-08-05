namespace BankApp.BankApp.Common.Dtos.Eft.Switch;

public class PendingEftOutboxDto
{
    public int OutboxMessageId { get; set; }
    public int EftTransferId { get; set; }
    public int AttemptCount { get; set; }
    public string SenderReference { get; set; } = string.Empty;
    public string ReceiverIban { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string? Description { get; set; }

    public SubmitEftRequestDto ToRequest()
    {
        return new SubmitEftRequestDto
        {
            SenderReference = SenderReference,
            ReceiverIban = ReceiverIban,
            ReceiverName = ReceiverName,
            Amount = Amount,
            CurrencyCode = CurrencyCode,
            Description = Description
        };
    }
}
