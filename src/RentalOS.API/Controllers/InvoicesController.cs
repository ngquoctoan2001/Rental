using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalOS.Application.Modules.Invoices.Commands.BulkGenerateInvoices;
using RentalOS.Application.Modules.Invoices.Commands.CancelInvoice;
using RentalOS.Application.Modules.Invoices.Commands.CreateInvoice;
using RentalOS.Application.Modules.Invoices.Commands.SendInvoice;
using RentalOS.Application.Modules.Invoices.Commands.UpdateInvoiceMeter;
using RentalOS.Application.Modules.Invoices.Queries;
using RentalOS.Application.Modules.Invoices.Queries.GetInvoiceById;
using RentalOS.Application.Modules.Invoices.Queries.GetInvoices;
using RentalOS.Application.Modules.Invoices.Queries.GetPendingMeter;

namespace RentalOS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class InvoicesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> GetInvoices([FromQuery] GetInvoicesQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetInvoice(Guid id)
    {
        var result = await mediator.Send(new GetInvoiceByIdQuery(id));
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage);
    }

    [HttpGet("pending-meter")]
    public async Task<ActionResult> GetPendingMeter([FromQuery] GetPendingMeterInvoicesQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<ActionResult> CreateInvoice(CreateInvoiceCommand command)
    {
        var result = await mediator.Send(command);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpPost("bulk-generate")]
    public async Task<ActionResult> BulkGenerate(BulkGenerateInvoicesCommand command)
    {
        var result = await mediator.Send(command);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpPut("{id}/meter")]
    public async Task<ActionResult> UpdateMeter(Guid id, UpdateInvoiceMeterCommand command)
    {
        if (id != command.InvoiceId) return BadRequest("Mã hóa đơn không khớp.");
        var result = await mediator.Send(command);
        return result.IsSuccess ? Ok() : BadRequest(result.ErrorMessage);
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult> CancelInvoice(Guid id, CancelInvoiceCommand command)
    {
        if (id != command.InvoiceId) return BadRequest("Mã hóa đơn không khớp.");
        var result = await mediator.Send(command);
        return result.IsSuccess ? Ok() : BadRequest(result.ErrorMessage);
    }

    [HttpPost("{id}/send")]
    public async Task<ActionResult> SendInvoice(Guid id, SendInvoiceCommand command)
    {
        if (id != command.InvoiceId) return BadRequest("Mã hóa đơn không khớp.");
        var result = await mediator.Send(command);
        return result.IsSuccess ? Ok() : BadRequest(result.ErrorMessage);
    }
}
