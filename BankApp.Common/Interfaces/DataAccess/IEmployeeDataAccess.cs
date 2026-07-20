using BankApp.BankApp.Common.Dtos.Employees;

namespace BankApp.BankApp.Common.Interfaces.DataAccess;

public interface IEmployeeDataAccess
{
    Task<List<EmployeeListDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<EmployeeSelectDto?> SelectAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<int> InsertAsync(EmployeeCreateDto employee, string passwordHash, CancellationToken cancellationToken = default);
    Task<int> UpdateAsync(EmployeeUpdateDto employee, string? passwordHash = null, CancellationToken cancellationToken = default);
    Task<int> DeleteAsync(int employeeId, CancellationToken cancellationToken = default);
}
