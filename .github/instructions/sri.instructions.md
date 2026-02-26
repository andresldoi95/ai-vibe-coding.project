---
applyTo: "**"
---

# SRI Electronic Billing Instructions

You are an expert in Ecuador SRI (Servicio de Rentas Internas) electronic billing integration.
Consolidates: **SRI Agent** — XML generation, digital signing, SOAP services, RIDE PDF, status machine.

---

## System Context

Ecuador SRI mandates electronic documents for all authorized taxpayers. Every document goes through:

```
Generate XML → Sign XML (XAdES-BES) → Submit to SRI (SOAP) → Poll Authorization → Generate RIDE PDF
```

Two environments exist — **never mix them**:

| `SriEnvironment` | Value | Reception Endpoint | Authorization Endpoint |
|---|---|---|---|
| `Test` | `1` | `https://celcer.sri.gob.ec/comprobantes-electronicos-ws/RecepcionComprobantesOffline` | `https://celcer.sri.gob.ec/comprobantes-electronicos-ws/AutorizacionComprobantesOffline` |
| `Production` | `2` | `https://cel.sri.gob.ec/comprobantes-electronicos-ws/RecepcionComprobantesOffline` | `https://cel.sri.gob.ec/comprobantes-electronicos-ws/AutorizacionComprobantesOffline` |

Endpoint URLs are resolved at runtime from `appsettings.json` (`Sri:ReceptionEndpoint` / `Sri:AuthorizationEndpoint`). The `SriSoapClient` defaults to Test if not configured.

---

## Document Types

| `DocumentType` enum | `int` value | `codDoc` (XML) | Root Element | version | Entity module |
|---|---|---|---|---|---|
| `Invoice` | `1` | `01` | `<factura>` | `1.1.0` | `Invoices` |
| `CreditNote` | `4` | `04` | `<notaCredito>` | `1.1.0` | `CreditNotes` |
| `DebitNote` | `5` | `05` | `<notaDebito>` | `1.0.0` | `DebitNotes` |
| `Retention` | `7` | `07` | `<comprobanteRetencion>` | `1.0.0` | `Retentions` |

Always cast with `((int)documentType).ToString("00")` to produce the two-digit code.

---

## Access Key (Clave de Acceso) — 49 digits

Format (no separators, left-padded with zeros):

```
DD MM YYYY TT RRRRRRRRRRRRR E EEE SSS ######### NNNNNNNN T C
 2   2   4  2      13       1  3   3      9          8    1 1 = 49 total
```

| Position | Length | Field | Source |
|---|---|---|---|
| 1-2 | 2 | Day | `issueDate.ToString("dd")` |
| 3-4 | 2 | Month | `issueDate.ToString("MM")` |
| 5-8 | 4 | Year | `issueDate.ToString("yyyy")` |
| 9-10 | 2 | Document type | `((int)documentType).ToString("00")` |
| 11-23 | 13 | RUC | `sriConfiguration.CompanyRuc` |
| 24 | 1 | Environment | `((int)environment).ToString()` |
| 25-27 | 3 | Establishment code | `establishment.EstablishmentCode` — always 3 digits |
| 28-30 | 3 | Emission point code | `emissionPoint.EmissionPointCode` — always 3 digits |
| 31-39 | 9 | Sequential | `sequential.ToString("000000000")` — max 999999999 |
| 40-47 | 8 | Numeric code | 8 random digits (generated internally) |
| 48 | 1 | Emission type | `((int)emissionType).ToString()` — `1`=Normal, `2`=Contingency |
| 49 | 1 | Check digit | Modulo 11 algorithm (see `AccessKey.CalculateModulo11CheckDigit`) |

**Rules:**
- **Always use `AccessKey.Generate(...)`** — never construct the key manually
- Validate inputs before calling: RUC= 13 chars, establishment= 3 chars, emissionPoint= 3 chars, sequential 1–999999999
- The check digit uses Modulo 11 with weights `[2, 3, 4, 5, 6, 7]` cycling right-to-left; if result is 10 → `1`, if 11 → `0`

---

## XML Generation Rules

### Core constraints
- One service per document type: `IInvoiceXmlService`, `ICreditNoteXmlService`, `IDebitNoteXmlService`, `IRetentionXmlService`
- All interfaces live in `Application/Interfaces/`; implementations in `Infrastructure/Services/`
- Root element **must** carry `id="comprobante"` — the XML signer references it via `URI="#comprobante"`
- Include `version` attribute directly on the root element (not a child)
- Build XML with `System.Xml.Linq.XElement` (see existing `InvoiceXmlService.cs`)
- **Do not** add an XML declaration when using `XElement.ToString()` — the signer handles encoding
- Numeric values: amounts always `ToString("F2")`, quantities always `ToString("F6")`
- Date format: `dd/MM/yyyy` (slash-separated, SRI requirement)
- `obligadoContabilidad`: `"SI"` or `"NO"` (all caps)
- `contribuyenteEspecial`: include element only when `sriConfiguration.SpecialTaxpayerNumber` is not empty — omit entirely otherwise
- `infoAdicional` must include at minimum: email (`nombre="Email"`); add phone and notes when available
- File naming: `{accessKey}.xml` (unsigned) → `{accessKey}_signed.xml` (after signing)
- Storage path: `{storagePath}/{tenantId}/{module}/{year}/{month:00}/`

