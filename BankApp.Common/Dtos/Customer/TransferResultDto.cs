namespace BankApp.BankApp.Common.Dtos.Customer;

public class TransferResultDto
{
    public string TransferStatus { get; set; } = string.Empty;
    public int? PendingTransferId { get; set; }
}
