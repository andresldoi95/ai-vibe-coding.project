# Stock Movements Feature - Implementation Instructions

## 🎯 Overview

This directory contains all the necessary scripts and documentation to implement the complete Stock Movements feature for the SaaS Billing + Inventory system.

## 📦 What's Included

### Core Files (Already Created)
- ✅ `Domain/Enums/MovementType.cs` - Movement type enumeration
- ✅ `Domain/Entities/StockMovement.cs` - Main entity
- ✅ `Application/DTOs/StockMovementDto.cs` - Data transfer object
- ✅ `Application/Common/Interfaces/IStockMovementRepository.cs` - Repository interface

### Modified Files (Already Updated)
- ✅ `ApplicationDbContext.cs` - Added DbSet<StockMovement>
- ✅ `IUnitOfWork.cs` - Added StockMovements property
- ✅ `UnitOfWork.cs` - Integrated repository
- ✅ `Program.cs` - Registered DI
- ✅ `CreateProductCommand.cs` - Added initial inventory fields
- ✅ `CreateProductCommandHandler.cs` - Auto-creates stock movements

### Implementation Scripts
1. **create-stock-movements-cqrs.sh** - Creates all CQRS commands and queries
2. **create-stock-movements-infrastructure.sh** - Creates repository and EF configuration
3. **create-stock-movements-controller.sh** - Creates API controller
4. **implement-stock-movements.sh** - Master script (runs all above)
5. **quickstart-stock-movements.sh** - Complete guide with migration

### Documentation
- **STOCK_MOVEMENTS_COMPLETE.md** - Comprehensive implementation guide
- **STOCK_MOVEMENTS_IMPLEMENTATION.md** - Implementation status tracker
- **update-existing-files-instructions.sh** - Manual update instructions (reference only)

## 🚀 Quick Start (Recommended)

Run the complete quickstart guide which handles everything:

```bash
chmod +x quickstart-stock-movements.sh
./quickstart-stock-movements.sh
```

This will:
1. Create all CQRS and infrastructure files
2. Generate and apply database migration
3. Restart backend container
4. Provide testing instructions

## 📋 Step-by-Step (Manual)

If you prefer to run each step manually:

### Step 1: Create All Files

```bash
chmod +x implement-stock-movements.sh
./implement-stock-movements.sh
```

### Step 2: Generate Migration

```bash
cd backend/src
dotnet ef migrations add AddStockMovementEntity \
  --project Infrastructure \
  --startup-project Api
```

### Step 3: Apply Migration

```bash
dotnet ef database update \
  --project Infrastructure \
  --startup-project Api
```

### Step 4: Restart Backend

```bash
cd ../../..
docker-compose restart backend
```

### Step 5: Test API

Open Swagger UI: http://localhost:5000/swagger

Test endpoints:
- GET /api/v1/stock-movements
- POST /api/v1/stock-movements
- GET /api/v1/stock-movements/{id}
- PUT /api/v1/stock-movements/{id}
- DELETE /api/v1/stock-movements/{id}

## 🧪 Testing Product Creation with Initial Inventory

```json
POST /api/v1/products
{
  "name": "Sample Product",
  "code": "PROD-001",
  "sku": "SKU-001",
  "unitPrice": 100.00,
  "costPrice": 75.00,
  "minimumStockLevel": 10,
  "initialQuantity": 50,
  "initialWarehouseId": "<warehouse-guid>"
}
```

This will:
1. Create the product
2. Automatically create an InitialInventory stock movement
3. Record quantity, cost, and reference

## 📁 File Structure Created

