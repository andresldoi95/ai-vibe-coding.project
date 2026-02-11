# Seeder Update Summary
**Date**: February 10, 2026
**Task**: Regenerate seeder to add new permissions and taxes data

## Changes Made

### 1. New Migration: AddInvoiceAndTaxPermissions
**File**: [`backend/src/Infrastructure/Persistence/Migrations/20260210214220_AddInvoiceAndTaxPermissions.cs`](backend/src/Infrastructure/Persistence/Migrations/20260210214220_AddInvoiceAndTaxPermissions.cs)

#### New Permissions Added

**Invoices Permissions** (7 permissions):
- `invoices.read` - View invoices
- `invoices.create` - Create invoices
- `invoices.update` - Update invoices
- `invoices.delete` - Delete invoices
- `invoices.send` - Send invoices to customers
- `invoices.void` - Void/cancel invoices
- `invoices.export` - Export invoice data

**Tax Rates Permissions** (4 permissions):
- `tax-rates.read` - View tax rates
- `tax-rates.create` - Create tax rates
- `tax-rates.update` - Update tax rates
- `tax-rates.delete` - Delete tax rates

**Invoice Configuration Permissions** (2 permissions):
- `invoice-config.read` - View invoice configuration
- `invoice-config.update` - Update invoice configuration

**Total**: 13 new permissions

#### Permission Assignments by Role

| Role | Permissions Granted |
|------|---------------------|
| **Owner** | ✅ All invoice, tax-rates, and invoice-config permissions (13 total) |
| **Admin** | ✅ All invoice, tax-rates, and invoice-config permissions (13 total) |
| **Manager** | ✅ invoices: read, create, update, send, export<br>✅ tax-rates: read, update<br>✅ invoice-config: read<br>**Total**: 8 permissions |
| **User** | ✅ invoices.read<br>✅ tax-rates.read<br>✅ invoice-config.read<br>**Total**: 3 read-only permissions |

### 2. Updated SeedController
**File**: [`backend/src/Api/Controllers/SeedController.cs`](backend/src/Api/Controllers/SeedController.cs)

#### Updated Method: CreateRolesForTenant

**Before**:
```csharp
// Manager: Full access to warehouses, products, customers, stock
var managerPermissions = allPermissions
    .Where(p => new[] { "warehouses", "products", "customers", "stock" }.Contains(p.Resource))
```

**After**:
```csharp
// Manager: Full access to warehouses, products, customers, stock
// Read/create/update/send/export for invoices, read/update for tax-rates, read for invoice-config
var managerPermissions = allPermissions
    .Where(p =>
        new[] { "warehouses", "products", "customers", "stock" }.Contains(p.Resource) ||
        (p.Resource == "invoices" && new[] { "read", "create", "update", "send", "export" }.Contains(p.Action)) ||
        (p.Resource == "tax-rates" && new[] { "read", "update" }.Contains(p.Action)) ||
        (p.Resource == "invoice-config" && p.Action == "read"))
```

This ensures that when demo data is seeded via the `/api/seed/demo` endpoint, Manager roles will have appropriate invoice and tax management permissions.

