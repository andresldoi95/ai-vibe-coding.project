using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SaaS.Application.Interfaces;
using SaaS.Domain.Entities;
using SaaS.Domain.Enums;

namespace SaaS.Infrastructure.Services;

/// <summary>
/// Service for generating RIDE (Representación Impresa del Comprobante Electrónico) PDFs
/// Legal requirement for electronic invoicing in Ecuador
/// </summary>
public class RideGenerationService : IRideGenerationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RideGenerationService> _logger;
    private readonly string _storagePath;

    public RideGenerationService(
        IConfiguration configuration,
        ILogger<RideGenerationService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _storagePath = _configuration["StorageSettings:BasePath"] ?? "storage";

        // Set QuestPDF license (Community license for free use)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<string> GenerateRidePdfAsync(
        Invoice invoice,
        SriConfiguration sriConfiguration,
        Establishment establishment,
        EmissionPoint emissionPoint)
    {
        try
        {
            _logger.LogInformation("Generating RIDE PDF for invoice {InvoiceId}", invoice.Id);

            // Validate required data
            if (string.IsNullOrEmpty(invoice.AccessKey))
            {
                throw new InvalidOperationException("Access key is required to generate RIDE");
            }

            if (string.IsNullOrEmpty(invoice.SriAuthorization))
            {
                throw new InvalidOperationException("SRI authorization is required to generate RIDE");
            }

            // Create storage directory
            var year = invoice.IssueDate.Year;
            var month = invoice.IssueDate.Month;
            var directoryPath = Path.Combine(_storagePath, invoice.TenantId.ToString(), "invoices", year.ToString(), month.ToString("D2"));
            Directory.CreateDirectory(directoryPath);

            // Generate file path
            var fileName = $"{invoice.AccessKey}_ride.pdf";
            var filePath = Path.Combine(directoryPath, fileName);

            // Generate PDF document
            await Task.Run(() =>
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(1, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                        page.Header().Element(c => ComposeHeader(c, invoice, sriConfiguration, establishment, emissionPoint));
                        page.Content().Element(c => ComposeContent(c, invoice, sriConfiguration));
                        page.Footer().Element(c => ComposeFooter(c, invoice));
                    });
                }).GeneratePdf(filePath);
            });

            _logger.LogInformation("RIDE PDF generated successfully at {FilePath}", filePath);

            return filePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating RIDE PDF for invoice {InvoiceId}", invoice.Id);
            throw;
        }
    }

    private void ComposeHeader(IContainer container, Invoice invoice, SriConfiguration sriConfiguration, Establishment establishment, EmissionPoint emissionPoint)
    {
        container.Column(column =>
        {
            // Main header row with company info and document type
            column.Item().Row(row =>
            {
                // Company information (left side)
                row.RelativeItem(3).Column(innerColumn =>
                {
                    innerColumn.Item().Text(sriConfiguration.LegalName ?? sriConfiguration.TradeName)
                        .FontSize(12).Bold();
                    innerColumn.Item().Text($"RUC: {sriConfiguration.CompanyRuc}").FontSize(9);
                    innerColumn.Item().Text($"{sriConfiguration.MainAddress}").FontSize(8);

                    if (!string.IsNullOrEmpty(establishment.Address))
                    {
                        innerColumn.Item().Text($"Establecimiento: {establishment.Name}").FontSize(8);
                        innerColumn.Item().Text($"Dirección: {establishment.Address}").FontSize(8);
                    }

                    innerColumn.Item().PaddingTop(5).Text($"Obligado a llevar contabilidad: {(sriConfiguration.AccountingRequired ? "SI" : "NO")}")
                        .FontSize(8);
                });

                // Document type box (right side)
                row.RelativeItem(2).Border(1).BorderColor(Colors.Grey.Darken2).Padding(5).Column(innerColumn =>
                {
                    innerColumn.Item().AlignCenter().Text("FACTURA").FontSize(14).Bold();
                    innerColumn.Item().AlignCenter().Text($"No. {invoice.InvoiceNumber}").FontSize(10).Bold();
                    innerColumn.Item().PaddingTop(3).AlignCenter().Text("NÚMERO DE AUTORIZACIÓN").FontSize(7);
                    innerColumn.Item().AlignCenter().Text(invoice.SriAuthorization ?? "").FontSize(8).Bold();
                    innerColumn.Item().PaddingTop(3).AlignCenter().Text($"FECHA AUTORIZACIÓN: {invoice.AuthorizationDate?.ToString("dd/MM/yyyy HH:mm:ss")}").FontSize(7);
                    innerColumn.Item().AlignCenter().Text($"AMBIENTE: {(invoice.Environment == SriEnvironment.Production ? "PRODUCCIÓN" : "PRUEBAS")}").FontSize(7);
                    innerColumn.Item().AlignCenter().Text($"EMISIÓN: {GetEmissionTypeText(invoice.DocumentType)}").FontSize(7);
                    innerColumn.Item().AlignCenter().Text("CLAVE DE ACCESO").FontSize(7);
                    innerColumn.Item().AlignCenter().Text(invoice.AccessKey ?? "").FontSize(7);
                });
            });

            // QR Code and customer info row
            column.Item().PaddingTop(10).Row(row =>
            {
                // QR Code (left)
                row.ConstantItem(100).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Column(qrColumn =>
                {
                    qrColumn.Item().AlignCenter().Text("QR VALIDACIÓN").FontSize(7).Bold();
                    qrColumn.Item().PaddingTop(5).Height(80).Element(element =>
                    {
                        // Generate QR code data
                        var qrData = GenerateQrData(invoice, sriConfiguration);
                        // Note: QuestPDF doesn't have built-in QR generation
                        // In production, use a QR library like QRCoder and embed the image
                        element.Background(Colors.Grey.Lighten3).AlignMiddle().AlignCenter()
                            .Text("[QR Code]").FontSize(8);
                    });
                });

                // Customer information (right)
                row.RelativeItem().PaddingLeft(10).Column(customerColumn =>
                {
                    customerColumn.Item().Text("DATOS DEL CLIENTE").FontSize(9).Bold();
                    customerColumn.Item().PaddingTop(3).Text($"Razón Social: {invoice.Customer?.Name ?? ""}").FontSize(8);
                    customerColumn.Item().Text($"Identificación: {invoice.Customer?.TaxId ?? ""}").FontSize(8);
                    var billingAddress = BuildAddress(invoice.Customer?.BillingStreet, invoice.Customer?.BillingCity, invoice.Customer?.BillingState);
                    customerColumn.Item().Text($"Dirección: {billingAddress}").FontSize(8);
                    customerColumn.Item().Text($"Teléfono: {invoice.Customer?.Phone ?? ""}").FontSize(8);
                    customerColumn.Item().Text($"Email: {invoice.Customer?.Email ?? ""}").FontSize(8);
                });
            });
        });
    }

    private void ComposeContent(IContainer container, Invoice invoice, SriConfiguration sriConfiguration)
    {
        container.PaddingTop(15).Column(column =>
        {
            // Invoice details
            column.Item().Text("DETALLE DE LA FACTURA").FontSize(10).Bold();

            // Items table
            column.Item().PaddingTop(5).Table(table =>
            {
                // Define columns
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(30); // Cantidad
                    columns.RelativeColumn(2); // Descripción
                    columns.ConstantColumn(60); // Precio Unit.
                    columns.ConstantColumn(50); // Descuento
                    columns.ConstantColumn(70); // Total sin IVA
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Cant.").FontSize(8).Bold();
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Descripción").FontSize(8).Bold();
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(3).AlignRight().Text("P. Unit.").FontSize(8).Bold();
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(3).AlignRight().Text("Descuento").FontSize(8).Bold();
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(3).AlignRight().Text("Total").FontSize(8).Bold();
                });

                // Items
                foreach (var item in invoice.Items)
                {
                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3)
                        .Text(item.Quantity.ToString("N2")).FontSize(8);
                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3)
                        .Text($"{item.ProductName}\n{item.Description}").FontSize(8);
                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3)
                        .AlignRight().Text($"${item.UnitPrice:N2}").FontSize(8);
                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3)
                        .AlignRight().Text("$0.00").FontSize(8);
                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3)
                        .AlignRight().Text($"${item.SubtotalAmount:N2}").FontSize(8);
                }
            });

            // Additional Information
            column.Item().PaddingTop(10).Row(row =>
            {
                // Left: Additional info
                row.RelativeItem().Column(leftColumn =>
                {
                    leftColumn.Item().Text("INFORMACIÓN ADICIONAL").FontSize(9).Bold();
                    leftColumn.Item().PaddingTop(3).Text($"Fecha Emisión: {invoice.IssueDate:dd/MM/yyyy}").FontSize(8);
                    leftColumn.Item().Text($"Forma de Pago: {GetPaymentMethodText(invoice.PaymentMethod)}").FontSize(8);
                    if (!string.IsNullOrEmpty(invoice.Notes))
                    {
                        leftColumn.Item().Text($"Observaciones: {invoice.Notes}").FontSize(8);
                    }
                });

                // Right: Totals
                row.ConstantItem(200).Column(totalsColumn =>
                {
                    totalsColumn.Item().Row(r =>
                    {
                        r.RelativeItem().Text("SUBTOTAL SIN IVA:").FontSize(8);
                        r.ConstantItem(70).AlignRight().Text($"${invoice.SubtotalAmount:N2}").FontSize(8);
                    });

                    totalsColumn.Item().Row(r =>
                    {
                        r.RelativeItem().Text("IVA:").FontSize(8);
                        r.ConstantItem(70).AlignRight().Text($"${invoice.TaxAmount:N2}").FontSize(8);
                    });

                    totalsColumn.Item().PaddingTop(3).Border(1).BorderColor(Colors.Grey.Darken1).Padding(3).Row(r =>
                    {
                        r.RelativeItem().Text("TOTAL:").FontSize(10).Bold();
                        r.ConstantItem(70).AlignRight().Text($"${invoice.TotalAmount:N2}").FontSize(10).Bold();
                    });
                });
            });
        });
    }

    private void ComposeFooter(IContainer container, Invoice invoice)
    {
        container.AlignCenter().Column(column =>
        {
            column.Item().PaddingTop(20).Text("DOCUMENTO ELECTRÓNICO AUTORIZADO POR EL SRI").FontSize(7).Italic();
            column.Item().Text($"Clave de Acceso: {invoice.AccessKey}").FontSize(6);
            column.Item().Text($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm:ss}").FontSize(6);
        });
    }

    private string GenerateQrData(Invoice invoice, SriConfiguration sriConfiguration)
    {
        // QR format for SRI Ecuador: AccessKey|IssueDate|RUC|Environment|AuthorizationNumber
        var issueDate = invoice.IssueDate.ToString("ddMMyyyy");
        var environment = invoice.Environment == SriEnvironment.Production ? "1" : "2";

        return $"{invoice.AccessKey}|{issueDate}|{sriConfiguration.CompanyRuc}|{environment}|{invoice.SriAuthorization}";
    }

    private string BuildAddress(string? street, string? city, string? state)
    {
        var parts = new[] { street, city, state }.Where(p => !string.IsNullOrEmpty(p));
        return string.Join(", ", parts);
    }

    private string GetEmissionTypeText(DocumentType documentType)
    {
        return documentType switch
        {
            DocumentType.Invoice => "NORMAL",
            _ => "NORMAL"
        };
    }

    private string GetPaymentMethodText(SriPaymentMethod paymentMethod)
    {
        return paymentMethod switch
        {
            SriPaymentMethod.Cash => "EFECTIVO",
            SriPaymentMethod.Check => "CHEQUE",
            SriPaymentMethod.BankTransfer => "TRANSFERENCIA BANCARIA",
            SriPaymentMethod.CreditCard => "TARJETA DE CRÉDITO",
            SriPaymentMethod.DebitCard => "TARJETA DE DÉBITO",
            SriPaymentMethod.Other => "OTROS",
            _ => "NO DEFINIDO"
        };
    }
}
