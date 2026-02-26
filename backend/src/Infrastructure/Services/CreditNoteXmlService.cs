using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using SaaS.Application.Interfaces;
using SaaS.Domain.Entities;

namespace SaaS.Infrastructure.Services;

/// <summary>
/// Service for generating SRI-compliant XML files for electronic credit notes (Nota de Crédito)
/// Document type 04, version 1.1.0
/// </summary>
public class CreditNoteXmlService : ICreditNoteXmlService
{
    private readonly string _storagePath;
    private readonly ISriAccessKeyService _accessKeyService;

    public CreditNoteXmlService(IConfiguration configuration, ISriAccessKeyService accessKeyService)
    {
        _storagePath = configuration["StorageSettings:BasePath"] ?? "storage";
        _accessKeyService = accessKeyService;
    }

    public async Task<(string FilePath, string AccessKey)> GenerateCreditNoteXmlAsync(
        CreditNote creditNote,
        SriConfiguration sriConfiguration,
        Establishment establishment,
        EmissionPoint emissionPoint)
    {
        // Generate access key using CreditNoteSequence
        var sequential = int.Parse(creditNote.CreditNoteNumber.Split('-').Last());
        var accessKey = _accessKeyService.GenerateAccessKey(
            creditNote.IssueDate,
            creditNote.DocumentType,
            sriConfiguration.CompanyRuc,
            creditNote.Environment,
            establishment.EstablishmentCode,
            emissionPoint.EmissionPointCode,
            sequential
        );

        // codDocModificado for original invoice is always "01" (Factura)
        const string codDocModificado = "01";

        // Build XML structure per SRI specification for notaCredito
        var xml = new XElement("notaCredito",
            new XAttribute("id", "comprobante"),
            new XAttribute("version", "1.1.0"),

            // infoTributaria - Tax information (same structure for all document types)
            new XElement("infoTributaria",
                new XElement("ambiente", ((int)creditNote.Environment).ToString()),
                new XElement("tipoEmision", "1"), // Normal emission
                new XElement("razonSocial", sriConfiguration.LegalName),
                new XElement("nombreComercial", sriConfiguration.TradeName),
                new XElement("ruc", sriConfiguration.CompanyRuc),
                new XElement("claveAcceso", accessKey.Value),
                new XElement("codDoc", ((int)creditNote.DocumentType).ToString("00")),
                new XElement("estab", establishment.EstablishmentCode),
                new XElement("ptoEmi", emissionPoint.EmissionPointCode),
                new XElement("secuencial", creditNote.CreditNoteNumber.Split('-').Last().PadLeft(9, '0')),
                new XElement("dirMatriz", sriConfiguration.MainAddress)
            ),

            // infoNotaCredito — Credit note specific information
            new XElement("infoNotaCredito",
                new XElement("fechaEmision", creditNote.IssueDate.ToString("dd/MM/yyyy")),
                new XElement("dirEstablecimiento", establishment.Address),
                new XElement("tipoIdentificacionComprador", ((int)creditNote.Customer.IdentificationType).ToString("00")),
                new XElement("razonSocialComprador", creditNote.Customer.Name),
                new XElement("identificacionComprador", creditNote.Customer.TaxId ?? "9999999999999"),
                string.IsNullOrWhiteSpace(sriConfiguration.SpecialTaxpayerNumber)
                    ? null
                    : new XElement("contribuyenteEspecial", sriConfiguration.SpecialTaxpayerNumber),
                new XElement("obligadoContabilidad", sriConfiguration.AccountingRequired ? "SI" : "NO"),
                new XElement("codDocModificado", codDocModificado),
                new XElement("numDocModificado", creditNote.OriginalInvoiceNumber),
                new XElement("fechaEmisionDocSustento", creditNote.OriginalInvoiceDate.ToString("dd/MM/yyyy")),
                new XElement("totalSinImpuestos", creditNote.SubtotalAmount.ToString("F2")),
                new XElement("valorModificacion", creditNote.ValueModification.ToString("F2")),
                new XElement("moneda", "DOLAR"),
                new XElement("totalConImpuestos",
                    GenerateTaxTotals(creditNote)
                ),
                new XElement("motivo", creditNote.Reason)
            ),

            // detalles — Credit note line items
            new XElement("detalles",
                creditNote.Items.Select(item => GenerateItemDetail(item))
            ),

            // infoAdicional — Additional information
            new XElement("infoAdicional",
                new XElement("campoAdicional", new XAttribute("nombre", "Email"), creditNote.Customer.Email),
                string.IsNullOrEmpty(creditNote.Customer.Phone)
                    ? null
                    : new XElement("campoAdicional", new XAttribute("nombre", "Telefono"), creditNote.Customer.Phone),
                string.IsNullOrEmpty(creditNote.Notes)
                    ? null
                    : new XElement("campoAdicional", new XAttribute("nombre", "Observaciones"), creditNote.Notes)
            )
        );

        // Save XML to file
        var filePath = await SaveXmlToFileAsync(xml, creditNote.TenantId.ToString(), accessKey.Value);

        return (filePath, accessKey.Value);
    }

