# Database Schema - Billing & Inventory System (Ecuador)

## Overview
Multi-tenant billing and inventory management system with full SRI (Servicio de Rentas Internas) compliance for Ecuador.

**Architecture**: Shared database with `company_id` for multi-tenancy  
**Database**: PostgreSQL  
**Key Features**: SRI electronic invoicing, multi-warehouse inventory, RBAC, comprehensive audit trails

---

## Multi-Tenancy & Authentication

### companies
Represents each tenant/business using the system.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Unique identifier |
| ruc | VARCHAR(13) | NOT NULL, UNIQUE | Ecuador tax identification |
| business_name | VARCHAR(255) | NOT NULL | Legal business name (razón social) |
| trade_name | VARCHAR(255) | | Commercial name (nombre comercial) |
| address | TEXT | | Physical address |
| phone | VARCHAR(20) | | Contact phone |
| email | VARCHAR(255) | | Contact email |
| accounting_required | BOOLEAN | DEFAULT false | Obligado a llevar contabilidad |
| special_taxpayer_number | VARCHAR(20) | | Contribuyente especial number |
| sri_environment | INTEGER | DEFAULT 1 | 1=pruebas, 2=producción |
| digital_certificate_path | TEXT | | Path to firma electrónica file |
| certificate_password | TEXT | | Encrypted certificate password |
| status | VARCHAR(20) | DEFAULT 'active' | active, suspended, inactive |
| created_at | TIMESTAMP | DEFAULT NOW() | |
| updated_at | TIMESTAMP | DEFAULT NOW() | |

**Indexes:**
- `idx_companies_ruc` ON (ruc)
- `idx_companies_status` ON (status)

---

### users
System users who can belong to multiple companies.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Unique identifier |
| email | VARCHAR(255) | NOT NULL, UNIQUE | Login email |
| password_hash | VARCHAR(255) | NOT NULL | Bcrypt hashed password |
| full_name | VARCHAR(255) | NOT NULL | User's full name |
| phone | VARCHAR(20) | | Contact phone |
| status | VARCHAR(20) | DEFAULT 'active' | active, inactive, locked |
| last_login_at | TIMESTAMP | | Last successful login |
| created_at | TIMESTAMP | DEFAULT NOW() | |
| updated_at | TIMESTAMP | DEFAULT NOW() | |

**Indexes:**
- `idx_users_email` ON (email)
- `idx_users_status` ON (status)

---

### roles
Predefined permission sets.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Unique identifier |
| name | VARCHAR(50) | NOT NULL, UNIQUE | Role name (Administrador, Contador, etc.) |
| description | TEXT | | Role description |
| permissions | JSONB | DEFAULT '[]' | Array of permission strings |
| is_system_role | BOOLEAN | DEFAULT false | Cannot be modified/deleted |
| created_at | TIMESTAMP | DEFAULT NOW() | |
| updated_at | TIMESTAMP | DEFAULT NOW() | |

**Example Roles:**
- `Administrador`: Full access to everything
- `Contador`: Financial reports, tax documents
- `Cajero`: Create invoices, process payments
- `Bodeguero`: Manage inventory, transfers
- `Auditor`: Read-only access

**Indexes:**
- `idx_roles_name` ON (name)

---

### user_company_roles
Links users to companies with specific roles (many-to-many-to-many).

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Unique identifier |
| user_id | UUID | FK users(id) | User reference |
| company_id | UUID | FK companies(id) | Company reference |
| role_id | UUID | FK roles(id) | Role reference |
| assigned_at | TIMESTAMP | DEFAULT NOW() | When role was assigned |
| assigned_by | UUID | FK users(id) | Who assigned the role |

**Indexes:**
- `idx_ucr_user_company` ON (user_id, company_id)
- `idx_ucr_company` ON (company_id)
- UNIQUE constraint ON (user_id, company_id, role_id)

---

## Customers & Suppliers

