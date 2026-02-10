# Excel and CSV Export Feature

## Overview

The SaaS Billing + Inventory system now supports exporting data to Excel (.xlsx) and CSV formats for stock summaries and stock movements.

## Features

✅ **Export Formats**
- Excel (XLSX) - Uses ClosedXML library
- CSV - RFC 4180 compliant

✅ **Export Types**
1. **Stock Movements** - Detailed transaction history with filters
2. **Warehouse Stock Summary** - Current inventory levels per warehouse

✅ **Advanced Filtering**
- Filter by product brand
- Filter by product category
- Filter by warehouse
- Filter by date range (from/to)

## API Endpoints

### 1. Export Stock Movements

Export detailed stock movement transactions with optional filters.

**Endpoint:**
```
GET /api/v1/stock-movements/export
```

**Headers:**
```
Authorization: Bearer {your-jwt-token}
X-Tenant-Id: {your-tenant-id}
```

**Query Parameters:**
| Parameter | Type | Required | Description | Example |
|-----------|------|----------|-------------|---------|
| `format` | string | No | Export format: `csv` or `excel` (default: excel) | `format=csv` |
| `brand` | string | No | Filter by product brand | `brand=Nike` |
| `category` | string | No | Filter by product category | `category=Electronics` |
| `warehouseId` | uuid | No | Filter by warehouse ID | `warehouseId=123e4567-...` |
| `fromDate` | datetime | No | Start date for movements (yyyy-MM-dd) | `fromDate=2026-01-01` |
| `toDate` | datetime | No | End date for movements (yyyy-MM-dd) | `toDate=2026-02-10` |

**Example Requests:**

```bash
# Export all stock movements as Excel
curl -X GET "http://localhost:5000/api/v1/stock-movements/export?format=excel" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "X-Tenant-Id: YOUR_TENANT_ID" \
  -o stock-movements.xlsx

# Export as CSV with filters
curl -X GET "http://localhost:5000/api/v1/stock-movements/export?format=csv&brand=Nike&category=Shoes" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "X-Tenant-Id: YOUR_TENANT_ID" \
  -o stock-movements.csv

# Export movements within date range
curl -X GET "http://localhost:5000/api/v1/stock-movements/export?format=excel&fromDate=2026-01-01&toDate=2026-02-10" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "X-Tenant-Id: YOUR_TENANT_ID" \
  -o stock-movements-january.xlsx
```

**Response:**
- Content-Type: `text/csv` or `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`
- Content-Disposition: `attachment; filename=stock-movements-{timestamp}.{csv|xlsx}`

**Exported Columns:**
- MovementDate
- MovementType
- ProductCode
- ProductName
- Brand
- Category
- WarehouseCode
- WarehouseName
- DestinationWarehouseCode (for transfers)
- DestinationWarehouseName (for transfers)
- Quantity
- UnitCost
- TotalCost
- Reference
- Notes

---

### 2. Export Warehouse Stock Summary

Export current stock levels aggregated by warehouse and product.

**Endpoint:**
```
GET /api/v1/warehouses/export/stock-summary
```

**Headers:**
```
Authorization: Bearer {your-jwt-token}
X-Tenant-Id: {your-tenant-id}
```

**Query Parameters:**
| Parameter | Type | Required | Description | Example |
|-----------|------|----------|-------------|---------|
| `format` | string | No | Export format: `csv` or `excel` (default: excel) | `format=csv` |

**Required Permission:** `warehouses.read`

**Example Requests:**

```bash
# Export warehouse stock summary as Excel
curl -X GET "http://localhost:5000/api/v1/warehouses/export/stock-summary?format=excel" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "X-Tenant-Id: YOUR_TENANT_ID" \
  -o warehouse-stock-summary.xlsx

# Export as CSV
curl -X GET "http://localhost:5000/api/v1/warehouses/export/stock-summary?format=csv" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "X-Tenant-Id: YOUR_TENANT_ID" \
  -o warehouse-stock-summary.csv
```

**Response:**
- Content-Type: `text/csv` or `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`
- Content-Disposition: `attachment; filename=warehouse-stock-summary-{timestamp}.{csv|xlsx}`

**Exported Columns:**
- WarehouseCode
- WarehouseName
- ProductCode
- ProductName
- Brand
- Category
- Quantity
- ReservedQuantity
- AvailableQuantity
- LastMovementDate

---

## Using the Exports in Your Application

### JavaScript/TypeScript Example

