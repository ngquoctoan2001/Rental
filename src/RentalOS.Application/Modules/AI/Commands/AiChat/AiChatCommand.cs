using MediatR;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;
using RentalOS.Application.Common.Services;
using RentalOS.Application.Modules.AI.Services;
using System.Runtime.CompilerServices;

namespace RentalOS.Application.Modules.AI.Commands.AiChat;

public record AiChatCommand(string? ConversationId, string Message) : IRequest<IAsyncEnumerable<AiStreamChunk>>;

public class AiChatCommandHandler(
    IApplicationDbContext dbContext,
    ITenantContext tenantContext,
    IPlanService planService,
    IAiStreamingService aiService,
    AiToolHandler toolHandler) : IRequestHandler<AiChatCommand, IAsyncEnumerable<AiStreamChunk>>
{
    public async Task<IAsyncEnumerable<AiStreamChunk>> Handle(AiChatCommand request, CancellationToken cancellationToken)
    {
        // 1. Feature Gate
        var tenant = await dbContext.Tenants.FindAsync(tenantContext.TenantId);
        if (tenant == null || !planService.HasFeature(tenant.Plan, "ai"))
            throw new Exception("Gói cước hiện tại không hỗ trợ AI Agent.");

        // 2. Build flow
        return StreamResponse(request, cancellationToken);
    }

    private async IAsyncEnumerable<AiStreamChunk> StreamResponse(AiChatCommand request, [EnumeratorCancellation] CancellationToken ct)
    {
        // logic xử lý conversation history & Anthropic stream
        await foreach (var chunk in aiService.StreamChatAsync(new List<object>(), "System Prompt", ct))
        {
            if (chunk.Type == AiChunkType.ToolUse)
            {
                yield return new AiStreamChunk(AiChunkType.Text, $"\n[AI đang sử dụng công cụ: {chunk.ToolName}...]\n");
                var toolResult = await toolHandler.ExecuteToolAsync(chunk.ToolName!, chunk.Content);
                yield return new AiStreamChunk(AiChunkType.ToolResult, toolResult);
            }
            else
            {
                yield return chunk;
            }
        }
    }
}

