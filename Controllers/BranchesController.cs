using BankApp.BankApp.Common.Dtos.Branches;
using BankApp.BankApp.Common.Constants;
using BankApp.BankApp.Common.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = RoleNames.AdminOrEmployee)]
public class BranchesController : ControllerBase
{
    private readonly IBranchService _branchService;

    public BranchesController(IBranchService branchService)
    {
        _branchService = branchService;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _branchService.ListAsync(cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpGet("{branchId:int}")]
    public async Task<IActionResult> Select(int branchId, CancellationToken cancellationToken)
    {
        var result = await _branchService.SelectAsync(branchId, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(BranchCreateDto dto, CancellationToken cancellationToken)
    {
        var result = await _branchService.CreateAsync(dto, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpPut("{branchId:int}")]
    public async Task<IActionResult> Update(int branchId, BranchUpdateDto dto, CancellationToken cancellationToken)
    {
        dto.BranchId = branchId;

        var result = await _branchService.UpdateAsync(dto, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }

    [HttpDelete("{branchId:int}")]
    public async Task<IActionResult> Delete(int branchId, CancellationToken cancellationToken)
    {
        var result = await _branchService.DeleteAsync(branchId, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return Ok(result);
    }
}
