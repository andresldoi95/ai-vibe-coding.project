# Product Feature Implementation - Quick Start Guide

## 🎯 Objective

Implement a complete backend for the **Products** feature in the Inventory module, following the exact same patterns as the Warehouses reference implementation.

## ✅ What's Already Done

The following foundational files have been created and are ready:

### Core Files (11 files)
1. **Product.cs** - Entity with all properties (name, code, SKU, pricing, inventory, etc.)
2. **ProductDto.cs** - Data transfer object
3. **ProductFilters.cs** - Advanced filtering DTO
4. **IProductRepository.cs** - Repository interface
5. **ProductRepository.cs** - Repository implementation with filtering logic
6. **ProductConfiguration.cs** - EF Core configuration with indexes and constraints
7. **ApplicationDbContext.cs** - Updated with Products DbSet
8. **IUnitOfWork.cs** - Updated with Products property
9. **UnitOfWork.cs** - Updated with Products injection
10. **Program.cs** - Updated with ProductRepository DI registration

### Implementation Scripts (6 scripts)
1. **create-product-feature.sh** - Creates all CQRS and controller files (MAIN SCRIPT)
2. **execute-product-implementation.sh** - Simple execution wrapper
3. **implement-product-feature.sh** - Full implementation orchestrator
4. **run-product-implementation.sh** - Interactive implementation script
5. **master-product-implementation.sh** - Self-contained implementation

### Documentation (3 files)
1. **PRODUCT_IMPLEMENTATION_COMPLETE.md** - Complete technical documentation
2. **PRODUCT_IMPLEMENTATION_STATUS.md** - Current status and next steps
3. **README_PRODUCT_IMPLEMENTATION.md** - This file

## 🚀 How to Complete the Implementation

### One-Command Execution (RECOMMENDED)

```bash
cd /home/runner/work/ai-vibe-coding.project/ai-vibe-coding.project
chmod +x execute-product-implementation.sh
./execute-product-implementation.sh
```

This will:
1. Execute `create-product-feature.sh` to create all CQRS files
2. Generate the EF Core migration
3. Build the backend
4. Display next steps

### What Gets Created

The script creates 13 additional files:

**Commands (8 files):**
- CreateProduct: Command + Validator + Handler
- UpdateProduct: Command + Validator + Handler
- DeleteProduct: Command + Handler

**Queries (4 files):**
- GetAllProducts: Query + Handler
- GetProductById: Query + Handler

**Controller (1 file):**
- ProductsController with 5 endpoints

**Total:** 24 files created/modified

## 📋 After Execution

### 1. Apply the Migration

```bash
cd /home/runner/work/ai-vibe-coding.project/ai-vibe-coding.project
docker-compose restart backend
```

The migration will be automatically applied on backend startup.

### 2. Test in Swagger

Open: http://localhost:5000/swagger

### 3. Test the Endpoints

#### Create a Product
```http
POST /api/v1/products
Content-Type: application/json
Authorization: Bearer {your-token}

{
  "name": "Gaming Laptop",
  "code": "LAP-001",
  "sku": "SKU-LAP-001",
  "description": "High-performance gaming laptop",
  "category": "Electronics",
  "brand": "TechBrand",
  "unitPrice": 1299.99,
  "costPrice": 800.00,
  "minimumStockLevel": 5,
  "currentStockLevel": 25,
  "weight": 2.5,
  "dimensions": "35x25x2",
  "isActive": true
}
```

#### List All Products
```http
GET /api/v1/products
Authorization: Bearer {your-token}
```

#### Filter Products
```http
# By category
GET /api/v1/products?category=Electronics

# By price range
GET /api/v1/products?minPrice=500&maxPrice=1500

# Low stock products
GET /api/v1/products?lowStockOnly=true

# Combined filters
GET /api/v1/products?category=Electronics&brand=TechBrand&isActive=true
```

#### Get Product by ID
```http
GET /api/v1/products/{id}
Authorization: Bearer {your-token}
```

#### Update Product
```http
PUT /api/v1/products/{id}
Content-Type: application/json
Authorization: Bearer {your-token}

{
  "id": "{same-id}",
  "name": "Gaming Laptop Pro",
  "code": "LAP-001",
  "sku": "SKU-LAP-001",
  ...
}
```

#### Delete Product (Soft Delete)
```http
DELETE /api/v1/products/{id}
Authorization: Bearer {your-token}
```

## 🏗️ Architecture Overview

### Pattern: CQRS (Command Query Responsibility Segregation)

```
┌─────────────────────────────────────────────────────┐
│                   API Layer                         │
│  ProductsController                                 │
│  - GET, POST, PUT, DELETE endpoints                │
└─────────────────┬───────────────────────────────────┘
                  │ MediatR
┌─────────────────┴───────────────────────────────────┐
│              Application Layer                      │
│                                                     │
│  Commands:                  Queries:                │
│  • CreateProduct            • GetAllProducts        │
│  • UpdateProduct            • GetProductById        │
│  • DeleteProduct                                    │
│                                                     │
│  Each with: Validator + Handler                    │
└─────────────────┬───────────────────────────────────┘
                  │ UnitOfWork
┌─────────────────┴───────────────────────────────────┐
│            Infrastructure Layer                     │
│  ProductRepository                                  │
│  - GetByCodeAsync                                   │
│  - GetBySKUAsync                                    │
│  - GetAllByTenantAsync (with filters)               │
└─────────────────┬───────────────────────────────────┘
                  │ EF Core
┌─────────────────┴───────────────────────────────────┐
│               Database (PostgreSQL)                 │
│  Products Table                                     │
│  - Unique constraints on Code and SKU per tenant    │
│  - Indexes for performance                          │
│  - Soft delete support                              │
└─────────────────────────────────────────────────────┘
```

