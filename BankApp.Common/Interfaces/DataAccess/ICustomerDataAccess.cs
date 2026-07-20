using BankApp.BankApp.Common.Dtos.Customers;

namespace BankApp.BankApp.Common.Interfaces.DataAccess;

public interface ICustomerDataAccess
{
    Task<List<CustomerListDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<CustomerSelectDto?> SelectAsync(int customerId, CancellationToken cancellationToken = default);
    Task<int> InsertAsync(CustomerCreateDto customer, string passwordHash, bool isActive = true, CancellationToken cancellationToken = default);
    Task<int> UpdateAsync(CustomerUpdateDto customer, string? passwordHash = null, CancellationToken cancellationToken = default);
    Task<int> DeleteAsync(int customerId, CancellationToken cancellationToken = default);
}
