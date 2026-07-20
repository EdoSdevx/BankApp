using BankApp.BankApp.Common.Dtos.Transactions;
using BankApp.BankApp.Common.Constants;
using BankApp.BankApp.Common.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = RoleNames.AdminOrEmployee)]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _transactionService.ListAsync(cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpGet("{transactionId:int}")]
    public async Task<IActionResult> Select(int transactionId, CancellationToken cancellationToken)
    {
        var result = await _transactionService.SelectAsync(transactionId, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(TransactionCreateDto dto, CancellationToken cancellationToken)
    {
        var result = await _transactionService.CreateAsync(dto, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpPut("{transactionId:int}")]
    public async Task<IActionResult> Update(int transactionId, TransactionUpdateDto dto, CancellationToken cancellationToken)
    {
        dto.TransactionId = transactionId;

        var result = await _transactionService.UpdateAsync(dto, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpDelete("{transactionId:int}")]
    public async Task<IActionResult> Delete(int transactionId, CancellationToken cancellationToken)
    {
        var result = await _transactionService.DeleteAsync(transactionId, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }
}
