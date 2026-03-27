using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Domain.Entities;

namespace RentalOS.Application.Modules.Rooms.Commands.ImportRoomsCsv;

public record ImportRoomsCsvCommand : IRequest<ImportRoomsCsvResult>
{
    public Guid PropertyId { get; init; }
    public string CsvContent { get; init; } = string.Empty;
}

public record ImportRoomsCsvResult(int Imported, int Skipped, List<string> Errors);

public class ImportRoomsCsvCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ImportRoomsCsvCommand, ImportRoomsCsvResult>
{
    public async Task<ImportRoomsCsvResult> Handle(ImportRoomsCsvCommand request, CancellationToken cancellationToken)
    {
        var property = await context.Properties
            .FirstOrDefaultAsync(p => p.Id == request.PropertyId, cancellationToken)
            ?? throw new KeyNotFoundException("Property not found");

        var lines = request.CsvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        int imported = 0, skipped = 0;
        var errors = new List<string>();

        // Skip header row
        var dataLines = lines.Skip(1).ToArray();

        for (int i = 0; i < dataLines.Length; i++)
        {
            var lineNum = i + 2;
            var cols = dataLines[i].Split(',');
            if (cols.Length < 3)
            {
                errors.Add($"Dòng {lineNum}: thiếu cột (cần ít nhất RoomNumber, Floor, BasePrice)");
                skipped++;
                continue;
            }

            var roomNumber = cols[0].Trim().Trim('"');
            if (string.IsNullOrWhiteSpace(roomNumber))
            {
                errors.Add($"Dòng {lineNum}: số phòng không được trống");
                skipped++;
                continue;
            }

            var exists = await context.Rooms
                .AnyAsync(r => r.PropertyId == request.PropertyId && r.RoomNumber == roomNumber, cancellationToken);
            if (exists)
            {
                errors.Add($"Dòng {lineNum}: phòng '{roomNumber}' đã tồn tại");
                skipped++;
                continue;
            }

            if (!int.TryParse(cols[1].Trim().Trim('"'), out var floor)) floor = 1;
            if (!decimal.TryParse(cols[2].Trim().Trim('"'), out var basePrice))
            {
                errors.Add($"Dòng {lineNum}: giá thuê không hợp lệ");
                skipped++;
                continue;
            }

            decimal elecPrice = cols.Length > 3 && decimal.TryParse(cols[3].Trim().Trim('"'), out var ep) ? ep : 3500;
            decimal waterPrice = cols.Length > 4 && decimal.TryParse(cols[4].Trim().Trim('"'), out var wp) ? wp : 15000;
            decimal? areaSqm = cols.Length > 5 && decimal.TryParse(cols[5].Trim().Trim('"'), out var area) ? area : null;

            context.Rooms.Add(new Room
            {
                PropertyId = request.PropertyId,
                RoomNumber = roomNumber,
                Floor = floor,
                BasePrice = basePrice,
                ElectricityPrice = elecPrice,
                WaterPrice = waterPrice,
                AreaSqm = areaSqm,
                Status = RentalOS.Domain.Enums.RoomStatus.Available,
            });
            imported++;
        }

        await context.SaveChangesAsync(cancellationToken);
        return new ImportRoomsCsvResult(imported, skipped, errors);
    }
}
