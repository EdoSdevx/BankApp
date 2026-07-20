using System.Security.Claims;
using BankApp.BankApp.Common.Constants;
using BankApp.BankApp.Common.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = RoleNames.AdminOrEmployee)]
public class AdminApprovalController : ControllerBase
{
    private readonly IAdminApprovalService _service;

    public AdminApprovalController(IAdminApprovalService service)
    {
        _service = service;
    }

    [HttpGet("pending-transfers")]
    public async Task<IActionResult> PendingTransfers(CancellationToken cancellationToken)
    {
        var result = await _service.GetPendingTransfersAsync(cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpPost("pending-transfers/{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, CancellationToken cancellationToken)
    {
        var employeeId = GetEmployeeId();
        var result = await _service.ApproveTransferAsync(id, employeeId, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpPost("pending-transfers/{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] RejectDto dto, CancellationToken cancellationToken)
    {
        var employeeId = GetEmployeeId();
        var result = await _service.RejectTransferAsync(id, employeeId, dto.Reason, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    private int GetEmployeeId()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }
}

public class RejectDto
{
    public string? Reason { get; set; }
}
