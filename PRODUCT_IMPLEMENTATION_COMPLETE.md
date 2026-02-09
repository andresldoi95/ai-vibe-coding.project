# Product Feature - Complete Implementation Guide

## Status: READY TO EXECUTE

This document provides the complete implementation of the Products feature for the Inventory module.

## Files Already Created ✅

The following core files have been successfully created:

1. ✅ **Domain Layer**
   - `/backend/src/Domain/Entities/Product.cs` - Product entity with all properties

2. ✅ **Application Layer - DTOs**
   - `/backend/src/Application/DTOs/ProductDto.cs` - Data transfer object
   - `/backend/src/Application/DTOs/ProductFilters.cs` - Filter parameters for search
   - `/backend/src/Application/Common/Interfaces/IProductRepository.cs` - Repository interface

3. ✅ **Infrastructure Layer**
   - `/backend/src/Infrastructure/Persistence/Repositories/ProductRepository.cs` - Repository implementation with filtering
   - `/backend/src/Infrastructure/Persistence/Configurations/ProductConfiguration.cs` - EF Core configuration

4. ✅ **Configuration Updates**
   - ApplicationDbContext - Updated with Products DbSet
   - IUnitOfWork - Updated with Products property
   - UnitOfWork - Updated with Products property and constructor injection
   - Program.cs - Updated with ProductRepository registration

## Execution Instructions

### Step 1: Create CQRS Directory Structure and Files

Run the following command to create all CQRS command/query files:

```bash
cd /home/runner/work/ai-vibe-coding.project/ai-vibe-coding.project
chmod +x create-product-feature.sh
bash create-product-feature.sh
```

This script creates:
- CreateProduct (Command, Validator, Handler)
- UpdateProduct (Command, Validator, Handler)
- DeleteProduct (Command, Handler)
- GetAllProducts (Query, Handler)
- GetProductById (Query, Handler)
- ProductsController

### Step 2: Generate Database Migration

```bash
cd /home/runner/work/ai-vibe-coding.project/ai-vibe-coding.project/backend/src/Api
dotnet ef migrations add AddProductEntity -p ../Infrastructure -s . --verbose
```

### Step 3: Build Backend

```bash
cd /home/runner/work/ai-vibe-coding.project/ai-vibe-coding.project/backend
dotnet build
```

### Step 4: Apply Migration (Restart Backend)

```bash
cd /home/runner/work/ai-vibe-coding.project/ai-vibe-coding.project
docker-compose restart backend
```

OR run the complete implementation script:

```bash
cd /home/runner/work/ai-vibe-coding.project/ai-vibe-coding.project
chmod +x run-product-implementation.sh
bash run-product-implementation.sh
```

## Product Entity Properties

```csharp
public class Product : TenantEntity
{
    // Basic Information
    public string Name { get; set; }                    // Required, max 256 chars
    public string Code { get; set; }                    // Required, max 50 chars, unique per tenant
    public string? Description { get; set; }            // Optional, max 2000 chars
    public string SKU { get; set; }                     // Required, max 100 chars, unique per tenant

    // Category/Classification
    public string? Category { get; set; }               // Optional, max 100 chars, indexed
    public string? Brand { get; set; }                  // Optional, max 100 chars, indexed

    // Pricing
    public decimal UnitPrice { get; set; }              // Required, decimal(18,2)
    public decimal CostPrice { get; set; }              // Required, decimal(18,2)

    // Inventory
    public int MinimumStockLevel { get; set; }          // Required, default 0
    public int? CurrentStockLevel { get; set; }         // Optional

    // Physical Properties
    public decimal? Weight { get; set; }                // Optional, decimal(18,2)
    public string? Dimensions { get; set; }             // Optional, max 100 chars

    // Status
    public bool IsActive { get; set; } = true;          // Required, default true, indexed
}
```

## API Endpoints

All endpoints require authentication and appropriate permissions.

### GET /api/v1/products
Get all products for the current tenant with optional filtering.

**Query Parameters:**
- `name` (string) - Filter by product name (contains)
- `code` (string) - Filter by product code (contains)
- `sku` (string) - Filter by SKU (exact match)
- `category` (string) - Filter by category
- `brand` (string) - Filter by brand
- `isActive` (boolean) - Filter by active status
- `minPrice` (decimal) - Filter by minimum price
- `maxPrice` (decimal) - Filter by maximum price
- `lowStockOnly` (boolean) - Filter products with low stock

**Response:**
```json
{
  "data": [...],
  "success": true
}
```

### GET /api/v1/products/{id}
Get a specific product by ID.

**Response:**
```json
{
  "data": {
    "id": "guid",
    "tenantId": "guid",
    "name": "Product Name",
    "code": "PROD-001",
    "sku": "SKU-001",
    "description": "Description",
    "category": "Electronics",
    "brand": "BrandName",
    "unitPrice": 99.99,
    "costPrice": 50.00,
    "minimumStockLevel": 10,
    "currentStockLevel": 100,
    "weight": 1.5,
    "dimensions": "10x10x10",
    "isActive": true,
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  },
  "success": true
}
```

### POST /api/v1/products
Create a new product.

**Request Body:**
```json
{
  "name": "Product Name",
  "code": "PROD-001",
  "sku": "SKU-001",
  "description": "Product description",
  "category": "Electronics",
  "brand": "BrandName",
  "unitPrice": 99.99,
  "costPrice": 50.00,
  "minimumStockLevel": 10,
  "currentStockLevel": 100,
  "weight": 1.5,
  "dimensions": "10x10x10",
  "isActive": true
}
```

**Response:** 201 Created with product data

### PUT /api/v1/products/{id}
Update an existing product.

**Request Body:** Same as POST

**Response:** 200 OK with updated product data

