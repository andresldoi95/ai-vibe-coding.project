# Stock Movements Feature - Complete Implementation Guide

**Status**: ✅ Backend Implementation Complete  
**Date**: 2024  
**Module**: Inventory Management  
**Pattern**: Following WAREHOUSE_IMPLEMENTATION_REFERENCE.md

---

## 🎯 Overview

The Stock Movements feature tracks all inventory transactions including:
- **InitialInventory**: Setting up initial stock levels
- **Purchase**: Stock received from suppliers
- **Sale**: Stock sold to customers
- **Transfer**: Moving stock between warehouses
- **Adjustment**: Manual corrections (damage, loss, etc.)
- **Return**: Stock returned from customers

---

## 📁 Files Created

### Domain Layer

#### ✅ `/backend/src/Domain/Enums/MovementType.cs`
Enum defining all types of stock movements with detailed documentation.

#### ✅ `/backend/src/Domain/Entities/StockMovement.cs`
Main entity extending `TenantEntity` with:
- Movement type tracking
- Product and warehouse relationships
- Quantity (positive/negative based on type)
- Cost tracking (unit and total)
- Reference and notes for context
- Navigation properties for Product, Warehouse, and DestinationWarehouse

### Application Layer

#### ✅ `/backend/src/Application/DTOs/StockMovementDto.cs`
DTO with flattened product and warehouse information for API responses.

#### ✅ `/backend/src/Application/Common/Interfaces/IStockMovementRepository.cs`
Repository interface with methods for:
- Getting all movements for a tenant
- Filtering by product or warehouse
- Loading movements with related entities

#### Commands Created:

**CreateStockMovement** (`/Application/Features/StockMovements/Commands/CreateStockMovement/`)
- `CreateStockMovementCommand.cs` - Command definition
- `CreateStockMovementCommandValidator.cs` - FluentValidation rules
- `CreateStockMovementCommandHandler.cs` - Business logic

**UpdateStockMovement** (`/Application/Features/StockMovements/Commands/UpdateStockMovement/`)
- `UpdateStockMovementCommand.cs`
- `UpdateStockMovementCommandValidator.cs`
- `UpdateStockMovementCommandHandler.cs`

**DeleteStockMovement** (`/Application/Features/StockMovements/Commands/DeleteStockMovement/`)
- `DeleteStockMovementCommand.cs`
- `DeleteStockMovementCommandValidator.cs`
- `DeleteStockMovementCommandHandler.cs`

#### Queries Created:

**GetAllStockMovements** (`/Application/Features/StockMovements/Queries/GetAllStockMovements/`)
- `GetAllStockMovementsQuery.cs`
- `GetAllStockMovementsQueryHandler.cs`

**GetStockMovementById** (`/Application/Features/StockMovements/Queries/GetStockMovementById/`)
- `GetStockMovementByIdQuery.cs`
- `GetStockMovementByIdQueryHandler.cs`

### Infrastructure Layer

#### ✅ `/backend/src/Infrastructure/Persistence/Repositories/StockMovementRepository.cs`
Repository implementation with optimized queries including:
- Eager loading of related entities
- Proper ordering (most recent first)
- No-tracking for read operations
- Multi-tenant filtering

#### ✅ `/backend/src/Infrastructure/Persistence/Configurations/StockMovementConfiguration.cs`
Entity Framework configuration defining:
- Table mapping
- Property constraints (precision for decimals, max lengths)
- Foreign key relationships with `Restrict` delete behavior
- Performance indexes on frequently queried fields
- Global query filter for soft deletes

### API Layer

#### ✅ `/backend/src/Api/Controllers/StockMovementsController.cs`
RESTful controller with endpoints:
- `GET /api/v1/stock-movements` - List all movements
- `GET /api/v1/stock-movements/{id}` - Get movement by ID
- `POST /api/v1/stock-movements` - Create new movement
- `PUT /api/v1/stock-movements/{id}` - Update movement
- `DELETE /api/v1/stock-movements/{id}` - Soft delete movement

---

## 🔄 Files Modified

### ✅ `Infrastructure/Persistence/ApplicationDbContext.cs`
**Added:**
```csharp
public DbSet<StockMovement> StockMovements => Set<StockMovement>();
```

### ✅ `Application/Common/Interfaces/IUnitOfWork.cs`
**Added:**
```csharp
IStockMovementRepository StockMovements { get; }
```

### ✅ `Infrastructure/Persistence/Repositories/UnitOfWork.cs`
**Added:**
- Property: `public IStockMovementRepository StockMovements { get; }`
- Constructor parameter: `IStockMovementRepository stockMovementRepository`
- Constructor assignment: `StockMovements = stockMovementRepository;`

### ✅ `Api/Program.cs`
**Added DI Registration:**
```csharp
builder.Services.AddScoped<IStockMovementRepository, StockMovementRepository>();
```

