#!/bin/bash

# Complete Product Feature Implementation and Deployment
# This master script orchestrates the entire Product feature implementation

set -e

echo "=========================================="
echo " PRODUCT FEATURE - COMPLETE IMPLEMENTATION"
echo "=========================================="
echo ""
echo "This script will:"
echo "  1. Create all Product feature files (entities, DTOs, commands, queries)"
echo "  2. Create Product repository and configuration"
echo "  3. Create Products API controller"
echo "  4. Generate database migration"
echo "  5. Build the backend"
echo "  6. Apply migrations (restart backend container)"
echo ""
read -p "Press Enter to continue or Ctrl+C to cancel..."
echo ""

# Navigate to project root
cd /home/runner/work/ai-vibe-coding.project/ai-vibe-coding.project

# Make scripts executable
chmod +x create-product-feature.sh
chmod +x implement-product-feature.sh

# Execute the implementation script
bash implement-product-feature.sh

if [ $? -eq 0 ]; then
    echo ""
    echo "=========================================="
    echo "  IMPLEMENTATION SUCCESSFUL!"
    echo "=========================================="
    echo ""
    echo "The Product feature has been fully implemented."
    echo ""
    echo "To apply the migration and start using the Product API:"
    echo ""
    echo "  Method 1 - Using Docker:"
    echo "    docker-compose restart backend"
    echo ""
    echo "  Method 2 - Manual:"
    echo "    cd backend/src/Api"
    echo "    dotnet ef database update"
    echo ""
    echo "After restart, access Swagger UI at:"
    echo "  http://localhost:5000/swagger"
    echo ""
    echo "Test the Product endpoints:"
    echo "  GET    /api/v1/products           - List all products (with optional filters)"
    echo "  GET    /api/v1/products/{id}      - Get product by ID"
    echo "  POST   /api/v1/products           - Create new product"
    echo "  PUT    /api/v1/products/{id}      - Update product"
    echo "  DELETE /api/v1/products/{id}      - Delete product (soft delete)"
    echo ""
    echo "Example Product JSON for POST request:"
    echo '  {'
    echo '    "name": "Sample Product",'
    echo '    "code": "PROD-001",'
    echo '    "sku": "SKU-001",'
    echo '    "description": "A sample product",'
    echo '    "category": "Electronics",'
    echo '    "brand": "SampleBrand",'
    echo '    "unitPrice": 99.99,'
    echo '    "costPrice": 50.00,'
    echo '    "minimumStockLevel": 10,'
    echo '    "currentStockLevel": 100,'
    echo '    "weight": 1.5,'
    echo '    "dimensions": "10x10x10",'
    echo '    "isActive": true'
    echo '  }'
    echo ""
else
    echo ""
    echo "=========================================="
    echo "  IMPLEMENTATION FAILED"
    echo "=========================================="
    echo ""
    echo "Please check the error messages above."
    exit 1
fi
