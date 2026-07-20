using System.Security.Claims;
using BankApp.BankApp.Common.Constants;
using BankApp.BankApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Controllers;

[ApiController]
[Route("api/customer/chat")]
[Authorize(Roles = RoleNames.Customer)]
public class CustomerChatController : ControllerBase
{
    private readonly ChatService _chat;

    public CustomerChatController(ChatService chat)
    {
        _chat = chat;
    }

    [HttpPost]
    public async Task<IActionResult> Chat([FromBody] ChatRequestDto request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
            return BadRequest(new { reply = "Please ask a question." });

        var reply = await _chat.ChatAsync(GetCustomerId(), request.Question, ct);
        return Ok(new { reply });
    }

    private int GetCustomerId()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }
}

public class ChatRequestDto
{
    public string Question { get; set; } = string.Empty;
}
