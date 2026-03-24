using ClosedXML.Excel;

namespace RentalOS.Infrastructure.Services.Excel;

public class ExcelService
{
    // TODO: implement Excel report export via ClosedXML
    public byte[] ExportToExcel<T>(IReadOnlyList<T> data, string sheetName = "Sheet1")
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add(sheetName);
        // TODO: auto-map columns from T properties
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}
