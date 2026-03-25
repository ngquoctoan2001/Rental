using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Constants;
using RentalOS.Application.Common.Interfaces;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace RentalOS.Application.Modules.Rooms.Commands.UpdateRoom;

public record UpdateRoomCommand : IRequest<Unit>
{
    public Guid Id { get; init; }
    public string RoomNumber { get; init; } = string.Empty;
    public int Floor { get; init; }
    public decimal? AreaSqm { get; init; }
    public decimal BasePrice { get; init; }
    public decimal ElectricityPrice { get; init; }
    public decimal WaterPrice { get; init; }
    public decimal ServiceFee { get; init; }
    public decimal InternetFee { get; init; }
    public decimal GarbageFee { get; init; }
    public List<string> Amenities { get; init; } = [];
    public string? Notes { get; init; }
}

public class UpdateRoomCommandValidator : AbstractValidator<UpdateRoomCommand>
{
    public UpdateRoomCommandValidator()
    {
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

public class UpdateRoomCommandHandler : IRequestHandler<UpdateRoomCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public UpdateRoomCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UpdateRoomCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Rooms
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            throw new Exception("ROOM_NOT_FOUND");
        }

        entity.RoomNumber = request.RoomNumber;
        entity.Floor = request.Floor;
        entity.AreaSqm = request.AreaSqm;
        entity.BasePrice = request.BasePrice;
        entity.ElectricityPrice = request.ElectricityPrice;
        entity.WaterPrice = request.WaterPrice;
        entity.ServiceFee = request.ServiceFee;
        entity.InternetFee = request.InternetFee;
        entity.GarbageFee = request.GarbageFee;
        entity.Amenities = JsonSerializer.Serialize(request.Amenities);
        entity.Notes = request.Notes;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
