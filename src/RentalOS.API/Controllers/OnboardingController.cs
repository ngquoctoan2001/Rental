using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalOS.Application.Modules.Onboarding.Commands.CompleteOnboarding;
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
    public async Task<IActionResult> Complete()
    {
        return Ok(await mediator.Send(new CompleteOnboardingCommand()));
    }

    [HttpGet("steps")]
    public async Task<IActionResult> GetSteps()
    {
        var status = await mediator.Send(new GetOnboardingStatusQuery());
        return Ok(new[]
        {
            new { step = 1, title = "Hồ sơ doanh nghiệp", done = status.HasProfile },
            new { step = 2, title = "Thêm cơ sở đầu tiên", done = status.HasProperty },
            new { step = 3, title = "Thêm phòng đầu tiên", done = status.HasRoom },
        });
    }

    [HttpPost("reset")]
    public async Task<IActionResult> Reset() => Ok();
}
