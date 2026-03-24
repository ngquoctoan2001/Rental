using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Modules.Properties.Dtos;
using RentalOS.Domain.Enums;

namespace RentalOS.Application.Modules.Properties.Queries.GetPropertyStats;

public record GetPropertyStatsQuery(Guid Id) : IRequest<PropertyStatsDto>;

public class GetPropertyStatsQueryHandler : IRequestHandler<GetPropertyStatsQuery, PropertyStatsDto>
{
    private readonly IApplicationDbContext _context;

    public GetPropertyStatsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PropertyStatsDto> Handle(GetPropertyStatsQuery request, CancellationToken cancellationToken)
    {
        var property = await _context.Properties
            .Include(p => p.Rooms)
                .ThenInclude(r => r.Contracts)
                    .ThenInclude(c => c.Invoices)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (property == null)
        {
            throw new Exception("PROPERTY_NOT_FOUND");
        }

        var rooms = property.Rooms.ToList();
        var totalRooms = rooms.Count;
        
        if (totalRooms == 0)
        {
            return new PropertyStatsDto();
        }

        var rentedRooms = rooms.Count(r => r.Status == RoomStatus.Rented);
        var availableRooms = rooms.Count(r => r.Status == RoomStatus.Available);
        var maintenanceRooms = rooms.Count(r => r.Status == RoomStatus.Maintenance);
        
        var occupancyRate = Math.Round((double)rentedRooms / totalRooms * 100, 2);

        var now = DateTime.UtcNow;
        var currentMonth = new DateOnly(now.Year, now.Month, 1);
        var next30Days = DateOnly.FromDateTime(now.AddDays(30));
        var today = DateOnly.FromDateTime(now);

        // Revenue this month (Paid invoices)
        var monthlyRevenue = rooms
            .SelectMany(r => r.Contracts)
            .SelectMany(c => c.Invoices)
            .Where(i => i.BillingMonth == currentMonth && i.Status == InvoiceStatus.Paid)
            .Sum(i => i.TotalAmount);

        // Outstanding
        var outstandingInvoices = rooms
            .SelectMany(r => r.Contracts)
            .SelectMany(c => c.Invoices)
            .Where(i => i.Status == InvoiceStatus.Pending || i.Status == InvoiceStatus.Overdue || i.Status == InvoiceStatus.Partial)
            .ToList();

        var totalOutstandingAmount = outstandingInvoices.Sum(i => i.TotalAmount - (i.PartialPaidAmount ?? 0));

        // Upcoming expiries
        var upcomingExpiries = rooms
            .SelectMany(r => r.Contracts)
            .Count(c => c.Status == ContractStatus.Active && c.EndDate > today && c.EndDate <= next30Days);

        // Average Rent
        var activeContracts = rooms
            .SelectMany(r => r.Contracts)
            .Where(c => c.Status == ContractStatus.Active)
            .ToList();

        var averageRent = activeContracts.Any() 
            ? activeContracts.Average(c => c.MonthlyRent) 
            : 0;

        return new PropertyStatsDto
        {
            TotalRooms = totalRooms,
            RentedRooms = rentedRooms,
            AvailableRooms = availableRooms,
            MaintenanceRooms = maintenanceRooms,
            OccupancyRate = occupancyRate,
            MonthlyRevenue = monthlyRevenue,
            OutstandingInvoices = outstandingInvoices.Count,
            TotalOutstandingAmount = totalOutstandingAmount,
            UpcomingContractExpiries = upcomingExpiries,
            AverageRent = Math.Round(averageRent, 0)
        };
    }
}