### contacts
Unified table for both customers and suppliers.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Unique identifier |
| company_id | UUID | FK companies(id), NOT NULL | Owner company |
| type | VARCHAR(20) | NOT NULL | customer, supplier, both |
| identification_type | VARCHAR(2) | NOT NULL | 04=RUC, 05=Cedula, 06=Pasaporte, 07=Consumidor Final |
| identification | VARCHAR(20) | NOT NULL | RUC, cedula, passport number |
| business_name | VARCHAR(255) | NOT NULL | Legal name (razón social) |
| trade_name | VARCHAR(255) | | Commercial name |
| email | VARCHAR(255) | | Contact email |
| phone | VARCHAR(20) | | Contact phone |
| address | TEXT | | Physical address |
| retention_agent | BOOLEAN | DEFAULT false | Is agente de retención |
| special_taxpayer | BOOLEAN | DEFAULT false | Is contribuyente especial |
| credit_limit | DECIMAL(10,2) | DEFAULT 0 | Maximum credit allowed |
| payment_terms_days | INTEGER | DEFAULT 0 | Payment terms in days |
| notes | TEXT | | Additional notes |
| created_at | TIMESTAMP | DEFAULT NOW() | |
| updated_at | TIMESTAMP | DEFAULT NOW() | |
| deleted_at | TIMESTAMP | | Soft delete timestamp |

**Indexes:**
- `idx_contacts_company_id` ON (company_id)
- `idx_contacts_identification` ON (company_id, identification)
- `idx_contacts_type` ON (company_id, type)
- `idx_contacts_deleted_at` ON (deleted_at)

---

## Products & Inventory

### product_categories
Hierarchical product categorization.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Unique identifier |
| company_id | UUID | FK companies(id), NOT NULL | Owner company |
| name | VARCHAR(255) | NOT NULL | Category name |
| parent_id | UUID | FK product_categories(id) | Parent category for hierarchy |
| description | TEXT | | Category description |
| created_at | TIMESTAMP | DEFAULT NOW() | |
| updated_at | TIMESTAMP | DEFAULT NOW() | |

**Indexes:**
- `idx_categories_company_id` ON (company_id)
- `idx_categories_parent_id` ON (parent_id)

---

### products
Product catalog.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Unique identifier |
| company_id | UUID | FK companies(id), NOT NULL | Owner company |
| sku | VARCHAR(50) | | Internal SKU code |
| barcode | VARCHAR(50) | | Product barcode |
| name | VARCHAR(255) | NOT NULL | Product name |
| description | TEXT | | Product description |
| category_id | UUID | FK product_categories(id) | Category reference |
| unit_of_measure | VARCHAR(20) | DEFAULT 'UND' | UND, KG, LT, etc. |
| cost_method | VARCHAR(20) | DEFAULT 'weighted_average' | FIFO, weighted_average |
| track_inventory | BOOLEAN | DEFAULT true | Track stock levels |
| track_serial_numbers | BOOLEAN | DEFAULT false | Track by serial number |
| track_batches | BOOLEAN | DEFAULT false | Track by batch/lot |
| default_tax_type_id | UUID | FK tax_types(id) | Default tax for this product |
| ice_applicable | BOOLEAN | DEFAULT false | Subject to ICE (special consumption tax) |
| ice_rate | DECIMAL(5,2) | | ICE rate if applicable |
| status | VARCHAR(20) | DEFAULT 'active' | active, inactive |
| created_at | TIMESTAMP | DEFAULT NOW() | |
| updated_at | TIMESTAMP | DEFAULT NOW() | |
| deleted_at | TIMESTAMP | | Soft delete timestamp |

**Indexes:**
- `idx_products_company_id` ON (company_id)
- `idx_products_sku` ON (company_id, sku)
- `idx_products_barcode` ON (barcode)
- `idx_products_category_id` ON (category_id)
- `idx_products_deleted_at` ON (deleted_at)

---