```
backend/src/
├── Application/
│   ├── DTOs/
│   │   └── StockMovementDto.cs ✅
│   ├── Common/Interfaces/
│   │   └── IStockMovementRepository.cs ✅
│   └── Features/
│       └── StockMovements/
│           ├── Commands/
│           │   ├── CreateStockMovement/
│           │   │   ├── CreateStockMovementCommand.cs
│           │   │   ├── CreateStockMovementCommandValidator.cs
│           │   │   └── CreateStockMovementCommandHandler.cs
│           │   ├── UpdateStockMovement/
│           │   │   ├── UpdateStockMovementCommand.cs
│           │   │   ├── UpdateStockMovementCommandValidator.cs
│           │   │   └── UpdateStockMovementCommandHandler.cs
│           │   └── DeleteStockMovement/
│           │       ├── DeleteStockMovementCommand.cs
│           │       ├── DeleteStockMovementCommandValidator.cs
│           │       └── DeleteStockMovementCommandHandler.cs
│           └── Queries/
│               ├── GetAllStockMovements/
│               │   ├── GetAllStockMovementsQuery.cs
│               │   └── GetAllStockMovementsQueryHandler.cs
│               └── GetStockMovementById/
│                   ├── GetStockMovementByIdQuery.cs
│                   └── GetStockMovementByIdQueryHandler.cs
├── Domain/
│   ├── Entities/
│   │   └── StockMovement.cs ✅
│   └── Enums/
│       └── MovementType.cs ✅
├── Infrastructure/
│   └── Persistence/
│       ├── Repositories/
│       │   └── StockMovementRepository.cs
│       └── Configurations/
│           └── StockMovementConfiguration.cs
└── Api/
    └── Controllers/
        └── StockMovementsController.cs
```

## 📊 Movement Types

| Type | Value | Use Case |
|------|-------|----------|
| InitialInventory | 0 | First-time stock setup |
| Purchase | 1 | Stock received from supplier |
| Sale | 2 | Stock sold to customer |
| Transfer | 3 | Moving between warehouses |
| Adjustment | 4 | Manual corrections |
| Return | 5 | Customer returns |

## 🔍 Validation Rules

### All Movements
- MovementType must be valid enum value
- ProductId is required and must exist
- WarehouseId is required and must exist
- Quantity cannot be zero
- UnitCost must be ≥ 0 (if provided)
- TotalCost must be ≥ 0 (if provided)
- Reference max 100 characters
- Notes max 500 characters

### Transfer Movements
- DestinationWarehouseId is required
- DestinationWarehouseId must differ from WarehouseId
- DestinationWarehouse must exist and belong to tenant

### Multi-Tenant
- All queries filtered by TenantId
- Cross-tenant access prevented
- Validation ensures tenant ownership

## 📚 Next Steps

After implementation:

1. **Update Seed Data** - Add sample stock movements in SeedController
2. **Frontend Implementation** - Follow warehouse reference pattern
3. **Add Permissions** - stock-movements.read, create, update, delete
4. **Add Reports** - Stock level summaries, movement history
5. **Integration** - Connect with invoicing and purchasing modules

## 🐛 Troubleshooting

### Migration Fails
```bash
# Drop and recreate database (development only)
dotnet ef database drop --project Infrastructure --startup-project Api --force
dotnet ef database update --project Infrastructure --startup-project Api
```

### Backend Won't Start
```bash
# Check logs
docker-compose logs backend

# Rebuild container
docker-compose down
docker-compose up --build backend
```

### Files Not Found
Ensure you ran the implementation script:
```bash
./implement-stock-movements.sh
```

## 📖 Documentation

- **Complete Guide**: `STOCK_MOVEMENTS_COMPLETE.md`
- **Status Tracker**: `STOCK_MOVEMENTS_IMPLEMENTATION.md`
- **Reference Pattern**: `docs/WAREHOUSE_IMPLEMENTATION_REFERENCE.md`
- **Swagger Docs**: http://localhost:5000/swagger

## ✅ Success Criteria

Implementation is complete when:
- [x] All files created via scripts
- [x] Existing files modified correctly
- [ ] Migration applied successfully
- [ ] Backend starts without errors
- [ ] All API endpoints work in Swagger
- [ ] Product creation with initial inventory works
- [ ] Multi-tenant isolation verified
- [ ] Soft deletes functioning

## 🎉 Support

For issues or questions:
1. Check `STOCK_MOVEMENTS_COMPLETE.md` for detailed information
2. Review `docs/WAREHOUSE_IMPLEMENTATION_REFERENCE.md` for patterns
3. Check backend logs: `docker-compose logs backend`
4. Verify database state: `docker-compose exec postgres psql -U postgres -d saas_db`

---

**Last Updated**: 2024  
**Status**: Ready for Implementation  
**Version**: 1.0.0
