#!/bin/bash

# Product Feature Complete Implementation Script
# This script creates all necessary files and runs the migration

set -e

echo "=========================================="
echo "Product Feature Complete Implementation"
echo "=========================================="
echo ""

# Base directories
BACKEND_BASE="/home/runner/work/ai-vibe-coding.project/ai-vibe-coding.project/backend/src"
APP_DIR="$BACKEND_BASE/Application/Features/Products"
API_DIR="$BACKEND_BASE/Api/Controllers"

# Step 1: Create directory structure
echo "Step 1: Creating directory structure..."
mkdir -p "$APP_DIR/Commands/CreateProduct"
mkdir -p "$APP_DIR/Commands/UpdateProduct"
mkdir -p "$APP_DIR/Commands/DeleteProduct"
mkdir -p "$APP_DIR/Queries/GetAllProducts"
mkdir -p "$APP_DIR/Queries/GetProductById"
echo "✅ Directory structure created"
echo ""

# Step 2: Run the feature creation script
echo "Step 2: Creating Product feature files..."
if [ -f "/home/runner/work/ai-vibe-coding.project/ai-vibe-coding.project/create-product-feature.sh" ]; then
    bash /home/runner/work/ai-vibe-coding.project/ai-vibe-coding.project/create-product-feature.sh
else
    echo "❌ Error: create-product-feature.sh not found"
    exit 1
fi
echo ""

# Step 3: Verify files created
echo "Step 3: Verifying created files..."
FILES=(
    "$BACKEND_BASE/Domain/Entities/Product.cs"
    "$BACKEND_BASE/Application/DTOs/ProductDto.cs"
    "$BACKEND_BASE/Application/DTOs/ProductFilters.cs"
    "$BACKEND_BASE/Application/Common/Interfaces/IProductRepository.cs"
    "$BACKEND_BASE/Infrastructure/Persistence/Repositories/ProductRepository.cs"
    "$BACKEND_BASE/Infrastructure/Persistence/Configurations/ProductConfiguration.cs"
    "$APP_DIR/Commands/CreateProduct/CreateProductCommand.cs"
    "$APP_DIR/Commands/CreateProduct/CreateProductCommandValidator.cs"
    "$APP_DIR/Commands/CreateProduct/CreateProductCommandHandler.cs"
    "$APP_DIR/Commands/UpdateProduct/UpdateProductCommand.cs"
    "$APP_DIR/Commands/UpdateProduct/UpdateProductCommandValidator.cs"
    "$APP_DIR/Commands/UpdateProduct/UpdateProductCommandHandler.cs"
    "$APP_DIR/Commands/DeleteProduct/DeleteProductCommand.cs"
    "$APP_DIR/Commands/DeleteProduct/DeleteProductCommandHandler.cs"
    "$APP_DIR/Queries/GetAllProducts/GetAllProductsQuery.cs"
    "$APP_DIR/Queries/GetAllProducts/GetAllProductsQueryHandler.cs"
    "$APP_DIR/Queries/GetProductById/GetProductByIdQuery.cs"
    "$APP_DIR/Queries/GetProductById/GetProductByIdQueryHandler.cs"
    "$API_DIR/ProductsController.cs"
)

MISSING_FILES=0
for file in "${FILES[@]}"; do
    if [ -f "$file" ]; then
        echo "  ✅ $file"
    else
        echo "  ❌ Missing: $file"
        MISSING_FILES=$((MISSING_FILES + 1))
    fi
done

if [ $MISSING_FILES -gt 0 ]; then
    echo ""
    echo "❌ Error: $MISSING_FILES file(s) missing. Please check the output above."
    exit 1
fi
echo "✅ All files verified"
echo ""

# Step 4: Generate migration
echo "Step 4: Generating Entity Framework migration..."
cd /home/runner/work/ai-vibe-coding.project/ai-vibe-coding.project/backend/src/Api
dotnet ef migrations add AddProductEntity \
    -p ../Infrastructure \
    -s . \
    --verbose

if [ $? -eq 0 ]; then
    echo "✅ Migration generated successfully"
else
    echo "❌ Migration generation failed"
    exit 1
fi
echo ""

# Step 5: Build the backend
echo "Step 5: Building backend..."
cd /home/runner/work/ai-vibe-coding.project/ai-vibe-coding.project/backend
dotnet build

if [ $? -eq 0 ]; then
    echo "✅ Backend built successfully"
else
    echo "❌ Backend build failed"
    exit 1
fi
echo ""

echo "=========================================="
echo "✅ Product Feature Implementation Complete!"
echo "=========================================="
echo ""
echo "Summary:"
echo "  - Product entity created with all properties"
echo "  - DTOs created (ProductDto, ProductFilters)"
echo "  - Repository interface and implementation created"
echo "  - CQRS Commands created (Create, Update, Delete)"
echo "  - CQRS Queries created (GetAll with filters, GetById)"
echo "  - ProductsController created with all endpoints"
echo "  - ProductConfiguration created with indexes and constraints"
echo "  - ApplicationDbContext updated with Products DbSet"
echo "  - IUnitOfWork and UnitOfWork updated with Products property"
echo "  - ProductRepository registered in DI (Program.cs)"
echo "  - Migration generated: AddProductEntity"
echo "  - Backend built successfully"
echo ""
echo "Next steps:"
echo "  1. Review the migration in: backend/src/Infrastructure/Persistence/Migrations/"
echo "  2. Restart backend container: docker-compose restart backend"
echo "  3. Test endpoints in Swagger: http://localhost:5000/swagger"
echo "  4. Test the following endpoints:"
echo "     - GET    /api/v1/products"
echo "     - GET    /api/v1/products/{id}"
echo "     - POST   /api/v1/products"
echo "     - PUT    /api/v1/products/{id}"
echo "     - DELETE /api/v1/products/{id}"
echo ""
echo "Product Filtering:"
echo "  The GET /api/v1/products endpoint supports the following query parameters:"
echo "     - name         (string)  - Filter by product name (contains)"
echo "     - code         (string)  - Filter by product code (contains)"
echo "     - sku          (string)  - Filter by SKU (exact match)"
echo "     - category     (string)  - Filter by category"
echo "     - brand        (string)  - Filter by brand"
echo "     - isActive     (bool)    - Filter by active status"
echo "     - minPrice     (decimal) - Filter by minimum price"
echo "     - maxPrice     (decimal) - Filter by maximum price"
echo "     - lowStockOnly (bool)    - Filter products with low stock"
echo ""