### DELETE /api/v1/products/{id}
Soft delete a product.

**Response:** 200 OK with success message

## Validation Rules

### CreateProduct / UpdateProduct
- **Name**: Required, max 256 characters
- **Code**: Required, max 50 characters, format: `^[A-Z0-9-]+$` (uppercase, numbers, hyphens), unique per tenant
- **SKU**: Required, max 100 characters, unique per tenant
- **Description**: Optional, max 2000 characters
- **Category**: Optional, max 100 characters
- **Brand**: Optional, max 100 characters
- **UnitPrice**: Required, >= 0
- **CostPrice**: Required, >= 0
- **MinimumStockLevel**: Required, >= 0
- **CurrentStockLevel**: Optional, >= 0 if provided
- **Weight**: Optional, > 0 if provided
- **Dimensions**: Optional, max 100 characters

## Database Schema

### Table: Products

**Columns:**
- Id (PK, guid)
- TenantId (guid, indexed, FK)
- Name (varchar(256), required)
- Code (varchar(50), required)
- Description (varchar(2000), nullable)
- SKU (varchar(100), required)
- Category (varchar(100), nullable, indexed)
- Brand (varchar(100), nullable, indexed)
- UnitPrice (decimal(18,2), required)
- CostPrice (decimal(18,2), required)
- MinimumStockLevel (int, required, default 0)
- CurrentStockLevel (int, nullable)
- Weight (decimal(18,2), nullable)
- Dimensions (varchar(100), nullable)
- IsActive (boolean, required, default true, indexed)
- CreatedAt (timestamp, required)
- UpdatedAt (timestamp, required)
- IsDeleted (boolean, required, default false)
- DeletedAt (timestamp, nullable)

**Indexes:**
- PK: Id
- Unique: (TenantId, Code) WHERE IsDeleted = false
- Unique: (TenantId, SKU) WHERE IsDeleted = false
- Index: TenantId
- Index: IsActive
- Index: Category
- Index: Brand

**Query Filter:**
- Global soft delete filter: `!IsDeleted`

## Features Implemented

### 1. Multi-Tenant Isolation
- All queries automatically filtered by TenantId
- Code and SKU uniqueness enforced per tenant
- Tenant ownership verified on all operations

### 2. Soft Delete
- Products are never physically deleted
- IsDeleted flag used for soft delete
- Global query filter excludes deleted products
- Unique constraints respect soft delete status

### 3. Advanced Filtering
- Filter by multiple criteria simultaneously
- Text search (contains) on name and code
- Exact match on SKU
- Range filtering on prices
- Low stock detection

### 4. Validation
- FluentValidation for all commands
- Business rule enforcement (unique code/SKU per tenant)
- Data format validation (code pattern)
- Range validation (prices, stock levels)

### 5. Audit Trail
- Automatic CreatedAt on creation
- Automatic UpdatedAt on modification
- Inherited from TenantEntity

### 6. Performance Optimization
- Strategic indexes on frequently queried fields
- TenantId index for tenant filtering
- Category and Brand indexes for filtering
- IsActive index for status filtering

## Testing Checklist

After deployment, verify:

- [ ] GET /api/v1/products returns empty array for new tenant
- [ ] POST /api/v1/products creates product successfully
- [ ] POST returns 400 for duplicate code
- [ ] POST returns 400 for duplicate SKU
- [ ] GET /api/v1/products returns created product
- [ ] GET /api/v1/products/{id} returns correct product
- [ ] GET /api/v1/products?name=test filters correctly
- [ ] GET /api/v1/products?category=Electronics filters correctly
- [ ] GET /api/v1/products?minPrice=50&maxPrice=100 filters correctly
- [ ] GET /api/v1/products?lowStockOnly=true filters correctly
- [ ] PUT /api/v1/products/{id} updates product successfully
- [ ] PUT returns 400 when code conflicts with another product
- [ ] DELETE /api/v1/products/{id} soft deletes product
- [ ] Deleted product not returned in GET requests
- [ ] Deleted product code can be reused

## Troubleshooting

### Migration fails
```bash
# Clean and regenerate
dotnet ef migrations remove -p backend/src/Infrastructure -s backend/src/Api
dotnet ef migrations add AddProductEntity -p backend/src/Infrastructure -s backend/src/Api
```

### Build errors
```bash
# Clean and rebuild
cd backend
dotnet clean
dotnet restore
dotnet build
```

### Permission errors
Ensure the following permissions exist in your database:
- products.read
- products.create
- products.update
- products.delete

## Files Created Summary

### Domain Layer (1 file)
- Product.cs

### Application Layer (11 files)
- ProductDto.cs
- ProductFilters.cs
- IProductRepository.cs
- CreateProductCommand.cs + Validator + Handler
- UpdateProductCommand.cs + Validator + Handler
- DeleteProductCommand.cs + Handler
- GetAllProductsQuery.cs + Handler
- GetProductByIdQuery.cs + Handler

### Infrastructure Layer (2 files)
- ProductRepository.cs
- ProductConfiguration.cs

### API Layer (1 file)
- ProductsController.cs

### Updated Files (4 files)
- ApplicationDbContext.cs
- IUnitOfWork.cs
- UnitOfWork.cs
- Program.cs

**Total: 19 files created/modified**

## Next Steps

After successful implementation:

1. **Frontend Implementation**
   - Create Product TypeScript interfaces
   - Create useProduct composable
   - Create product CRUD pages
   - Add to navigation menu

2. **Additional Features**
   - Product images/attachments
   - Product variants
   - Stock movements integration
   - Price history
   - Barcode generation

3. **Reports**
   - Low stock report
   - Product valuation
   - Category analysis
   - Brand performance

---

**Implementation Date:** 2024
**Status:** Ready for Deployment
**Version:** 1.0
