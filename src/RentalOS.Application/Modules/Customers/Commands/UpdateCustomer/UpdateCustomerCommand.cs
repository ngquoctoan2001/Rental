using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;

namespace RentalOS.Application.Modules.Customers.Commands.UpdateCustomer;

public record UpdateCustomerCommand : IRequest<Result<bool>>
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? IdCardNumber { get; init; }
    public DateOnly? DateOfBirth { get; init; }
    public string? Gender { get; init; }
    public string? Hometown { get; init; }
    public string? CurrentAddress { get; init; }
    public string? Occupation { get; init; }
    public string? Workplace { get; init; }
    public string? EmergencyContactName { get; init; }
    public string? EmergencyContactPhone { get; init; }
    public string? EmergencyContactRelationship { get; init; }
    public string? Notes { get; init; }
}

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public UpdateCustomerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (customer == null)
        {
            return Result<bool>.Fail("CUSTOMER_NOT_FOUND", "Không tìm thấy khách hàng.");
        }

        customer.FullName = request.FullName;
        customer.Phone = request.Phone;
        customer.Email = request.Email;
        customer.IdCardNumber = request.IdCardNumber;
        customer.DateOfBirth = request.DateOfBirth;
        customer.Gender = request.Gender;
        customer.Hometown = request.Hometown;
        customer.CurrentAddress = request.CurrentAddress;
        customer.Occupation = request.Occupation;
        customer.Workplace = request.Workplace;
        customer.EmergencyContactName = request.EmergencyContactName;
        customer.EmergencyContactPhone = request.EmergencyContactPhone;
        customer.EmergencyContactRelationship = request.EmergencyContactRelationship;
        customer.Notes = request.Notes;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Ok(true);
    }
}