### warehouses
Physical storage locations.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Unique identifier |
| company_id | UUID | FK companies(id), NOT NULL | Owner company |
| code | VARCHAR(10) | NOT NULL | Warehouse code |
| name | VARCHAR(255) | NOT NULL | Warehouse name |
| address | TEXT | | Physical address |
| is_main | BOOLEAN | DEFAULT false | Is main warehouse |
| status | VARCHAR(20) | DEFAULT 'active' | active, inactive |
| created_at | TIMESTAMP | DEFAULT NOW() | |
| updated_at | TIMESTAMP | DEFAULT NOW() | |

**Indexes:**
- `idx_warehouses_company_id` ON (company_id)
- UNIQUE constraint ON (company_id, code)

---

### inventory
Current stock levels per product and warehouse.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Unique identifier |
| company_id | UUID | FK companies(id), NOT NULL | Owner company |
| product_id | UUID | FK products(id), NOT NULL | Product reference |
| warehouse_id | UUID | FK warehouses(id), NOT NULL | Warehouse reference |
| quantity | DECIMAL(10,2) | DEFAULT 0 | Total quantity |
| reserved_quantity | DECIMAL(10,2) | DEFAULT 0 | Quantity reserved (pending orders) |
| available_quantity | DECIMAL(10,2) | GENERATED | quantity - reserved_quantity |
| min_stock | DECIMAL(10,2) | DEFAULT 0 | Minimum stock alert level |
| max_stock | DECIMAL(10,2) | | Maximum stock level |
| last_cost | DECIMAL(10,2) | | Last purchase cost |
| average_cost | DECIMAL(10,2) | | Weighted average cost |
| updated_at | TIMESTAMP | DEFAULT NOW() | |

**Indexes:**
- `idx_inventory_company_product_warehouse` ON (company_id, product_id, warehouse_id)
- UNIQUE constraint ON (company_id, product_id, warehouse_id)

---

### inventory_transactions
Historical record of all inventory movements.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Unique identifier |
| company_id | UUID | FK companies(id), NOT NULL | Owner company |
| product_id | UUID | FK products(id), NOT NULL | Product reference |
| warehouse_id | UUID | FK warehouses(id), NOT NULL | Warehouse reference |
| transaction_type | VARCHAR(20) | NOT NULL | purchase, sale, adjustment, transfer_in, transfer_out |
| reference_type | VARCHAR(50) | | Type of source document |
| reference_id | UUID | | Source document ID |
| quantity | DECIMAL(10,2) | NOT NULL | Quantity changed (+ or -) |
| unit_cost | DECIMAL(10,2) | | Cost per unit |
| balance_after | DECIMAL(10,2) | NOT NULL | Stock balance after transaction |
| notes | TEXT | | Transaction notes |
| created_by | UUID | FK users(id) | User who created transaction |
| created_at | TIMESTAMP | DEFAULT NOW() | |

**Indexes:**
- `idx_inv_trans_company_product` ON (company_id, product_id, created_at DESC)
- `idx_inv_trans_reference` ON (reference_type, reference_id)

---

## Tax Configuration (Ecuador)

### tax_types
Tax types according to SRI regulations.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Unique identifier |
| code | VARCHAR(10) | NOT NULL, UNIQUE | Tax code (IVA_12, IVA_0, etc.) |
| name | VARCHAR(100) | NOT NULL | Tax name |
| rate | DECIMAL(5,2) | NOT NULL | Tax rate percentage |
| sri_code | VARCHAR(10) | NOT NULL | SRI official code |
| active | BOOLEAN | DEFAULT true | Is active |
| created_at | TIMESTAMP | DEFAULT NOW() | |
| updated_at | TIMESTAMP | DEFAULT NOW() | |

**Default Values:**
- IVA 12%: code='IVA_12', rate=12.00, sri_code='2'
- IVA 0%: code='IVA_0', rate=0.00, sri_code='0'
- No IVA: code='NO_IVA', rate=0.00, sri_code='6'
- IVA Exento: code='IVA_EXEMPT', rate=0.00, sri_code='7'

