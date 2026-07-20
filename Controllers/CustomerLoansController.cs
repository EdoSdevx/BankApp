using System.Security.Claims;
using BankApp.BankApp.Common.Constants;
using BankApp.BankApp.Common.Dtos.Loan;
using BankApp.BankApp.Common.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Controllers;

[ApiController]
[Route("api/customer/loans")]
[Authorize(Roles = RoleNames.Customer)]
public class CustomerLoansController : ControllerBase
{
    private readonly ICustomerLoanService _service;
    private readonly ILoanService _loanService;

    public CustomerLoansController(ICustomerLoanService service, ILoanService loanService)
    {
        _service = service;
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
    public async Task<IActionResult> MyLoans(CancellationToken cancellationToken)
    {
        var result = await _service.GetMyLoansAsync(GetCustomerId(), cancellationToken);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    [HttpPost("apply")]
    public async Task<IActionResult> Apply(LoanApplyDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.ApplyAsync(dto, GetCustomerId(), cancellationToken);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    [HttpGet("{loanId:int}")]
    public async Task<IActionResult> GetDetail(int loanId, CancellationToken cancellationToken)
    {
        var result = await _loanService.SelectAsync(loanId, cancellationToken);

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

    [HttpPost("pay")]
    public async Task<IActionResult> Pay([FromBody] LoanPayDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.MakePaymentAsync(GetCustomerId(), dto.LoanId, dto.ScheduleId, dto.AccountId, cancellationToken);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    [HttpPost("close-early")]
    public async Task<IActionResult> CloseEarly([FromBody] CloseEarlyDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.CloseEarlyAsync(GetCustomerId(), dto.LoanId, dto.AccountId, cancellationToken);

        if (!result.Success)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    private int GetCustomerId()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }
}

public class LoanPayDto
{
    public int LoanId { get; set; }
    public int ScheduleId { get; set; }
    public int AccountId { get; set; }
}

public class CloseEarlyDto
{
    public int LoanId { get; set; }
    public int AccountId { get; set; }
}
