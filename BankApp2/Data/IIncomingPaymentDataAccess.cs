using BankApp2.Contracts.IncomingPayments;

namespace BankApp2.Data;

public interface IIncomingPaymentDataAccess
{
    Task<IncomingPaymentResponse> ProcessAsync(
        IncomingPaymentRequest request,
        string requestHash,
        CancellationToken cancellationToken = default);
}
