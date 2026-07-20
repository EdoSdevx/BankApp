using BankApp.BankApp.Common.Dtos.Customers;
using BankApp.BankApp.Common.Constants;
using BankApp.BankApp.Common.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = RoleNames.AdminOrEmployee)]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _customerService.ListAsync(cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpGet("{customerId:int}")]
    public async Task<IActionResult> Select(int customerId, CancellationToken cancellationToken)
    {
        var result = await _customerService.SelectAsync(customerId, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CustomerCreateDto dto, CancellationToken cancellationToken)
    {
        var result = await _customerService.CreateAsync(dto, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpPut("{customerId:int}")]
    public async Task<IActionResult> Update(int customerId, CustomerUpdateDto dto, CancellationToken cancellationToken)
    {
        dto.CustomerId = customerId;

        var result = await _customerService.UpdateAsync(dto, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpDelete("{customerId:int}")]
    public async Task<IActionResult> Delete(int customerId, CancellationToken cancellationToken)
    {
        var result = await _customerService.DeleteAsync(customerId, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }
}