**Indexes:**
- `idx_tax_types_code` ON (code)
- `idx_tax_types_active` ON (active)

---

### retention_types
Withholding tax types.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Unique identifier |
| type | VARCHAR(10) | NOT NULL | IR (income tax), IVA |
| code | VARCHAR(10) | NOT NULL | Retention code |
| description | VARCHAR(255) | NOT NULL | Description |
| rate | DECIMAL(5,2) | NOT NULL | Retention rate |
| sri_code | VARCHAR(10) | NOT NULL | SRI official code |
| active | BOOLEAN | DEFAULT true | Is active |
| created_at | TIMESTAMP | DEFAULT NOW() | |
| updated_at | TIMESTAMP | DEFAULT NOW() | |

**Indexes:**
- `idx_retention_types_type` ON (type)
- `idx_retention_types_active` ON (active)

---

### payment_methods
Payment methods according to SRI.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Unique identifier |
| name | VARCHAR(100) | NOT NULL | Payment method name |
| sri_code | VARCHAR(2) | NOT NULL, UNIQUE | SRI code (01, 19, 20, etc.) |
| active | BOOLEAN | DEFAULT true | Is active |
| created_at | TIMESTAMP | DEFAULT NOW() | |

**Default Values:**
- Efectivo: sri_code='01'
- Tarjeta de crédito: sri_code='19'
- Otros con utilización del sistema financiero: sri_code='20'

**Indexes:**
- `idx_payment_methods_sri_code` ON (sri_code)

---

## Invoicing (Core)

### document_sequences
Sequential numbering for SRI documents (no gaps allowed).

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Unique identifier |
| company_id | UUID | FK companies(id), NOT NULL | Owner company |
| document_type | VARCHAR(2) | NOT NULL | 01=factura, 04=nota_credito, 05=nota_debito, 06=guia_remision, 07=retencion |
| establishment_code | VARCHAR(3) | NOT NULL | Establishment code (001) |
| emission_point | VARCHAR(3) | NOT NULL | Emission point (001) |
| current_number | BIGINT | DEFAULT 0 | Last used sequential number |
| last_used_at | TIMESTAMP | | When last number was used |
| locked_until | TIMESTAMP | | Lock timeout for allocated numbers |
| created_at | TIMESTAMP | DEFAULT NOW() | |
| updated_at | TIMESTAMP | DEFAULT NOW() | |

**Indexes:**
- UNIQUE constraint ON (company_id, document_type, establishment_code, emission_point)
- `idx_sequences_company` ON (company_id)

**Usage Pattern:**
```sql
-- Allocate next number with row lock
SELECT current_number + 1 
FROM document_sequences 
WHERE company_id = ? AND document_type = ? 
  AND establishment_code = ? AND emission_point = ?
FOR UPDATE;
```

---

