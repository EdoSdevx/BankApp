using BankApp.BankApp.Common;
using BankApp.BankApp.Common.Dtos.Customer;
using BankApp.BankApp.Common.Interfaces.DataAccess;
using BankApp.BankApp.Common.Interfaces.Services;
using BankApp.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;

namespace BankApp.BankApp.Services;

public class AdminApprovalService : IAdminApprovalService
{
    private readonly IAdminApprovalDataAccess _dataAccess;
    private readonly IHubContext<NotificationHub> _hubContext;

    public AdminApprovalService(IAdminApprovalDataAccess dataAccess, IHubContext<NotificationHub> hubContext)
    {
        _dataAccess = dataAccess;
        _hubContext = hubContext;
    }

    public async Task<Result<List<PendingTransferDto>>> GetPendingTransfersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var transfers = await _dataAccess.GetPendingTransfersAsync(cancellationToken);
            return Result<List<PendingTransferDto>>.Ok(transfers);
        }
        catch (Exception ex)
        {
            return Result<List<PendingTransferDto>>.DatabaseError(ex.Message);
        }
    }

    public async Task<Result> ApproveTransferAsync(int pendingTransferId, int employeeId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (pendingTransferId <= 0)
                return Result.Fail("Pending transfer ID must be greater than zero.");

            var result = await _dataAccess.ApproveTransferAsync(pendingTransferId, employeeId, cancellationToken);

            await _hubContext.Clients.User(result.CreatedByCustomerId.ToString()).SendAsync("TransferResolved", new
            {
                TransferId = pendingTransferId,
                Status = "Approved"
            }, cancellationToken);

            return Result.Ok("Transfer approved.");
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

    public async Task<Result> RejectTransferAsync(int pendingTransferId, int employeeId, string? reason, CancellationToken cancellationToken = default)
    {
        try
        {
            if (pendingTransferId <= 0)
                return Result.Fail("Pending transfer ID must be greater than zero.");

            var result = await _dataAccess.RejectTransferAsync(pendingTransferId, employeeId, reason, cancellationToken);

            await _hubContext.Clients.User(result.CreatedByCustomerId.ToString()).SendAsync("TransferResolved", new
            {
                TransferId = pendingTransferId,
                Status = "Rejected"
            }, cancellationToken);

            return Result.Ok("Transfer rejected.");
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
}
