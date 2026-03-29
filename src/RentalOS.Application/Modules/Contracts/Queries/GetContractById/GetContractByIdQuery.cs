using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;
using RentalOS.Application.Modules.Contracts.Dtos;
using RentalOS.Domain.Entities;

namespace RentalOS.Application.Modules.Contracts.Queries.GetContractById;

public record GetContractByIdQuery(Guid Id) : IRequest<Result<ContractDto>>;

public class GetContractByIdQueryHandler : IRequestHandler<GetContractByIdQuery, Result<ContractDto>>
{
    private readonly IApplicationDbContext _context;

    public GetContractByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ContractDto>> Handle(GetContractByIdQuery request, CancellationToken cancellationToken)
    {
        var contract = await _context.Contracts
            .Include(c => c.Room)
            .Include(c => c.Customer)
            .Include(c => c.CoTenants)
                .ThenInclude(ct => ct.Customer)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        
        if (contract == null) return Result<ContractDto>.Fail("CONTRACT_NOT_FOUND", "Không tìm thấy hợp đồng.");

        var dto = new ContractDto
        {
            Id = contract.Id,
            ContractCode = contract.ContractCode,
            RoomId = contract.RoomId,
            RoomNumber = contract.Room.RoomNumber,
            RoomFloor = contract.Room.Floor,
            CustomerId = contract.CustomerId,
            CustomerName = contract.Customer.FullName,
            CustomerPhone = contract.Customer.Phone,
            StartDate = contract.StartDate,
            EndDate = contract.EndDate,
            MonthlyRent = contract.MonthlyRent,
            DepositAmount = contract.DepositAmount,
            DepositPaid = contract.DepositPaid,
            SignedByCustomer = contract.SignedByCustomer,
            Status = contract.Status,
            PdfUrl = contract.PdfUrl,
            CoTenants = contract.CoTenants
                .OrderByDescending(ct => ct.IsPrimary)
                .ThenBy(ct => ct.MovedInAt)
                .Select(ct => new ContractCoTenantDto
                {
                    Id = ct.Id,
                    CustomerId = ct.CustomerId,
                    FullName = ct.Customer.FullName,
                    Phone = ct.Customer.Phone
                })
                .ToList()
        };

        return Result<ContractDto>.Ok(dto);
    }
}
