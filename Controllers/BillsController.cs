using BankApp.BankApp.Common.Dtos.Bills;
using BankApp.BankApp.Common.Constants;
using BankApp.BankApp.Common.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = RoleNames.AdminOrEmployee)]
public class BillsController : ControllerBase
{
    private readonly IBillService _billService;

    public BillsController(IBillService billService)
    {
        _billService = billService;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _billService.ListAsync(cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpGet("{billId:int}")]
    public async Task<IActionResult> Select(int billId, CancellationToken cancellationToken)
    {
        var result = await _billService.SelectAsync(billId, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(BillCreateDto dto, CancellationToken cancellationToken)
    {
        var result = await _billService.CreateAsync(dto, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpPut("{billId:int}")]
    public async Task<IActionResult> Update(int billId, BillUpdateDto dto, CancellationToken cancellationToken)
    {
        dto.BillId = billId;

        var result = await _billService.UpdateAsync(dto, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpPut("{billId:int}/mark-paid")]
    public async Task<IActionResult> MarkPaid(int billId, CancellationToken cancellationToken)
    {
        var result = await _billService.MarkPaidAsync(billId, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpDelete("{billId:int}")]
    public async Task<IActionResult> Delete(int billId, CancellationToken cancellationToken)
    {
        var result = await _billService.DeleteAsync(billId, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }
}
