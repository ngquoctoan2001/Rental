using Google.Cloud.Vision.V1;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Modules.Customers.Dtos;
using System.Text.RegularExpressions;

namespace RentalOS.Infrastructure.Services;

public class OcrService : IOcrService
{
    public async Task<OcrResultDto> ExtractIdCardInfoAsync(Stream imageStream, CancellationToken cancellationToken = default)
    {
        var client = await ImageAnnotatorClient.CreateAsync(cancellationToken);
        var image = Image.FromStream(imageStream);
        
        var response = await client.DetectTextAsync(image);
        var text = response.FirstOrDefault()?.Description ?? string.Empty;

        return ParseVietnameseIdCard(text);
    }

    private OcrResultDto ParseVietnameseIdCard(string text)
    {
        // Simple regex-based parsing for VN ID cards (CCCD/CMND)
        // This is a basic implementation, can be improved with more robust logic
        
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        
        string? fullName = null;
        string? idCardNumber = null;
        DateOnly? dob = null;
        string? gender = null;
        string? hometown = null;
        string? address = null;
        DateOnly? issueDate = null;
        DateOnly? expiryDate = null;

        // ID Number (9 or 12 digits)
        var idMatch = Regex.Match(text, @"\b(\d{9}|\d{12})\b");
        if (idMatch.Success) idCardNumber = idMatch.Groups[1].Value;

        // Full Name (usually in CAPITALS)
        foreach (var line in lines)
        {
            if (line.All(c => char.IsUpper(c) || char.IsWhiteSpace(c)) && line.Length > 5 && !line.Contains("CỘNG HÒA"))
            {
                fullName = line;
                break;
            }
        }

        // DOB (dd/mm/yyyy)
        var dobMatch = Regex.Match(text, @"\b(\d{2}/\d{2}/\d{4})\b");
        if (dobMatch.Success) dob = ParseDate(dobMatch.Groups[1].Value);

        // Gender (Nam/Nữ)
        if (text.Contains("Nam", StringComparison.OrdinalIgnoreCase)) gender = "male";
        else if (text.Contains("Nữ", StringComparison.OrdinalIgnoreCase)) gender = "female";

        // Hometown & Address (Usually after keywords)
        // Note: Very basic extraction, real OCR logic usually uses coordinate-based or context-aware parsing
        
        return new OcrResultDto
        {
            FullName = fullName,
            IdCardNumber = idCardNumber,
            DateOfBirth = dob,
            Gender = gender,
            Hometown = hometown,
            Address = address,
            IssueDate = issueDate,
            ExpiryDate = expiryDate,
            Confidence = 0.9 // Mock confidence for now
        };
    }

    private DateOnly? ParseDate(string value)
    {
        if (DateOnly.TryParseExact(value, "dd/mm/yyyy", out var date)) return date;
        return null;
    }
}
