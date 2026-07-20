using BankApp.BankApp.Common.Dtos.Customers;

namespace BankApp.BankApp.Common.Interfaces.Services;

public interface ICustomerService
{
    Task<Result<List<CustomerListDto>>> ListAsync(CancellationToken cancellationToken = default);
    Task<Result<CustomerSelectDto>> SelectAsync(int customerId, CancellationToken cancellationToken = default);
    Task<Result> CreateAsync(CustomerCreateDto dto, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(CustomerUpdateDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int customerId, CancellationToken cancellationToken = default);
}
