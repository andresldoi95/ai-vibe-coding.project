using Xunit;
using FluentAssertions;
using SaaS.Application.Services;
using System.Text;
using ClosedXML.Excel;

namespace Application.Tests.Services;

public class ExportServiceTests
{
    private readonly ExportService _service;

    public ExportServiceTests()
    {
        _service = new ExportService();
    }

    #region Test Data Classes

    public class SimpleTestData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    public class ComplexTestData
    {
        public Guid Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    #endregion

    #region ExportToExcel Tests

    [Fact]
    public void ExportToExcel_WithData_ReturnsNonEmptyByteArray()
    {
        // Arrange
        var data = new List<SimpleTestData>
        {
            new() { Id = 1, Name = "Product A", Price = 10.99m },
            new() { Id = 2, Name = "Product B", Price = 20.50m }
        };

        // Act
        var result = _service.ExportToExcel(data, "Products");

        // Assert
        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ExportToExcel_WithData_CreatesValidExcelFile()
    {
        // Arrange
        var data = new List<SimpleTestData>
        {
            new() { Id = 1, Name = "Product A", Price = 10.99m },
            new() { Id = 2, Name = "Product B", Price = 20.50m }
        };

        // Act
        var result = _service.ExportToExcel(data, "Products");

        // Assert - Verify it's a valid Excel file by loading it
        using var stream = new MemoryStream(result);
        using var workbook = new XLWorkbook(stream);

        workbook.Worksheets.Count.Should().Be(1);
        var worksheet = workbook.Worksheet(1);
        worksheet.Name.Should().Be("Products");

        // Verify data rows (1 header + 2 data rows)
        worksheet.RangeUsed().RowCount().Should().Be(3);
    }

    [Fact]
    public void ExportToExcel_WithData_ContainsCorrectHeaders()
    {
        // Arrange
        var data = new List<SimpleTestData>
        {
            new() { Id = 1, Name = "Product A", Price = 10.99m }
        };

        // Act
        var result = _service.ExportToExcel(data, "Products");

        // Assert
        using var stream = new MemoryStream(result);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet(1);

        worksheet.Cell(1, 1).Value.ToString().Should().Be("Id");
        worksheet.Cell(1, 2).Value.ToString().Should().Be("Name");
        worksheet.Cell(1, 3).Value.ToString().Should().Be("Price");
    }

    [Fact]
    public void ExportToExcel_WithData_ContainsCorrectValues()
    {
        // Arrange
        var data = new List<SimpleTestData>
        {
            new() { Id = 1, Name = "Product A", Price = 10.99m },
            new() { Id = 2, Name = "Product B", Price = 20.50m }
        };

        // Act
        var result = _service.ExportToExcel(data, "Products");

        // Assert
        using var stream = new MemoryStream(result);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet(1);

        // First data row
        worksheet.Cell(2, 1).Value.ToString().Should().Be("1");
        worksheet.Cell(2, 2).Value.ToString().Should().Be("Product A");
        worksheet.Cell(2, 3).Value.ToString().Should().Be("10.99");

        // Second data row
        worksheet.Cell(3, 1).Value.ToString().Should().Be("2");
        worksheet.Cell(3, 2).Value.ToString().Should().Be("Product B");
        worksheet.Cell(3, 3).Value.ToString().Should().Be("20.5");
    }

    [Fact]
    public void ExportToExcel_EmptyData_CreatesWorkbookWithHeadersOnly()
    {
        // Arrange
        var data = new List<SimpleTestData>();

        // Act
        var result = _service.ExportToExcel(data, "Products");

        // Assert
        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);

        using var stream = new MemoryStream(result);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet(1);

        worksheet.Name.Should().Be("Products");

        // Should have headers
        worksheet.Cell(1, 1).Value.ToString().Should().Be("Id");
        worksheet.Cell(1, 2).Value.ToString().Should().Be("Name");
        worksheet.Cell(1, 3).Value.ToString().Should().Be("Price");

        // Should have only 1 row (headers)
        worksheet.RangeUsed().RowCount().Should().Be(1);
    }

    [Fact]
    public void ExportToExcel_ComplexData_HandlesAllPropertyTypes()
    {
        // Arrange
        var data = new List<ComplexTestData>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ProductName = "Widget",
                Description = "A useful widget",
                Quantity = 100,
                UnitPrice = 15.99m,
                CreatedAt = new DateTime(2024, 1, 1, 10, 30, 0),
                IsActive = true
            }
        };

        // Act
        var result = _service.ExportToExcel(data, "Inventory");

