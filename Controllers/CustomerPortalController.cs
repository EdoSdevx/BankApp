using System.Security.Claims;
using BankApp.BankApp.Common.Constants;
using BankApp.BankApp.Common.Dtos.Customer;
using BankApp.BankApp.Common.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Controllers;

[ApiController]
[Route("api/customer")]
[Authorize(Roles = RoleNames.Customer)]
public class CustomerPortalController : ControllerBase
{
    private readonly ICustomerPortalService _service;

    public CustomerPortalController(ICustomerPortalService service)
    {
        _service = service;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
    {
        var result = await _service.GetDashboardAsync(GetCustomerId(), cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpGet("accounts")]
    public async Task<IActionResult> Accounts(CancellationToken cancellationToken)
    {
        var result = await _service.GetAccountsAsync(GetCustomerId(), cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpGet("accounts/{accountId:int}")]
    public async Task<IActionResult> AccountDetail(int accountId, CancellationToken cancellationToken)
    {
        var result = await _service.GetAccountAsync(accountId, GetCustomerId(), cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpGet("accounts/{accountId:int}/owner")]
    public async Task<IActionResult> AccountOwner(int accountId, CancellationToken cancellationToken)
    {
        var result = await _service.LookupOwnerAsync(accountId, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpGet("accounts/{accountId:int}/recent-transfers")]
    public async Task<IActionResult> RecentTransfers(int accountId, CancellationToken cancellationToken)
    {
        var result = await _service.GetRecentTransfersAsync(accountId, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpPost("accounts")]
    public async Task<IActionResult> CreateAccount(CreateCustomerAccountDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAccountAsync(GetCustomerId(), dto.BranchId, dto.CurrencyCode, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> Transactions(CancellationToken cancellationToken)
    {
        var result = await _service.GetTransactionsAsync(GetCustomerId(), cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer(TransferRequestDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.TransferAsync(GetCustomerId(), dto, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpPost("exchange")]
    public async Task<IActionResult> Exchange(ExchangeRequestDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.ExchangeAsync(GetCustomerId(), dto, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpGet("bills")]
    public async Task<IActionResult> Bills(CancellationToken cancellationToken)
    {
        var result = await _service.GetBillsAsync(GetCustomerId(), cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpPost("bills/{billId:int}/pay")]
    public async Task<IActionResult> PayBill(int billId, [FromBody] PayBillDto? dto, CancellationToken cancellationToken)
    {
        var accountId = dto?.AccountId;
        var result = await _service.PayBillAsync(GetCustomerId(), billId, accountId, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpGet("branches")]
    public async Task<IActionResult> Branches(CancellationToken cancellationToken)
    {
        var result = await _service.GetBranchesAsync(cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpGet("currencies")]
    public async Task<IActionResult> Currencies(CancellationToken cancellationToken)
    {
        var result = await _service.GetCurrenciesAsync(cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpGet("exchange-rates")]
    public async Task<IActionResult> ExchangeRates(CancellationToken cancellationToken)
    {
        var result = await _service.GetExchangeRatesAsync(cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpPost("transfer-between")]
    public async Task<IActionResult> TransferBetween(AccountTransferDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.TransferBetweenAsync(GetCustomerId(), dto, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    private int GetCustomerId()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }
}

public class PayBillDto
{
    public int? AccountId { get; set; }
}
