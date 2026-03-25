using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;
using RentalOS.Application.Modules.Properties.Dtos;
using RentalOS.Domain.Entities;
using RentalOS.Domain.Enums;

namespace RentalOS.Application.Modules.Properties.Queries.GetPropertyStats;

public class GetPropertyStatsQueryHandler(IApplicationDbContext context) 
    : IRequestHandler<GetPropertyStatsQuery, Result<PropertyStatsDto>>
{
    public async Task<Result<PropertyStatsDto>> Handle(GetPropertyStatsQuery request, CancellationToken cancellationToken)
    {
        var property = await context.Properties
            .Include(p => p.Rooms)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (property == null)
        {
            return Result<PropertyStatsDto>.Fail("PROPERTY_NOT_FOUND", "Không tìm thấy nhà trọ.");
        }

        var rooms = property.Rooms.Where(r => r.IsActive).ToList();
        var totalRooms = rooms.Count;
        if (totalRooms == 0)
        {
            return Result<PropertyStatsDto>.Ok(new PropertyStatsDto());
        }

        var availableRooms = rooms.Count(r => r.Status == RoomStatus.Available);
        var rentedRooms = rooms.Count(r => r.Status == RoomStatus.Rented);
        var maintenanceRooms = rooms.Count(r => r.Status == RoomStatus.Maintenance);
        
        var roomIds = rooms.Select(r => r.Id).ToList();

        // 1. Monthly Revenue (Paid invoices in current month)
        var todayTime = DateTime.UtcNow;
        var firstDayOfMonth = new DateOnly(todayTime.Year, todayTime.Month, 1);
        var firstDayOfNextMonth = firstDayOfMonth.AddMonths(1);
        
        var monthlyRevenue = await context.Invoices
            .Where(i => roomIds.Contains(i.Contract.RoomId) 
                        && i.Status == InvoiceStatus.Paid 
                        && i.BillingMonth >= firstDayOfMonth 
                        && i.BillingMonth < firstDayOfNextMonth)
            .SumAsync(i => i.TotalAmount, cancellationToken);

        // 2. Outstanding Invoices
        var outstandingInvoices = await context.Invoices
            .Where(i => roomIds.Contains(i.Contract.RoomId) 
                        && i.Status != InvoiceStatus.Paid 
                        && i.Status != InvoiceStatus.Cancelled)
            .Select(i => new { i.TotalAmount, PartialPaidAmount = i.PartialPaidAmount ?? 0m })
            .ToListAsync(cancellationToken);

        var outstandingCount = outstandingInvoices.Count;
        var outstandingAmount = outstandingInvoices.Sum(i => i.TotalAmount - i.PartialPaidAmount);

        // 3. Upcoming Contract Expiries (next 30 days)
        var todayDate = DateOnly.FromDateTime(todayTime);
        var thirtyDaysFromNow = todayDate.AddDays(30);
        
        var upcomingExpiries = await context.Contracts
            .Where(c => roomIds.Contains(c.RoomId) 
                        && c.Status == ContractStatus.Active 
                        && c.EndDate >= todayDate 
                        && c.EndDate <= thirtyDaysFromNow)
            .CountAsync(cancellationToken);

        // 4. Occupancy Rate
        var occupancyRate = totalRooms > 0 ? (double)rentedRooms / totalRooms : 0;

        // 5. Average Rent
        var averageRent = totalRooms > 0 ? rooms.Average(r => r.BasePrice) : 0m;

        var stats = new PropertyStatsDto
        {
            TotalRooms = totalRooms,
            AvailableRooms = availableRooms,
            RentedRooms = rentedRooms,
            MaintenanceRooms = maintenanceRooms,
            OccupancyRate = occupancyRate,
            MonthlyRevenue = monthlyRevenue,
            OutstandingInvoices = outstandingCount,
            TotalOutstandingAmount = outstandingAmount,
            UpcomingContractExpiries = upcomingExpiries,
            AverageRent = averageRent
        };

        return Result<PropertyStatsDto>.Ok(stats);
    }
}