```typescript
async function exportStockMovements(filters: {
  format?: 'csv' | 'excel',
  brand?: string,
  category?: string,
  warehouseId?: string,
  fromDate?: string,
  toDate?: string
}) {
  const params = new URLSearchParams();
  Object.entries(filters).forEach(([key, value]) => {
    if (value) params.append(key, value);
  });

  const response = await fetch(
    `${API_BASE_URL}/stock-movements/export?${params}`,
    {
      headers: {
        'Authorization': `Bearer ${token}`,
        'X-Tenant-Id': tenantId
      }
    }
  );

  const blob = await response.blob();
  const url = window.URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = `stock-movements-${new Date().toISOString()}.${filters.format || 'xlsx'}`;
  a.click();
}

// Usage
exportStockMovements({
  format: 'excel',
  brand: 'Nike',
  fromDate: '2026-01-01',
  toDate: '2026-02-10'
});
```

### Python Example

```python
import requests

def export_warehouse_stock_summary(token, tenant_id, format='excel'):
    headers = {
        'Authorization': f'Bearer {token}',
        'X-Tenant-Id': tenant_id
    }
    
    response = requests.get(
        f'http://localhost:5000/api/v1/warehouses/export/stock-summary',
        params={'format': format},
        headers=headers
    )
    
    if response.status_code == 200:
        filename = f'warehouse-stock-summary.{format if format == "csv" else "xlsx"}'
        with open(filename, 'wb') as f:
            f.write(response.content)
        print(f'Exported to {filename}')
    else:
        print(f'Error: {response.status_code}')

# Usage
export_warehouse_stock_summary(token, tenant_id, format='csv')
```

---

## Implementation Details

### Backend Architecture

**Libraries Used:**
- **ClosedXML 0.102.2**: Excel file generation
- **Built-in CSV**: Custom CSV export with RFC 4180 compliance

**Key Components:**

1. **ExportService** (`Application/Services/ExportService.cs`)
   - Generic export functionality for any data type
   - Excel generation with auto-fit columns
   - CSV export with proper character escaping

2. **Query Handlers:**
   - `GetWarehouseStockSummaryQueryHandler`: Aggregates inventory data
   - `GetStockMovementsForExportQueryHandler`: Filters and retrieves movements

3. **DTOs:**
   - `WarehouseStockSummaryDto`: Flattened warehouse inventory data
   - `StockMovementExportDto`: Complete movement details

4. **Repository Methods:**
   - `GetAllWithDetailsForTenantAsync`: Warehouse inventory with relationships
   - `GetForExportAsync`: Stock movements with filtering logic

### Multi-Tenancy

All exports are automatically scoped to the authenticated user's tenant. The `X-Tenant-Id` header is required and validated against the user's JWT token.

### Performance Considerations

- Queries use `AsNoTracking()` for read-only operations
- Includes are optimized to only load required relationships
- Date range filters should be used for large datasets

---

## Testing

### Manual Testing with cURL

```bash
# 1. Login to get token
TOKEN=$(curl -s -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "user@example.com", "password": "YourPassword"}' \
  | jq -r '.data.accessToken')

TENANT_ID=$(curl -s -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "user@example.com", "password": "YourPassword"}' \
  | jq -r '.data.tenants[0].id')

# 2. Test CSV export
curl -X GET "http://localhost:5000/api/v1/stock-movements/export?format=csv" \
  -H "Authorization: Bearer $TOKEN" \
  -H "X-Tenant-Id: $TENANT_ID" \
  -o test-export.csv

# 3. Test Excel export with filters
curl -X GET "http://localhost:5000/api/v1/stock-movements/export?format=excel&brand=Nike" \
  -H "Authorization: Bearer $TOKEN" \
  -H "X-Tenant-Id: $TENANT_ID" \
  -o test-export.xlsx

# 4. Verify file
file test-export.xlsx  # Should show "Microsoft Excel 2007+"
```

---

## Troubleshooting

### 403 Forbidden Error
- **Cause**: User lacks required permission
- **Solution**: Ensure user has `warehouses.read` permission for stock summary exports

### 400 Bad Request - "X-Tenant-Id header is required"
- **Cause**: Missing tenant header
- **Solution**: Include `X-Tenant-Id` header in request

### Empty Export
- **Cause**: No data matches filters or tenant has no data
- **Solution**: Check filters and ensure data exists in database

### Excel File Corrupted
- **Cause**: Attempting to read binary Excel as text
- **Solution**: Save response as binary (`wb` mode in Python, `blob` in JavaScript)

---

## Future Enhancements

Potential additions for future versions:

- [ ] Batch exports with pagination for large datasets
- [ ] Scheduled exports with email delivery
- [ ] Custom column selection
- [ ] Additional file formats (PDF, JSON)
- [ ] Export templates with branding
- [ ] Compressed exports (ZIP) for large files

---

## License

This feature uses ClosedXML which is licensed under the MIT License.

## Support

For issues or questions about the export functionality:
1. Check Swagger documentation at `/swagger`
2. Review API logs for detailed error messages
3. Verify permissions and tenant configuration