### SRI Catalog Codes — IVA Tax (`codigo = "2"`)

| `codigoPorcentaje` | Rate |
|---|---|
| `"0"` | 0% |
| `"2"` | 12% |
| `"3"` | 14% |
| `"4"` | 15% |
| `"6"` | No IVA (exempt) |
| `"7"` | No IVA (not subject) |

Use the `GetTaxRateCode(decimal taxRate)` switch pattern from `InvoiceXmlService.cs`.

### Identification Type Codes

| `IdentificationType` | `int` | XML value |
|---|---|---|
| `Ruc` | `4` | `"04"` |
| `Cedula` | `5` | `"05"` |
| `Passport` | `6` | `"06"` |
| `ConsumerFinal` | `7` | `"07"` |
| `ForeignId` | `8` | `"08"` |

Always cast: `((int)identificationType).ToString("00")`.

**Consumer Final (`07`)**: `identificacionComprador` must be `"9999999999999"`.

### Payment Method Codes (`formaPago`)

| `SriPaymentMethod` | XML `formaPago` |
|---|---|
| `Cash` | `"01"` |
| `Check` | `"02"` |
| `BankTransfer` | `"03"` |
| `AccountDeposit` | `"04"` |
| `DebitCard` | `"16"` |
| `CreditCard` | `"19"` |
| `ElectronicMoney` | `"17"` |
| `Other` | `"20"` |

---

## Complete XML Skeletons

### Factura — `codDoc: 01` — version `1.1.0`

```xml
<?xml version="1.0" encoding="UTF-8"?>
<factura id="comprobante" version="1.1.0">
  <infoTributaria>
    <ambiente>1</ambiente>                        <!-- SriEnvironment int value -->
    <tipoEmision>1</tipoEmision>                  <!-- 1=Normal, 2=Contingency -->
    <razonSocial>EMPRESA S.A.</razonSocial>        <!-- sriConfiguration.LegalName -->
    <nombreComercial>EMPRESA</nombreComercial>     <!-- sriConfiguration.TradeName -->
    <ruc>0990000000001</ruc>                       <!-- sriConfiguration.CompanyRuc -->
    <claveAcceso>4902202501019990000000001100100100000000112345671</claveAcceso>
    <codDoc>01</codDoc>
    <estab>001</estab>                             <!-- establishment.EstablishmentCode -->
    <ptoEmi>001</ptoEmi>                           <!-- emissionPoint.EmissionPointCode -->
    <secuencial>000000001</secuencial>             <!-- sequential.ToString("000000000") -->
    <dirMatriz>AV. PRINCIPAL 123</dirMatriz>       <!-- sriConfiguration.MainAddress -->
  </infoTributaria>
  <infoFactura>
    <fechaEmision>25/02/2026</fechaEmision>        <!-- issueDate.ToString("dd/MM/yyyy") -->
    <dirEstablecimiento>AV. SUCURSAL 456</dirEstablecimiento>  <!-- establishment.Address -->
    <!-- Include ONLY when sriConfiguration.SpecialTaxpayerNumber is non-empty: -->
    <contribuyenteEspecial>12345</contribuyenteEspecial>
    <obligadoContabilidad>SI</obligadoContabilidad>  <!-- "SI" | "NO" -->
    <tipoIdentificacionComprador>04</tipoIdentificacionComprador>
    <razonSocialComprador>CLIENTE S.A.</razonSocialComprador>
    <identificacionComprador>0990000000002</identificacionComprador>
    <totalSinImpuestos>100.00</totalSinImpuestos>
    <totalDescuento>0.00</totalDescuento>
    <totalConImpuestos>
      <totalImpuesto>
        <codigo>2</codigo>                         <!-- always "2" for IVA -->
        <codigoPorcentaje>4</codigoPorcentaje>     <!-- tax rate code: 0|2|3|4|6|7 -->
        <baseImponible>100.00</baseImponible>
        <valor>15.00</valor>
      </totalImpuesto>
    </totalConImpuestos>
    <propina>0.00</propina>
    <importeTotal>115.00</importeTotal>
    <moneda>DOLAR</moneda>
    <pagos>
      <pago>
        <formaPago>01</formaPago>
        <total>115.00</total>
        <plazo>0</plazo>
        <unidadTiempo>dias</unidadTiempo>
      </pago>
    </pagos>
  </infoFactura>
  <detalles>
    <detalle>
      <codigoPrincipal>SKU-001</codigoPrincipal>   <!-- max 25 chars -->
      <descripcion>Producto A</descripcion>
      <cantidad>1.000000</cantidad>
      <precioUnitario>100.000000</precioUnitario>
      <descuento>0.00</descuento>
      <precioTotalSinImpuesto>100.00</precioTotalSinImpuesto>
      <impuestos>
        <impuesto>
          <codigo>2</codigo>
          <codigoPorcentaje>4</codigoPorcentaje>
          <tarifa>15.00</tarifa>
          <baseImponible>100.00</baseImponible>
          <valor>15.00</valor>
        </impuesto>
      </impuestos>
    </detalle>
  </detalles>
  <infoAdicional>
    <campoAdicional nombre="Email">cliente@email.com</campoAdicional>
    <campoAdicional nombre="Telefono">0999999999</campoAdicional>  <!-- omit if empty -->
    <campoAdicional nombre="Observaciones">Nota</campoAdicional>   <!-- omit if empty -->
  </infoAdicional>
</factura>
```

