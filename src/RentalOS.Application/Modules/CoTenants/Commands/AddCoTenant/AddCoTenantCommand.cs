using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;
using RentalOS.Domain.Entities;

namespace RentalOS.Application.Modules.CoTenants.Commands.AddCoTenant;

public record AddCoTenantCommand : IRequest<Result<Guid>>
{
    public Guid ContractId { get; init; }
    public Guid? CustomerId { get; init; }
    public string? FullName { get; init; }
    public string? Phone { get; init; }
    public string? IdCardNumber { get; init; }
    public DateOnly? MovedInAt { get; init; }
}

public class AddCoTenantCommandHandler : IRequestHandler<AddCoTenantCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public AddCoTenantCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(AddCoTenantCommand request, CancellationToken cancellationToken)
    {
        var contract = await _context.Contracts
            .FirstOrDefaultAsync(c => c.Id == request.ContractId, cancellationToken);

        if (contract == null)
        {
            return Result<Guid>.Fail("CONTRACT_NOT_FOUND", "Không tìm thấy hợp đồng.");
        }

        Guid customerId;

        if (request.CustomerId.HasValue)
        {
            customerId = request.CustomerId.Value;
            var customerExists = await _context.Customers.AnyAsync(c => c.Id == customerId, cancellationToken);
            if (!customerExists)
            {
                return Result<Guid>.Fail("CUSTOMER_NOT_FOUND", "Không tìm thấy khách hàng.");
            }
        }
        else
        {
            if (string.IsNullOrEmpty(request.FullName) || string.IsNullOrEmpty(request.Phone))
            {
                return Result<Guid>.Fail("INVALID_INPUT", "Họ tên và số điện thoại là bắt buộc khi tạo mới khách thuê.");
            }

            var customer = new Customer
            {
                FullName = request.FullName,
                Phone = request.Phone,
                IdCardNumber = request.IdCardNumber
            };

            _context.Customers.Add(customer);
            customerId = customer.Id;
        }

        // Check if already in this contract
        var alreadyExists = await _context.ContractCoTenants
            .AnyAsync(ct => ct.ContractId == request.ContractId && ct.CustomerId == customerId && ct.MovedOutAt == null, cancellationToken);

        if (alreadyExists)
        {
            return Result<Guid>.Fail("ALREADY_EXISTS", "Khách thuê này đã có trong hợp đồng.");
        }

        var coTenant = new ContractCoTenant
        {
            ContractId = request.ContractId,
            CustomerId = customerId,
            IsPrimary = false,
            MovedInAt = request.MovedInAt ?? DateOnly.FromDateTime(DateTime.Today)
        };

        _context.ContractCoTenants.Add(coTenant);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Ok(coTenant.Id);
    }
}
