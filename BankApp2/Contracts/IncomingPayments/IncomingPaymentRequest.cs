namespace BankApp2.Contracts.IncomingPayments;

public class IncomingPaymentRequest
{
    public string CentralReference { get; set; } = string.Empty;
    public string SenderBankCode { get; set; } = string.Empty;
    public string ReceiverIban { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string? Description { get; set; }
}
