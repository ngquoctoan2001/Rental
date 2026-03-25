using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;
using RentalOS.Application.Modules.Customers.Dtos;
using RentalOS.Domain.Enums;

namespace RentalOS.Application.Modules.Customers.Queries.GetCustomerById;

public record GetCustomerByIdQuery(Guid Id) : IRequest<Result<CustomerDto>>;

public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, Result<CustomerDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCustomerByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CustomerDto>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .Include(c => c.Contracts)
                .ThenInclude(cn => cn.Room)
                    .ThenInclude(r => r.Property)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (customer == null)
        {
            return Result<CustomerDto>.Fail("CUSTOMER_NOT_FOUND", "Không tìm thấy khách hàng.");
        }

        var dto = new CustomerDto
        {
            Id = customer.Id,
            FullName = customer.FullName,
            Phone = customer.Phone,
            Email = customer.Email,
            IdCardNumber = customer.IdCardNumber,
            IdCardImageFront = customer.IdCardImageFront,
            IdCardImageBack = customer.IdCardImageBack,
            PortraitImage = customer.PortraitImage,
            DateOfBirth = customer.DateOfBirth,
            Gender = customer.Gender,
            Hometown = customer.Hometown,
            CurrentAddress = customer.CurrentAddress,
            Occupation = customer.Occupation,
            Workplace = customer.Workplace,
            EmergencyContactName = customer.EmergencyContactName,
            EmergencyContactPhone = customer.EmergencyContactPhone,
            EmergencyContactRelationship = customer.EmergencyContactRelationship,
            IsBlacklisted = customer.IsBlacklisted,
            BlacklistReason = customer.BlacklistReason,
            BlacklistedAt = customer.BlacklistedAt,
            BlacklistedBy = customer.BlacklistedBy,
            Notes = customer.Notes,
            CreatedAt = customer.CreatedAt,
            ActiveContract = customer.Contracts
                .Where(cn => cn.Status == ContractStatus.Active)
                .Select(cn => new ActiveContractDto
                {
                    ContractCode = cn.ContractCode,
                    RoomNumber = cn.Room.RoomNumber,
                    PropertyName = cn.Room.Property.Name
                })
                .FirstOrDefault()
        };

        return Result<CustomerDto>.Ok(dto);
    }
}
