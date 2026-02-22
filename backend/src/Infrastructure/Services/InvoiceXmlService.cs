using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using SaaS.Application.Interfaces;
using SaaS.Domain.Entities;

namespace SaaS.Infrastructure.Services;

/// <summary>
/// Service for generating SRI-compliant XML files for electronic invoices
/// </summary>
public class InvoiceXmlService : IInvoiceXmlService
{
    private readonly string _storagePath;
    private readonly ISriAccessKeyService _accessKeyService;

    public InvoiceXmlService(IConfiguration configuration, ISriAccessKeyService accessKeyService)
    {
        _storagePath = configuration["StorageSettings:BasePath"] ?? "storage";
        _accessKeyService = accessKeyService;
    }

    public async Task<(string FilePath, string AccessKey)> GenerateInvoiceXmlAsync(
        Invoice invoice,
        SriConfiguration sriConfiguration,
        Establishment establishment,
        EmissionPoint emissionPoint)
    {
        // Generate access key
        var accessKey = _accessKeyService.GenerateAccessKey(
            invoice.IssueDate,
            invoice.DocumentType,
            sriConfiguration.CompanyRuc,
            invoice.Environment,
            establishment.EstablishmentCode,
            emissionPoint.EmissionPointCode,
            invoice.InvoiceNumber.Split('-').Last() != null ? int.Parse(invoice.InvoiceNumber.Split('-').Last()) : 1
        );

        // Build XML structure per SRI specification
        var xml = new XElement("factura",
            new XAttribute("id", "comprobante"),
            new XAttribute("version", "1.1.0"),

            // infoTributaria - Tax information
            new XElement("infoTributaria",
                new XElement("ambiente", ((int)invoice.Environment).ToString()),
                new XElement("tipoEmision", "1"), // Normal emission
                new XElement("razonSocial", sriConfiguration.LegalName),
                new XElement("nombreComercial", sriConfiguration.TradeName),
                new XElement("ruc", sriConfiguration.CompanyRuc),
                new XElement("claveAcceso", accessKey.Value),
                new XElement("codDoc", ((int)invoice.DocumentType).ToString("00")),
                new XElement("estab", establishment.EstablishmentCode),
                new XElement("ptoEmi", emissionPoint.EmissionPointCode),
                new XElement("secuencial", invoice.InvoiceNumber.Split('-').Last().PadLeft(9, '0')),
                new XElement("dirMatriz", sriConfiguration.MainAddress)
            ),

            // infoFactura - Invoice information
            new XElement("infoFactura",
                new XElement("fechaEmision", invoice.IssueDate.ToString("dd/MM/yyyy")),
                new XElement("dirEstablecimiento", establishment.Address),
                string.IsNullOrWhiteSpace(sriConfiguration.SpecialTaxpayerNumber)
                    ? null
                    : new XElement("contribuyenteEspecial", sriConfiguration.SpecialTaxpayerNumber),
                new XElement("obligadoContabilidad", sriConfiguration.AccountingRequired ? "SI" : "NO"),
                new XElement("tipoIdentificacionComprador", ((int)invoice.Customer.IdentificationType).ToString("00")),
                new XElement("razonSocialComprador", invoice.Customer.Name),
                new XElement("identificacionComprador", invoice.Customer.TaxId ?? "9999999999999"),
                new XElement("totalSinImpuestos", invoice.SubtotalAmount.ToString("F2")),
                new XElement("totalDescuento", "0.00"),
                // totalConImpuestos
                new XElement("totalConImpuestos",
                    GenerateTaxTotals(invoice)
                ),new XElement("propina", "0.00"),
                new XElement("importeTotal", invoice.TotalAmount.ToString("F2")),
                new XElement("moneda", "DOLAR"),
                // pagos
                new XElement("pagos",
                    new XElement("pago",
                        new XElement("formaPago", ((int)invoice.PaymentMethod).ToString("00")),
                        new XElement("total", invoice.TotalAmount.ToString("F2")),
                        new XElement("plazo", "0"),
                        new XElement("unidadTiempo", "dias")
                    )
                )
            ),

            // detalles - Invoice items
            new XElement("detalles",
                invoice.Items.Select(item => GenerateItemDetail(item))
            ),

            // infoAdicional - Additional information
            new XElement("infoAdicional",
                new XElement("campoAdicional", new XAttribute("nombre", "Email"), invoice.Customer.Email),
                string.IsNullOrEmpty(invoice.Customer.Phone)
                    ? null
                    : new XElement("campoAdicional", new XAttribute("nombre", "Telefono"), invoice.Customer.Phone),
                string.IsNullOrEmpty(invoice.Notes)
                    ? null
                    : new XElement("campoAdicional", new XAttribute("nombre", "Observaciones"), invoice.Notes)
            )
        );

        // Save XML to file
        var filePath = await SaveXmlToFileAsync(xml, invoice.TenantId.ToString(), accessKey.Value);

        return (filePath, accessKey.Value);
    }

    private IEnumerable<XElement> GenerateTaxTotals(Invoice invoice)
    {
        // Group items by tax rate
        var taxGroups = invoice.Items.GroupBy(i => i.TaxRate);

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

    private XElement GenerateItemDetail(InvoiceItem item)
    {
        return new XElement("detalle",
            new XElement("codigoPrincipal", item.ProductCode ?? "N/A"),
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

    private string GetTaxRateCode(decimal taxRate)
    {
        // SRI tax rate codes
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

        var directory = Path.Combine(_storagePath, tenantId, "invoices", year.ToString(), month.ToString("00"));
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
            // Basic XML validation
            var xml = XDocument.Load(xmlFilePath);

            // Check for required elements
            var hasInfoTributaria = xml.Descendants("infoTributaria").Any();
            var hasInfoFactura = xml.Descendants("infoFactura").Any();
            var hasDetalles = xml.Descendants("detalles").Any();

            return Task.FromResult(hasInfoTributaria && hasInfoFactura && hasDetalles);
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
