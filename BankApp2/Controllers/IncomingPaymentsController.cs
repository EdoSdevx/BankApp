using BankApp2.Contracts.IncomingPayments;
using BankApp2.Middleware;
using BankApp2.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace BankApp2.Controllers;

[ApiController]
[Route("api/incoming-payments")]
public class IncomingPaymentsController : ControllerBase
{
    private readonly IIncomingPaymentService _service;

    public IncomingPaymentsController(IIncomingPaymentService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Process(
        IncomingPaymentRequest request,
        CancellationToken cancellationToken)
    {
        if (HttpContext.Items[
                SwitchHmacAuthenticationMiddleware.AuthenticatedSwitchItem] is not string)
            return Unauthorized(new { message = "The payment switch was not authenticated." });

        try
        {
            var response = await _service.ProcessAsync(request, cancellationToken);
            return Ok(response);
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