---

### Nota de Crédito — `codDoc: 04` — version `1.1.0`

**Required document-specific fields:** `codDocModificado`, `numDocModificado`, `fechaEmisionDocSustento`, `valorModificacion`, `motivo`.

```xml
<?xml version="1.0" encoding="UTF-8"?>
<notaCredito id="comprobante" version="1.1.0">
  <infoTributaria>
    <ambiente>1</ambiente>
    <tipoEmision>1</tipoEmision>
    <razonSocial>EMPRESA S.A.</razonSocial>
    <nombreComercial>EMPRESA</nombreComercial>
    <ruc>0990000000001</ruc>
    <claveAcceso>2502202504019990000000001100100100000000122345671</claveAcceso>
    <codDoc>04</codDoc>
    <estab>001</estab>
    <ptoEmi>001</ptoEmi>
    <secuencial>000000001</secuencial>
    <dirMatriz>AV. PRINCIPAL 123</dirMatriz>
  </infoTributaria>
  <infoNotaCredito>
    <fechaEmision>25/02/2026</fechaEmision>
    <dirEstablecimiento>AV. SUCURSAL 456</dirEstablecimiento>
    <tipoIdentificacionComprador>04</tipoIdentificacionComprador>
    <razonSocialComprador>CLIENTE S.A.</razonSocialComprador>
    <identificacionComprador>0990000000002</identificacionComprador>
    <!-- Include ONLY when sriConfiguration.SpecialTaxpayerNumber is non-empty: -->
    <contribuyenteEspecial>12345</contribuyenteEspecial>
    <obligadoContabilidad>SI</obligadoContabilidad>
    <codDocModificado>01</codDocModificado>          <!-- codDoc of original document -->
    <numDocModificado>001-001-000000001</numDocModificado>  <!-- estab-ptoEmi-secuencial -->
    <fechaEmisionDocSustento>20/02/2026</fechaEmisionDocSustento>
    <totalSinImpuestos>100.00</totalSinImpuestos>
    <valorModificacion>115.00</valorModificacion>    <!-- total amount being credited -->
    <moneda>DOLAR</moneda>
    <totalConImpuestos>
      <totalImpuesto>
        <codigo>2</codigo>
        <codigoPorcentaje>4</codigoPorcentaje>
        <baseImponible>100.00</baseImponible>
        <valor>15.00</valor>
      </totalImpuesto>
    </totalConImpuestos>
    <motivo>Devolución de mercadería por defecto</motivo>  <!-- required, free text -->
  </infoNotaCredito>
  <detalles>
    <detalle>
      <codigoPrincipal>SKU-001</codigoPrincipal>
      <descripcion>Producto A</descripcion>
      <cantidad>1.000000</cantidad>
      <precioUnitario>100.000000</precioUnitario>
      <descuento>0.00</descuento>
      <precioTotalSinImpuesto>100.00</precioTotalSinImpuesto>
      <impuestos>
        <impuesto>
          <codigo>2</codigo>
          <codigoPorcentaje>4</codigoPorcentaje>
          <tarifa>15.00</tarifa>
          <baseImponible>100.00</baseImponible>
          <valor>15.00</valor>
        </impuesto>
      </impuestos>
    </detalle>
  </detalles>
  <infoAdicional>
    <campoAdicional nombre="Email">cliente@email.com</campoAdicional>
  </infoAdicional>
</notaCredito>
```

---

### Nota de Débito — `codDoc: 05` — version `1.0.0`

**Required document-specific fields:** `codDocModificado`, `numDocModificado`, `fechaEmisionDocSustento`. Line items go in `<motivos>` (not `<detalles>`).

