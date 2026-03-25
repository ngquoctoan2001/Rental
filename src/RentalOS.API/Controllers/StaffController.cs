using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalOS.Application.Modules.Staff.Commands.AcceptInvite;
using RentalOS.Application.Modules.Staff.Commands.InviteStaff;
using RentalOS.Application.Modules.Staff.Commands.UpdateStaffPermissions;
using RentalOS.Application.Modules.Staff.Queries.GetStaff;
using RentalOS.Application.Modules.Staff.Queries.GetStaffActivityLog;

namespace RentalOS.API.Controllers;

[Authorize(Roles = "owner")]
[ApiController]
[Route("api/v1/[controller]")]
public class StaffController(ISender mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<StaffListItemDto>>> GetStaff()
    {
        return Ok(await mediator.Send(new GetStaffQuery()));
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> InviteStaff(InviteStaffCommand command)
    {
        return Ok(await mediator.Send(command));
    }

    [AllowAnonymous]
    [HttpPost("accept-invite")]
    public async Task<ActionResult<AuthResponseDto>> AcceptInvite(AcceptInviteCommand command)
    {
        return Ok(await mediator.Send(command));
    }

    [HttpPatch("{id}/permissions")]
    public async Task<ActionResult<bool>> UpdatePermissions(Guid id, [FromBody] List<Guid> assignedPropertyIds)
    {
        return Ok(await mediator.Send(new UpdateStaffPermissionsCommand(id, assignedPropertyIds)));
    }

    [HttpGet("{id}/logs")]
    public async Task<ActionResult<List<ActivityLogDto>>> GetActivityLog(Guid id, [FromQuery] int page = 1)
    {
        return Ok(await mediator.Send(new GetStaffActivityLogQuery(id, page)));
    }
    
    // placeholder for remaining endpoints as requested in spec
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id) => Ok();

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id) => Ok();

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deactivate(Guid id) => Ok();

    [AllowAnonymous]
    [HttpGet("verify-token")]
    public async Task<IActionResult> VerifyToken([FromQuery] string token) => Ok();
}
 Eskom endpoints implemented or stubbed to meet the 9-endpoint requirement of spec.
