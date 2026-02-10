# useWarehouseInventory Tests - Quick Reference

## Run Tests

```bash
# Run only useWarehouseInventory tests
npm test -- tests/composables/useWarehouseInventory.test.ts

# Run all composable tests
npm test -- tests/composables/

# Run with coverage
npm test -- --coverage tests/composables/useWarehouseInventory.test.ts
```

## Test File Location
```
/frontend/tests/composables/useWarehouseInventory.test.ts
```

## Test Summary
- **Total Tests**: 25
- **Test Suites**: 6 describe blocks
- **Execution Time**: ~11ms
- **Coverage**: 100% of all functions

## Function Test Counts

| Function | Tests | Type |
|----------|-------|------|
| `getProductInventory` | 3 | API Call |
| `getTotalStock` | 4 | Pure Calculation |
| `getTotalAvailable` | 4 | Pure Calculation |
| `getWarehouseStock` | 5 | Find/Filter |
| `isLowStock` | 7 | Boolean Check |
| Integration Tests | 2 | End-to-End |

## Key Test Scenarios

### API Testing
```typescript
// Test: Successful API call
getProductInventory('product-1')
// Expects: Array of InventoryLevel objects

// Test: Empty inventory
getProductInventory('product-999')
// Expects: Empty array

// Test: Error handling
getProductInventory('invalid')
// Expects: Error thrown
```

### Calculation Testing
```typescript
// Test: Total stock calculation
getTotalStock(inventory)
// Input: [100, 50, 30] quantities
// Expects: 180

// Test: Total available calculation
getTotalAvailable(inventory)
// Input: [80, 40, 25] available quantities
// Expects: 145
```

### Find/Filter Testing
```typescript
// Test: Find warehouse
getWarehouseStock(inventory, 'warehouse-2')
// Expects: InventoryLevel for warehouse-2

// Test: Warehouse not found
getWarehouseStock(inventory, 'warehouse-999')
// Expects: undefined
```

### Boolean Check Testing
```typescript
// Test: Low stock detection
isLowStock(inventory, 200)
// Total: 180, Min: 200
// Expects: true

// Test: Adequate stock
isLowStock(inventory, 150)
// Total: 180, Min: 150
// Expects: false
```

## Mock Data Factory

```typescript
const createMockInventory = (overrides?: Partial<InventoryLevel>[])
```

**Default Data**: 3 warehouses with inventory
- Warehouse 1: 100 qty, 80 available
- Warehouse 2: 50 qty, 40 available
- Warehouse 3: 30 qty, 25 available

**Usage Examples**:
```typescript
// Default mock data
const inventory = createMockInventory()

// Custom single warehouse
const inventory = createMockInventory([{
  warehouseId: 'custom-wh',
  quantity: 500,
  availableQuantity: 450,
}])

// Custom multiple warehouses
const inventory = createMockInventory([
  { quantity: 100 },
  { quantity: 200 },
])
```

## Common Test Patterns

### Pattern 1: API Function Test
```typescript
it('should fetch product inventory', async () => {
  const mockInventory = createMockInventory()
  mockApiFetch.mockResolvedValue({ data: mockInventory })

  const { getProductInventory } = useWarehouseInventory()
  const result = await getProductInventory('product-1')

  expect(mockApiFetch).toHaveBeenCalledWith('/products/product-1/inventory')
  expect(result).toEqual(mockInventory)
})
```

### Pattern 2: Pure Function Test
```typescript
it('should calculate total stock', () => {
  const mockInventory = createMockInventory()

  const { getTotalStock } = useWarehouseInventory()
  const result = getTotalStock(mockInventory)

  expect(result).toBe(180)
})
```

### Pattern 3: Edge Case Test
```typescript
it('should handle empty inventory', () => {
  const { getTotalStock } = useWarehouseInventory()
  const result = getTotalStock([])

  expect(result).toBe(0)
})
```

### Pattern 4: Integration Test
```typescript
it('should work with all functions together', async () => {
  const mockInventory = createMockInventory()
  mockApiFetch.mockResolvedValue({ data: mockInventory })

  const { getProductInventory, getTotalStock, isLowStock } = useWarehouseInventory()

  const inventory = await getProductInventory('product-1')
  const total = getTotalStock(inventory)
  const isLow = isLowStock(inventory, 200)

  expect(inventory).toHaveLength(3)
  expect(total).toBe(180)
  expect(isLow).toBe(true)
})
```

## Edge Cases Covered

### Empty Data
- ✅ Empty inventory array
- ✅ Zero quantities
- ✅ No available stock (all reserved)

### Not Found
- ✅ Warehouse not found (returns undefined)
- ✅ Product not found (API error)

### Boundary Conditions
- ✅ Stock equals minimum level
- ✅ Stock below minimum level
- ✅ Minimum level is zero
- ✅ Single warehouse scenario

### Error Handling
- ✅ API network errors
- ✅ Product not found errors

## Mock Configuration

The tests use `$apiFetch` from `useNuxtApp()`:

```typescript
// In tests/setup.ts
const useNuxtApp = vi.fn(() => ({
  $apiFetch: mockApiFetch,
}))

globalThis.useNuxtApp = useNuxtApp
```

**Reset in beforeEach**:
```typescript
beforeEach(() => {
  mockApiFetch.mockReset()
})
```

## Calculation Reference

### Total Stock
```
Sum of quantity field across all warehouses
Example: 100 + 50 + 30 = 180
```

### Total Available
```
Sum of availableQuantity field across all warehouses
Example: 80 + 40 + 25 = 145
```

### Low Stock Check
```
(Total Stock < Minimum Level) = true (low stock)
(Total Stock >= Minimum Level) = false (adequate stock)
```

## Type Reference

```typescript
interface InventoryLevel {
  id: string
  tenantId: string
  productId: string
  productName: string
  productCode: string
  warehouseId: string
  warehouseName: string
  warehouseCode: string
  quantity: number
  reservedQuantity: number
  availableQuantity: number
  lastMovementDate?: string
  createdAt: string
  updatedAt: string
}
```

## Files to Review

1. **Composable**: `/frontend/composables/useWarehouseInventory.ts`
2. **Tests**: `/frontend/tests/composables/useWarehouseInventory.test.ts`
3. **Types**: `/frontend/types/inventory.ts`
4. **Setup**: `/frontend/tests/setup.ts`

## Verification Commands

```bash
# Run tests
npm test -- tests/composables/useWarehouseInventory.test.ts

# Lint tests
npx eslint tests/composables/useWarehouseInventory.test.ts

# Type check
npm run type-check

# Run all composable tests
npm test -- tests/composables/
```

## Test Results

```
✓ tests/composables/useWarehouseInventory.test.ts (25 tests) 11ms

Test Files  1 passed (1)
     Tests  25 passed (25)
```

## Related Tests

- `useProduct.test.ts` - 17 tests
- `useWarehouse.test.ts` - 8 tests
- `useStockMovement.test.ts` - 18 tests
- `useCustomer.test.ts` - 18 tests
- `useWarehouseInventory.test.ts` - 25 tests

**Total**: 86 tests across 5 composables
