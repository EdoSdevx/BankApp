namespace BankApp.BankApp.Entity;

public class LoanType
{
    public int LoanTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal AnnualInterestRate { get; set; }
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public int MinTermMonths { get; set; }
    public int MaxTermMonths { get; set; }
    public bool IsActive { get; set; }
}