### invoices
Main invoice/factura table.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Unique identifier |
| company_id | UUID | FK companies(id), NOT NULL | Owner company |
| customer_id | UUID | FK contacts(id), NOT NULL | Customer reference |
| document_type | VARCHAR(2) | DEFAULT '01' | 01=factura |
| establishment | VARCHAR(3) | NOT NULL | Establishment code |
| emission_point | VARCHAR(3) | NOT NULL | Emission point |
| sequential | VARCHAR(9) | NOT NULL | Sequential number (000000001) |
| full_number | VARCHAR(17) | NOT NULL | Complete number (001-001-000000001) |
| issue_date | DATE | NOT NULL | Invoice date |
| due_date | DATE | | Payment due date |
| subtotal_0 | DECIMAL(10,2) | DEFAULT 0 | Subtotal with IVA 0% |
| subtotal_12 | DECIMAL(10,2) | DEFAULT 0 | Subtotal with IVA 12% |
| subtotal_exempt | DECIMAL(10,2) | DEFAULT 0 | Exempt subtotal |
| iva_12 | DECIMAL(10,2) | DEFAULT 0 | IVA 12% amount |
| ice | DECIMAL(10,2) | DEFAULT 0 | ICE tax amount |
| total_discount | DECIMAL(10,2) | DEFAULT 0 | Total discounts |
| tip | DECIMAL(10,2) | DEFAULT 0 | Tip amount |
| total | DECIMAL(10,2) | NOT NULL | Total amount |
| sri_access_key | VARCHAR(49) | UNIQUE | 48-digit clave de acceso |
| sri_authorization_number | VARCHAR(50) | | Authorization number from SRI |
| sri_authorization_date | TIMESTAMP | | When SRI authorized |
| sri_environment | INTEGER | NOT NULL | 1=pruebas, 2=producción |
| xml_path | TEXT | | Path to stored XML |
| ride_pdf_path | TEXT | | Path to RIDE PDF |
| status | VARCHAR(20) | DEFAULT 'draft' | draft, authorized, void, rejected, pending |
| notes | TEXT | | Additional notes |
| created_by | UUID | FK users(id) | User who created |
| created_at | TIMESTAMP | DEFAULT NOW() | |
| updated_at | TIMESTAMP | DEFAULT NOW() | |
| deleted_at | TIMESTAMP | | Soft delete (void) |

**Indexes:**
- `idx_invoices_company_id` ON (company_id)
- `idx_invoices_customer_id` ON (customer_id)
- `idx_invoices_full_number` ON (company_id, full_number)
- `idx_invoices_sri_access_key` ON (sri_access_key)
- `idx_invoices_issue_date` ON (company_id, issue_date DESC)
- `idx_invoices_status` ON (company_id, status)

---

### invoice_lines
Line items for invoices.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Unique identifier |
| invoice_id | UUID | FK invoices(id), NOT NULL | Invoice reference |
| line_number | INTEGER | NOT NULL | Line sequence |
| product_id | UUID | FK products(id) | Product reference (nullable for custom items) |
| description | VARCHAR(255) | NOT NULL | Line item description |
| quantity | DECIMAL(10,2) | NOT NULL | Quantity |
| unit_price | DECIMAL(10,2) | NOT NULL | Price per unit |
| discount_percentage | DECIMAL(5,2) | DEFAULT 0 | Discount % |
| discount_amount | DECIMAL(10,2) | DEFAULT 0 | Discount amount |
| subtotal | DECIMAL(10,2) | NOT NULL | Line subtotal before tax |
| created_at | TIMESTAMP | DEFAULT NOW() | |

**Indexes:**
- `idx_invoice_lines_invoice_id` ON (invoice_id)
- `idx_invoice_lines_product_id` ON (product_id)

---

### invoice_line_taxes
Taxes applied to each invoice line.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Unique identifier |
| invoice_line_id | UUID | FK invoice_lines(id), NOT NULL | Line reference |
| tax_type_id | UUID | FK tax_types(id), NOT NULL | Tax type reference |
| taxable_base | DECIMAL(10,2) | NOT NULL | Amount subject to tax |
| rate | DECIMAL(5,2) | NOT NULL | Tax rate (snapshot) |
| amount | DECIMAL(10,2) | NOT NULL | Calculated tax amount |

**Indexes:**
- `idx_invoice_line_taxes_line_id` ON (invoice_line_id)

---

### invoice_payments
Payment records for invoices.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Unique identifier |
| invoice_id | UUID | FK invoices(id), NOT NULL | Invoice reference |
| payment_method_id | UUID | FK payment_methods(id), NOT NULL | Payment method |
| amount | DECIMAL(10,2) | NOT NULL | Payment amount |
| reference | VARCHAR(100) | | Payment reference/transaction ID |
| payment_date | DATE | NOT NULL | Payment date |
| notes | TEXT | | Payment notes |
| created_by | UUID | FK users(id) | User who recorded payment |
| created_at | TIMESTAMP | DEFAULT NOW() | |