```xml
<?xml version="1.0" encoding="UTF-8"?>
<notaDebito id="comprobante" version="1.0.0">
  <infoTributaria>
    <ambiente>1</ambiente>
    <tipoEmision>1</tipoEmision>
    <razonSocial>EMPRESA S.A.</razonSocial>
    <nombreComercial>EMPRESA</nombreComercial>
    <ruc>0990000000001</ruc>
    <claveAcceso>2502202505019990000000001100100100000000132345671</claveAcceso>
    <codDoc>05</codDoc>
    <estab>001</estab>
    <ptoEmi>001</ptoEmi>
    <secuencial>000000001</secuencial>
    <dirMatriz>AV. PRINCIPAL 123</dirMatriz>
  </infoTributaria>
  <infoNotaDebito>
    <fechaEmision>25/02/2026</fechaEmision>
    <tipoIdentificacionComprador>04</tipoIdentificacionComprador>
    <razonSocialComprador>CLIENTE S.A.</razonSocialComprador>
    <identificacionComprador>0990000000002</identificacionComprador>
    <!-- Include ONLY when sriConfiguration.SpecialTaxpayerNumber is non-empty: -->
    <contribuyenteEspecial>12345</contribuyenteEspecial>
    <obligadoContabilidad>SI</obligadoContabilidad>
    <codDocModificado>01</codDocModificado>
    <numDocModificado>001-001-000000001</numDocModificado>
    <fechaEmisionDocSustento>20/02/2026</fechaEmisionDocSustento>
    <totalSinImpuestos>10.00</totalSinImpuestos>
    <impuestos>
      <impuesto>
        <codigo>2</codigo>
        <codigoPorcentaje>4</codigoPorcentaje>
        <tarifa>15.00</tarifa>
        <baseImponible>10.00</baseImponible>
        <valor>1.50</valor>
      </impuesto>
    </impuestos>
    <valorTotal>11.50</valorTotal>
    <moneda>DOLAR</moneda>
  </infoNotaDebito>
  <motivos>
    <motivo>
      <razon>Intereses por mora en pago</razon>  <!-- description of the charge -->
      <valor>10.00</valor>
    </motivo>
  </motivos>
  <infoAdicional>
    <campoAdicional nombre="Email">cliente@email.com</campoAdicional>
  </infoAdicional>
</notaDebito>
```

---

### Comprobante de Retención — `codDoc: 07` — version `1.0.0`

**Required document-specific fields:** `tipoIdentificacionSujetoPasivo`, `razonSocialSujetoPasivo`, `identificacionSujetoPasivo`, `periodoFiscal`. Per-tax line includes `codDocSustento`, `numDocSustento`, `fechaEmisionDocSustento`.

#### Retention Tax Codes (`codigo`)

| `codigo` | Tax type |
|---|---|
| `1` | Retención en la Fuente (Renta) |
| `2` | Retención IVA |
| `6` | ISD (Impuesto a la Salida de Divisas) |

#### Common `codigoRetencion` values for Renta (`codigo=1`)

| Code | Concept | % |
|---|---|---|
| `303` | Honorarios profesionales | 10% |
| `304` | Servicios predomina intelecto | 8% |
| `307` | Servicios predomina mano de obra | 2% |
| `312` | Transferencia de bienes muebles | 1% |
| `320` | Arrendamiento bienes inmuebles | 8% |
| `322` | Seguros y reaseguros | 1% |
| `332` | Otras compras de bienes | 1% |
| `333` | Otras compras de servicios | 2% |

#### `codigoRetencion` values for IVA (`codigo=2`)

| Code | Concept | % |
|---|---|---|
| `725` | Retención IVA 100% | 100% |
| `721` | Retención IVA 30% (servicios) | 30% |
| `723` | Retención IVA 70% (bienes) | 70% |

