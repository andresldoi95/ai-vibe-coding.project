# Warehouse Module - Complete Implementation Reference

**Created**: February 7, 2026
**Purpose**: Reference implementation for all future CRUD modules in the SaaS Billing + Inventory system

## Overview

The Warehouse module demonstrates the complete implementation of a CRUD feature following our established architecture patterns. Use this as a template when implementing new entities like Products, Customers, Invoices, etc.

## Architecture Layers

### 1. Domain Layer (`backend/src/Domain`)

#### Entity: `Warehouse.cs`

**Location**: `Domain/Entities/Warehouse.cs`

```csharp
public class Warehouse : TenantEntity
{
    // Basic Information
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Address Information
    public string? StreetAddress { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }

    // Contact Information
    public string? Phone { get; set; }
    public string? Email { get; set; }

    // Additional Information
    public int? SquareFootage { get; set; }
    public int? Capacity { get; set; }
    public bool IsActive { get; set; } = true;
}
```

**Key Features**:
- Extends `TenantEntity` for automatic multi-tenant support
- Inherits audit fields (CreatedAt, UpdatedAt, etc.)
- Inherits soft delete support (IsDeleted, DeletedAt)
- `Code` is unique per tenant
- `IsActive` for enabling/disabling warehouses

---

### 2. Application Layer (`backend/src/Application`)

#### DTO: `WarehouseDto.cs`

**Location**: `Application/DTOs/WarehouseDto.cs`

```csharp
public class WarehouseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? StreetAddress { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int? SquareFootage { get; set; }
    public int? Capacity { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

#### Commands

**CreateWarehouseCommand** (`Application/Features/Warehouses/Commands/CreateWarehouse/`)
- Command, Validator, Handler
- Validates: Name, Code format, uniqueness
- Returns: `Result<WarehouseDto>`

**UpdateWarehouseCommand** (`Application/Features/Warehouses/Commands/UpdateWarehouse/`)
- Validates warehouse exists and belongs to tenant
- Checks code uniqueness (excluding current warehouse)
- Returns: `Result<WarehouseDto>`

**DeleteWarehouseCommand** (`Application/Features/Warehouses/Commands/DeleteWarehouse/`)
- Soft deletes warehouse (sets IsDeleted = true)
- Validates ownership before deletion
- Returns: `Result<bool>`

#### Queries

**GetAllWarehousesQuery** (`Application/Features/Warehouses/Queries/GetAllWarehouses/`)
- Automatically filtered by TenantId
- Filters out soft-deleted warehouses
- Returns: `Result<List<WarehouseDto>>`

**GetWarehouseByIdQuery** (`Application/Features/Warehouses/Queries/GetWarehouseById/`)
- Validates tenant ownership
- Returns 404 if not found or belongs to different tenant
- Returns: `Result<WarehouseDto>`

#### Repository Interface

**Location**: `Application/Common/Interfaces/IWarehouseRepository.cs`

```csharp
public interface IWarehouseRepository : IRepository<Warehouse>
{
    Task<Warehouse?> GetByCodeAsync(string code, Guid tenantId);
    Task<List<Warehouse>> GetAllForTenantAsync(Guid tenantId);
}
```

---

### 3. Infrastructure Layer (`backend/src/Infrastructure`)

#### Entity Configuration

**Location**: `Infrastructure/Persistence/Configurations/WarehouseConfiguration.cs`

```csharp
public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("Warehouses");

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(w => w.Code)
            .IsRequired()
            .HasMaxLength(50);

        // Unique constraint: Code must be unique per tenant (excluding soft-deleted)
        builder.HasIndex(w => new { w.TenantId, w.Code })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        // Performance indexes
        builder.HasIndex(w => w.TenantId);
        builder.HasIndex(w => w.IsActive);

        // Global query filter for soft deletes
        builder.HasQueryFilter(w => !w.IsDeleted);
    }
}
```

#### Repository Implementation

**Location**: `Infrastructure/Persistence/Repositories/WarehouseRepository.cs`

```csharp
public class WarehouseRepository : Repository<Warehouse>, IWarehouseRepository
{
    public WarehouseRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Warehouse?> GetByCodeAsync(string code, Guid tenantId)
    {
        return await _context.Warehouses
            .FirstOrDefaultAsync(w => w.Code == code && w.TenantId == tenantId);
    }

