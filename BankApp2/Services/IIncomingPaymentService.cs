using BankApp2.Contracts.IncomingPayments;

namespace BankApp2.Services;

public interface IIncomingPaymentService
{
    Task<IncomingPaymentResponse> ProcessAsync(
        IncomingPaymentRequest request,
        CancellationToken cancellationToken = default);
}
