using BankApp.BankApp.Common.Dtos.Currencies;
using BankApp.BankApp.Common.Constants;
using BankApp.BankApp.Common.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = RoleNames.AdminOrEmployee)]
public class CurrenciesController : ControllerBase
{
    private readonly ICurrencyService _currencyService;

    public CurrenciesController(ICurrencyService currencyService)
    {
        _currencyService = currencyService;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _currencyService.ListAsync(cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpGet("{currencyCode}")]
    public async Task<IActionResult> Select(string currencyCode, CancellationToken cancellationToken)
    {
        var result = await _currencyService.SelectAsync(currencyCode, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CurrencyCreateDto dto, CancellationToken cancellationToken)
    {
        var result = await _currencyService.CreateAsync(dto, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpPut("{currencyCode}")]
    public async Task<IActionResult> Update(string currencyCode, CurrencyUpdateDto dto, CancellationToken cancellationToken)
    {
        dto.CurrencyCode = currencyCode;

        var result = await _currencyService.UpdateAsync(dto, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpDelete("{currencyCode}")]
    public async Task<IActionResult> Delete(string currencyCode, CancellationToken cancellationToken)
    {
        var result = await _currencyService.DeleteAsync(currencyCode, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }
}
