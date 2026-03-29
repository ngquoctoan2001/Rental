using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;
using RentalOS.Application.Modules.Contracts.Dtos;
using RentalOS.Domain.Enums;

namespace RentalOS.Application.Modules.Contracts.Queries.GetContracts;

public record GetContractsQuery : IRequest<Result<List<ContractDto>>>
{
    public ContractStatus? Status { get; init; }
    public string? SearchTerm { get; init; }
    public Guid? PropertyId { get; init; }
}

public class GetContractsQueryHandler : IRequestHandler<GetContractsQuery, Result<List<ContractDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetContractsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<ContractDto>>> Handle(GetContractsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Contracts
            .Include(c => c.Room)
            .Include(c => c.Customer)
            .AsQueryable();

        if (string.Equals(_currentUserService.Role, "tenant", StringComparison.OrdinalIgnoreCase))
        {
            var currentUserId = Guid.TryParse(_currentUserService.UserId, out var parsedUserId) ? parsedUserId : Guid.Empty;
            var currentUserEmail = await _context.ApplicationUsers
                .Where(u => u.Id == currentUserId)
                .Select(u => u.Email)
                .FirstOrDefaultAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(currentUserEmail))
            {
                query = query.Where(c => c.Customer.Email == currentUserEmail);
            }
        }

        if (request.Status.HasValue)
        {
            query = query.Where(c => c.Status == request.Status.Value);
        }

        if (request.PropertyId.HasValue)
        {
            query = query.Where(c => c.Room.PropertyId == request.PropertyId.Value);
        }

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var search = request.SearchTerm.ToLower();
            query = query.Where(c =>
                c.ContractCode.ToLower().Contains(search) ||
                c.Customer.FullName.ToLower().Contains(search) ||
                c.Room.RoomNumber.ToLower().Contains(search));
        }

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ContractDto
            {
                Id = c.Id,
                ContractCode = c.ContractCode,
                RoomId = c.RoomId,
                RoomNumber = c.Room.RoomNumber,
                RoomFloor = c.Room.Floor,
                CustomerId = c.CustomerId,
                CustomerName = c.Customer.FullName,
                CustomerPhone = c.Customer.Phone,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                MonthlyRent = c.MonthlyRent,
                DepositAmount = c.DepositAmount,
                DepositPaid = c.DepositPaid,
                SignedByCustomer = c.SignedByCustomer,
                Status = c.Status,
                PdfUrl = c.PdfUrl,
            })
            .ToListAsync(cancellationToken);

        return Result<List<ContractDto>>.Ok(items);
    }
}
