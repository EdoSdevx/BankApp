namespace BankApp.BankApp.Common.Dtos.Eft;

public class CreateEftRequestDto
{
    public Guid RequestId { get; set; }
    public int SourceAccountId { get; set; }
    public string ReceiverIban { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
}
