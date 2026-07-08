using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Contracts.Responses;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Extensions;
using PaymentGateway.Api.Contracts.Requests;
using PaymentGateway.Api.Domain;
using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentController : Controller
{
    private readonly IPaymentService _paymentsService;

    public PaymentController(IPaymentService paymentsService)
    {
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
                detail: "The payment could not be processed because the acquiring bank is currently unavailable",
                statusCode: StatusCodes.Status502BadGateway
            );
        }

    }
}