**Indexes:**
- `idx_invoice_payments_invoice_id` ON (invoice_id)
- `idx_invoice_payments_payment_date` ON (payment_date)

---

## Other SRI Documents

### credit_notes
Credit notes (notas de crédito).

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Unique identifier |
| company_id | UUID | FK companies(id), NOT NULL | Owner company |
| customer_id | UUID | FK contacts(id), NOT NULL | Customer reference |
| original_invoice_id | UUID | FK invoices(id), NOT NULL | Original invoice being credited |
| establishment | VARCHAR(3) | NOT NULL | Establishment code |
| emission_point | VARCHAR(3) | NOT NULL | Emission point |
| sequential | VARCHAR(9) | NOT NULL | Sequential number |
| full_number | VARCHAR(17) | NOT NULL | Complete number |
| issue_date | DATE | NOT NULL | Credit note date |
| reason | TEXT | NOT NULL | Reason for credit note |
| modification_type | VARCHAR(2) | | SRI modification type code |
| total | DECIMAL(10,2) | NOT NULL | Total credit amount |
| sri_access_key | VARCHAR(49) | UNIQUE | Clave de acceso |
| sri_authorization_number | VARCHAR(50) | | Authorization number |
| sri_authorization_date | TIMESTAMP | | Authorization date |
| xml_path | TEXT | | XML file path |
| ride_pdf_path | TEXT | | RIDE PDF path |
| status | VARCHAR(20) | DEFAULT 'draft' | draft, authorized, rejected |
| created_by | UUID | FK users(id) | |
| created_at | TIMESTAMP | DEFAULT NOW() | |
| updated_at | TIMESTAMP | DEFAULT NOW() | |

**Indexes:**
- `idx_credit_notes_company_id` ON (company_id)
- `idx_credit_notes_original_invoice` ON (original_invoice_id)
- `idx_credit_notes_full_number` ON (company_id, full_number)

---

### debit_notes
Debit notes (notas de débito).

Same structure as `credit_notes` above.

---

### withholdings
Withholding receipts (comprobantes de retención).

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Unique identifier |
| company_id | UUID | FK companies(id), NOT NULL | Owner company (retention agent) |
| supplier_id | UUID | FK contacts(id), NOT NULL | Supplier being withheld from |
| invoice_reference | VARCHAR(50) | | Reference to supplier's invoice |
| establishment | VARCHAR(3) | NOT NULL | Establishment code |
| emission_point | VARCHAR(3) | NOT NULL | Emission point |
| sequential | VARCHAR(9) | NOT NULL | Sequential number |
| full_number | VARCHAR(17) | NOT NULL | Complete number |
| issue_date | DATE | NOT NULL | Withholding date |
| fiscal_period | VARCHAR(7) | NOT NULL | Fiscal period (MM/YYYY) |
| total_withheld | DECIMAL(10,2) | NOT NULL | Total amount withheld |
| sri_access_key | VARCHAR(49) | UNIQUE | Clave de acceso |
| sri_authorization_number | VARCHAR(50) | | Authorization number |
| sri_authorization_date | TIMESTAMP | | Authorization date |
| xml_path | TEXT | | XML file path |
| ride_pdf_path | TEXT | | RIDE PDF path |
| status | VARCHAR(20) | DEFAULT 'draft' | draft, authorized, rejected |
| created_by | UUID | FK users(id) | |
| created_at | TIMESTAMP | DEFAULT NOW() | |
| updated_at | TIMESTAMP | DEFAULT NOW() | |

**Indexes:**
- `idx_withholdings_company_id` ON (company_id)
- `idx_withholdings_supplier_id` ON (supplier_id)
- `idx_withholdings_full_number` ON (company_id, full_number)

---

