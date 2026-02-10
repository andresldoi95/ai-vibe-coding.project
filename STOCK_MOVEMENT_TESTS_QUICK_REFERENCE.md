# Stock Movement Tests - Quick Reference

## 📁 File Location
```
/frontend/tests/composables/useStockMovement.test.ts
```

## 🎯 Test Count: 18 Tests

### CRUD Operations (7 tests)
1. ✅ `getAllStockMovements` - Fetch all successfully
2. ✅ `getAllStockMovements` - Handle empty list
3. ✅ `getStockMovementById` - Fetch by ID successfully
4. ✅ `createStockMovement` - Create new movement
5. ✅ `createStockMovement` - Create transfer with destination
6. ✅ `updateStockMovement` - Update existing movement
7. ✅ `deleteStockMovement` - Delete movement

### Export Functionality (7 tests)
8. ✅ `exportStockMovements` - Export with default format
9. ✅ `exportStockMovements` - Export with CSV format
10. ✅ `exportStockMovements` - Export with all filters
11. ✅ `exportStockMovements` - Error: No tenant selected
12. ✅ `exportStockMovements` - Error: Not authenticated
13. ✅ `exportStockMovements` - Error: Export fails
14. ✅ `exportStockMovements` - Default filename when header missing

### Error Handling (4 tests)
15. ✅ Error handling - Fetching stock movements
16. ✅ Error handling - Creating stock movement
17. ✅ Error handling - Updating stock movement
18. ✅ Error handling - Deleting stock movement

## 🧪 Running Tests

### Run Stock Movement Tests Only
```bash
npm test -- tests/composables/useStockMovement.test.ts
```

### Run All Composable Tests
```bash
npm test -- tests/composables/
```

### Run with Verbose Output
```bash
npm test -- tests/composables/ --reporter=verbose
```

### Run with Coverage
```bash
npm test -- --coverage tests/composables/useStockMovement.test.ts
```

## 🔧 Mocked Dependencies

### From setup.ts
- `mockApiFetch` - API call mocking
- `mockAuthStore` - { token: string }
- `mockTenantStore` - { currentTenantId: string }
- `mockRuntimeConfig` - { public: { apiBase: string } }

### Export Tests Mocks
- `global.fetch` - HTTP fetch API
- `global.URL` - URL.createObjectURL/revokeObjectURL
- `global.document` - document.createElement/body methods

## 📦 Test Data Examples

### Stock Movement Types
```typescript
MovementType.InitialInventory = 0
MovementType.Purchase = 1
MovementType.Sale = 2
MovementType.Transfer = 3
MovementType.Adjustment = 4
MovementType.Return = 5
```

### Sample Test Data
```typescript
{
  id: '1',
  tenantId: 'tenant-123',
  productId: 'product-1',
  warehouseId: 'warehouse-1',
  destinationWarehouseId?: 'warehouse-2', // For transfers
  movementType: MovementType.Purchase,
  quantity: 100,
  unitCost: 10.5,
  totalCost: 1050,
  reference: 'PO-001',
  notes: 'Initial purchase',
  movementDate: '2024-01-01T00:00:00Z',
  createdAt: '2024-01-01T00:00:00Z',
  updatedAt: '2024-01-01T00:00:00Z',
}
```

## 🎨 Test Pattern Example

```typescript
describe('useStockMovement', () => {
  beforeEach(() => {
    mockApiFetch.mockReset()
    mockAuthStore.token = 'mock-token-123'
    mockTenantStore.currentTenantId = 'tenant-123'
  })

  it('should fetch all stock movements successfully', async () => {
    const mockData: StockMovement[] = [/* ... */]
    mockApiFetch.mockResolvedValue({ data: mockData, success: true })

    const { getAllStockMovements } = useStockMovement()
    const result = await getAllStockMovements()

    expect(mockApiFetch).toHaveBeenCalledWith('/stock-movements', {
      method: 'GET',
    })
    expect(result).toEqual(mockData)
  })
})
```

## ✅ Code Quality Checklist

- ✅ Single quotes (not double quotes)
- ✅ No semicolons
- ✅ Trailing commas
- ✅ TypeScript types from ~/types/inventory
- ✅ Mock reset in beforeEach
- ✅ Follows existing patterns
- ✅ All async functions use await
- ✅ All expect assertions present

## 📊 Test Results
```
✓ tests/composables/useStockMovement.test.ts (18 tests) 18ms
  ✓ getAllStockMovements (2)
  ✓ getStockMovementById (1)
  ✓ createStockMovement (2)
  ✓ updateStockMovement (1)
  ✓ deleteStockMovement (1)
  ✓ exportStockMovements (7)
  ✓ error handling (4)

Test Files  1 passed (1)
Tests  18 passed (18)
```

## 🚀 Next Steps

### Recommended Additions
- [ ] Integration tests with real API
- [ ] Performance tests with large datasets
- [ ] Snapshot testing for complex objects
- [ ] E2E tests for complete workflows

### Maintenance
- Update tests when composable API changes
- Add tests for new features
- Keep mocks synchronized with actual implementations
- Review and update test data periodically

## 📚 Related Files

- **Composable**: `/frontend/composables/useStockMovement.ts`
- **Types**: `/frontend/types/inventory.ts`
- **Setup**: `/frontend/tests/setup.ts`
- **Reference**: `/frontend/tests/composables/useWarehouse.test.ts`

## 💡 Tips

1. **Mock Cleanup**: Always reset mocks in `beforeEach`
2. **Export Tests**: Remember to mock all browser APIs (fetch, URL, document)
3. **Error Tests**: Use `expect().rejects.toThrow()` for async errors
4. **Type Safety**: Import types from `~/types/inventory`
5. **Assertions**: Always verify both the call and the result

---

**Status**: ✅ All 18 tests passing
**Coverage**: 100% of composable functions
**Quality**: Production-ready