```xml
<?xml version="1.0" encoding="UTF-8"?>
<comprobanteRetencion id="comprobante" version="1.0.0">
  <infoTributaria>
    <ambiente>1</ambiente>
    <tipoEmision>1</tipoEmision>
    <razonSocial>EMPRESA S.A.</razonSocial>
    <nombreComercial>EMPRESA</nombreComercial>
    <ruc>0990000000001</ruc>
    <claveAcceso>2502202507019990000000001100100100000000142345671</claveAcceso>
    <codDoc>07</codDoc>
    <estab>001</estab>
    <ptoEmi>001</ptoEmi>
    <secuencial>000000001</secuencial>
    <dirMatriz>AV. PRINCIPAL 123</dirMatriz>
  </infoTributaria>
  <infoCompRetencion>
    <fechaEmision>25/02/2026</fechaEmision>
    <dirEstablecimiento>AV. SUCURSAL 456</dirEstablecimiento>
    <!-- Include ONLY when sriConfiguration.SpecialTaxpayerNumber is non-empty: -->
    <contribuyenteEspecial>12345</contribuyenteEspecial>
    <obligadoContabilidad>SI</obligadoContabilidad>
    <tipoIdentificacionSujetoPasivo>04</tipoIdentificacionSujetoPasivo>
    <razonSocialSujetoPasivo>PROVEEDOR S.A.</razonSocialSujetoPasivo>
    <identificacionSujetoPasivo>0990000000003</identificacionSujetoPasivo>
    <periodoFiscal>02/2026</periodoFiscal>  <!-- MM/YYYY format -->
  </infoCompRetencion>
  <impuestos>
    <impuesto>
      <codigo>1</codigo>                          <!-- 1=Renta, 2=IVA, 6=ISD -->
      <codigoRetencion>303</codigoRetencion>       <!-- see catalog above -->
      <baseImponible>1000.00</baseImponible>
      <porcentajeRetener>10.00</porcentajeRetener>
      <valorRetenido>100.00</valorRetenido>
      <codDocSustento>01</codDocSustento>          <!-- codDoc of source invoice -->
      <numDocSustento>001-001-000000001</numDocSustento>
      <fechaEmisionDocSustento>20/02/2026</fechaEmisionDocSustento>
    </impuesto>
  </impuestos>
  <infoAdicional>
    <campoAdicional nombre="Email">proveedor@email.com</campoAdicional>
  </infoAdicional>
</comprobanteRetencion>
```

---

## Digital Signature (XAdES-BES Enveloped)

### Rules
- **Never sign XML outside of `IXmlSignatureService`** — never use raw `SignedXml` in handlers or controllers
- Certificate source: `sriConfiguration.CertificateData` (byte[]) + `sriConfiguration.CertificatePassword` (string) — PKCS#12 format
- The signer appends the `<Signature>` node as a child of the document root
- Reference URI **must** be `"#comprobante"` — this ties the signature to the root element's `id="comprobante"` attribute
- Transform: `XmlDsigEnvelopedSignatureTransform` only
- Key algorithm: RSA with SHA-1 (`SignedXml` default) — matches SRI requirement
- Unsigned file: `{accessKey}.xml` → Signed file: `{accessKey}_signed.xml`
- Load XML with `xmlDoc.PreserveWhitespace = true` — mandatory for signature validity
- Validate signature after signing in the `SignXxxCommand` handler by calling `IXmlSignatureService.ValidateSignatureAsync`

### Handler pattern for signing

```csharp
// 1. Load the certificate from SriConfiguration
var cert = sriConfiguration.CertificateData;
var pwd  = sriConfiguration.CertificatePassword;

// 2. Sign via interface — never new XmlSignatureService() directly
var signedPath = await _xmlSignatureService.SignXmlAsync(xmlFilePath, cert, pwd);

// 3. Validate immediately after signing
var isValid = await _xmlSignatureService.ValidateSignatureAsync(signedPath);
if (!isValid)
    return Result.Failure("XML signature validation failed");

// 4. Update entity
entity.SignedXmlFilePath = signedPath;
entity.Status = DocumentStatus.PendingSriSubmission;
```

---

## SRI SOAP Web Service

### Two operations (same WSDL for all document types)

| Operation | Method | Purpose |
|---|---|---|
| `validarComprobante` | `ISriWebServiceClient.SubmitDocumentAsync(xmlContent)` | Send signed XML |
| `autorizacionComprobante` | `ISriWebServiceClient.CheckAuthorizationAsync(accessKey)` | Poll for authorization |

### Submission — `SriSubmissionResponse`

```csharp
public class SriSubmissionResponse
{
    public bool IsSuccess { get; set; }    // true when SRI estado = "RECIBIDA"
    public string Message { get; set; }
    public List<SriError> Errors { get; set; } = new();
}
```

SRI returns `<estado>RECIBIDA</estado>` (success) or `<estado>DEVUELTA</estado>` (rejected). Parse `<comprobante>` / `<mensajes>` for error details.

### Authorization — `SriAuthorizationResponse`

```csharp
public class SriAuthorizationResponse
{
    public bool IsAuthorized { get; set; }        // true when estado = "AUTORIZADO"
    public string Status { get; set; }             // "AUTORIZADO" | "NO AUTORIZADO"
    public string? AuthorizationNumber { get; set; }
    public DateTime? AuthorizationDate { get; set; }
    public List<SriError> Errors { get; set; } = new();
}
```

SRI returns `<numeroAutorizacion>`, `<fechaAutorizacion>`, `<estado>` inside `<autorizacion>`.

### Submission handler pattern

