using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalOS.Application.Modules.Onboarding.Queries.GetOnboardingStatus;

namespace RentalOS.API.Controllers;

[Authorize(Roles = "owner")]
[ApiController]
[Route("api/v1/[controller]")]
public class OnboardingController(ISender mediator) : ControllerBase
{
    [HttpGet("status")]
    public async Task<ActionResult<OnboardingStatusDto>> GetStatus()
    {
        return Ok(await mediator.Send(new GetOnboardingStatusQuery()));
    }

    [HttpPost("complete")]
    public async Task<IActionResult> Complete() => Ok();

    [HttpGet("steps")]
    public async Task<IActionResult> GetSteps() => Ok();

    [HttpPost("reset")]
    public async Task<IActionResult> Reset() => Ok();
}
 Eskom onboarding logic endpoints. Eskom owner authorization logic.
