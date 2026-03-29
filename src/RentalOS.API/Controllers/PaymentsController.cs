using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Modules.Payments.Commands.CreateMoMoPayment;
using RentalOS.Application.Modules.Payments.Commands.CreateVNPayPayment;
using RentalOS.Application.Modules.Payments.Commands.UpdatePaymentSettings;
using RentalOS.Application.Modules.Payments.Queries.GetPaymentSettings;

namespace RentalOS.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PaymentsController(IMediator mediator, IMoMoService momoService, IVNPayService vnpayService) : ControllerBase
{
    [HttpGet("settings")]
    [Authorize(Roles = "admin,landlord")]
    public async Task<ActionResult> GetSettings()
    {
        var result = await mediator.Send(new GetPaymentSettingsQuery());
        return Ok(result.Data);
    }

    [HttpPut("settings")]
    [Authorize(Roles = "admin,landlord")]
    public async Task<ActionResult> UpdateSettings(UpdatePaymentSettingsCommand command)
    {
        var result = await mediator.Send(command);
        return result.IsSuccess ? Ok() : BadRequest(result.ErrorMessage);
    }

    [HttpPost("momo")]
    [AllowAnonymous]
    public async Task<ActionResult> CreateMoMoPayment(CreateMoMoPaymentCommand command)
    {
        var result = await mediator.Send(command);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpPost("vnpay")]
    [AllowAnonymous]
    public async Task<ActionResult> CreateVNPayPayment(CreateVNPayPaymentCommand command)
    {
        var result = await mediator.Send(command);
        return result.IsSuccess ? Ok(new { paymentUrl = result.Data }) : BadRequest(result.ErrorMessage);
    }

    [HttpPost("momo/webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> MoMoWebhook()
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();
        var headers = Request.Headers.ToDictionary(x => x.Key, x => x.Value.ToString());

        var result = await momoService.ProcessWebhookAsync(body, headers);
        
        if (result.IsSuccess) return NoContent();
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("vnpay/webhook")] // VNPay calls IPN via GET
    [AllowAnonymous]
    public async Task<IActionResult> VNPayWebhook()
    {
        var queryParams = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());
        var result = await vnpayService.ProcessWebhookAsync(queryParams);

        if (result.IsSuccess) return Ok(new { RspCode = "00", Message = "Confirm Success" });
        return Ok(new { RspCode = "99", Message = result.ErrorMessage });
    }

    [HttpGet("vnpay/return")]
    [AllowAnonymous]
    public async Task<IActionResult> VNPayReturn()
    {
        var queryParams = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());
        var result = await vnpayService.ProcessReturnAsync(queryParams);
        
        // Redirect to frontend payment status page
        var baseUrl = "http://localhost:3000"; // Should be from config
        var status = result.IsSuccess ? "success" : "failed";
        return Redirect($"{baseUrl}/payment-status?status={status}&invoiceCode={result.InvoiceCode}");
    }
}