    private IEnumerable<XElement> GenerateTaxTotals(CreditNote creditNote)
    {
        var taxGroups = creditNote.Items.GroupBy(i => i.TaxRate);

        foreach (var group in taxGroups)
        {
            var taxRate = group.Key;
            var baseImponible = group.Sum(i => i.Quantity * i.UnitPrice);
            var valorImpuesto = group.Sum(i => i.TaxAmount);

            yield return new XElement("totalImpuesto",
                new XElement("codigo", "2"), // IVA code
                new XElement("codigoPorcentaje", GetTaxRateCode(taxRate)),
                new XElement("baseImponible", baseImponible.ToString("F2")),
                new XElement("valor", valorImpuesto.ToString("F2"))
            );
        }
    }

    private static XElement GenerateItemDetail(CreditNoteItem item)
    {
        return new XElement("detalle",
            new XElement("codigoPrincipal", (item.ProductCode ?? "N/A")[..Math.Min(25, (item.ProductCode ?? "N/A").Length)]),
            new XElement("descripcion", item.Description),
            new XElement("cantidad", item.Quantity.ToString("F6")),
            new XElement("precioUnitario", item.UnitPrice.ToString("F6")),
            new XElement("descuento", "0.00"),
            new XElement("precioTotalSinImpuesto", (item.Quantity * item.UnitPrice).ToString("F2")),
            new XElement("impuestos",
                new XElement("impuesto",
                    new XElement("codigo", "2"), // IVA
                    new XElement("codigoPorcentaje", GetTaxRateCode(item.TaxRate)),
                    new XElement("tarifa", item.TaxRate.ToString("F2")),
                    new XElement("baseImponible", (item.Quantity * item.UnitPrice).ToString("F2")),
                    new XElement("valor", item.TaxAmount.ToString("F2"))
                )
            )
        );
    }

    private static string GetTaxRateCode(decimal taxRate)
    {
        return taxRate switch
        {
            0m => "0",      // 0%
            12m => "2",     // 12%
            14m => "3",     // 14%
            15m => "4",     // 15%
            _ => "0"        // Default to 0%
        };
    }

    private async Task<string> SaveXmlToFileAsync(XElement xml, string tenantId, string accessKey)
    {
        var year = DateTime.Now.Year;
        var month = DateTime.Now.Month;

        var directory = Path.Combine(_storagePath, tenantId, "credit-notes", year.ToString(), month.ToString("00"));
        Directory.CreateDirectory(directory);

        var fileName = $"{accessKey}.xml";
        var filePath = Path.Combine(directory, fileName);

        await File.WriteAllTextAsync(filePath, xml.ToString());

        return filePath;
    }

    public Task<bool> ValidateXmlAsync(string xmlFilePath)
    {
        try
        {
            var xml = XDocument.Load(xmlFilePath);

            var hasInfoTributaria = xml.Descendants("infoTributaria").Any();
            var hasInfoNotaCredito = xml.Descendants("infoNotaCredito").Any();
            var hasDetalles = xml.Descendants("detalles").Any();

            return Task.FromResult(hasInfoTributaria && hasInfoNotaCredito && hasDetalles);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public async Task<string> ReadXmlContentAsync(string xmlFilePath)
    {
        return await File.ReadAllTextAsync(xmlFilePath);
    }
}
