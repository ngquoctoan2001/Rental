using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalOS.Application.Common.Models;
using RentalOS.Application.Modules.MeterReadings.Commands.CreateMeterReading;
using RentalOS.Application.Modules.MeterReadings.Commands.DeleteMeterReading;
using RentalOS.Application.Modules.MeterReadings.Commands.UpdateMeterReading;
using RentalOS.Application.Modules.MeterReadings.Dtos;
using RentalOS.Application.Modules.MeterReadings.Queries.GetMeterReadings;
using RentalOS.Application.Modules.MeterReadings.Queries.GetMeterReadingById;

namespace RentalOS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/meter-readings")]
public class MeterReadingsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<MeterReadingDto>>> GetMeterReadings([FromQuery] GetMeterReadingsQuery query)
    {
        return Ok(await mediator.Send(query));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MeterReadingDto>> GetMeterReadingById(Guid id)
    {
        return Ok(await mediator.Send(new GetMeterReadingByIdQuery(id)));
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateMeterReading([FromBody] CreateMeterReadingCommand command)
    {
        var id = await mediator.Send(command);
        return CreatedAtAction(nameof(GetMeterReadingById), new { id }, id);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMeterReading(Guid id, [FromBody] UpdateMeterReadingCommand command)
    {
        if (id != command.Id) return BadRequest();
        await mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMeterReading(Guid id)
    {
        await mediator.Send(new DeleteMeterReadingCommand(id));
        return NoContent();
    }
}
