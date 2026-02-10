# Quick Reference: Stock Update Functionality

## TL;DR - What You Need to Know

### ❌ Don't Do This
```javascript
// Editing a product to change stock - THIS WON'T WORK
PUT /api/v1/products/{id}
{
  "currentStockLevel": 150  // ❌ This field is ignored!
}
```

### ✅ Do This Instead
```javascript
// Use stock movements to change inventory
POST /api/v1/stock-movements
{
  "productId": "product-guid",
  "warehouseId": "warehouse-guid",
  "movementType": 1,  // 1=Purchase, 2=Sale, 3=Transfer, 4=Adjustment
  "quantity": 50,
  "notes": "Reason for change"
}
```

---

## Quick Start

### Creating a Product with Initial Stock

**Frontend**: Use the product creation form
1. Fill in product details
2. Enter **Initial Quantity** (e.g., 100)
3. Select **Initial Warehouse** from dropdown
4. Save

**API**:
```json
POST /api/v1/products
{
  "name": "New Product",
  "code": "PROD-001",
  "sku": "SKU-001",
  "unitPrice": 100,
  "costPrice": 75,
  "minimumStockLevel": 10,
  "initialQuantity": 100,
  "initialWarehouseId": "warehouse-guid"
}
```

### Updating Inventory

**Frontend**: Navigate to Inventory → Stock Movements → New

**API**:
```json
POST /api/v1/stock-movements
{
  "productId": "product-guid",
  "warehouseId": "warehouse-guid",
  "movementType": 1,
  "quantity": 50,
  "unitCost": 25.00,
  "reference": "PO-12345",
  "notes": "Stock delivery"
}
```

### Movement Types

| Type | Code | Use Case | Effect |
|------|------|----------|--------|
| **InitialInventory** | 0 | First-time stock setup | Adds stock |
| **Purchase** | 1 | Receiving from supplier | Adds stock |
| **Sale** | 2 | Selling to customer | Removes stock |
| **Transfer** | 3 | Moving between warehouses | Removes from source, adds to destination |
| **Adjustment** | 4 | Manual corrections | Adds or removes |
| **Return** | 5 | Customer returns | Adds stock |

---

## Common Scenarios

### Scenario 1: Receive Stock from Supplier
```json
{
  "movementType": 1,
  "quantity": 100,
  "reference": "PO-2024-001",
  "notes": "ACME Corp delivery"
}
```

### Scenario 2: Sell to Customer
```json
{
  "movementType": 2,
  "quantity": -5,
  "reference": "INV-2024-123",
  "notes": "Customer order"
}
```

### Scenario 3: Transfer Between Warehouses
```json
{
  "movementType": 3,
  "warehouseId": "source-warehouse-guid",
  "destinationWarehouseId": "dest-warehouse-guid",
  "quantity": 20,
  "notes": "Rebalancing stock"
}
```

### Scenario 4: Stock Adjustment (Damage/Loss)
```json
{
  "movementType": 4,
  "quantity": -3,
  "notes": "Damaged during inspection"
}
```

---

## What Changed

### Product Edit Page
- ✅ Stock level field is now **disabled** (read-only)
- ✅ Shows hint: "Stock level is calculated from warehouse inventory"
- ✅ Clear message to use Stock Movements

### Product Create Page
- ❌ Removed: `currentStockLevel` field
- ✅ Added: `initialQuantity` field (optional)
- ✅ Added: `initialWarehouse` dropdown (required if quantity is set)
- ✅ Helpful hints and validation

### Backend API
- ❌ `UpdateProductCommand.currentStockLevel` removed
- ✅ Stock automatically calculated from warehouse inventory
- ✅ Better validation for initial stock setup

---

## Architecture

```
┌─────────────────────┐
│  Stock Movement     │
│  (Create/Update)    │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│  StockLevelService  │
│  (Auto-processes)   │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│ WarehouseInventory  │ ◄── Single Source of Truth
│  (Updated per WH)   │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│   Product Query     │
│ (Calculates Total)  │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│ CurrentStockLevel   │
│   (Read-Only)       │
└─────────────────────┘
```

---

## Documentation

For detailed information:

- **[Stock Management Guide](./STOCK_MANAGEMENT_GUIDE.md)** - Complete guide (11KB)
  - How the system works
  - API examples
  - Common scenarios
  - Troubleshooting

- **[Implementation Summary](./STOCK_UPDATE_IMPLEMENTATION_SUMMARY.md)** - Technical details (10KB)
  - All changes made
  - Migration guide
  - User impact

---

## FAQ

**Q: Can I edit stock level when editing a product?**  
A: No, the field is disabled. Use Stock Movements instead.

**Q: How do I set initial stock for a new product?**  
A: Use `initialQuantity` and `initialWarehouse` when creating the product.

**Q: What if I need to correct stock levels?**  
A: Create an Adjustment movement (type 4) with positive or negative quantity.

**Q: How do I transfer stock between warehouses?**  
A: Create a Transfer movement (type 3) with both source and destination warehouses.

**Q: Where can I see stock per warehouse?**  
A: Query `/api/v1/warehouse-inventory?productId={id}`

**Q: Is this backward compatible?**  
A: Yes! Existing code continues to work. Deprecated fields are clearly marked.

---

## Support

If you encounter issues:
1. Check the [Stock Management Guide](./STOCK_MANAGEMENT_GUIDE.md)
2. Review the [Implementation Summary](./STOCK_UPDATE_IMPLEMENTATION_SUMMARY.md)
3. Check backend logs for errors
4. Verify warehouse inventory is updating

---

**Last Updated**: February 9, 2026  
**Status**: ✅ Production Ready  
**Version**: 1.0