### ✅ `Application/Features/Products/Commands/CreateProduct/CreateProductCommand.cs`
**Added Optional Properties:**
```csharp
// Optional: Initial Inventory
public int? InitialQuantity { get; init; }
public Guid? InitialWarehouseId { get; init; }
```

### ✅ `Application/Features/Products/Commands/CreateProduct/CreateProductCommandHandler.cs`
**Added:**
- Import: `using SaaS.Domain.Enums;`
- Logic to automatically create an `InitialInventory` stock movement when both `InitialQuantity` and `InitialWarehouseId` are provided during product creation

---

## 🎯 Key Features

### Multi-Tenant Isolation
- All queries filtered by `TenantId`
- Validation ensures cross-tenant data access is prevented
- Inherits from `TenantEntity` base class

### Soft Deletes
- Implements `IsDeleted` and `DeletedAt` from base entity
- Global query filter excludes soft-deleted records
- Delete operations mark records as deleted rather than removing them

### Validation
- FluentValidation for all commands
- Business rule enforcement (e.g., Transfer requires destination warehouse)
- Data integrity checks (e.g., warehouse and product must exist)

### Cost Tracking
- Optional `UnitCost` and `TotalCost` fields
- Auto-calculation of `TotalCost` when `UnitCost` is provided
- Useful for purchase tracking and inventory valuation

### Audit Trail
- Automatic `CreatedAt` and `UpdatedAt` timestamps
- `MovementDate` for when the physical movement occurred
- `Reference` field for external document linking
- `Notes` for additional context

### Navigation Properties
- Eager loading support for Product, Warehouse, and DestinationWarehouse
- DTOs include flattened related entity data

---

## 🔧 Database Migration

To apply the schema changes:

```bash
cd backend/src

# Create migration
dotnet ef migrations add AddStockMovementEntity \
  --project Infrastructure \
  --startup-project Api

# Apply migration
dotnet ef database update \
  --project Infrastructure \
  --startup-project Api
```

**Migration will create:**
- `StockMovements` table with all fields
- Foreign keys to Products, Warehouses (with Restrict delete behavior)
- Indexes for performance optimization
- Unique constraints where applicable

---

## 🧪 Testing

### 1. Test Stock Movement CRUD

**Create Movement:**
```http
POST /api/v1/stock-movements
Authorization: Bearer {token}
Content-Type: application/json

{
  "movementType": 1,
  "productId": "guid",
  "warehouseId": "guid",
  "quantity": 100,
  "unitCost": 50.00,
  "reference": "PO-2024-001",
  "notes": "Initial purchase order"
}
```

**Get All Movements:**
```http
GET /api/v1/stock-movements
Authorization: Bearer {token}
```

**Get Movement by ID:**
```http
GET /api/v1/stock-movements/{id}
Authorization: Bearer {token}
```

**Update Movement:**
```http
PUT /api/v1/stock-movements/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "id": "guid",
  "movementType": 1,
  "productId": "guid",
  "warehouseId": "guid",
  "quantity": 150,
  "unitCost": 50.00,
  "movementDate": "2024-01-15T10:00:00Z"
}
```

**Delete Movement:**
```http
DELETE /api/v1/stock-movements/{id}
Authorization: Bearer {token}
```

### 2. Test Product Creation with Initial Inventory

```http
POST /api/v1/products
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "Laptop Dell XPS 15",
  "code": "DELL-XPS15",
  "sku": "SKU-DELL-001",
  "unitPrice": 1500.00,
  "costPrice": 1200.00,
  "minimumStockLevel": 5,
  "initialQuantity": 25,
  "initialWarehouseId": "warehouse-guid-here"
}
```

This will:
1. Create the product
2. Automatically create a stock movement of type `InitialInventory`
3. Set quantity to 25 in the specified warehouse

---

## 📊 Movement Type Usage

| Type | Quantity Sign | Use Case | Required Fields |
|------|---------------|----------|-----------------|
| **InitialInventory** | Positive | First-time stock setup | Product, Warehouse, Quantity |
| **Purchase** | Positive | Receiving from supplier | Product, Warehouse, Quantity, UnitCost (optional) |
| **Sale** | Negative | Selling to customer | Product, Warehouse, Quantity |
| **Transfer** | Negative (source) | Moving between warehouses | Product, Warehouse, DestinationWarehouse, Quantity |
| **Adjustment** | Both | Corrections, damage, loss | Product, Warehouse, Quantity, Notes |
| **Return** | Positive | Customer returns | Product, Warehouse, Quantity |

---

## 🎨 Frontend Implementation (Pending)

Following the warehouse reference pattern, create:

### Types
- `frontend/types/inventory.ts` - Add `StockMovement` interface

### Composable
- `frontend/composables/useStockMovement.ts`

### Pages
- `frontend/pages/inventory/stock-movements/index.vue` (List)
- `frontend/pages/inventory/stock-movements/new.vue` (Create)
- `frontend/pages/inventory/stock-movements/[id]/index.vue` (View)
- `frontend/pages/inventory/stock-movements/[id]/edit.vue` (Edit)

