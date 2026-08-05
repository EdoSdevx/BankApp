using TcmbSimulator.Contracts.Payments;

namespace TcmbSimulator.Data;

public interface IPaymentOrderDataAccess
{
    Task<SubmitPaymentResponse> AcceptAsync(
        string senderBankCode,
        string receiverBankCode,
        string requestHash,
        string centralReference,
        SubmitPaymentRequest request,
        CancellationToken cancellationToken = default);
}
