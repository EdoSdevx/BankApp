using BankApp.BankApp.Common.Dtos.ExchangeRates;
using BankApp.BankApp.Common.Constants;
using BankApp.BankApp.Common.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = RoleNames.AdminOrEmployee)]
public class ExchangeRatesController : ControllerBase
{
    private readonly IExchangeRateService _exchangeRateService;

    public ExchangeRatesController(IExchangeRateService exchangeRateService)
    {
        _exchangeRateService = exchangeRateService;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _exchangeRateService.ListAsync(cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpGet("{rateId:int}")]
    public async Task<IActionResult> Select(int rateId, CancellationToken cancellationToken)
    {
        var result = await _exchangeRateService.SelectAsync(rateId, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(ExchangeRateCreateDto dto, CancellationToken cancellationToken)
    {
        var result = await _exchangeRateService.CreateAsync(dto, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpPut("{rateId:int}")]
    public async Task<IActionResult> Update(int rateId, ExchangeRateUpdateDto dto, CancellationToken cancellationToken)
    {
        dto.RateId = rateId;

        var result = await _exchangeRateService.UpdateAsync(dto, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpDelete("{rateId:int}")]
    public async Task<IActionResult> Delete(int rateId, CancellationToken cancellationToken)
    {
        var result = await _exchangeRateService.DeleteAsync(rateId, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }
}
