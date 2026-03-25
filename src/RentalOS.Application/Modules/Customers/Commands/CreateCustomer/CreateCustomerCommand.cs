using MediatR;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;
using RentalOS.Domain.Entities;

namespace RentalOS.Application.Modules.Customers.Commands.CreateCustomer;

public record CreateCustomerCommand : IRequest<Result<Guid>>
{
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

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateCustomerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = new Customer
        {
            FullName = request.FullName,
            Phone = request.Phone,
            Email = request.Email,
            IdCardNumber = request.IdCardNumber,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            Hometown = request.Hometown,
            CurrentAddress = request.CurrentAddress,
            Occupation = request.Occupation,
            Workplace = request.Workplace,
            EmergencyContactName = request.EmergencyContactName,
            EmergencyContactPhone = request.EmergencyContactPhone,
            EmergencyContactRelationship = request.EmergencyContactRelationship,
            Notes = request.Notes
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Ok(customer.Id);
    }
}
