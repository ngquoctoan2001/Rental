using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalOS.Application.Common.Models;
using RentalOS.Application.Modules.CoTenants.Commands.AddCoTenant;
using RentalOS.Application.Modules.CoTenants.Commands.RemoveCoTenantByCustomer;
using RentalOS.Application.Modules.Contracts.Commands.CreateContract;
using RentalOS.Application.Modules.Contracts.Commands.RenewContract;
using RentalOS.Application.Modules.Contracts.Commands.SignContract;
using RentalOS.Application.Modules.Contracts.Commands.TerminateContract;
using RentalOS.Application.Modules.Contracts.Commands.UpdateContract;
using RentalOS.Application.Modules.Contracts.Queries.GetContractById;
using RentalOS.Application.Modules.Contracts.Queries.GetContractInvoices;
using RentalOS.Application.Modules.Contracts.Queries.GetContractPdf;
using RentalOS.Application.Modules.Contracts.Queries.GetContracts;
using RentalOS.Application.Modules.Contracts.Queries.GetExpiringContracts;
using RentalOS.Domain.Enums;

namespace RentalOS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class ContractsController(IMediator mediator) : ControllerBase
{
    // 1. GET /api/v1/contracts - Danh sách hợp đồng
    [HttpGet]
    public async Task<ActionResult> GetContracts([FromQuery] ContractStatus? status, [FromQuery] string? search, [FromQuery] Guid? propertyId)
    {
        var result = await mediator.Send(new GetContractsQuery { Status = status, SearchTerm = search, PropertyId = propertyId });
        return Ok(result.Data);
    }

    // 2. GET /api/v1/contracts/{id} - Chi tiết hợp đồng
    [HttpGet("{id}")]
    public async Task<ActionResult> GetContract(Guid id)
    {
        var result = await mediator.Send(new GetContractByIdQuery(id));
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage);
    }

    // 3. POST /api/v1/contracts - Tạo hợp đồng mới (11 bước logic)
    [HttpPost]
    public async Task<ActionResult> CreateContract(CreateContractCommand command)
    {
        var result = await mediator.Send(command);
        return result.IsSuccess ? CreatedAtAction(nameof(GetContract), new { id = result.Data }, result.Data) : BadRequest(new { error = result.ErrorMessage, code = result.ErrorCode });
    }

    // 3b. PUT /api/v1/contracts/{id} - Cập nhật hợp đồng
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateContract(Guid id, UpdateContractCommand command)
    {
        if (id != command.Id) return BadRequest("Mã hợp đồng không khớp.");
        var result = await mediator.Send(command);
        return result.IsSuccess ? Ok() : BadRequest(result.ErrorMessage);
    }

    // 4. PUT /api/v1/contracts/{id}/terminate - Thanh lý hợp đồng
    [HttpPut("{id}/terminate")]
    public async Task<ActionResult> TerminateContract(Guid id, TerminateContractCommand command)
    {
        if (id != command.Id) return BadRequest("Mã hợp đồng không khớp.");
        var result = await mediator.Send(command);
        return result.IsSuccess ? Ok() : BadRequest(result.ErrorMessage);
    }

    // 5. POST /api/v1/contracts/{id}/renew - Gia hạn hợp đồng
    [HttpPost("{id}/renew")]
    public async Task<ActionResult> RenewContract(Guid id, RenewContractCommand command)
    {
        if (id != command.OldContractId) return BadRequest("Mã hợp đồng không khớp.");
        var result = await mediator.Send(command);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    // 6. POST /api/v1/contracts/{id}/sign - Khách ký tên
    [HttpPost("{id}/sign")]
    public async Task<ActionResult> SignContract(Guid id)
    {
        var result = await mediator.Send(new SignContractCommand(id));
        return result.IsSuccess ? Ok() : BadRequest(result.ErrorMessage);
    }

    // 7. GET /api/v1/contracts/expiring - Hợp đồng sắp hết hạn (30 ngày)
    [HttpGet("expiring")]
    public async Task<ActionResult> GetExpiringContracts()
    {
        var result = await mediator.Send(new GetExpiringContractsQuery());
        return Ok(result);
    }

    // 8. GET /api/v1/contracts/{id}/invoices - Danh sách hóa đơn của hợp đồng
    [HttpGet("{id}/invoices")]
    public async Task<ActionResult> GetContractInvoices(Guid id)
    {
        var result = await mediator.Send(new GetContractInvoicesQuery(id));
        return Ok(result.Data);
    }

    // 9. GET /api/v1/contracts/{id}/pdf - Lấy link PDF hợp đồng
    [HttpGet("{id}/pdf")]
    public async Task<ActionResult> GetContractPdf(Guid id)
    {
        var result = await mediator.Send(new GetContractPdfQuery(id));
        return result.IsSuccess ? Ok(new { url = result.Data }) : BadRequest(result.ErrorMessage);
    }

    // 10. POST /api/v1/contracts/{id}/cotenants - Thêm người ở cùng
    [HttpPost("{id}/cotenants")]
    public async Task<ActionResult> AddCoTenant(Guid id, AddCoTenantCommand command)
    {
        if (id != command.ContractId) return BadRequest("Mã hợp đồng không khớp.");
        var result = await mediator.Send(command);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    // 11. DELETE /api/v1/contracts/{id}/cotenants/{customerId} - Xóa người ở cùng
    [HttpDelete("{id}/cotenants/{customerId}")]
    public async Task<ActionResult> RemoveCoTenant(Guid id, Guid customerId)
    {
        var result = await mediator.Send(new RemoveCoTenantByCustomerCommand(id, customerId));
        return result.IsSuccess ? NoContent() : BadRequest(result.ErrorMessage);
    }
}
