using Microsoft.AspNetCore.Mvc;
using RentalOS.Application.Modules.Transactions.Commands.RecordCashPayment;
using RentalOS.Application.Modules.Transactions.Commands.RecordDepositRefund;
using RentalOS.Application.Modules.Transactions.Queries.ExportTransactions;
using RentalOS.Application.Modules.Transactions.Queries.GetTransactionById;
using RentalOS.Application.Modules.Transactions.Queries.GetTransactionSummary;
using RentalOS.Application.Modules.Transactions.Queries.GetTransactions;
using RentalOS.Application.Modules.Transactions.DTOs;
using RentalOS.Application.Common.Models;
using MediatR;

namespace RentalOS.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class TransactionsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<TransactionDto>>> GetTransactions([FromQuery] GetTransactionsQuery query)
    {
        var result = await mediator.Send(query);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TransactionDto>> GetById(Guid id)
    {
        var result = await mediator.Send(new GetTransactionByIdQuery(id));
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<TransactionSummaryDto>> GetSummary([FromQuery] GetTransactionSummaryQuery query)
    {
        var result = await mediator.Send(query);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] ExportTransactionsQuery query)
    {
        var result = await mediator.Send(query);
        if (!result.IsSuccess) return BadRequest(result.ErrorMessage);

        var dateStr = DateTime.Now.ToString("yyyyMMdd");
        return File(result.Data!, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Transactions_{dateStr}.xlsx");
    }

    [HttpPost("cash-payment")]
    public async Task<ActionResult<Guid>> RecordCashPayment([FromBody] RecordCashPaymentCommand command)
    {
        var result = await mediator.Send(command);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpPost("deposit-refund")]
    public async Task<ActionResult<Guid>> RecordDepositRefund([FromBody] RecordDepositRefundCommand command)
    {
        var result = await mediator.Send(command);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }
}
