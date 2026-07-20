using BankApp.BankApp.Common.Dtos.Branches;

namespace BankApp.BankApp.Common.Interfaces.DataAccess;

public interface IBranchDataAccess
{
    Task<List<BranchListDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<BranchSelectDto?> SelectAsync(int branchId, CancellationToken cancellationToken = default);
    Task<int> InsertAsync(BranchCreateDto branch, CancellationToken cancellationToken = default);
    Task<int> UpdateAsync(BranchUpdateDto branch, CancellationToken cancellationToken = default);
    Task<int> DeleteAsync(int branchId, CancellationToken cancellationToken = default);
}
