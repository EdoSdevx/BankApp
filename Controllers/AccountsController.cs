using BankApp.BankApp.Common.Dtos.Accounts;
using BankApp.BankApp.Common.Dtos.Customer;
using BankApp.BankApp.Common.Constants;
using BankApp.BankApp.Common.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = RoleNames.AdminOrEmployee)]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountsController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _accountService.ListAsync(cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpGet("{accountId:int}")]
    public async Task<IActionResult> Select(int accountId, CancellationToken cancellationToken)
    {
        var result = await _accountService.SelectAsync(accountId, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(AccountCreateDto dto, CancellationToken cancellationToken)
    {
        var result = await _accountService.CreateAsync(dto, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpPut("{accountId:int}")]
    public async Task<IActionResult> Update(int accountId, AccountUpdateDto dto, CancellationToken cancellationToken)
    {
        dto.AccountId = accountId;

        var result = await _accountService.UpdateAsync(dto, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpDelete("{accountId:int}")]
    public async Task<IActionResult> Delete(int accountId, CancellationToken cancellationToken)
    {
        var result = await _accountService.DeleteAsync(accountId, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpPost("transfer-between")]
    public async Task<IActionResult> TransferBetween(AccountTransferDto dto, CancellationToken cancellationToken)
    {
        var result = await _accountService.TransferBetweenAsync(dto, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }
}
