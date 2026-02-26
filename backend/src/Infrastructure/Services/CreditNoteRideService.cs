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
/// Service for generating RIDE PDFs for electronic credit notes (Nota de Crédito)
/// Legal requirement for electronic billing in Ecuador
/// </summary>
public class CreditNoteRideService : ICreditNoteRideService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<CreditNoteRideService> _logger;
    private readonly string _storagePath;

    public CreditNoteRideService(
        IConfiguration configuration,
        ILogger<CreditNoteRideService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _storagePath = _configuration["StorageSettings:BasePath"] ?? "storage";

        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<string> GenerateRidePdfAsync(
        CreditNote creditNote,
        SriConfiguration sriConfiguration,
        Establishment establishment,
        EmissionPoint emissionPoint)
    {
        try
        {
            _logger.LogInformation("Generating RIDE PDF for credit note {CreditNoteId}", creditNote.Id);

            if (string.IsNullOrEmpty(creditNote.AccessKey))
                throw new InvalidOperationException("Access key is required to generate RIDE");

            if (string.IsNullOrEmpty(creditNote.SriAuthorization))
                throw new InvalidOperationException("SRI authorization is required to generate RIDE");

            var year = creditNote.IssueDate.Year;
            var month = creditNote.IssueDate.Month;
            var directoryPath = Path.Combine(_storagePath, creditNote.TenantId.ToString(), "credit-notes", year.ToString(), month.ToString("D2"));
            Directory.CreateDirectory(directoryPath);

            var fileName = $"{creditNote.AccessKey}_ride.pdf";
            var filePath = Path.Combine(directoryPath, fileName);

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

                        page.Header().Element(c => ComposeHeader(c, creditNote, sriConfiguration, establishment, emissionPoint));
                        page.Content().Element(c => ComposeContent(c, creditNote, sriConfiguration));
                        page.Footer().Element(c => ComposeFooter(c, creditNote));
                    });
                }).GeneratePdf(filePath);
            });

            _logger.LogInformation("RIDE PDF generated successfully at {FilePath}", filePath);

            return filePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating RIDE PDF for credit note {CreditNoteId}", creditNote.Id);
            throw;
        }
    }

    private void ComposeHeader(IContainer container, CreditNote creditNote, SriConfiguration sriConfiguration, Establishment establishment, EmissionPoint emissionPoint)
    {
        container.Column(column =>
        {
            // Test environment watermark
            if (creditNote.Environment == SriEnvironment.Test)
            {
                column.Item().AlignCenter().Background(Colors.Yellow.Lighten3).Padding(3)
                    .Text("⚠ AMBIENTE DE PRUEBAS — NO VÁLIDO TRIBUTARIAMENTE")
                    .FontSize(8).Bold().FontColor(Colors.Orange.Darken2);
            }

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
                    innerColumn.Item().AlignCenter().Text("NOTA DE CRÉDITO").FontSize(13).Bold();
                    innerColumn.Item().AlignCenter().Text($"No. {creditNote.CreditNoteNumber}").FontSize(10).Bold();
                    innerColumn.Item().PaddingTop(3).AlignCenter().Text("NÚMERO DE AUTORIZACIÓN").FontSize(7);
                    innerColumn.Item().AlignCenter().Text(creditNote.SriAuthorization ?? "").FontSize(8).Bold();
                    innerColumn.Item().PaddingTop(3).AlignCenter().Text($"FECHA AUTORIZACIÓN: {creditNote.AuthorizationDate?.ToString("dd/MM/yyyy HH:mm:ss")}").FontSize(7);
                    innerColumn.Item().AlignCenter().Text($"AMBIENTE: {(creditNote.Environment == SriEnvironment.Production ? "PRODUCCIÓN" : "PRUEBAS")}").FontSize(7);
                    innerColumn.Item().AlignCenter().Text("CLAVE DE ACCESO").FontSize(7);
                    innerColumn.Item().AlignCenter().Text(creditNote.AccessKey ?? "").FontSize(7);
                });
            });

            // Customer info row
            column.Item().PaddingTop(10).Row(row =>
            {
                row.ConstantItem(100).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Column(qrColumn =>
                {
                    qrColumn.Item().AlignCenter().Text("QR VALIDACIÓN").FontSize(7).Bold();
                    qrColumn.Item().PaddingTop(5).Height(80).Element(element =>
                    {
                        element.Background(Colors.Grey.Lighten3).AlignMiddle().AlignCenter()
                            .Text("[QR Code]").FontSize(8);
                    });
                });

                row.RelativeItem().PaddingLeft(10).Column(customerColumn =>
                {
                    customerColumn.Item().Text("DATOS DEL CLIENTE").FontSize(9).Bold();
                    customerColumn.Item().PaddingTop(3).Text($"Razón Social: {creditNote.Customer?.Name ?? ""}").FontSize(8);
                    customerColumn.Item().Text($"Identificación: {creditNote.Customer?.TaxId ?? ""}").FontSize(8);
                    customerColumn.Item().Text($"Teléfono: {creditNote.Customer?.Phone ?? ""}").FontSize(8);
                    customerColumn.Item().Text($"Email: {creditNote.Customer?.Email ?? ""}").FontSize(8);
                    customerColumn.Item().PaddingTop(5).Text("DOCUMENTO MODIFICADO").FontSize(9).Bold();
                    customerColumn.Item().Text($"Núm. Documento: {creditNote.OriginalInvoiceNumber}").FontSize(8);
                    customerColumn.Item().Text($"Fecha Emisión Doc.: {creditNote.OriginalInvoiceDate:dd/MM/yyyy}").FontSize(8);
                });
            });
        });
    }

    private void ComposeContent(IContainer container, CreditNote creditNote, SriConfiguration sriConfiguration)
    {
        container.PaddingTop(15).Column(column =>
        {
            // Reason for credit note
            column.Item().Background(Colors.Grey.Lighten3).Padding(5).Row(row =>
            {
                row.RelativeItem().Text("MOTIVO:").FontSize(9).Bold();
                row.RelativeItem(4).Text(creditNote.Reason).FontSize(9);
            });

            // Items table
            column.Item().PaddingTop(10).Text("DETALLE DE LA NOTA DE CRÉDITO").FontSize(10).Bold();
            column.Item().PaddingTop(5).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(30);   // Cantidad
                    columns.RelativeColumn(2);     // Descripción
                    columns.ConstantColumn(60);    // Precio Unit.
                    columns.ConstantColumn(50);    // Descuento
                    columns.ConstantColumn(70);    // Total sin IVA
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Cant.").FontSize(8).Bold();
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Descripción").FontSize(8).Bold();
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(3).AlignRight().Text("P. Unit.").FontSize(8).Bold();
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(3).AlignRight().Text("Descuento").FontSize(8).Bold();
                    header.Cell().Background(Colors.Grey.Lighten2).Padding(3).AlignRight().Text("Total").FontSize(8).Bold();
                });

                foreach (var item in creditNote.Items)
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

            // Totals area
            column.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Column(leftColumn =>
                {
                    leftColumn.Item().Text("INFORMACIÓN ADICIONAL").FontSize(9).Bold();
                    leftColumn.Item().PaddingTop(3).Text($"Fecha Emisión: {creditNote.IssueDate:dd/MM/yyyy}").FontSize(8);
                    leftColumn.Item().Text($"Forma de Pago: {GetPaymentMethodText(creditNote.PaymentMethod)}").FontSize(8);
                    if (!string.IsNullOrEmpty(creditNote.Notes))
                    {
                        leftColumn.Item().Text($"Observaciones: {creditNote.Notes}").FontSize(8);
                    }
                });

                row.ConstantItem(200).Column(totalsColumn =>
                {
                    totalsColumn.Item().Row(r =>
                    {
                        r.RelativeItem().Text("SUBTOTAL SIN IVA:").FontSize(8);
                        r.ConstantItem(70).AlignRight().Text($"${creditNote.SubtotalAmount:N2}").FontSize(8);
                    });

                    totalsColumn.Item().Row(r =>
                    {
                        r.RelativeItem().Text("IVA:").FontSize(8);
                        r.ConstantItem(70).AlignRight().Text($"${creditNote.TaxAmount:N2}").FontSize(8);
                    });

                    totalsColumn.Item().PaddingTop(3).Border(1).BorderColor(Colors.Grey.Darken1).Padding(3).Row(r =>
                    {
                        r.RelativeItem().Text("VALOR MODIFICACIÓN:").FontSize(9).Bold();
                        r.ConstantItem(70).AlignRight().Text($"${creditNote.ValueModification:N2}").FontSize(9).Bold();
                    });
                });
            });
        });
    }

    private void ComposeFooter(IContainer container, CreditNote creditNote)
    {
        container.AlignCenter().Column(column =>
        {
            column.Item().PaddingTop(20).Text("DOCUMENTO ELECTRÓNICO AUTORIZADO POR EL SRI").FontSize(7).Italic();
            if (creditNote.Environment == SriEnvironment.Test)
            {
                column.Item().Text("AMBIENTE DE PRUEBAS — NO VÁLIDO TRIBUTARIAMENTE").FontSize(7).Bold().FontColor(Colors.Orange.Darken2);
            }
            column.Item().Text($"Clave de Acceso: {creditNote.AccessKey}").FontSize(6);
            column.Item().Text($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm:ss}").FontSize(6);
        });
    }

    private static string GetPaymentMethodText(SriPaymentMethod paymentMethod)
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
