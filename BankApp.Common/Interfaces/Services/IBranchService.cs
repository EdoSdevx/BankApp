using BankApp.BankApp.Common.Dtos.Branches;

namespace BankApp.BankApp.Common.Interfaces.Services;

public interface IBranchService
{
    Task<Result<List<BranchListDto>>> ListAsync(CancellationToken cancellationToken = default);
    Task<Result<BranchSelectDto>> SelectAsync(int branchId, CancellationToken cancellationToken = default);
    Task<Result> CreateAsync(BranchCreateDto dto, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(BranchUpdateDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int branchId, CancellationToken cancellationToken = default);
}