### 3. Tax Rates Already Configured
**File**: [backend/src/Api/Controllers/SeedController.cs](backend/src/Api/Controllers/SeedController.cs#L916-L963)

The seeder already includes Ecuador tax rates in the `CreateTaxRatesForTenant` method:

```csharp
private List<TaxRate> CreateTaxRatesForTenant(Guid tenantId, DateTime now)
{
    return new List<TaxRate>
    {
        new TaxRate { Code = "IVA_0", Name = "IVA 0%", Rate = 0.00m, ... },
        new TaxRate { Code = "IVA_12", Name = "IVA 12%", Rate = 0.12m, ... },
        new TaxRate { Code = "IVA_15", Name = "IVA 15%", Rate = 0.15m, IsDefault = true, ... }
    };
}
```

**Ecuador Tax Rates**:
- ✅ IVA 0% (tax-exempt goods)
- ✅ IVA 12% (reduced rate)
- ✅ IVA 15% (standard rate - default)

## Migration Status

✅ **Migration Created**: `20260210214220_AddInvoiceAndTaxPermissions`
✅ **Migration Applied**: Database updated successfully
✅ **Permissions Seeded**: 13 new permissions added to database
✅ **Role Permissions Assigned**: All existing roles updated with new permissions

## Testing

### How to Test the Seeder

1. **Reset Database** (if needed):
   ```bash
   docker-compose down -v
   docker-compose up -d db
   dotnet ef database update --project src/Infrastructure --startup-project src/Api --connection "Host=localhost;Port=5432;Database=saas;Username=postgres;Password=postgres"
   ```

2. **Seed Demo Data**:
   ```bash
   # Start backend API
   docker-compose up -d backend

   # Call seed endpoint
   curl -X POST http://localhost:5000/api/seed/demo
   ```

3. **Verify Permissions**:
   ```sql
   -- Check all permissions
   SELECT * FROM "Permissions" WHERE "Resource" IN ('invoices', 'tax-rates', 'invoice-config');

   -- Check Manager role permissions for invoices
   SELECT p."Name", p."Description", r."Name" as RoleName
   FROM "RolePermissions" rp
   JOIN "Permissions" p ON rp."PermissionId" = p."Id"
   JOIN "Roles" r ON rp."RoleId" = r."Id"
   WHERE r."Name" = 'Manager' AND p."Resource" IN ('invoices', 'tax-rates', 'invoice-config')
   ORDER BY p."Resource", p."Action";
   ```

4. **Verify Tax Rates**:
   ```sql
   -- Check tax rates for all tenants
   SELECT t."Name" as TenantName, tr."Code", tr."Name", tr."Rate", tr."IsDefault"
   FROM "TaxRates" tr
   JOIN "Tenants" t ON tr."TenantId" = t."Id"
   ORDER BY t."Name", tr."Rate";
   ```

## Complete Permission Catalog

The system now has permissions for the following resources:

| Resource | Actions | Total |
|----------|---------|-------|
| warehouses | read, create, update, delete | 4 |
| products | read, create, update, delete | 4 |
| customers | read, create, update, delete | 4 |
| stock | read, create, update, delete | 4 |
| **invoices** | **read, create, update, delete, send, void, export** | **7** |
| **tax-rates** | **read, create, update, delete** | **4** |
| **invoice-config** | **read, update** | **2** |
| tenants | read, create, update, delete | 4 |
| users | read, create, update, delete, invite, remove | 6 |
| roles | read, manage | 2 |
| **Total** | | **41** |

## Demo Data Seeded

When calling `POST /api/seed/demo`, the following data is created:

### Tenants (3)
- Demo Company (`demo-company`)
- Tech Startup Inc (`tech-startup`)
- Manufacturing Corp (`manufacturing-corp`)

### Users (4) - Shared across all tenants
- owner@demo.com (password: `password`)
- admin@demo.com (password: `password`)
- manager@demo.com (password: `password`)
- user@demo.com (password: `password`)

### Per Tenant Data
| Data Type | Count per Tenant |
|-----------|------------------|
| Tax Rates | 3 (IVA 0%, 12%, 15%) |
| Invoice Configuration | 1 |
| Warehouses | 3 |
| Products | 5 |
| Customers | 5 |
| Stock Movements | ~30 |
| Warehouse Inventory | ~15 |

## Next Steps

1. ✅ **Migration applied** - New permissions are in the database
2. ✅ **Seeder updated** - Manager roles now have invoice permissions
3. ⏭️ **Test seeder** - Run `POST /api/seed/demo` to verify
4. ⏭️ **Frontend integration** - Update frontend to use new invoice permissions
5. ⏭️ **Authorization guards** - Ensure invoice controllers use the new permissions

## Files Modified

1. [`backend/src/Infrastructure/Persistence/Migrations/20260210214220_AddInvoiceAndTaxPermissions.cs`](backend/src/Infrastructure/Persistence/Migrations/20260210214220_AddInvoiceAndTaxPermissions.cs) - **Created**
2. [`backend/src/Api/Controllers/SeedController.cs`](backend/src/Api/Controllers/SeedController.cs) - **Updated** (Manager permissions)

## Summary

✅ All new invoice and tax rate permissions have been added
✅ Migration successfully applied to the database
✅ Seeder updated to assign appropriate permissions to Manager role
✅ Tax rates already configured for Ecuador (IVA 0%, 12%, 15%)
✅ Ready for testing and frontend integration
