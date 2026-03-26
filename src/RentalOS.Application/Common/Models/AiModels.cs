namespace RentalOS.Application.Common.Models;

public enum AiChunkType { Text, ToolUse, ToolResult, Done }

public record AiStreamChunk(AiChunkType Type, string Content, string? ToolName = null, string? ToolId = null);
