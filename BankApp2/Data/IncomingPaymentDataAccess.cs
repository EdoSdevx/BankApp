using System.Data;
using BankApp2.Contracts.IncomingPayments;
using Microsoft.Data.SqlClient;

namespace BankApp2.Data;

public class IncomingPaymentDataAccess : IIncomingPaymentDataAccess
{
    private readonly RecipientDatabaseContext _context;

    public IncomingPaymentDataAccess(RecipientDatabaseContext context)
    {
        _context = context;
    }

    public async Task<IncomingPaymentResponse> ProcessAsync(
        IncomingPaymentRequest request,
        string requestHash,
        CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = new SqlCommand("sp_IncomingPayments_Process", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add("@CentralReference", SqlDbType.VarChar, 64).Value =
            request.CentralReference;
        command.Parameters.Add("@RequestHash", SqlDbType.Char, 64).Value = requestHash;
        command.Parameters.Add("@SenderBankCode", SqlDbType.Char, 5).Value =
            request.SenderBankCode;
        command.Parameters.Add("@ReceiverIban", SqlDbType.VarChar, 34).Value =
            request.ReceiverIban;
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

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            throw new InvalidOperationException(
                "The incoming-payment procedure returned no result.");

        var failureReasonOrdinal = reader.GetOrdinal("FailureReason");

        return new IncomingPaymentResponse
        {
            CentralReference = reader.GetString(reader.GetOrdinal("CentralReference")),
            Status = reader.GetString(reader.GetOrdinal("Status")),
            ProcessedAtUtc = reader.GetDateTime(reader.GetOrdinal("ProcessedAtUtc")),
            FailureReason = reader.IsDBNull(failureReasonOrdinal)
                ? null
                : reader.GetString(failureReasonOrdinal)
        };
    }
}
