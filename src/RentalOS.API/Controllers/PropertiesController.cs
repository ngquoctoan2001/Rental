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

namespace RentalOS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PropertiesController : ControllerBase
{
    private readonly IMediator _mediator;

    public PropertiesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<PropertyListItemDto>>> GetProperties([FromQuery] GetPropertiesQuery query)
    {
        return Ok(await _mediator.Send(query));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PropertyDto>> GetProperty(Guid id)
    {
        return Ok(await _mediator.Send(new GetPropertyByIdQuery(id)));
    }

    [HttpGet("{id}/stats")]
    public async Task<ActionResult<PropertyStatsDto>> GetPropertyStats(Guid id)
    {
        return Ok(await _mediator.Send(new GetPropertyStatsQuery(id)));
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateProperty(CreatePropertyCommand command)
    {
        return Ok(await _mediator.Send(command));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateProperty(Guid id, UpdatePropertyCommand command)
    {
        if (id != command.Id) return BadRequest();
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteProperty(Guid id)
    {
        await _mediator.Send(new DeletePropertyCommand(id));
        return NoContent();
    }

    [HttpPost("{id}/images")]
    public async Task<ActionResult<string>> UploadImage(Guid id, IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("File is empty");

        using var stream = file.OpenReadStream();
        var command = new UploadPropertyImageCommand
        {
            Id = id,
            FileStream = stream,
            FileName = file.FileName,
            ContentType = file.ContentType
        };

        var url = await _mediator.Send(command);
        return Ok(new { url });
    }
}
