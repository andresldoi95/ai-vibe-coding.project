# Stock Movements Feature - Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        STOCK MOVEMENTS FEATURE                          │
│                     Complete Backend Implementation                      │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│                            API LAYER (Api/)                             │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌────────────────────────────────────────────────────────────────┐    │
│  │         StockMovementsController.cs                           │    │
│  ├────────────────────────────────────────────────────────────────┤    │
│  │  • GET    /api/v1/stock-movements        (List all)           │    │
│  │  • GET    /api/v1/stock-movements/{id}   (Get by ID)          │    │
│  │  • POST   /api/v1/stock-movements        (Create)             │    │
│  │  • PUT    /api/v1/stock-movements/{id}   (Update)             │    │
│  │  • DELETE /api/v1/stock-movements/{id}   (Soft Delete)        │    │
│  │                                                                │    │
│  │  Authorization: [Authorize]                                   │    │
│  │  Uses: MediatR for CQRS pattern                              │    │
│  └────────────────────────────────────────────────────────────────┘    │
│                                   ↓                                      │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│                     APPLICATION LAYER (Application/)                     │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌──────────────────────────────────────────────────────────────────┐  │
│  │                      COMMANDS (Write)                            │  │
│  ├──────────────────────────────────────────────────────────────────┤  │
│  │  CreateStockMovement/                                            │  │
│  │    • CreateStockMovementCommand                                  │  │
│  │    • CreateStockMovementCommandValidator (FluentValidation)      │  │
│  │    • CreateStockMovementCommandHandler                           │  │
│  │                                                                   │  │
│  │  UpdateStockMovement/                                            │  │
│  │    • UpdateStockMovementCommand                                  │  │
│  │    • UpdateStockMovementCommandValidator                         │  │
│  │    • UpdateStockMovementCommandHandler                           │  │
│  │                                                                   │  │
│  │  DeleteStockMovement/                                            │  │
│  │    • DeleteStockMovementCommand                                  │  │
│  │    • DeleteStockMovementCommandValidator                         │  │
│  │    • DeleteStockMovementCommandHandler                           │  │
│  └──────────────────────────────────────────────────────────────────┘  │
│                                                                          │
│  ┌──────────────────────────────────────────────────────────────────┐  │
│  │                       QUERIES (Read)                             │  │
│  ├──────────────────────────────────────────────────────────────────┤  │
│  │  GetAllStockMovements/                                           │  │
│  │    • GetAllStockMovementsQuery                                   │  │
│  │    • GetAllStockMovementsQueryHandler                            │  │
│  │                                                                   │  │
│  │  GetStockMovementById/                                           │  │
│  │    • GetStockMovementByIdQuery                                   │  │
│  │    • GetStockMovementByIdQueryHandler                            │  │
│  └──────────────────────────────────────────────────────────────────┘  │
│                                                                          │
│  ┌──────────────────────────────────────────────────────────────────┐  │
│  │                      DATA CONTRACTS                              │  │
│  ├──────────────────────────────────────────────────────────────────┤  │
│  │  • StockMovementDto                    (DTOs/)                   │  │
│  │  • IStockMovementRepository            (Interfaces/)             │  │
│  │  • IUnitOfWork → StockMovements        (Interfaces/)             │  │
│  └──────────────────────────────────────────────────────────────────┘  │
│                                   ↓                                      │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│                   INFRASTRUCTURE LAYER (Infrastructure/)                 │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌──────────────────────────────────────────────────────────────────┐  │
│  │                      DATA ACCESS                                 │  │
│  ├──────────────────────────────────────────────────────────────────┤  │
│  │  StockMovementRepository.cs                                      │  │
│  │    • GetAllForTenantAsync()        → Includes Product, WH        │  │
│  │    • GetByProductIdAsync()         → Filter by product           │  │
│  │    • GetByWarehouseIdAsync()       → Filter by warehouse         │  │
│  │    • GetByIdWithDetailsAsync()     → Eager loading               │  │
│  │                                                                   │  │
│  │  Uses: Entity Framework Core, LINQ, AsNoTracking                │  │
│  └──────────────────────────────────────────────────────────────────┘  │
│                                                                          │
│  ┌──────────────────────────────────────────────────────────────────┐  │
│  │                  ENTITY CONFIGURATION                            │  │
│  ├──────────────────────────────────────────────────────────────────┤  │
│  │  StockMovementConfiguration.cs                                   │  │
│  │    • Table: StockMovements                                       │  │
│  │    • Foreign Keys: Product, Warehouse, DestinationWarehouse      │  │
│  │    • Indexes: TenantId, ProductId, WarehouseId, MovementType     │  │
│  │    • Composite Indexes: (TenantId, ProductId), (TenantId, WH)    │  │
│  │    • Query Filter: !IsDeleted                                    │  │
│  │    • Precision: UnitCost, TotalCost → DECIMAL(18,2)              │  │
│  └──────────────────────────────────────────────────────────────────┘  │
│                                                                          │
│  ┌──────────────────────────────────────────────────────────────────┐  │
│  │                      UNIT OF WORK                                │  │
│  ├──────────────────────────────────────────────────────────────────┤  │
│  │  UnitOfWork.cs                                                   │  │
│  │    • StockMovements: IStockMovementRepository                    │  │
│  │    • Products: IProductRepository                                │  │
│  │    • Warehouses: IWarehouseRepository                            │  │
│  │    • SaveChangesAsync()                                          │  │
│  │    • Transaction support                                         │  │
│  └──────────────────────────────────────────────────────────────────┘  │
│                                   ↓                                      │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│                        DOMAIN LAYER (Domain/)                            │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌──────────────────────────────────────────────────────────────────┐  │
│  │                         ENTITY                                   │  │
│  ├──────────────────────────────────────────────────────────────────┤  │
│  │  StockMovement : TenantEntity                                    │  │
│  │    • MovementType: MovementType enum                             │  │
│  │    • ProductId: Guid (FK)                                        │  │
│  │    • WarehouseId: Guid (FK)                                      │  │
│  │    • DestinationWarehouseId: Guid? (FK, optional)                │  │
│  │    • Quantity: int (non-zero)                                    │  │
│  │    • UnitCost: decimal? (optional)                               │  │
│  │    • TotalCost: decimal? (auto-calculated)                       │  │
│  │    • Reference: string? (max 100)                                │  │
│  │    • Notes: string? (max 500)                                    │  │
│  │    • MovementDate: DateTime (defaults to UtcNow)                 │  │
│  │                                                                   │  │
│  │  Inherited from TenantEntity:                                    │  │
│  │    • Id, TenantId, CreatedAt, UpdatedAt                          │  │
│  │    • IsDeleted, DeletedAt (soft delete support)                  │  │
│  │                                                                   │  │
│  │  Navigation Properties:                                          │  │
│  │    • Product: Product                                            │  │
│  │    • Warehouse: Warehouse                                        │  │
│  │    • DestinationWarehouse: Warehouse?                            │  │
│  └──────────────────────────────────────────────────────────────────┘  │
│                                                                          │
│  ┌──────────────────────────────────────────────────────────────────┐  │
│  │                          ENUM                                    │  │
│  ├──────────────────────────────────────────────────────────────────┤  │
│  │  MovementType                                                    │  │
│  │    • InitialInventory = 0   (First-time stock setup)             │  │
│  │    • Purchase = 1           (Receiving from supplier)            │  │
│  │    • Sale = 2               (Selling to customer)                │  │
│  │    • Transfer = 3           (Moving between warehouses)          │  │
│  │    • Adjustment = 4         (Manual corrections)                 │  │
│  │    • Return = 5             (Customer returns)                   │  │
│  └──────────────────────────────────────────────────────────────────┘  │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│                      DATABASE (PostgreSQL)                               │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌──────────────────────────────────────────────────────────────────┐  │
│  │  TABLE: StockMovements                                           │  │
│  ├──────────────────────────────────────────────────────────────────┤  │
│  │  PK: Id (UUID)                                                   │  │
│  │  FK: TenantId → Tenants.Id                                       │  │
│  │  FK: ProductId → Products.Id (Restrict)                          │  │
│  │  FK: WarehouseId → Warehouses.Id (Restrict)                      │  │
│  │  FK: DestinationWarehouseId → Warehouses.Id (Restrict, Nullable) │  │
│  │                                                                   │  │
│  │  Indexes:                                                        │  │
│  │    • IX_StockMovements_TenantId                                  │  │
│  │    • IX_StockMovements_ProductId                                 │  │
│  │    • IX_StockMovements_WarehouseId                               │  │
│  │    • IX_StockMovements_DestinationWarehouseId                    │  │
│  │    • IX_StockMovements_MovementType                              │  │
│  │    • IX_StockMovements_MovementDate                              │  │
│  │    • IX_StockMovements_TenantId_ProductId                        │  │
│  │    • IX_StockMovements_TenantId_WarehouseId                      │  │
│  └──────────────────────────────────────────────────────────────────┘  │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│                        CROSS-CUTTING CONCERNS                            │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  • Multi-Tenant Isolation:     TenantContext + Query Filters            │
│  • Validation:                  FluentValidation                         │
│  • Error Handling:              Result<T> pattern + Global middleware    │
│  • Logging:                     Serilog (ILogger)                        │
│  • Authentication:              JWT Bearer tokens                        │
│  • Authorization:               [Authorize] attribute                    │
│  • Soft Deletes:                Global query filters                     │
│  • Audit Trail:                 Auto CreatedAt/UpdatedAt                 │
│  • Transaction Support:         UnitOfWork.BeginTransaction()            │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│                         PRODUCT INTEGRATION                              │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  CreateProductCommandHandler Enhanced:                                   │
│                                                                          │
│  ┌────────────────────────────────────────────────────────────┐         │
│  │  POST /api/v1/products                                     │         │
│  │  {                                                         │         │
│  │    "name": "Product",                                      │         │
│  │    "code": "PROD-001",                                     │         │
│  │    "initialQuantity": 100,         ← NEW                   │         │
│  │    "initialWarehouseId": "guid"    ← NEW                   │         │
│  │  }                                                         │         │
│  └────────────────────────────────────────────────────────────┘         │
│                            ↓                                             │
│  ┌────────────────────────────────────────────────────────────┐         │
│  │  1. Creates Product                                        │         │
│  │  2. Validates Warehouse exists                             │         │
│  │  3. Auto-creates StockMovement:                            │         │
│  │     • MovementType: InitialInventory                       │         │
│  │     • Quantity: initialQuantity                            │         │
│  │     • UnitCost: costPrice                                  │         │
│  │     • Reference: "Initial inventory for {code}"            │         │
│  └────────────────────────────────────────────────────────────┘         │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│                         DATA FLOW EXAMPLE                                │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  POST /api/v1/stock-movements                                            │
│     ↓                                                                    │
│  StockMovementsController.Create()                                       │
│     ↓                                                                    │
│  _mediator.Send(CreateStockMovementCommand)                              │
│     ↓                                                                    │
│  CreateStockMovementCommandValidator.Validate()                          │
│     ↓                                                                    │
│  CreateStockMovementCommandHandler.Handle()                              │
│     ↓                                                                    │
│  1. Get TenantId from ITenantContext                                     │
│  2. Validate Product exists (via IUnitOfWork.Products)                   │
│  3. Validate Warehouse exists (via IUnitOfWork.Warehouses)               │
│  4. Calculate TotalCost if needed                                        │
│  5. Create StockMovement entity                                          │
│  6. Add to repository (IUnitOfWork.StockMovements)                       │
│  7. Save changes (IUnitOfWork.SaveChangesAsync)                          │
│  8. Load with details (GetByIdWithDetailsAsync)                          │
│  9. Map to DTO                                                           │
│  10. Return Result<StockMovementDto>                                     │
│     ↓                                                                    │
│  Controller returns 201 Created with location header                     │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│                          IMPLEMENTATION STATUS                           │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  Backend:  ████████████████████████████████████████  100% ✅            │
│  Database: ████████████████░░░░░░░░░░░░░░░░░░░░░░░░   40% ⏳ (Migration)│
│  Testing:  ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░    0% ⏳            │
│  Frontend: ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░    0% ⏳            │
│                                                                          │
│  Next Action: Run ./quickstart-stock-movements.sh                        │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## Legend

- **✅** Complete
- **⏳** Pending
- **FK** Foreign Key
- **PK** Primary Key
- **WH** Warehouse
- **IX** Index

---

*Architecture Diagram v1.0*  
*Stock Movements Feature*  
*Generated: 2024*