## 🔍 Key Features

### 1. Multi-Tenancy
- Automatic tenant filtering on all queries
- Tenant ownership verification on commands
- Code and SKU uniqueness per tenant

### 2. Advanced Filtering
```csharp
public class ProductFilters
{
    public string? Name { get; set; }           // Contains search
    public string? Code { get; set; }           // Contains search
    public string? SKU { get; set; }            // Exact match
    public string? Category { get; set; }       // Exact match
    public string? Brand { get; set; }          // Exact match
    public bool? IsActive { get; set; }         // Boolean filter
    public decimal? MinPrice { get; set; }      // Price range
    public decimal? MaxPrice { get; set; }      // Price range
    public bool? LowStockOnly { get; set; }     // Stock level check
}
```

### 3. Validation
- FluentValidation for all commands
- Code format: `^[A-Z0-9-]+$` (uppercase, numbers, hyphens)
- Unique code and SKU per tenant
- Price validation (>= 0)
- Stock level validation (>= 0)

### 4. Soft Delete
- Products are never physically deleted
- `IsDeleted` flag used
- Code/SKU can be reused after deletion
- Global query filter excludes deleted products

### 5. Performance
- Strategic indexes:
  - `(TenantId, Code)` - Unique
  - `(TenantId, SKU)` - Unique
  - `TenantId` - Filtering
  - `IsActive` - Filtering
  - `Category` - Filtering
  - `Brand` - Filtering

## 📊 Database Schema

```sql
CREATE TABLE "Products" (
    "Id" UUID PRIMARY KEY,
    "TenantId" UUID NOT NULL,
    "Name" VARCHAR(256) NOT NULL,
    "Code" VARCHAR(50) NOT NULL,
    "Description" VARCHAR(2000),
    "SKU" VARCHAR(100) NOT NULL,
    "Category" VARCHAR(100),
    "Brand" VARCHAR(100),
    "UnitPrice" DECIMAL(18,2) NOT NULL,
    "CostPrice" DECIMAL(18,2) NOT NULL,
    "MinimumStockLevel" INT NOT NULL DEFAULT 0,
    "CurrentStockLevel" INT,
    "Weight" DECIMAL(18,2),
    "Dimensions" VARCHAR(100),
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "CreatedAt" TIMESTAMP NOT NULL,
    "UpdatedAt" TIMESTAMP NOT NULL,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "DeletedAt" TIMESTAMP
);

-- Unique Constraints
CREATE UNIQUE INDEX "IX_Products_TenantId_Code" 
    ON "Products" ("TenantId", "Code") 
    WHERE "IsDeleted" = false;

CREATE UNIQUE INDEX "IX_Products_TenantId_SKU" 
    ON "Products" ("TenantId", "SKU") 
    WHERE "IsDeleted" = false;

-- Performance Indexes
CREATE INDEX "IX_Products_TenantId" ON "Products" ("TenantId");
CREATE INDEX "IX_Products_IsActive" ON "Products" ("IsActive");
CREATE INDEX "IX_Products_Category" ON "Products" ("Category");
CREATE INDEX "IX_Products_Brand" ON "Products" ("Brand");
```

## 🧪 Testing Checklist

- [ ] Create product - Success
- [ ] Create product with duplicate code - Fails with error
- [ ] Create product with duplicate SKU - Fails with error
- [ ] Create product with invalid code format - Fails with validation error
- [ ] Get all products - Returns list
- [ ] Get product by ID - Returns correct product
- [ ] Filter by name - Returns matching products
- [ ] Filter by category - Returns matching products
- [ ] Filter by price range - Returns matching products
- [ ] Filter by low stock - Returns products below minimum stock
- [ ] Update product - Success
- [ ] Update with conflicting code - Fails with error
- [ ] Delete product - Success (soft delete)
- [ ] Deleted product not in list - Success
- [ ] Can create new product with deleted product's code - Success

## 🐛 Troubleshooting

### Migration Error: "Migration already exists"
```bash
cd backend/src/Api
dotnet ef migrations remove -p ../Infrastructure -s .
dotnet ef migrations add AddProductEntity -p ../Infrastructure -s .
```

### Build Error: "Type or namespace not found"
```bash
cd backend
dotnet clean
dotnet restore
dotnet build
```

### Runtime Error: "Products.read permission denied"
Ensure permissions are seeded in database:
- products.read
- products.create
- products.update
- products.delete

## 📚 Documentation

- **PRODUCT_IMPLEMENTATION_COMPLETE.md** - Complete technical documentation
- **PRODUCT_IMPLEMENTATION_STATUS.md** - Implementation status and file checklist
- **docs/WAREHOUSE_IMPLEMENTATION_REFERENCE.md** - Pattern reference (Warehouses)

## 🎓 Learning Resources

This implementation demonstrates:
- ✅ CQRS pattern with MediatR
- ✅ Repository pattern with Unit of Work
- ✅ FluentValidation
- ✅ Entity Framework Core configuration
- ✅ Multi-tenant architecture
- ✅ Soft delete pattern
- ✅ RESTful API design
- ✅ Dependency injection

## 🚦 Status

**Current:** ✅ Ready to Execute  
**Next:** ⏳ Run `execute-product-implementation.sh`  
**After:** 🎉 Test in Swagger and implement frontend

---

**Questions?** Check PRODUCT_IMPLEMENTATION_COMPLETE.md for detailed information.
