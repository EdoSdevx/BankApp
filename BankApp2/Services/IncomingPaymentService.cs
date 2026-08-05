using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BankApp2.Contracts.IncomingPayments;
using BankApp2.Data;

namespace BankApp2.Services;

public class IncomingPaymentService : IIncomingPaymentService
{
    private readonly IIncomingPaymentDataAccess _dataAccess;

    public IncomingPaymentService(IIncomingPaymentDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public Task<IncomingPaymentResponse> ProcessAsync(
        IncomingPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        NormalizeAndValidate(request);
        var requestHash = ComputeRequestHash(request);

        return _dataAccess.ProcessAsync(request, requestHash, cancellationToken);
    }

    private static void NormalizeAndValidate(IncomingPaymentRequest request)
    {
        request.CentralReference = request.CentralReference.Trim();
        request.SenderBankCode = request.SenderBankCode.Trim();
        request.ReceiverIban = string.Concat(
            request.ReceiverIban.Where(character => !char.IsWhiteSpace(character)))
            .ToUpperInvariant();
        request.ReceiverName = request.ReceiverName.Trim();
        request.CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant();
        request.Description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();

        if (request.CentralReference.Length is < 1 or > 64)
            throw new ArgumentException("Central reference must contain 1 to 64 characters.");

        if (request.SenderBankCode.Length != 5 ||
            !request.SenderBankCode.All(char.IsDigit))
            throw new ArgumentException("Sender bank code must contain five digits.");

        if (request.ReceiverIban.Length != 26 ||
            !request.ReceiverIban.StartsWith("TR", StringComparison.Ordinal) ||
            !request.ReceiverIban[2..].All(char.IsDigit))
            throw new ArgumentException(
                "Receiver IBAN must be a valid 26-character Turkish IBAN.");

        if (request.ReceiverIban.Substring(4, 5) != "00002")
            throw new ArgumentException("Receiver IBAN does not belong to BankApp2.");

        if (request.ReceiverName.Length is < 1 or > 200)
            throw new ArgumentException("Receiver name must contain 1 to 200 characters.");

        if (request.Amount <= 0 || decimal.Round(request.Amount, 2) != request.Amount)
            throw new ArgumentException(
                "Amount must be positive and contain at most two decimals.");

        if (request.CurrencyCode != "TRY")
            throw new ArgumentException("Only TRY payments are supported.");

        if (request.Description?.Length > 255)
            throw new ArgumentException("Description cannot exceed 255 characters.");
    }

    private static string ComputeRequestHash(IncomingPaymentRequest request)
    {
        var canonicalPayment = JsonSerializer.Serialize(new
        {
            request.CentralReference,
            request.SenderBankCode,
            request.ReceiverIban,
            request.ReceiverName,
            request.Amount,
            request.CurrencyCode,
            request.Description
        });

        return Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(canonicalPayment)));
    }
}
