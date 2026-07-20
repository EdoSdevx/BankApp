using BankApp.BankApp.Common.Dtos.Roles;

namespace BankApp.BankApp.Common.Interfaces.Services;

public interface IRoleService
{
    Task<Result<List<RoleListDto>>> ListAsync(CancellationToken cancellationToken = default);
    Task<Result<RoleSelectDto>> SelectAsync(int roleId, CancellationToken cancellationToken = default);
    Task<Result> CreateAsync(RoleCreateDto dto, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(RoleUpdateDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int roleId, CancellationToken cancellationToken = default);
}