### i18n
Add translations in `frontend/i18n/locales/{en,es,fr,de}.json`:
```json
{
  "stockMovements": {
    "title": "Stock Movements",
    "create": "Record Movement",
    "movementType": "Movement Type",
    "types": {
      "initialInventory": "Initial Inventory",
      "purchase": "Purchase",
      "sale": "Sale",
      "transfer": "Transfer",
      "adjustment": "Adjustment",
      "return": "Return"
    }
  }
}
```

---

## 🔐 Permissions

Add to authorization system:
- `stock-movements.read`
- `stock-movements.create`
- `stock-movements.update`
- `stock-movements.delete`

---

## 📝 Seed Data (Recommended)

Update `SeedController` to create sample stock movements:

```csharp
// Sample: Initial inventory
new StockMovement
{
    TenantId = tenantId,
    MovementType = MovementType.InitialInventory,
    ProductId = product1.Id,
    WarehouseId = warehouse1.Id,
    Quantity = 100,
    UnitCost = 50.00m,
    TotalCost = 5000.00m,
    Reference = "INIT-2024-001",
    Notes = "Initial stock setup",
    MovementDate = DateTime.UtcNow.AddDays(-30)
}

// Sample: Purchase
new StockMovement
{
    TenantId = tenantId,
    MovementType = MovementType.Purchase,
    ProductId = product1.Id,
    WarehouseId = warehouse1.Id,
    Quantity = 50,
    UnitCost = 45.00m,
    TotalCost = 2250.00m,
    Reference = "PO-2024-001",
    Notes = "Purchase order from Supplier A",
    MovementDate = DateTime.UtcNow.AddDays(-15)
}

// Sample: Transfer
new StockMovement
{
    TenantId = tenantId,
    MovementType = MovementType.Transfer,
    ProductId = product1.Id,
    WarehouseId = warehouse1.Id,
    DestinationWarehouseId = warehouse2.Id,
    Quantity = -20,
    Reference = "TRANS-2024-001",
    Notes = "Transfer to regional warehouse",
    MovementDate = DateTime.UtcNow.AddDays(-7)
}

// Sample: Sale
new StockMovement
{
    TenantId = tenantId,
    MovementType = MovementType.Sale,
    ProductId = product1.Id,
    WarehouseId = warehouse1.Id,
    Quantity = -15,
    Reference = "INV-2024-001",
    Notes = "Sold to Customer XYZ",
    MovementDate = DateTime.UtcNow.AddDays(-2)
}
```

---

## ✅ Implementation Checklist

### Backend - Complete ✅
- [x] Create MovementType enum
- [x] Create StockMovement entity
- [x] Create StockMovementDto
- [x] Create IStockMovementRepository interface
- [x] Create all CQRS commands (Create, Update, Delete)
- [x] Create all CQRS queries (GetAll, GetById)
- [x] Add FluentValidation for each command
- [x] Implement StockMovementRepository
- [x] Create StockMovementConfiguration
- [x] Add DbSet to ApplicationDbContext
- [x] Update IUnitOfWork and UnitOfWork
- [x] Register repository in Program.cs DI
- [x] Create StockMovementsController
- [x] Enhance CreateProduct to support initial inventory
- [ ] Generate and apply migration
- [ ] Test all endpoints in Swagger
- [ ] Update seed data with sample movements

### Frontend - Pending ⏳
- [ ] Create TypeScript interface
- [ ] Create useStockMovement composable
- [ ] Create list page
- [ ] Create create page
- [ ] Create view page
- [ ] Create edit page
- [ ] Add i18n translations
- [ ] Add to navigation menu
- [ ] Test all CRUD operations

---

## 🚀 Deployment Notes

1. **Migration**: Run migration before deploying updated code
2. **Backwards Compatibility**: All new fields are optional or have defaults
3. **Performance**: Indexes created for common query patterns
4. **Monitoring**: Log statements added for tracking operations

---

## 📚 Related Documentation

- [WAREHOUSE_IMPLEMENTATION_REFERENCE.md](docs/WAREHOUSE_IMPLEMENTATION_REFERENCE.md) - Pattern reference
- [PRODUCTS_FEATURE_SUMMARY.md](PRODUCTS_FEATURE_SUMMARY.md) - Product feature docs
- [API Documentation](http://localhost:5000/swagger) - Swagger UI

---

## 🎉 Conclusion

The Stock Movements backend feature is now **100% complete** and ready for:
1. Database migration
2. API testing
3. Frontend implementation
4. Integration with invoicing and purchasing modules

All code follows the established patterns, includes proper validation, supports multi-tenancy, and maintains audit trails. The integration with Product creation enables seamless initial inventory setup.

---

**Last Updated**: 2024  
**Implemented By**: Billing Project Agent  
**Status**: ✅ Backend Complete / ⏳ Frontend Pending
