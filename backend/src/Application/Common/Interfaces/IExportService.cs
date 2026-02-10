namespace SaaS.Application.Common.Interfaces;

/// <summary>
/// Service for exporting data to different file formats
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Export data to Excel format
    /// </summary>
    /// <typeparam name="T">Type of data to export</typeparam>
    /// <param name="data">Data to export</param>
    /// <param name="sheetName">Name of the Excel sheet</param>
    /// <returns>Byte array containing the Excel file</returns>
    byte[] ExportToExcel<T>(IEnumerable<T> data, string sheetName) where T : class;

    /// <summary>
    /// Export data to CSV format
    /// </summary>
    /// <typeparam name="T">Type of data to export</typeparam>
    /// <param name="data">Data to export</param>
    /// <returns>Byte array containing the CSV file</returns>
    byte[] ExportToCsv<T>(IEnumerable<T> data) where T : class;
}
