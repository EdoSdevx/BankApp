namespace BankAppWPF.Models
{
    public class EmployeeListItem
    {
        public int EmployeeId { get; set; }
        public int BranchId { get; set; }
        public int RoleId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string AuthRole { get; set; } = string.Empty;
    }
}
