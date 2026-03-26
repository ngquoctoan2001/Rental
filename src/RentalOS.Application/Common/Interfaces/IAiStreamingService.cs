using RentalOS.Application.Common.Models;

namespace RentalOS.Application.Common.Interfaces;

public interface IAiStreamingService
{
    IAsyncEnumerable<AiStreamChunk> StreamChatAsync(
        IEnumerable<object> messages,
        string systemPrompt,
        CancellationToken cancellationToken);
}
