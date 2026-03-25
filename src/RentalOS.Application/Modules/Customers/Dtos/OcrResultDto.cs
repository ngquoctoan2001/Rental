namespace RentalOS.Application.Modules.Customers.Dtos;

public record OcrResultDto
{
    public string? FullName { get; init; }
    public string? IdCardNumber { get; init; }
    public DateOnly? DateOfBirth { get; init; }
    public string? Gender { get; init; }
    public string? Hometown { get; init; }
    public string? Address { get; init; }
    public DateOnly? IssueDate { get; init; }
    public DateOnly? ExpiryDate { get; init; }
    public double Confidence { get; init; }
}
