using System.Security.Claims;
using BankApp.BankApp.Common.Constants;
using BankApp.BankApp.Common.Dtos.Eft;
using BankApp.BankApp.Common.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Controllers;

[ApiController]
[Route("api/customer/efts")]
[Authorize(Roles = RoleNames.Customer)]
public class CustomerEftsController : ControllerBase
{
    private readonly IEftService _service;

    public CustomerEftsController(IEftService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateEftRequestDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(GetCustomerId(), dto, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var result = await _service.GetByCustomerAsync(GetCustomerId(), cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpGet("{eftTransferId:int}")]
    public async Task<IActionResult> GetDetail(
        int eftTransferId,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(
            GetCustomerId(),
            eftTransferId,
            cancellationToken);

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
