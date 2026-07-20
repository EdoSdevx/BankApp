using BankApp.BankApp.Common.Dtos.Employees;
using BankApp.BankApp.Common.Constants;
using BankApp.BankApp.Common.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = RoleNames.AdminOrEmployee)]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _employeeService.ListAsync(cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpGet("{employeeId:int}")]
    public async Task<IActionResult> Select(int employeeId, CancellationToken cancellationToken)
    {
        var result = await _employeeService.SelectAsync(employeeId, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(EmployeeCreateDto dto, CancellationToken cancellationToken)
    {
        var result = await _employeeService.CreateAsync(dto, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpPut("{employeeId:int}")]
    public async Task<IActionResult> Update(int employeeId, EmployeeUpdateDto dto, CancellationToken cancellationToken)
    {
        dto.EmployeeId = employeeId;

        var result = await _employeeService.UpdateAsync(dto, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpDelete("{employeeId:int}")]
    public async Task<IActionResult> Delete(int employeeId, CancellationToken cancellationToken)
    {
        var result = await _employeeService.DeleteAsync(employeeId, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }
}