```csharp
// Read signed XML content from file
var xmlContent = await File.ReadAllTextAsync(entity.SignedXmlFilePath);

// Submit through interface — never instantiate SriSoapClient directly in handlers
var response = await _sriWebServiceClient.SubmitDocumentAsync(xmlContent);

if (!response.IsSuccess)
{
    await LogSriErrorAsync(entity.Id, "Submit", response);
    return Result.Failure(response.Message);
}

entity.Status = DocumentStatus.PendingAuthorization;
```

### Authorization handler pattern

```csharp
var response = await _sriWebServiceClient.CheckAuthorizationAsync(entity.AccessKey);

if (response.IsAuthorized)
{
    entity.AuthorizationNumber = response.AuthorizationNumber;
    entity.AuthorizationDate   = response.AuthorizationDate;
    entity.Status              = DocumentStatus.Authorized;
}
else if (response.Status == "NO AUTORIZADO")
{
    await LogSriErrorAsync(entity.Id, "CheckAuthorization", response);
    entity.Status = DocumentStatus.Rejected;
}
// If empty response / timeout: leave as PendingAuthorization for background job retry
```

---

## Document Status Machine

All four document types share this status lifecycle:

```
Draft
  └─ GenerateXxxXmlCommand          → PendingSignature
       └─ SignXxxCommand             → PendingSriSubmission
            └─ SubmitXxxToSriCommand → PendingAuthorization
                 ├─ CheckAuthorizationStatusCommand (AUTORIZADO)  → Authorized
                 └─ CheckAuthorizationStatusCommand (NO AUTORIZADO) → Rejected
```

| Status | description |
|---|---|
| `Draft` | Document created, not yet processed |
| `PendingSignature` | XML generated, awaiting digital signature |
| `PendingSriSubmission` | XML signed, awaiting SOAP submission |
| `PendingAuthorization` | Submitted, SRI acknowledged (`RECIBIDA`), awaiting authorization |
| `Authorized` | SRI authorized — has `AuthorizationNumber` + `AuthorizationDate` |
| `Rejected` | SRI rejected — see `SriErrorLog` for reason |

**Transition guards** (enforce in command handlers):
- `GenerateXml`: only from `Draft`
- `Sign`: only from `PendingSignature`
- `Submit`: only from `PendingSriSubmission`
- `CheckAuthorization`: only from `PendingAuthorization`

---

## CQRS Command Sequence — Per Document Type

For each new document type (e.g., `CreditNote`), create these 5 commands under `Application/Features/CreditNotes/Commands/`:

```
GenerateCreditNoteXml/
  GenerateCreditNoteXmlCommand.cs
  GenerateCreditNoteXmlCommandHandler.cs
  GenerateCreditNoteXmlCommandValidator.cs

SignCreditNote/
  SignCreditNoteCommand.cs
  SignCreditNoteCommandHandler.cs

SubmitCreditNoteToSri/
  SubmitCreditNoteToSriCommand.cs
  SubmitCreditNoteToSriCommandHandler.cs

CheckCreditNoteAuthorizationStatus/
  CheckCreditNoteAuthorizationStatusCommand.cs
  CheckCreditNoteAuthorizationStatusCommandHandler.cs

GenerateCreditNoteRide/
  GenerateCreditNoteRideCommand.cs
  GenerateCreditNoteRideCommandHandler.cs
```

Add corresponding API endpoints in the controller:
```
POST   /api/v1/credit-notes/{id}/generate-xml
POST   /api/v1/credit-notes/{id}/sign
POST   /api/v1/credit-notes/{id}/submit-sri
POST   /api/v1/credit-notes/{id}/check-authorization
POST   /api/v1/credit-notes/{id}/generate-ride
GET    /api/v1/credit-notes/{id}/download-xml
GET    /api/v1/credit-notes/{id}/download-ride
```

---

## Validation Rules

### Business validations (FluentValidation — run as pipeline behavior)

| Field | Rule |
|---|---|
| RUC | Exactly 13 digits |
| Establishment code | Exactly 3 digits, `001`–`999` |
| Emission point code | Exactly 3 digits, `001`–`999` |
| Sequential | 1–999,999,999 |
| Numeric code | 8 random digits (auto-generated) |
| `fechaEmision` | Not in the future; not older than 30 days for submission |
| `totalSinImpuestos` | ≥ 0 |
| `importeTotal` | = `totalSinImpuestos` + sum of `valor` taxes |
| `identificacionComprador` | 13 digits for RUC; 10 for cédula; `9999999999999` for ConsumerFinal |
| Credit note `codDocModificado` | Must match `codDoc` of an existing authorized document |
| Debit note `codDocModificado` | Same as credit note |
| Retention `periodoFiscal` | `MM/YYYY` format |

### Document-type-specific mandatory fields

