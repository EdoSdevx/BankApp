using System.Security.Claims;
using BankApp.BankApp.Common.Constants;
using BankApp.BankApp.Common.Dtos.Eft;
using BankApp.BankApp.Common.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Controllers;

[ApiController]
[Route("api/admin/efts")]
[Authorize(Roles = RoleNames.AdminOrEmployee)]
public class EftApprovalsController : ControllerBase
{
    private readonly IEftService _service;

    public EftApprovalsController(IEftService service)
    {
        _service = service;
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending(CancellationToken cancellationToken)
    {
        var result = await _service.GetPendingAsync(cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpPost("{eftTransferId:int}/approve")]
    public async Task<IActionResult> Approve(
        int eftTransferId,
        CancellationToken cancellationToken)
    {
        var result = await _service.ApproveAsync(
            eftTransferId,
            GetEmployeeId(),
            cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpPost("{eftTransferId:int}/reject")]
    public async Task<IActionResult> Reject(
        int eftTransferId,
        RejectEftDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _service.RejectAsync(
            eftTransferId,
            GetEmployeeId(),
            dto.Reason,
            cancellationToken);

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
