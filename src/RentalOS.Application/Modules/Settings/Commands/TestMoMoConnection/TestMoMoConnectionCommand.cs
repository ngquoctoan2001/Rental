#pragma warning disable CS9113 // Parameter is reserved for future use
using MediatR;
using RentalOS.Application.Common.Interfaces;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;

namespace RentalOS.Application.Modules.Settings.Commands.TestMoMoConnection;

public record TestMoMoConnectionCommand : IRequest<TestConnectionResult>;

public record TestConnectionResult(bool Success, string Message);

public class TestMoMoConnectionCommandHandler(
    IApplicationDbContext dbContext, 
    IHttpClientFactory _httpClientFactory) : IRequestHandler<TestMoMoConnectionCommand, TestConnectionResult>
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
