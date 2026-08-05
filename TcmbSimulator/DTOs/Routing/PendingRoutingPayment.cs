namespace TcmbSimulator.Contracts.Routing;

public class PendingRoutingPayment
{
    public int RoutingOutboxMessageId { get; set; }
    public int AttemptCount { get; set; }
    public int PaymentOrderId { get; set; }
    public string CentralReference { get; set; } = string.Empty;
    public string SenderBankCode { get; set; } = string.Empty;
    public string ReceiverBankCode { get; set; } = string.Empty;
    public string ReceiverIban { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ReceiverApiBaseUrl { get; set; } = string.Empty;

    public RoutePaymentRequest ToRequest()
    {
        return new RoutePaymentRequest
        {
            CentralReference = CentralReference,
            SenderBankCode = SenderBankCode,
            ReceiverIban = ReceiverIban,
            ReceiverName = ReceiverName,
            Amount = Amount,
            CurrencyCode = CurrencyCode,
            Description = Description
        };
    }
}
