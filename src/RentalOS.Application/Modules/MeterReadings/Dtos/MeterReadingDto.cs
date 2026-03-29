namespace RentalOS.Application.Modules.MeterReadings.Dtos;

public class MeterReadingDto
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string? PropertyName { get; set; }
    public DateOnly ReadingDate { get; set; }
    public int ElectricityReading { get; set; }
    public int WaterReading { get; set; }
    public string? ElectricityImage { get; set; }
    public string? WaterImage { get; set; }
    public string? Note { get; set; }
}
