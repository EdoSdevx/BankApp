using BankApp.BankApp.Common.Constants;
using BankApp.BankApp.Common.Dtos.Loan;
using BankApp.BankApp.Common.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = RoleNames.AdminOrEmployee)]
public class LoansController : ControllerBase
{
    private readonly ILoanService _loanService;

    public LoansController(ILoanService loanService)
    {
        _loanService = loanService;
    }

    [HttpGet("types")]
    public async Task<IActionResult> GetLoanTypes(CancellationToken cancellationToken)
    {
        var result = await _loanService.GetLoanTypesAsync(cancellationToken);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _loanService.ListAsync(cancellationToken);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    [HttpGet("{loanId:int}")]
    public async Task<IActionResult> Select(int loanId, CancellationToken cancellationToken)
    {
        var result = await _loanService.SelectAsync(loanId, cancellationToken);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    [HttpPost("{loanId:int}/approve")]
    public async Task<IActionResult> Approve(int loanId, CancellationToken cancellationToken)
    {
        var employeeId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var result = await _loanService.ApproveAsync(loanId, employeeId, cancellationToken);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    [HttpPost("{loanId:int}/reject")]
    public async Task<IActionResult> Reject(int loanId, [FromBody] RejectDto? dto, CancellationToken cancellationToken)
    {
        var result = await _loanService.RejectAsync(loanId, dto?.Reason, cancellationToken);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    [HttpGet("{loanId:int}/schedule")]
    public async Task<IActionResult> GetSchedule(int loanId, CancellationToken cancellationToken)
    {
        var result = await _loanService.GetScheduleAsync(loanId, cancellationToken);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    [HttpGet("{loanId:int}/payments")]
    public async Task<IActionResult> GetPayments(int loanId, CancellationToken cancellationToken)
    {
        var result = await _loanService.GetPaymentsAsync(loanId, cancellationToken);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }
}