        // Assert
        result.Should().NotBeNull();

        using var stream = new MemoryStream(result);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet(1);

        // Verify all property types are exported
        worksheet.Cell(1, 1).Value.ToString().Should().Be("Id");
        worksheet.Cell(1, 2).Value.ToString().Should().Be("ProductName");
        worksheet.Cell(1, 3).Value.ToString().Should().Be("Description");
        worksheet.Cell(1, 4).Value.ToString().Should().Be("Quantity");
        worksheet.Cell(1, 5).Value.ToString().Should().Be("UnitPrice");
        worksheet.Cell(1, 6).Value.ToString().Should().Be("CreatedAt");
        worksheet.Cell(1, 7).Value.ToString().Should().Be("IsActive");
    }

    [Fact]
    public void ExportToExcel_LargeDataSet_HandlesEfficiently()
    {
        // Arrange
        var data = Enumerable.Range(1, 1000).Select(i => new SimpleTestData
        {
            Id = i,
            Name = $"Product {i}",
            Price = i * 10.50m
        }).ToList();

        // Act
        var result = _service.ExportToExcel(data, "Bulk Products");

        // Assert
        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);

        using var stream = new MemoryStream(result);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet(1);

        // Should have 1001 rows (1 header + 1000 data)
        worksheet.RangeUsed().RowCount().Should().Be(1001);
    }

    #endregion

    #region ExportToCsv Tests

    [Fact]
    public void ExportToCsv_WithData_ReturnsNonEmptyByteArray()
    {
        // Arrange
        var data = new List<SimpleTestData>
        {
            new() { Id = 1, Name = "Product A", Price = 10.99m },
            new() { Id = 2, Name = "Product B", Price = 20.50m }
        };

        // Act
        var result = _service.ExportToCsv(data);

        // Assert
        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ExportToCsv_WithData_ContainsHeaders()
    {
        // Arrange
        var data = new List<SimpleTestData>
        {
            new() { Id = 1, Name = "Product A", Price = 10.99m }
        };

        // Act
        var result = _service.ExportToCsv(data);
        var csv = Encoding.UTF8.GetString(result);

        // Assert
        csv.Should().Contain("Id,Name,Price");
    }

    [Fact]
    public void ExportToCsv_WithData_ContainsDataRows()
    {
        // Arrange
        var data = new List<SimpleTestData>
        {
            new() { Id = 1, Name = "Product A", Price = 10.99m },
            new() { Id = 2, Name = "Product B", Price = 20.50m }
        };

        // Act
        var result = _service.ExportToCsv(data);
        var csv = Encoding.UTF8.GetString(result);
        var lines = csv.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        // Assert
        lines.Should().HaveCount(3); // Header + 2 data rows
        lines[0].Should().Be("Id,Name,Price");
        lines[1].Should().Be("1,Product A,10.99");
        lines[2].Should().Be("2,Product B,20.50");
    }

    [Fact]
    public void ExportToCsv_EmptyData_ReturnsHeadersOnly()
    {
        // Arrange
        var data = new List<SimpleTestData>();

        // Act
        var result = _service.ExportToCsv(data);
        var csv = Encoding.UTF8.GetString(result);
        var lines = csv.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        // Assert
        lines.Should().HaveCount(1); // Header only
        lines[0].Should().Be("Id,Name,Price");
    }

    [Fact]
    public void ExportToCsv_ValueWithComma_WrapsInQuotes()
    {
        // Arrange
        var data = new List<SimpleTestData>
        {
            new() { Id = 1, Name = "Product A, Special Edition", Price = 10.99m }
        };

        // Act
        var result = _service.ExportToCsv(data);
        var csv = Encoding.UTF8.GetString(result);
        var lines = csv.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        // Assert
        lines[1].Should().Be("1,\"Product A, Special Edition\",10.99");
    }

    [Fact]
    public void ExportToCsv_ValueWithQuotes_EscapesQuotes()
    {
        // Arrange
        var data = new List<SimpleTestData>
        {
            new() { Id = 1, Name = "Product \"Premium\"", Price = 10.99m }
        };

        // Act
        var result = _service.ExportToCsv(data);
        var csv = Encoding.UTF8.GetString(result);
        var lines = csv.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        // Assert
        lines[1].Should().Be("1,\"Product \"\"Premium\"\"\",10.99");
    }

    [Fact]
    public void ExportToCsv_ValueWithNewline_WrapsInQuotes()
    {
        // Arrange
        var data = new List<SimpleTestData>
        {
            new() { Id = 1, Name = "Product A\nLine 2", Price = 10.99m }
        };

        // Act
        var result = _service.ExportToCsv(data);
        var csv = Encoding.UTF8.GetString(result);

        // Assert
        csv.Should().Contain("\"Product A\nLine 2\"");
    }

    [Fact]
    public void ExportToCsv_ValueWithCarriageReturn_WrapsInQuotes()
    {
        // Arrange
        var data = new List<SimpleTestData>
        {
            new() { Id = 1, Name = "Product A\rLine 2", Price = 10.99m }
        };

        // Act
        var result = _service.ExportToCsv(data);
        var csv = Encoding.UTF8.GetString(result);

        // Assert
        csv.Should().Contain("\"Product A\rLine 2\"");
    }

    [Fact]
    public void ExportToCsv_NullPropertyValue_HandlesGracefully()
    {
        // Arrange
        var data = new List<SimpleTestData>
        {
            new() { Id = 1, Name = null!, Price = 10.99m }
        };

        // Act
        var result = _service.ExportToCsv(data);
        var csv = Encoding.UTF8.GetString(result);
        var lines = csv.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        // Assert
        lines[1].Should().Be("1,,10.99");
    }

    [Fact]
    public void ExportToCsv_EmptyStringValue_HandlesCorrectly()
    {
        // Arrange
        var data = new List<SimpleTestData>
        {
            new() { Id = 1, Name = "", Price = 10.99m }
        };

        // Act
        var result = _service.ExportToCsv(data);
        var csv = Encoding.UTF8.GetString(result);
        var lines = csv.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        // Assert
        lines[1].Should().Be("1,,10.99");
    }

    [Fact]
    public void ExportToCsv_ComplexData_HandlesAllPropertyTypes()
    {
        // Arrange
        var id = Guid.NewGuid();
        var createdAt = new DateTime(2024, 1, 1, 10, 30, 0);
        var data = new List<ComplexTestData>
        {
            new()
            {
                Id = id,
                ProductName = "Widget",
                Description = "A useful widget",
                Quantity = 100,
                UnitPrice = 15.99m,
                CreatedAt = createdAt,
                IsActive = true
            }
        };

        // Act
        var result = _service.ExportToCsv(data);
        var csv = Encoding.UTF8.GetString(result);
        var lines = csv.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        // Assert
        lines.Should().HaveCount(2); // Header + 1 data row
        lines[0].Should().Be("Id,ProductName,Description,Quantity,UnitPrice,CreatedAt,IsActive");
        lines[1].Should().Contain(id.ToString());
        lines[1].Should().Contain("Widget");
        lines[1].Should().Contain("100");
        lines[1].Should().Contain("15.99");
        lines[1].Should().Contain("True");
    }

    [Fact]
    public void ExportToCsv_LargeDataSet_HandlesEfficiently()
    {
        // Arrange
        var data = Enumerable.Range(1, 1000).Select(i => new SimpleTestData
        {
            Id = i,
            Name = $"Product {i}",
            Price = i * 10.50m
        }).ToList();

        // Act
        var result = _service.ExportToCsv(data);
        var csv = Encoding.UTF8.GetString(result);
        var lines = csv.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        // Assert
        lines.Should().HaveCount(1001); // Header + 1000 data rows
    }

    [Fact]
    public void ExportToCsv_SpecialCharactersCombined_EscapesCorrectly()
    {
        // Arrange
        var data = new List<SimpleTestData>
        {
            new() { Id = 1, Name = "Product \"A\", Edition\n2", Price = 10.99m }
        };

        // Act
        var result = _service.ExportToCsv(data);
        var csv = Encoding.UTF8.GetString(result);

        // Assert
        // Should wrap in quotes and escape internal quotes
        csv.Should().Contain("\"Product \"\"A\"\", Edition\n2\"");
    }

    #endregion

    #region UTF-8 Encoding Tests

    [Fact]
    public void ExportToCsv_UnicodeCharacters_EncodesCorrectly()
    {
        // Arrange
        var data = new List<SimpleTestData>
        {
            new() { Id = 1, Name = "Producto Español €", Price = 10.99m },
            new() { Id = 2, Name = "日本語製品", Price = 20.50m }
        };

        // Act
        var result = _service.ExportToCsv(data);
        var csv = Encoding.UTF8.GetString(result);

        // Assert
        csv.Should().Contain("Producto Español €");
        csv.Should().Contain("日本語製品");
    }

    #endregion
}
