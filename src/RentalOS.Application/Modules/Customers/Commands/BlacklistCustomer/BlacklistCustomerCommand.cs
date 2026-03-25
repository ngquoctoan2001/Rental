using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;

namespace RentalOS.Application.Modules.Customers.Commands.BlacklistCustomer;

public record BlacklistCustomerCommand : IRequest<Result<bool>>
{
    public Guid Id { get; init; }
    public bool IsBlacklist { get; init; }
    public string? Reason { get; init; }
    public Guid ActionBy { get; init; } // Set by controller from JWT
}

public class BlacklistCustomerCommandHandler : IRequestHandler<BlacklistCustomerCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public BlacklistCustomerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(BlacklistCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (customer == null)
        {
            return Result<bool>.Fail("CUSTOMER_NOT_FOUND", "Không tìm thấy khách hàng.");
        }

        if (request.IsBlacklist)
        {
            customer.IsBlacklisted = true;
            customer.BlacklistReason = request.Reason;
            customer.BlacklistedAt = DateTime.UtcNow;
            customer.BlacklistedBy = request.ActionBy;
        }
        else
        {
            customer.IsBlacklisted = false;
            customer.BlacklistReason = null;
            customer.BlacklistedAt = null;
            customer.BlacklistedBy = null;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Ok(true);
    }
}
