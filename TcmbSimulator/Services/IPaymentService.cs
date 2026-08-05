using TcmbSimulator.Contracts.Payments;

namespace TcmbSimulator.Services;

public interface IPaymentService
{
    Task<SubmitPaymentResponse> AcceptAsync(
        string senderBankCode,
        SubmitPaymentRequest request,
        CancellationToken cancellationToken = default);
}
