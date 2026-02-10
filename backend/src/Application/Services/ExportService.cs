using System.Globalization;
using System.Reflection;
using System.Text;
using ClosedXML.Excel;
using SaaS.Application.Common.Interfaces;

namespace SaaS.Application.Services;

/// <summary>
/// Service for exporting data to Excel and CSV formats
/// </summary>
public class ExportService : IExportService
{
    /// <summary>
    /// Export data to Excel format using ClosedXML
    /// </summary>
    public byte[] ExportToExcel<T>(IEnumerable<T> data, string sheetName) where T : class
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(sheetName);

        var dataList = data.ToList();
        if (!dataList.Any())
        {
            // Create empty workbook with headers only
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < properties.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = properties[i].Name;
            }
        }
        else
        {
            // Add data to worksheet - ClosedXML will automatically create headers
            var table = worksheet.Cell(1, 1).InsertTable(dataList);
            
            // Auto-fit columns for better readability
            worksheet.Columns().AdjustToContents();
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    /// <summary>
    /// Export data to CSV format
    /// </summary>
    public byte[] ExportToCsv<T>(IEnumerable<T> data) where T : class
    {
        var sb = new StringBuilder();
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Write header
        sb.AppendLine(string.Join(",", properties.Select(p => EscapeCsvValue(p.Name))));

        // Write data rows
        foreach (var item in data)
        {
            var values = properties.Select(p =>
            {
                var value = p.GetValue(item);
                return EscapeCsvValue(value?.ToString() ?? string.Empty);
            });
            sb.AppendLine(string.Join(",", values));
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    /// <summary>
    /// Escape CSV values to handle commas, quotes, and newlines
    /// </summary>
    private static string EscapeCsvValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        // If value contains comma, quote, or newline, wrap in quotes and escape existing quotes
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
