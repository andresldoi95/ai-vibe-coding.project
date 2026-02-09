#!/bin/bash

###############################################################################
# PRODUCT FEATURE - FINAL EXECUTION SCRIPT
# This script completes the Product feature implementation
###############################################################################

set -e

clear

echo "╔════════════════════════════════════════════════════════════════╗"
echo "║                                                                ║"
echo "║        PRODUCT FEATURE IMPLEMENTATION - FINAL EXECUTION        ║"
echo "║                                                                ║"
echo "╚════════════════════════════════════════════════════════════════╝"
echo ""
echo "This script will execute the create-product-feature.sh script"
echo "which creates all CQRS commands, queries, and the API controller."
echo ""
echo "Prerequisites (already completed):"
echo "  ✅ Product.cs entity created"
echo "  ✅ ProductDto.cs created"
echo "  ✅ ProductFilters.cs created"
echo "  ✅ IProductRepository.cs created"
echo "  ✅ ProductRepository.cs created"
echo "  ✅ ProductConfiguration.cs created"
echo "  ✅ ApplicationDbContext updated"
echo "  ✅ IUnitOfWork updated"
echo "  ✅ UnitOfWork updated"
echo "  ✅ Program.cs updated"
echo ""
echo "This script will complete:"
echo "  ⏳ All CQRS command files"
echo "  ⏳ All CQRS query files"
echo "  ⏳ ProductsController"
echo "  ⏳ Generate EF Core migration"
echo "  ⏳ Build backend"
echo ""
read -p "Press Enter to continue or Ctrl+C to cancel..."
echo ""

# Navigate to project root
cd /home/runner/work/ai-vibe-coding.project/ai-vibe-coding.project

# Make scripts executable
chmod +x create-product-feature.sh 2>/dev/null || true

# Check if script exists
if [ ! -f "create-product-feature.sh" ]; then
    echo "❌ Error: create-product-feature.sh not found!"
    echo ""
    echo "Expected location:"
    echo "  /home/runner/work/ai-vibe-coding.project/ai-vibe-coding.project/create-product-feature.sh"
    echo ""
    exit 1
fi

# Execute the feature creation script
echo "Executing create-product-feature.sh..."
echo ""
bash create-product-feature.sh

# Check if successful
if [ $? -ne 0 ]; then
    echo ""
    echo "❌ Feature creation failed!"
    exit 1
fi

echo ""
echo "════════════════════════════════════════════════════════════════"
echo ""

# Generate migration
echo "Generating EF Core migration..."
cd backend/src/Api

dotnet ef migrations add AddProductEntity \
    -p ../Infrastructure \
    -s . \
    --verbose

if [ $? -eq 0 ]; then
    echo "✅ Migration generated successfully"
else
    echo "❌ Migration generation failed"
    echo ""
    echo "You can try manually:"
    echo "  cd backend/src/Api"
    echo "  dotnet ef migrations add AddProductEntity -p ../Infrastructure -s ."
    exit 1
fi

echo ""
echo "════════════════════════════════════════════════════════════════"
echo ""

# Build backend
echo "Building backend..."
cd /home/runner/work/ai-vibe-coding.project/ai-vibe-coding.project/backend

dotnet build

if [ $? -eq 0 ]; then
    echo "✅ Backend built successfully"
else
    echo "❌ Backend build failed"
    echo ""
    echo "You can try manually:"
    echo "  cd backend"
    echo "  dotnet clean"
    echo "  dotnet build"
    exit 1
fi

echo ""
echo "╔════════════════════════════════════════════════════════════════╗"
echo "║                                                                ║"
echo "║              ✅ IMPLEMENTATION COMPLETE!                       ║"
echo "║                                                                ║"
echo "╚════════════════════════════════════════════════════════════════╝"
echo ""
echo "═══════════════════════ SUMMARY ═══════════════════════════════"
echo ""
echo "Files Created:"
echo "  ✅ CreateProductCommand + Validator + Handler"
echo "  ✅ UpdateProductCommand + Validator + Handler"
echo "  ✅ DeleteProductCommand + Handler"
echo "  ✅ GetAllProductsQuery + Handler"
echo "  ✅ GetProductByIdQuery + Handler"
echo "  ✅ ProductsController"
echo "  ✅ EF Core Migration: AddProductEntity"
echo ""
echo "═══════════════════════ NEXT STEPS ════════════════════════════"
echo ""
echo "1. APPLY MIGRATION (restart backend):"
echo "   $ cd /home/runner/work/ai-vibe-coding.project/ai-vibe-coding.project"
echo "   $ docker-compose restart backend"
echo ""
echo "2. TEST API (open Swagger UI):"
echo "   http://localhost:5000/swagger"
echo ""
echo "3. AVAILABLE ENDPOINTS:"
echo "   GET    /api/v1/products              (List with filters)"
echo "   GET    /api/v1/products/{id}         (Get by ID)"
echo "   POST   /api/v1/products              (Create)"
echo "   PUT    /api/v1/products/{id}         (Update)"
echo "   DELETE /api/v1/products/{id}         (Delete)"
echo ""
echo "4. FILTER PARAMETERS (GET /api/v1/products):"
echo "   • name (string)       - Filter by name (contains)"
echo "   • code (string)       - Filter by code (contains)"
echo "   • sku (string)        - Filter by SKU (exact)"
echo "   • category (string)   - Filter by category"
echo "   • brand (string)      - Filter by brand"
echo "   • isActive (bool)     - Filter by status"
echo "   • minPrice (decimal)  - Minimum price"
echo "   • maxPrice (decimal)  - Maximum price"
echo "   • lowStockOnly (bool) - Low stock products"
echo ""
echo "5. EXAMPLE PRODUCT JSON (for POST):"
echo '   {'
echo '     "name": "Sample Product",'
echo '     "code": "PROD-001",'
echo '     "sku": "SKU-001",'
echo '     "description": "Product description",'
echo '     "category": "Electronics",'
echo '     "brand": "BrandName",'
echo '     "unitPrice": 99.99,'
echo '     "costPrice": 50.00,'
echo '     "minimumStockLevel": 10,'
echo '     "currentStockLevel": 100,'
echo '     "weight": 1.5,'
echo '     "dimensions": "10x10x10",'
echo '     "isActive": true'
echo '   }'
echo ""
echo "═══════════════════════ DOCUMENTATION ═════════════════════════"
echo ""
echo "  📖 PRODUCT_IMPLEMENTATION_COMPLETE.md  - Full documentation"
echo "  📖 PRODUCT_IMPLEMENTATION_STATUS.md    - Implementation status"
echo "  📖 docs/WAREHOUSE_IMPLEMENTATION_REFERENCE.md - Pattern reference"
echo ""
echo "════════════════════════════════════════════════════════════════"
echo ""
