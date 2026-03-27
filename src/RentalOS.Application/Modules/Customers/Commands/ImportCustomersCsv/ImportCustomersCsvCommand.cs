using MediatR;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;
using RentalOS.Domain.Entities;

namespace RentalOS.Application.Modules.Customers.Commands.ImportCustomersCsv;

public record ImportCustomersCsvCommand : IRequest<ImportCustomersCsvResult>
{
    public string CsvContent { get; init; } = string.Empty;
}

public record ImportCustomersCsvResult(int Imported, int Skipped, List<string> Errors);

public class ImportCustomersCsvCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ImportCustomersCsvCommand, ImportCustomersCsvResult>
{
    public async Task<ImportCustomersCsvResult> Handle(ImportCustomersCsvCommand request, CancellationToken cancellationToken)
    {
        var lines = request.CsvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        int imported = 0, skipped = 0;
        var errors = new List<string>();

        // Skip header row
        var dataLines = lines.Skip(1).ToArray();

        for (int i = 0; i < dataLines.Length; i++)
        {
            var lineNum = i + 2;
            var cols = dataLines[i].Split(',');
            if (cols.Length < 2)
            {
                errors.Add($"Dòng {lineNum}: thiếu cột (cần ít nhất FullName, PhoneNumber)");
                skipped++;
                continue;
            }

            var fullName = cols[0].Trim().Trim('"');
            var phone = cols[1].Trim().Trim('"');

            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(phone))
            {
                errors.Add($"Dòng {lineNum}: họ tên và số điện thoại không được trống");
                skipped++;
                continue;
            }

            var idCardNumber = cols.Length > 2 ? cols[2].Trim().Trim('"') : null;
            var email = cols.Length > 3 ? cols[3].Trim().Trim('"') : null;
            var address = cols.Length > 4 ? cols[4].Trim().Trim('"') : null;

            context.Customers.Add(new Customer
            {
                FullName = fullName,
                Phone = phone,
                IdCardNumber = string.IsNullOrWhiteSpace(idCardNumber) ? null : idCardNumber,
                Email = string.IsNullOrWhiteSpace(email) ? null : email,
                CurrentAddress = string.IsNullOrWhiteSpace(address) ? null : address,
            });
            imported++;
        }

        await context.SaveChangesAsync(cancellationToken);
        return new ImportCustomersCsvResult(imported, skipped, errors);
    }
}
