using BankApp.BankApp.Common;
using BankApp.BankApp.Common.Dtos.Eft;
using BankApp.BankApp.Common.Interfaces.DataAccess;
using BankApp.BankApp.Common.Interfaces.Services;
using Microsoft.Data.SqlClient;

namespace BankApp.BankApp.Services;

public class EftService : IEftService
{
    private readonly IEftDataAccess _dataAccess;

    public EftService(IEftDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public async Task<Result> CreateAsync(
        int customerId,
        CreateEftRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        if (dto.RequestId == Guid.Empty)
        {
            return Result.Fail("Request ID is required.");
        }

        if (dto.SourceAccountId <= 0)
        {
            return Result.Fail("Source account is required.");
        }

        if (dto.Amount <= 0)
        {
            return Result.Fail("Amount must be greater than zero.");
        }

        if (decimal.Round(dto.Amount, 2) != dto.Amount)
        {
            return Result.Fail("Amount cannot contain more than two decimal places.");
        }

        var receiverIban = NormalizeIban(dto.ReceiverIban);
        if (!HasValidTurkishIbanFormat(receiverIban))
        {
            return Result.Fail("A 26-character Turkish IBAN format is required.");
        }

        var receiverName = dto.ReceiverName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(receiverName) || receiverName.Length > 200)
        {
            return Result.Fail("Receiver name is required and cannot exceed 200 characters.");
        }

        var description = dto.Description?.Trim();
        if (description?.Length > 255)
        {
            return Result.Fail("Description cannot exceed 255 characters.");
        }

        dto.ReceiverIban = receiverIban;
        dto.ReceiverName = receiverName;
        dto.Description = string.IsNullOrEmpty(description) ? null : description;

        var senderReference = $"BANKAPP-{Guid.NewGuid():N}";

        try
        {
            await _dataAccess.CreateAsync(
                customerId,
                dto,
                senderReference,
                cancellationToken);

            return Result.Ok("EFT request accepted.");
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            return Result.Conflict("This EFT request has already been submitted.");
        }
        catch (SqlException ex)
        {
            return Result.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return Result.DatabaseError(ex.Message);
        }
    }

    public async Task<Result<List<EftDetailDto>>> GetByCustomerAsync(
        int customerId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var transfers = await _dataAccess.GetByCustomerAsync(customerId, cancellationToken);
            return Result<List<EftDetailDto>>.Ok(transfers);
        }
        catch (Exception ex)
        {
            return Result<List<EftDetailDto>>.DatabaseError(ex.Message);
        }
    }

    public async Task<Result<EftDetailDto>> GetByIdAsync(
        int customerId,
        int eftTransferId,
        CancellationToken cancellationToken = default)
    {
        if (eftTransferId <= 0)
        {
            return Result<EftDetailDto>.Fail("EFT transfer ID must be greater than zero.");
        }

        try
        {
            var transfer = await _dataAccess.GetByIdAsync(customerId, eftTransferId, cancellationToken);

            return transfer is null
                ? Result<EftDetailDto>.NotFound("EFT transfer not found.")
                : Result<EftDetailDto>.Ok(transfer);
        }
        catch (Exception ex)
        {
            return Result<EftDetailDto>.DatabaseError(ex.Message);
        }
    }

    public async Task<Result<List<PendingEftDto>>> GetPendingAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var transfers = await _dataAccess.GetPendingAsync(cancellationToken);
            return Result<List<PendingEftDto>>.Ok(transfers);
        }
        catch (Exception ex)
        {
            return Result<List<PendingEftDto>>.DatabaseError(ex.Message);
        }
    }

    public async Task<Result<EftDetailDto>> ApproveAsync(
        int eftTransferId,
        int employeeId,
        CancellationToken cancellationToken = default)
    {
        if (eftTransferId <= 0)
        {
            return Result<EftDetailDto>.Fail("EFT transfer ID must be greater than zero.");
        }

        try
        {
            var transfer = await _dataAccess.ApproveAsync(
                eftTransferId,
                employeeId,
                cancellationToken);

            return Result<EftDetailDto>.Ok(transfer, "EFT approved and queued.");
        }
        catch (SqlException ex)
        {
            return Result<EftDetailDto>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<EftDetailDto>.DatabaseError(ex.Message);
        }
    }

    public async Task<Result<EftDetailDto>> RejectAsync(
        int eftTransferId,
        int employeeId,
        string? reason,
        CancellationToken cancellationToken = default)
    {
        if (eftTransferId <= 0)
        {
            return Result<EftDetailDto>.Fail("EFT transfer ID must be greater than zero.");
        }

        reason = reason?.Trim();
        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result<EftDetailDto>.Fail("A rejection reason is required.");
        }

        if (reason.Length > 500)
        {
            return Result<EftDetailDto>.Fail("Rejection reason cannot exceed 500 characters.");
        }

        try
        {
            var transfer = await _dataAccess.RejectAsync(
                eftTransferId,
                employeeId,
                reason,
                cancellationToken);

            return Result<EftDetailDto>.Ok(transfer, "EFT rejected and reserved funds returned.");
        }
        catch (SqlException ex)
        {
            return Result<EftDetailDto>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<EftDetailDto>.DatabaseError(ex.Message);
        }
    }

    private static string NormalizeIban(string? iban)
    {
        return string.IsNullOrWhiteSpace(iban)
            ? string.Empty
            : string.Concat(iban.Where(character => !char.IsWhiteSpace(character))).ToUpperInvariant();
    }

    private static bool HasValidTurkishIbanFormat(string iban)
    {
        return iban.Length == 26
            && iban.StartsWith("TR", StringComparison.Ordinal)
            && iban[2..].All(character => character is >= '0' and <= '9');
    }
}
