using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Constants;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Domain.Entities;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace RentalOS.Application.Modules.Rooms.Commands.CreateRoom;

public record CreateRoomCommand : IRequest<Guid>
{
    public Guid PropertyId { get; init; }
    public string RoomNumber { get; init; } = string.Empty;
    public int Floor { get; init; } = 1;
    public decimal? AreaSqm { get; init; }
    public decimal BasePrice { get; init; }
    public decimal ElectricityPrice { get; init; } = 3500;
    public decimal WaterPrice { get; init; } = 15000;
    public decimal ServiceFee { get; init; } = 0;
    public decimal InternetFee { get; init; } = 0;
    public decimal GarbageFee { get; init; } = 0;
    public List<string> Amenities { get; init; } = [];
    public string? Notes { get; init; }
}

public class CreateRoomCommandValidator : AbstractValidator<CreateRoomCommand>
{
    public CreateRoomCommandValidator()
    {
        RuleFor(v => v.PropertyId).NotEmpty();
        
        RuleFor(v => v.RoomNumber)
            .NotEmpty()
            .MaximumLength(50)
            .Must(v => Regex.IsMatch(v, @"^[a-zA-Z0-9\s-]+$"))
            .WithMessage("Mã phòng không được chứa ký tự đặc biệt.");

        RuleFor(v => v.BasePrice)
            .GreaterThanOrEqualTo(100000)
            .WithMessage("Giá thuê cơ bản phải từ 100,000đ.");

        RuleFor(v => v.ElectricityPrice)
            .GreaterThanOrEqualTo(1000)
            .WithMessage("Giá điện phải từ 1,000đ.");

        RuleFor(v => v.WaterPrice)
            .GreaterThanOrEqualTo(1000)
            .WithMessage("Giá nước phải từ 1,000đ.");

        RuleFor(v => v.Amenities)
            .Must(v => v == null || v.All(a => RentalOS.Application.Common.Constants.Amenities.IsValid(a)))
            .WithMessage("Tiện ích không hợp lệ.");
    }
}

public class CreateRoomCommandHandler : IRequestHandler<CreateRoomCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateRoomCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateRoomCommand request, CancellationToken cancellationToken)
    {
        var property = await _context.Properties
            .FirstOrDefaultAsync(p => p.Id == request.PropertyId, cancellationToken);

        if (property == null)
        {
            throw new Exception("PROPERTY_NOT_FOUND");
        }

        var entity = new Room
        {
            PropertyId = request.PropertyId,
            RoomNumber = request.RoomNumber,
            Floor = request.Floor,
            AreaSqm = request.AreaSqm,
            BasePrice = request.BasePrice,
            ElectricityPrice = request.ElectricityPrice,
            WaterPrice = request.WaterPrice,
            ServiceFee = request.ServiceFee,
            InternetFee = request.InternetFee,
            GarbageFee = request.GarbageFee,
            Amenities = JsonSerializer.Serialize(request.Amenities),
            Notes = request.Notes,
            Status = Domain.Enums.RoomStatus.Available
        };

        _context.Rooms.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
