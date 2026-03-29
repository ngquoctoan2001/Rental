using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalOS.Application.Modules.Subscriptions.Queries.GetCurrentSubscription;

namespace RentalOS.API.Controllers;

[Authorize(Roles = "admin,landlord")]
[ApiController]
[Route("api/v1/[controller]")]
public class SubscriptionsController(ISender mediator) : ControllerBase
{
    [HttpGet("current")]
    public async Task<ActionResult<SubscriptionDetailsDto>> GetCurrent()
    {
        return Ok(await mediator.Send(new GetCurrentSubscriptionQuery()));
    }

    [HttpPost("upgrade")]
    public async Task<IActionResult> Upgrade([FromBody] string plan) => Ok(new { paymentUrl = "https://momo.vn/pay/..." });

    [HttpGet("plans")]
    public async Task<IActionResult> GetPlans() => Ok();

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory() => Ok();

    [HttpPost("cancel")]
    public async Task<IActionResult> Cancel() => Ok();
    
    [HttpGet("check-payment/{id}")]
    public async Task<IActionResult> CheckPayment(Guid id) => Ok();
    
    [HttpPost("apply-promo")]
    public async Task<IActionResult> ApplyPromo([FromBody] string code) => Ok();
}
