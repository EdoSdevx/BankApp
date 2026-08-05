using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TcmbSimulator.Contracts.Payments;
using TcmbSimulator.Data;

namespace TcmbSimulator.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentOrderDataAccess _dataAccess;

    public PaymentService(IPaymentOrderDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public Task<SubmitPaymentResponse> AcceptAsync(
        string senderBankCode,
        SubmitPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        NormalizeAndValidate(senderBankCode, request);

        var receiverBankCode = request.ReceiverIban.Substring(4, 5);
        var requestHash = ComputeRequestHash(senderBankCode, request);
        var centralReference = $"TCMB-{Guid.NewGuid():N}";

        return _dataAccess.AcceptAsync(
            senderBankCode,
            receiverBankCode,
            requestHash,
            centralReference,
            request,
            cancellationToken);
    }

    private static void NormalizeAndValidate(
        string senderBankCode,
        SubmitPaymentRequest request)
    {
        request.SenderReference = request.SenderReference.Trim();
        request.ReceiverIban = request.ReceiverIban.Replace(" ", string.Empty).ToUpperInvariant();
        request.ReceiverName = request.ReceiverName.Trim();
        request.CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant();
        request.Description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();

        if (senderBankCode.Length != 5 || !senderBankCode.All(char.IsDigit))
            throw new ArgumentException("Sender bank code must contain five digits.");

        if (request.SenderReference.Length is < 1 or > 64)
            throw new ArgumentException("Sender reference must contain 1 to 64 characters.");

        if (request.ReceiverIban.Length != 26 ||
            !request.ReceiverIban.StartsWith("TR", StringComparison.Ordinal) ||
            !request.ReceiverIban[2..].All(char.IsDigit))
            throw new ArgumentException("Receiver IBAN must be a valid 26-character Turkish IBAN.");

        if (request.ReceiverName.Length is < 1 or > 200)
            throw new ArgumentException("Receiver name must contain 1 to 200 characters.");

        if (request.Amount <= 0 || decimal.Round(request.Amount, 2) != request.Amount)
            throw new ArgumentException("Amount must be positive and contain at most two decimals.");

        if (request.CurrencyCode != "TRY")
            throw new ArgumentException("Only TRY payments are supported.");

        if (request.Description?.Length > 255)
            throw new ArgumentException("Description cannot exceed 255 characters.");
    }

    private static string ComputeRequestHash(
        string senderBankCode,
        SubmitPaymentRequest request)
    {
        var canonicalPayment = JsonSerializer.Serialize(new
        {
            SenderBankCode = senderBankCode,
            request.SenderReference,
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
