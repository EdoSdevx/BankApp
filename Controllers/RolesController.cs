using BankApp.BankApp.Common.Dtos.Roles;
using BankApp.BankApp.Common.Constants;
using BankApp.BankApp.Common.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = RoleNames.AdminOrEmployee)]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _roleService.ListAsync(cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpGet("{roleId:int}")]
    public async Task<IActionResult> Select(int roleId, CancellationToken cancellationToken)
    {
        var result = await _roleService.SelectAsync(roleId, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(RoleCreateDto dto, CancellationToken cancellationToken)
    {
        var result = await _roleService.CreateAsync(dto, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpPut("{roleId:int}")]
    public async Task<IActionResult> Update(int roleId, RoleUpdateDto dto, CancellationToken cancellationToken)
    {
        dto.RoleId = roleId;

        var result = await _roleService.UpdateAsync(dto, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpDelete("{roleId:int}")]
    public async Task<IActionResult> Delete(int roleId, CancellationToken cancellationToken)
    {
        var result = await _roleService.DeleteAsync(roleId, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }
}
