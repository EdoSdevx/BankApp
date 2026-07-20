using BankApp.BankApp.Common.Dtos.Roles;

namespace BankApp.BankApp.Common.Interfaces.DataAccess;

public interface IRoleDataAccess
{
    Task<List<RoleListDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<RoleSelectDto?> SelectAsync(int roleId, CancellationToken cancellationToken = default);
    Task<int> InsertAsync(RoleCreateDto role, CancellationToken cancellationToken = default);
    Task<int> UpdateAsync(RoleUpdateDto role, CancellationToken cancellationToken = default);
    Task<int> DeleteAsync(int roleId, CancellationToken cancellationToken = default);
}
