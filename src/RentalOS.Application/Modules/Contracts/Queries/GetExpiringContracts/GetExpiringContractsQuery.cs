using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Domain.Enums;

namespace RentalOS.Application.Modules.Contracts.Queries.GetExpiringContracts;

public record GetExpiringContractsQuery : IRequest<List<ExpiringContractDto>>;

public record ExpiringContractDto(
    Guid Id,
    string ContractCode,
    string CustomerName,
    string RoomNumber,
    DateOnly EndDate,
    int DaysRemaining);

public class GetExpiringContractsQueryHandler(IApplicationDbContext context) : IRequestHandler<GetExpiringContractsQuery, List<ExpiringContractDto>>
{
    public async Task<List<ExpiringContractDto>> Handle(GetExpiringContractsQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var thirtyDaysLater = today.AddDays(30);

        return await context.Contracts
            .Include(c => c.Customer)
            .Include(c => c.Room)
            .Where(c => c.Status == ContractStatus.Active && 
                        c.EndDate >= today && 
                        c.EndDate <= thirtyDaysLater)
            .Select(c => new ExpiringContractDto(
                c.Id,
                c.ContractCode,
                c.Customer.FullName,
                c.Room.RoomNumber,
                c.EndDate,
                c.EndDate.DayNumber - today.DayNumber))
            .OrderBy(c => c.EndDate)
            .ToListAsync(cancellationToken);
    }
}
