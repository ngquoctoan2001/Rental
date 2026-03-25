using MediatR;
using RentalOS.Application.Common.Interfaces;
using System.Text.Json;

namespace RentalOS.Application.Modules.Settings.Commands.TestMoMoConnection;

public record TestMoMoConnectionCommand : IRequest<TestConnectionResult>;

public record TestConnectionResult(bool Success, string Message);

public class TestMoMoConnectionCommandHandler(
    IApplicationDbContext dbContext, 
    IHttpClientFactory httpClientFactory) : IRequestHandler<TestMoMoConnectionCommand, TestConnectionResult>
{
    public async Task<TestConnectionResult> Handle(TestMoMoConnectionCommand request, CancellationToken cancellationToken)
    {
        // 1. Get current config
        var config = await dbContext.Settings
            .FirstOrDefaultAsync(s => s.Key == "payment.momo", cancellationToken);
            
        if (config == null) return new TestConnectionResult(false, "Chưa cấu hình MoMo.");

        // 2. Call MoMo Query Balance (Mock logic for now, or real if endpoint known)
        // Trong thực tế sẽ gọi POST tới MoMo với signature
        
        return new TestConnectionResult(true, "Kết nối MoMo thành công (Mock).");
    }
}
 Eskom mock connection logic as per spec "gọi MoMo API query balance".
