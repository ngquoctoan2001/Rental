using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalOS.Application.Modules.Invoices.Queries.GetInvoiceByPaymentToken;

namespace RentalOS.API.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/v1/[controller]")]
public class PublicController(IMediator mediator) : ControllerBase
{
    // GET /api/v1/public/invoice/{token}
    [HttpGet("invoice/{token}")]
    public async Task<ActionResult> GetInvoiceByToken(string token)
    {
        var result = await mediator.Send(new GetInvoiceByPaymentTokenQuery(token));
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage);
    }
}
