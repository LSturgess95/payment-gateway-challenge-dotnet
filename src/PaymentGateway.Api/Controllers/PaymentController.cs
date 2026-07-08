using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Contracts.Responses;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Extensions;
using PaymentGateway.Api.Contracts.Requests;
using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Clients.Exceptions;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentController : Controller
{
    private readonly ILogger<PaymentController> _logger;
    private readonly IPaymentService _paymentsService;

    public PaymentController(ILogger<PaymentController> logger, IPaymentService paymentsService)
    {
        _logger = logger;
        _paymentsService = paymentsService;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<PaymentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult GetPayment(Guid id)
    {
        var payment = _paymentsService.Get(id);

        if (payment == null)
            return NotFound();

        return Ok(payment.ToResponse());
    }

    [HttpPost]
    [ProducesResponseType<PaymentResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProcessPayment(ProcessPaymentRequest request)
    {
        if (!ModelState.IsValid || request is null)
        {
            var errors = request is null
                ? new[] { "The request body is invalid or could not be parsed." }
                : ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(request.ToRejectedResponse(errors));
        }

        try
        {
            var payment = await _paymentsService.ProcessPaymentAsync(request);

            return CreatedAtAction(nameof(GetPayment), new { id = payment.Id }, payment?.ToResponse());
        }
        catch (BankUnavailableException)
        {
            return Problem(
                title: "Bank unavailable",
                detail: "The payment could not be processed because the acquiring bank is currently unavailable.",
                statusCode: StatusCodes.Status502BadGateway
            );
        }
        catch (BankMalformedRequestException)
        {
            return BadRequest(request.ToRejectedResponse(new[] { "The payment could not be processed because the acquiring bank rejected it." }));
        }
        catch (PaymentSaveFailureException)
        {
            return Problem(
                title: "Downstream system unavailable",
                detail: "The payment could not be processed because the downstream data source is currently unavailable.",
                statusCode: StatusCodes.Status502BadGateway
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unknown error has occured.");
            return Problem(
                title: "Unknown error occured",
                detail: "An unknown error has occured.",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}
