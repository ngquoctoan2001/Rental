using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalOS.Application.Modules.AI.Commands.AiChat;
using RentalOS.Infrastructure.Services.AI;
using System.Text.Json;

namespace RentalOS.API.Controllers;

[Authorize(Roles = "admin,landlord")]
[ApiController]
[Route("api/v1/[controller]")]
public class AiController(ISender mediator) : ControllerBase
{
    [HttpPost("chat")]
    public async Task Chat([FromBody] AiChatRequest request)
    {
        Response.ContentType = "text/event-stream";
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Connection", "keep-alive");

        var stream = await mediator.Send(new AiChatCommand(request.ConversationId, request.Message));

        await foreach (var chunk in stream)
        {
            var json = JsonSerializer.Serialize(chunk);
            await Response.WriteAsync($"data: {json}\n\n");
            await Response.Body.FlushAsync();
        }
    }

    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations() => Ok();

    [HttpDelete("conversations/{id}")]
    public async Task<IActionResult> DeleteConversation(Guid id) => Ok();
}

public record AiChatRequest(string? ConversationId, string Message);