### withholding_lines
Individual retention items.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Unique identifier |
| withholding_id | UUID | FK withholdings(id), NOT NULL | Withholding reference |
| retention_type_id | UUID | FK retention_types(id), NOT NULL | Retention type |
| taxable_base | DECIMAL(10,2) | NOT NULL | Base amount |
| rate | DECIMAL(5,2) | NOT NULL | Retention rate |
| amount | DECIMAL(10,2) | NOT NULL | Withheld amount |

**Indexes:**
- `idx_withholding_lines_withholding_id` ON (withholding_id)

---

### delivery_notes
Delivery notes (guías de remisión).

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Unique identifier |
| company_id | UUID | FK companies(id), NOT NULL | Owner company |
| establishment | VARCHAR(3) | NOT NULL | Establishment code |
| emission_point | VARCHAR(3) | NOT NULL | Emission point |
| sequential | VARCHAR(9) | NOT NULL | Sequential number |
| full_number | VARCHAR(17) | NOT NULL | Complete number |
| issue_date | DATE | NOT NULL | Issue date |
| transfer_reason | TEXT | NOT NULL | Reason for transfer |
| origin_address | TEXT | NOT NULL | Origin address |
| destination_address | TEXT | NOT NULL | Destination address |
| carrier_identification | VARCHAR(20) | | Carrier ID |
| carrier_name | VARCHAR(255) | | Carrier name |
| vehicle_plate | VARCHAR(20) | | Vehicle plate number |
| sri_access_key | VARCHAR(49) | UNIQUE | Clave de acceso |
| sri_authorization_number | VARCHAR(50) | | Authorization number |
| sri_authorization_date | TIMESTAMP | | Authorization date |
| xml_path | TEXT | | XML file path |
| ride_pdf_path | TEXT | | RIDE PDF path |
| status | VARCHAR(20) | DEFAULT 'draft' | draft, authorized, rejected |
| created_by | UUID | FK users(id) | |
| created_at | TIMESTAMP | DEFAULT NOW() | |
| updated_at | TIMESTAMP | DEFAULT NOW() | |

**Indexes:**
- `idx_delivery_notes_company_id` ON (company_id)
- `idx_delivery_notes_full_number` ON (company_id, full_number)

---

## SRI Integration

### sri_authorization_queue
Queue for processing SRI authorizations with retry logic.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Unique identifier |
| company_id | UUID | FK companies(id), NOT NULL | Owner company |
| document_type | VARCHAR(20) | NOT NULL | invoice, credit_note, withholding, etc. |
| document_id | UUID | NOT NULL | Reference to document |
| priority | INTEGER | DEFAULT 5 | 1=high, 5=normal, 10=low |
| retry_count | INTEGER | DEFAULT 0 | Number of retry attempts |
| max_retries | INTEGER | DEFAULT 3 | Maximum retry attempts |
| status | VARCHAR(20) | DEFAULT 'pending' | pending, processing, authorized, failed |
| last_attempt_at | TIMESTAMP | | Last attempt timestamp |
| next_retry_at | TIMESTAMP | | Next scheduled retry |
| error_message | TEXT | | Last error message |
| created_at | TIMESTAMP | DEFAULT NOW() | |
| updated_at | TIMESTAMP | DEFAULT NOW() | |

**Indexes:**
- `idx_sri_queue_status` ON (status, next_retry_at)
- `idx_sri_queue_company` ON (company_id, status)
- `idx_sri_queue_document` ON (document_type, document_id)

---

### sri_communication_logs
Complete log of all SRI API interactions.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Unique identifier |
| company_id | UUID | FK companies(id), NOT NULL | Owner company |
| document_type | VARCHAR(20) | NOT NULL | Document type |
| document_id | UUID | | Document reference |
| action | VARCHAR(50) | NOT NULL | validate, authorize, void, query |
| request_xml | TEXT | | Request XML sent |
| response_xml | TEXT | | Response XML received |
| http_status | INTEGER | | HTTP status code |
| error_code | VARCHAR(20) | | SRI error code |
| error_message | TEXT | | Error message |
| duration_ms | INTEGER | | Request duration in milliseconds |
| created_at | TIMESTAMP | DEFAULT NOW() | |