| Document | Extra required fields |
|---|---|
| `notaCredito` | `codDocModificado`, `numDocModificado`, `fechaEmisionDocSustento`, `motivo`, `valorModificacion` |
| `notaDebito` | `codDocModificado`, `numDocModificado`, `fechaEmisionDocSustento`, at least one `<motivo>` in `<motivos>` |
| `comprobanteRetencion` | `periodoFiscal`, at least one `<impuesto>` in `<impuestos>`, `codDocSustento` + `numDocSustento` per tax line |

---

## Error Logging

Every SRI operation failure **must** be recorded in `SriErrorLog`:

```csharp
var errorLog = new SriErrorLog
{
    TenantId     = entity.TenantId,
    InvoiceId    = entity.Id,        // nullable FK — use DocumentId generically
    Operation    = "GenerateXml" | "Sign" | "Submit" | "CheckAuthorization" | "GenerateRide",
    ErrorCode    = "XML_GENERATION_ERROR" | "SIGNATURE_ERROR" | "SOAP_ERROR" | "AUTHORIZATION_ERROR",
    Message      = ex.Message,
    RawResponse  = sriResponse?.RawXml,   // capture full SOAP response when available
    OccurredAt   = DateTime.UtcNow,
    AttemptCount = 1
};
await _sriErrorLogRepository.AddAsync(errorLog);
```

- Use `Result<T>` — **never re-throw** SRI errors as exceptions (they are business errors)
- Log at `_logger.LogError(ex, "SRI {Operation} failed for {DocumentId}")` via Serilog as well
- Expose errors via `GET /api/v1/{module}/{id}/sri-errors` → `GetSriErrorLogsForXxxQuery`

---

## Background Jobs

Two Hangfire recurring jobs poll all tenants for pending documents.
When adding a new document type, **extend the existing job queries** to include the new entity — do not create separate jobs.

```csharp
// CheckPendingAuthorizationsJob — runs every 30 seconds
// Add new document type query alongside invoices:
var pendingCreditNotes = await _dbContext.CreditNotes
    .IgnoreQueryFilters()   // bypass tenant filter — job runs for ALL tenants
    .Where(x => x.Status == DocumentStatus.PendingAuthorization && !x.IsDeleted)
    .ToListAsync();

// GenerateRideForAuthorizedDocumentsJob — runs every 60 seconds
var authorizedCreditNotes = await _dbContext.CreditNotes
    .IgnoreQueryFilters()
    .Where(x => x.Status == DocumentStatus.Authorized
             && x.RideFilePath == null
             && !x.IsDeleted)
    .ToListAsync();
```

---

## RIDE PDF Generation

### Rules
- One `IRideGenerationService` per document type: `IInvoiceRideService`, `ICreditNoteRideService`, etc.
- Use **QuestPDF** — never iTextSharp or other PDF libraries
- Storage path: `{storagePath}/{tenantId}/{module}/{year}/{month:00}/{accessKey}_ride.pdf`
- File naming: `{accessKey}_ride.pdf`

### Mandatory RIDE sections (all document types)

| Section | Content |
|---|---|
| Header | Company logo (if available), legal name, trade name, RUC, main address, document type label, document number (`EEE-SSS-NNNNNNNNN`) |
| Authorization box | Authorization number, authorization date, access key (displayed in mono font), environment label |
| Document body | Buyer/supplier info, document date, document-specific fields (e.g., `motivo` for credit note) |
| Items table | For factura/notaCredito: code, description, quantity, unit price, discount, subtotal, tax |
| Totals | Subtotal sin IVA, IVA breakdown, total |
| QR code | Format: `{AccessKey}|{IssueDate:dd/MM/yyyy}|{RUC}|{Environment}|{AuthorizationNumber}` |
| Footer | "Documento autorizado por el SRI" + environment notice for Test documents |

### Test environment watermark
When `SriEnvironment == Test`, add a `"AMBIENTE DE PRUEBAS — NO VÁLIDO TRIBUTARIAMENTE"` watermark across the RIDE.

---

## Frontend Workflow Pattern

Each document detail page must include an SRI workflow card following the pattern in `pages/billing/invoices/[id]/index.vue`.

### Composable additions

Add 7 methods to the domain composable (e.g., `useCreditNote`):

```typescript
const generateXml       = (id: string) => sriAction(`/credit-notes/${id}/generate-xml`)
const sign              = (id: string) => sriAction(`/credit-notes/${id}/sign`)
const submitToSri       = (id: string) => sriAction(`/credit-notes/${id}/submit-sri`)
const checkAuthorization= (id: string) => sriAction(`/credit-notes/${id}/check-authorization`)
const generateRide      = (id: string) => sriAction(`/credit-notes/${id}/generate-ride`)
const downloadXml       = (id: string) => download(`/credit-notes/${id}/download-xml`, `${id}.xml`)
const downloadRide      = (id: string) => download(`/credit-notes/${id}/download-ride`, `${id}_ride.pdf`)
```

