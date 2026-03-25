using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using RentalOS.Application.Common.Models;
using RentalOS.Application.Modules.Rooms.Commands.BulkCreateRooms;
using RentalOS.Application.Modules.Rooms.Commands.ChangeRoomStatus;
using RentalOS.Application.Modules.Rooms.Commands.CreateRoom;
using RentalOS.Application.Modules.Rooms.Commands.DeleteRoom;
using RentalOS.Application.Modules.Rooms.Commands.UpdateRoom;
using RentalOS.Application.Modules.Rooms.Dtos;
using RentalOS.Application.Modules.Rooms.Queries.GetAvailableRooms;
using RentalOS.Application.Modules.Rooms.Queries.GetRoomById;
using RentalOS.Application.Modules.Rooms.Queries.GetRoomHistory;
using RentalOS.Application.Modules.Rooms.Queries.GetRoomMeterReadings;
using RentalOS.Application.Modules.Rooms.Queries.GetRoomQrCode;
using RentalOS.Application.Modules.Rooms.Queries.GetRooms;
using RentalOS.Application.Modules.MeterReadings.Dtos;
using RentalOS.Application.Modules.Rooms.Commands.UploadRoomImage;

namespace RentalOS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class RoomsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<RoomListItemDto>>> GetRooms([FromQuery] GetRoomsQuery query)
    {
        return Ok(await mediator.Send(query));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RoomDto>> GetRoomById(Guid id)
    {
        return Ok(await mediator.Send(new GetRoomByIdQuery(id)));
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateRoom([FromBody] CreateRoomCommand command)
    {
        var id = await mediator.Send(command);
        return CreatedAtAction(nameof(GetRoomById), new { id }, id);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRoom(Guid id, [FromBody] UpdateRoomCommand command)
    {
        if (id != command.Id) return BadRequest();
        await mediator.Send(command);
        return NoContent();
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> ChangeRoomStatus(Guid id, [FromBody] ChangeRoomStatusCommand command)
    {
        if (id != command.Id) return BadRequest();
        await mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRoom(Guid id)
    {
        await mediator.Send(new DeleteRoomCommand(id));
        return NoContent();
    }

    [HttpPost("bulk-create")]
    public async Task<ActionResult<List<Guid>>> BulkCreateRooms([FromBody] BulkCreateRoomsCommand command)
    {
        return Ok(await mediator.Send(command));
    }

    [HttpGet("available")]
    public async Task<ActionResult<List<RoomListItemDto>>> GetAvailableRooms([FromQuery] Guid? propertyId)
    {
        return Ok(await mediator.Send(new GetAvailableRoomsQuery(propertyId)));
    }

    [HttpGet("{id}/qrcode")]
    public async Task<IActionResult> GetRoomQrCode(Guid id)
    {
        var imageBytes = await mediator.Send(new GetRoomQrCodeQuery(id));
        return File(imageBytes, "image/png");
    }

    [HttpGet("{id}/history")]
    public async Task<ActionResult<RoomHistoryDto>> GetRoomHistory(Guid id)
    {
        return Ok(await mediator.Send(new GetRoomHistoryQuery(id)));
    }

    [HttpGet("{id}/meter-readings")]
    public async Task<ActionResult<List<MeterReadingDto>>> GetRoomMeterReadings(Guid id, [FromQuery] string? month)
    {
        return Ok(await mediator.Send(new GetRoomMeterReadingsQuery(id, month)));
    }

    [HttpPost("{id}/images")]
    public async Task<ActionResult<string>> UploadRoomImage(Guid id, IFormFile file)
    {
        using var stream = file.OpenReadStream();
        var command = new UploadRoomImageCommand
        {
            RoomId = id,
            FileStream = stream,
            FileName = file.FileName,
            ContentType = file.ContentType
        };

        var result = await mediator.Send(command);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }
}
