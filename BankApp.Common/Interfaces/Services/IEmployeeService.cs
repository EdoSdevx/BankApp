using BankApp.BankApp.Common.Dtos.Employees;

namespace BankApp.BankApp.Common.Interfaces.Services;

public interface IEmployeeService
{
    Task<Result<List<EmployeeListDto>>> ListAsync(CancellationToken cancellationToken = default);
    Task<Result<EmployeeSelectDto>> SelectAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<Result> CreateAsync(EmployeeCreateDto dto, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(EmployeeUpdateDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int employeeId, CancellationToken cancellationToken = default);
}