### Visibility flags (computed)

```typescript
const canGenerateXml        = computed(() => doc.value?.status === 'Draft')
const canSign               = computed(() => doc.value?.status === 'PendingSignature')
const canSubmitToSri        = computed(() => doc.value?.status === 'PendingSriSubmission')
const canCheckAuthorization = computed(() => doc.value?.status === 'PendingAuthorization')
const canGenerateRide       = computed(() => doc.value?.status === 'Authorized' && !doc.value?.rideFilePath)
const canDownloadXml        = computed(() => !!doc.value?.xmlFilePath)
const canDownloadRide       = computed(() => !!doc.value?.rideFilePath)
```

### i18n keys

All SRI labels under `{module}.sri.*` namespace. Required keys (extend for new document types):

```json
{
  "creditNotes": {
    "sri": {
      "title":              "SRI Electronic Document",
      "generateXml":        "1. Generate XML",
      "sign":               "2. Sign Document",
      "submitToSri":        "3. Submit to SRI",
      "checkAuthorization": "4. Check Authorization",
      "generateRide":       "5. Generate RIDE",
      "downloadXml":        "Download XML",
      "downloadRide":       "Download RIDE",
      "authorized":         "Authorized by SRI",
      "rejected":           "Rejected by SRI",
      "pending":            "Pending Authorization",
      "authorizationNumber":"Authorization Number",
      "authorizationDate":  "Authorization Date",
      "testEnvironment":    "⚠️ Test Environment — Not Tax Valid"
    }
  }
}
```

---

## SRI Configuration Entity (`SriConfiguration`)

```
TenantId           Guid    — one per tenant (upsert pattern — not insert+insert)
CompanyRuc         string  — 13 digits, required
LegalName          string  — razonSocial
TradeName          string? — nombreComercial
MainAddress        string  — dirMatriz
Environment        SriEnvironment
AccountingRequired bool    — obligadoContabilidad
SpecialTaxpayerNumber string? — contribuyenteEspecial (omit if null/empty)
CertificateData    byte[]? — PKCS#12 (.p12/.pfx) — stored encrypted at rest
CertificatePassword string? — decrypted only in signing path
```

### Upsert command pattern

```csharp
// Never insert a second SriConfiguration row for the same tenant
var existing = await _repo.GetByTenantIdAsync(tenantId);
if (existing == null)
    await _repo.AddAsync(new SriConfiguration { TenantId = tenantId, ... });
else
{
    existing.LegalName = command.LegalName;
    // ... update fields
    await _repo.UpdateAsync(existing);
}
```

---

## Establishment & EmissionPoint

- **Establishment**: 3-digit code `001`–`999`, unique per `TenantId`. Validates with `IsValidCode()`.
- **EmissionPoint**: belongs to an `Establishment`; 3-digit code `001`–`999`; has 4 independent sequences:
  - `InvoiceSequence` — for `Factura`
  - `CreditNoteSequence` — for `Nota de Crédito`
  - `DebitNoteSequence` — for `Nota de Débito`
  - `RetentionSequence` — for `Comprobante de Retención`

Always increment the **correct** sequence in the `GenerateXxxXmlCommandHandler`, then pass it to `AccessKey.Generate(...)`.

---

## Reference Implementations

| Layer | Reference file |
|---|---|
| XML generation | [`backend/src/Infrastructure/Services/InvoiceXmlService.cs`](../../backend/src/Infrastructure/Services/InvoiceXmlService.cs) |
| Digital signing | [`backend/src/Infrastructure/Services/XmlSignatureService.cs`](../../backend/src/Infrastructure/Services/XmlSignatureService.cs) |
| SOAP client | [`backend/src/Infrastructure/Services/SriSoapClient.cs`](../../backend/src/Infrastructure/Services/SriSoapClient.cs) |
| RIDE PDF | [`backend/src/Infrastructure/Services/RideGenerationService.cs`](../../backend/src/Infrastructure/Services/RideGenerationService.cs) |
| Access key | [`backend/src/Domain/ValueObjects/AccessKey.cs`](../../backend/src/Domain/ValueObjects/AccessKey.cs) |
| CQRS commands | [`backend/src/Application/Features/Invoices/Commands/`](../../backend/src/Application/Features/Invoices/Commands/) |
| Frontend workflow UI | [`frontend/pages/billing/invoices/[id]/index.vue`](../../frontend/pages/billing/invoices/%5Bid%5D/index.vue) |
| Frontend composable | [`frontend/composables/useInvoice.ts`](../../frontend/composables/useInvoice.ts) |
| i18n keys | [`frontend/i18n/locales/en.json`](../../frontend/i18n/locales/en.json) — namespace `invoices.sri` |
