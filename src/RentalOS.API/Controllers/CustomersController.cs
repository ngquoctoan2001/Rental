using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalOS.Application.Modules.Customers.Commands.BlacklistCustomer;
using RentalOS.Application.Modules.Customers.Commands.CreateCustomer;
using RentalOS.Application.Modules.Customers.Commands.OcrIdCard;
using RentalOS.Application.Modules.Customers.Commands.UpdateCustomer;
using RentalOS.Application.Modules.Customers.Commands.UploadCustomerImage;
using RentalOS.Application.Modules.Customers.Queries.GetCustomerById;
using RentalOS.Application.Modules.Customers.Queries.GetCustomerContracts;
using RentalOS.Application.Modules.Customers.Queries.GetCustomerInvoices;
using RentalOS.Application.Modules.Customers.Queries.GetCustomers;
using RentalOS.Application.Modules.Customers.Queries.SearchCustomers;
using System.Security.Claims;

namespace RentalOS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class CustomersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> GetCustomers([FromQuery] GetCustomersQuery query)
    {
        return Ok(await mediator.Send(query));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetCustomerById(Guid id)
    {
        var result = await mediator.Send(new GetCustomerByIdQuery(id));
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage);
    }

    [HttpPost]
    public async Task<ActionResult> CreateCustomer(CreateCustomerCommand command)
    {
        var result = await mediator.Send(command);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateCustomer(Guid id, UpdateCustomerCommand command)
    {
        if (id != command.Id) return BadRequest("Mã khách hàng không khớp.");
        var result = await mediator.Send(command);
        return result.IsSuccess ? NoContent() : BadRequest(result.ErrorMessage);
    }

    [HttpPatch("{id}/blacklist")]
    [Authorize(Roles = "Owner")]
    public async Task<ActionResult> BlacklistCustomer(Guid id, [FromBody] BlacklistRequest request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();

        var userId = Guid.Parse(userIdClaim);
        var result = await mediator.Send(new BlacklistCustomerCommand 
        { 
            Id = id, 
            IsBlacklist = request.IsBlacklist, 
            Reason = request.Reason,
            ActionBy = userId
        });
        return result.IsSuccess ? Ok() : BadRequest(result.ErrorMessage);
    }

    [HttpPost("{id}/images")]
    public async Task<ActionResult> UploadImage(Guid id, [FromForm] IFormFile file, [FromQuery] string type = "portrait")
    {
        using var stream = file.OpenReadStream();
        var result = await mediator.Send(new UploadCustomerImageCommand
        {
            CustomerId = id,
            FileStream = stream,
            FileName = file.FileName,
            ContentType = file.ContentType,
            ImageType = type
        });
        return result.IsSuccess ? Ok(new { url = result.Data }) : BadRequest(result.ErrorMessage);
    }

    [HttpPost("ocr")]
    public async Task<ActionResult> OcrIdCard([FromForm] IFormFile file)
    {
        using var stream = file.OpenReadStream();
        var result = await mediator.Send(new OcrIdCardCommand
        {
            FileStream = stream,
            FileName = file.FileName,
            ContentType = file.ContentType
        });
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpGet("lookup")]
    public async Task<ActionResult> LookupCustomers([FromQuery] string q)
    {
        return Ok(await mediator.Send(new SearchCustomersQuery(q)));
    }

    [HttpGet("{id}/contracts")]
    public async Task<ActionResult> GetCustomerContracts(Guid id)
    {
        var result = await mediator.Send(new GetCustomerContractsQuery(id));
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpGet("{id}/invoices")]
    public async Task<ActionResult> GetCustomerInvoices(Guid id)
    {
        var result = await mediator.Send(new GetCustomerInvoicesQuery(id));
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }
}

public record BlacklistRequest(bool IsBlacklist, string? Reason);