    public async Task<List<Warehouse>> GetAllForTenantAsync(Guid tenantId)
    {
        return await _context.Warehouses
            .Where(w => w.TenantId == tenantId)
            .OrderBy(w => w.Name)
            .ToListAsync();
    }
}
```

#### Migration

**Location**: `Infrastructure/Persistence/Migrations/20260207181646_AddWarehouseEntity.cs`

- Creates `Warehouses` table
- Creates indexes for `TenantId`, `IsActive`, and unique constraint on `(TenantId, Code)`
- Auto-applied on container startup in development

---

### 4. API Layer (`backend/src/Api`)

#### Controller

**Location**: `Api/Controllers/WarehousesController.cs`

```csharp
[ApiController]
[Route("api/v1/warehouses")]
[Authorize]
public class WarehousesController : BaseController
{
    public WarehousesController(IMediator mediator) : base(mediator) { }

    [HttpGet]
    public async Task<IActionResult> GetAll() { /* Returns all warehouses */ }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id) { /* Returns single warehouse */ }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWarehouseCommand command)
    { /* Creates and returns 201 */ }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWarehouseCommand command)
    { /* Updates and returns 200 */ }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    { /* Soft deletes and returns 204 */ }
}
```

**Endpoints**:
- `GET /api/v1/warehouses` - List all
- `GET /api/v1/warehouses/{id}` - Get by ID
- `POST /api/v1/warehouses` - Create
- `PUT /api/v1/warehouses/{id}` - Update
- `DELETE /api/v1/warehouses/{id}` - Delete

---

## Frontend Implementation

### 1. Types (`frontend/types/inventory.ts`)

```typescript
export interface Warehouse {
  id: string
  name: string
  code: string
  description?: string
  streetAddress?: string
  city?: string
  state?: string
  postalCode?: string
  country?: string
  phone?: string
  email?: string
  squareFootage?: number
  capacity?: number
  isActive: boolean
  createdAt: string
  updatedAt: string
}
```

### 2. Composable (`frontend/composables/useWarehouse.ts`)

**Key Methods**:
- `getAllWarehouses()` - Fetch all warehouses
- `getWarehouseById(id)` - Fetch single warehouse
- `createWarehouse(warehouse)` - Create new
- `updateWarehouse(id, warehouse)` - Update existing
- `deleteWarehouse(id)` - Delete warehouse

**Important**:
- Use `config.public.apiBase` for base URL
- Never include `/api/v1` in endpoint paths (already in baseURL)
- Include error handling with toast notifications
- Use i18n for all messages

### 3. Pages

#### List Page (`frontend/pages/inventory/warehouses/index.vue`)

**Features**:
- PageHeader with "Create Warehouse" button
- DataTable with:
  - Columns: Code, Name, Location, Contact, Capacity, Status, Actions
  - Pagination (10 per page)
  - Sorting
  - Loading state
- Status badges (Active/Inactive with Tag component)
- Action buttons (View/Edit/Delete icons)
- Delete confirmation dialog
- EmptyState when no data
- Toast notifications

**Route**: `/inventory/warehouses`

#### Create Page (`frontend/pages/inventory/warehouses/new.vue`)

**Features**:
- Breadcrumb navigation
- Multi-section form in Card components:
  - Basic Info: Name, Code, Description
  - Address Info: Street, City, State, Postal Code, Country
  - Contact Info: Phone, Email
  - Additional Info: Square Footage, Capacity, IsActive toggle
- Vuelidate validation
- Helper text for complex fields
- Form actions: Cancel (outlined) + Save (primary)
- Loading state during submission

**Route**: `/inventory/warehouses/new`

#### View Page (`frontend/pages/inventory/warehouses/[id]/index.vue`)

**Features**:
- Breadcrumb: Home > Warehouses > {name}
- Header with warehouse name and Edit button
- Organized detail display matching form sections
- Status displayed with Tag component
- Empty value handling (shows "—" for nulls)
- Loading state
- Error handling

**Route**: `/inventory/warehouses/{id}`

#### Edit Page (`frontend/pages/inventory/warehouses/[id]/edit.vue`)

**Features**:
- Same structure as create page
- Pre-populated with existing data
- Validation on update
- Update confirmation

**Route**: `/inventory/warehouses/{id}/edit`

### 4. i18n Translations

**Location**: `frontend/i18n/locales/en.json` (and es, fr, de)

```json
{
  "warehouses": {
    "title": "Warehouses",
    "description": "Manage your warehouse locations and storage facilities",
    "create": "Create Warehouse",
    "edit": "Edit Warehouse",
    "name": "Warehouse Name",
    "code": "Warehouse Code",
    "code_helper": "Uppercase letters, numbers, and hyphens only. Must be unique.",
    "is_active": "Active warehouse",
    "created_successfully": "Warehouse created successfully",
    "updated_successfully": "Warehouse updated successfully",
    "deleted_successfully": "Warehouse deleted successfully",
    "confirm_delete": "Are you sure you want to delete warehouse {name}?"
  }
}
```

**Important**: Escape special characters like `@` as `{'@'}` in placeholders.

---

## Key Patterns & Best Practices

### Backend Patterns

1. **Entity Design**
   - Extend `TenantEntity` for multi-tenant entities
   - Use nullable types for optional fields
   - Add meaningful default values (e.g., `IsActive = true`)

2. **CQRS**
   - Separate commands and queries
   - One handler per operation
   - Use FluentValidation for all commands
   - Return `Result<T>` from handlers

3. **Repository**
   - Implement custom methods for specific queries
   - Always filter by TenantId for tenant-scoped entities
   - Use `AsNoTracking()` for read-only queries

4. **Entity Configuration**
   - Use `IEntityTypeConfiguration<T>`
   - Add unique constraints with tenant scoping
   - Add performance indexes (TenantId, IsActive, etc.)
   - Configure global query filters for soft deletes

5. **Controller**
   - Thin controllers - delegate to MediatR
   - Consistent response format: `{ data, success }`
   - Proper HTTP status codes
   - Route parameter validation

### Frontend Patterns

1. **Page Structure**
   - Use PageHeader component
   - Implement EmptyState and LoadingState
   - Add breadcrumb navigation
   - Consistent action button placement

2. **Forms**
   - Multi-section layout with Cards
   - Vuelidate for validation
   - Helper text for complex fields
   - Cancel (outlined) + Save (primary) buttons

3. **DataTable**
   - Pagination with page size options
   - Sortable columns
   - Action column with icon buttons
   - Status badges for boolean fields

4. **API Integration**
   - One composable per entity
   - Consistent method naming
   - Error handling with toasts
   - i18n for all messages
   - Type safety with TypeScript

5. **Routing**
   - List: `/entity`
   - Create: `/entity/new`
   - View: `/entity/{id}`
   - Edit: `/entity/{id}/edit`

---

## Checklist for New CRUD Modules

When implementing a new entity (e.g., Product, Invoice, Customer):

### Backend
- [ ] Create entity in `Domain/Entities/`
- [ ] Create DTO in `Application/DTOs/`
- [ ] Create repository interface in `Application/Common/Interfaces/`
- [ ] Implement CQRS commands (Create, Update, Delete)
- [ ] Implement CQRS queries (GetAll, GetById)
- [ ] Add FluentValidation for each command
- [ ] Implement repository in `Infrastructure/Persistence/Repositories/`
- [ ] Create entity configuration in `Infrastructure/Persistence/Configurations/`
- [ ] Add DbSet to `ApplicationDbContext`
- [ ] Add repository to `IUnitOfWork` and `UnitOfWork`
- [ ] Register repository in `Program.cs` DI
- [ ] Create controller in `Api/Controllers/`
- [ ] Generate migration (`dotnet ef migrations add`)
- [ ] Test all endpoints in Swagger

### Frontend
- [ ] Create TypeScript interface in `types/`
- [ ] Create composable in `composables/use{Entity}.ts`
- [ ] Create list page `pages/{module}/{entity}/index.vue`
- [ ] Create create page `pages/{module}/{entity}/new.vue`
- [ ] Create view page `pages/{module}/{entity}/[id]/index.vue`
- [ ] Create edit page `pages/{module}/{entity}/[id]/edit.vue`
- [ ] Add i18n translations (en, es, fr, de)
- [ ] Add navigation menu item in `layouts/default.vue`
- [ ] Test all CRUD operations
- [ ] Verify responsive design
- [ ] Test dark mode compatibility

---

## Common Issues & Solutions

### Backend

**Issue**: Migration not applied automatically
- **Solution**: Restart backend container: `docker-compose restart backend`

**Issue**: Duplicate key violation on Code
- **Solution**: Check unique constraint is scoped to TenantId with soft delete filter

**Issue**: Entity not filtered by tenant
- **Solution**: Ensure entity extends `TenantEntity` and has global query filter

### Frontend

**Issue**: 404 error with duplicate `/api/v1`
- **Solution**: Remove `/api/v1` prefix from composable endpoints (already in baseURL)

**Issue**: Page not found after creating new route
- **Solution**: Restart frontend container: `docker-compose restart frontend`

**Issue**: Translations not working
- **Solution**: Escape special characters (`@` as `{'@'}`) and add to all locale files

**Issue**: Form validation not triggering
- **Solution**: Ensure Vuelidate `v$` is defined and `v$.$touch()` is called on submit

---

## Next Steps

Use this warehouse implementation as a template for:

1. **Inventory Module**:
   - Products
   - Stock Movements
   - Suppliers

2. **Billing Module**:
   - Invoices
   - Customers
   - Payments

3. **Additional Features**:
   - Advanced filtering and search
   - Export functionality
   - Bulk operations
   - File uploads

---

**Last Updated**: February 7, 2026
