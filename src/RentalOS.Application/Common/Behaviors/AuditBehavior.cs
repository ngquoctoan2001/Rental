using MediatR;
using Microsoft.Extensions.Logging;

namespace RentalOS.Application.Common.Behaviors;

public class AuditBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // TODO: write audit trail to DB
        return await next();
    }
}
