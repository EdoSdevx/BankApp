using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using TcmbSimulator.Contracts.Payments;
using TcmbSimulator.Middleware;
using TcmbSimulator.Services;

namespace TcmbSimulator.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _service;

    public PaymentsController(IPaymentService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Accept(
        SubmitPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var senderBankCode = HttpContext.Items[
            BankHmacAuthenticationMiddleware.AuthenticatedBankCodeItem] as string;

        if (senderBankCode is null)
            return Unauthorized(new { message = "The sender bank was not authenticated." });

        try
        {
            var response = await _service.AcceptAsync(
                senderBankCode,
                request,
                cancellationToken);

            return Accepted(response);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (SqlException exception) when (exception.Number == 50000)
        {
            return BadRequest(new { message = exception.Message });
        }
    }
}
