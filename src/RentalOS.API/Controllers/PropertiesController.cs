using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalOS.Application.Common.Models;
using RentalOS.Application.Modules.Properties.Commands.CreateProperty;
using RentalOS.Application.Modules.Properties.Commands.DeleteProperty;
using RentalOS.Application.Modules.Properties.Commands.UpdateProperty;
using RentalOS.Application.Modules.Properties.Commands.UploadPropertyImage;
using RentalOS.Application.Modules.Properties.Dtos;
using RentalOS.Application.Modules.Properties.Queries.GetProperties;
using RentalOS.Application.Modules.Properties.Queries.GetPropertyById;
using RentalOS.Application.Modules.Properties.Queries.GetPropertyStats;
using RentalOS.Application.Modules.Rooms.Queries.GetRooms;
using RentalOS.Application.Modules.Rooms.Dtos;

namespace RentalOS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class PropertiesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<PropertyListItemDto>>> GetProperties([FromQuery] GetPropertiesQuery query)
    {
        return Ok(await mediator.Send(query));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PropertyDto>> GetProperty(Guid id)
    {
        var result = await mediator.Send(new GetPropertyByIdQuery(id));
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage);
    }

    [HttpGet("{id}/stats")]
    public async Task<ActionResult<PropertyStatsDto>> GetPropertyStats(Guid id)
    {
        var result = await mediator.Send(new GetPropertyStatsQuery(id));
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage);
    }

    [HttpGet("{id}/rooms")]
    public async Task<ActionResult<PagedResult<RoomListItemDto>>> GetPropertyRooms(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetRoomsQuery 
        { 
            PropertyId = id,
            Page = page,
            PageSize = pageSize
        };
        return Ok(await mediator.Send(query));
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateProperty([FromBody] CreatePropertyCommand command)
    {
        var result = await mediator.Send(command);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateProperty(Guid id, [FromBody] UpdatePropertyCommand command)
    {
        if (id != command.Id) return BadRequest("Mã nhà trọ không khớp.");
        
        var result = await mediator.Send(command);
        return result.IsSuccess ? NoContent() : NotFound(result.ErrorMessage);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteProperty(Guid id)
    {
        var result = await mediator.Send(new DeletePropertyCommand(id));
        return result.IsSuccess ? NoContent() : BadRequest(result.ErrorMessage);
    }

    [HttpPost("{id}/images")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB
    public async Task<ActionResult<string>> UploadImage(Guid id, IFormFile file, [FromQuery] bool isCover = false)
    {
        if (file == null || file.Length == 0) return BadRequest("File không hợp lệ.");
        
        var extension = Path.GetExtension(file.FileName).ToLower();
        if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
        {
            return BadRequest("Chỉ hỗ trợ định dạng .jpg, .jpeg, .png");
        }

        using var stream = file.OpenReadStream();
        var command = new UploadPropertyImageCommand
        {
            PropertyId = id,
            FileStream = stream,
            FileName = file.FileName,
            ContentType = file.ContentType,
            IsCover = isCover
        };

        var result = await mediator.Send(command);
        return result.IsSuccess ? Ok(new { url = result.Data }) : BadRequest(result.ErrorMessage);
    }
}