**Indexes:**
- `idx_sri_logs_company` ON (company_id, created_at DESC)
- `idx_sri_logs_document` ON (document_type, document_id)
- `idx_sri_logs_action` ON (action, created_at DESC)

**Retention**: Keep for 7+ years per Ecuador legal requirements

---

## Audit & Compliance

### audit_logs
Comprehensive audit trail of all data changes.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Unique identifier |
| company_id | UUID | FK companies(id) | Company context |
| user_id | UUID | FK users(id) | User who made change |
| table_name | VARCHAR(100) | NOT NULL | Affected table |
| record_id | UUID | NOT NULL | Affected record ID |
| action | VARCHAR(20) | NOT NULL | INSERT, UPDATE, DELETE |
| old_values | JSONB | | Previous values |
| new_values | JSONB | | New values |
| ip_address | VARCHAR(45) | | User's IP address |
| user_agent | TEXT | | User's browser/client |
| created_at | TIMESTAMP | DEFAULT NOW() | |

**Indexes:**
- `idx_audit_logs_company` ON (company_id, created_at DESC)
- `idx_audit_logs_record` ON (table_name, record_id, created_at DESC)
- `idx_audit_logs_user` ON (user_id, created_at DESC)

**Partitioning**: Partition by created_at (monthly or yearly) for performance

**Retention**: Keep for 7+ years per Ecuador legal requirements

---

## Data Integrity & Business Rules

### Constraints Summary

1. **Multi-tenancy**: `company_id` required on all tenant-scoped tables
2. **Sequential numbering**: Implemented via locked counter table + application logic
3. **Soft deletes**: Use `deleted_at` instead of physical deletion for audit trail
4. **Monetary values**: Always DECIMAL(10,2) for currency amounts
5. **Timestamps**: Always include `created_at`, `updated_at` on main entities
6. **Foreign keys**: All relationships enforced with FK constraints
7. **Unique constraints**: Natural business keys (RUC, invoice numbers, etc.)

### Critical Indexes

All tables must have:
- `company_id` indexed
- Foreign keys indexed
- Commonly filtered/joined columns indexed
- Composite indexes for multi-column queries

### Audit Requirements

Tables requiring comprehensive audit logging:
- invoices
- credit_notes
- debit_notes
- withholdings
- inventory_transactions
- invoice_payments

---

## Migration Strategy

### Phase 1: Core Structure
1. companies, users, roles, user_company_roles
2. contacts (customers/suppliers)
3. tax_types, retention_types, payment_methods

### Phase 2: Products & Inventory
1. product_categories, products
2. warehouses, inventory
3. inventory_transactions

### Phase 3: Invoicing
1. document_sequences
2. invoices, invoice_lines, invoice_line_taxes
3. invoice_payments

### Phase 4: SRI Documents
1. credit_notes, debit_notes
2. withholdings, withholding_lines
3. delivery_notes

### Phase 5: Integration & Audit
1. sri_authorization_queue
2. sri_communication_logs
3. audit_logs

---

## Notes

- **PostgreSQL Version**: 14+ recommended (for generated columns, partitioning)
- **UUID vs BIGSERIAL**: Using UUID for better distributed systems support
- **JSONB Usage**: For flexible schema (permissions, audit old/new values)
- **Partitioning**: Consider for audit_logs and sri_communication_logs
- **Row-Level Security**: Can be enabled for additional multi-tenant isolation
- **Offline Support**: Sync strategy needed for offline POS + local SQLite

---

## Related Documentation

- [SRI Integration Guide](sri-integration.md)
- [API Documentation](api-documentation.md)
- [Architecture Overview](architecture.md)
