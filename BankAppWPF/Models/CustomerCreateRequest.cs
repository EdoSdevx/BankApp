namespace BankAppWPF.Models
{
    public class CustomerCreateRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Address { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
