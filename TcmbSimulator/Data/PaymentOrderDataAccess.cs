using System.Data;
using Microsoft.Data.SqlClient;
using TcmbSimulator.Contracts.Payments;

namespace TcmbSimulator.Data;

public class PaymentOrderDataAccess : IPaymentOrderDataAccess
{
    private readonly TcmbDatabaseContext _context;

    public PaymentOrderDataAccess(TcmbDatabaseContext context)
    {
        _context = context;
    }

    public async Task<SubmitPaymentResponse> AcceptAsync(
        string senderBankCode,
        string receiverBankCode,
        string requestHash,
        string centralReference,
        SubmitPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = new SqlCommand("sp_PaymentOrders_Accept", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add("@SenderBankCode", SqlDbType.Char, 5).Value = senderBankCode;
        command.Parameters.Add("@SenderReference", SqlDbType.VarChar, 64).Value =
            request.SenderReference;
        command.Parameters.Add("@RequestHash", SqlDbType.Char, 64).Value = requestHash;
        command.Parameters.Add("@ReceiverBankCode", SqlDbType.Char, 5).Value = receiverBankCode;
        command.Parameters.Add("@ReceiverIban", SqlDbType.VarChar, 34).Value = request.ReceiverIban;
        command.Parameters.Add("@ReceiverName", SqlDbType.NVarChar, 200).Value =
            request.ReceiverName;

        var amountParameter = command.Parameters.Add("@Amount", SqlDbType.Decimal);
        amountParameter.Precision = 18;
        amountParameter.Scale = 2;
        amountParameter.Value = request.Amount;

        command.Parameters.Add("@CurrencyCode", SqlDbType.Char, 3).Value =
            request.CurrencyCode;
        command.Parameters.Add("@Description", SqlDbType.NVarChar, 255).Value =
            (object?)request.Description ?? DBNull.Value;
        command.Parameters.Add("@CentralReference", SqlDbType.VarChar, 64).Value =
            centralReference;

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException(
                "The payment acceptance procedure returned no result.");
        }

        return new SubmitPaymentResponse
        {
            CentralReference = reader.GetString(reader.GetOrdinal("CentralReference")),
            Status = reader.GetString(reader.GetOrdinal("Status")),
            AcceptedAtUtc = reader.GetDateTime(reader.GetOrdinal("AcceptedAtUtc"))
        };
    }
}
