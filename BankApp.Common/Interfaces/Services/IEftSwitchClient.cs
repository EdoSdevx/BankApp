using BankApp.BankApp.Common.Dtos.Eft.Switch;

namespace BankApp.BankApp.Common.Interfaces.Services;

public interface IEftSwitchClient
{
    Task<SubmitEftResponseDto> SubmitAsync(
        SubmitEftRequestDto request,
        CancellationToken cancellationToken = default);
}
