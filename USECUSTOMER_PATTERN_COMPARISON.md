# Test Pattern Comparison: useCustomer vs useWarehouse

## Overview
This document shows how `useCustomer.test.ts` follows the exact same pattern as `useWarehouse.test.ts`.

## File Structure Comparison

### useWarehouse.test.ts
```
├─ describe('useWarehouse')
│  ├─ beforeEach() - Reset mocks
│  ├─ describe('getAllWarehouses')
│  │  ├─ fetch all warehouses successfully
│  │  └─ handle empty warehouse list
│  ├─ describe('getWarehouseById')
│  │  └─ fetch a warehouse by id successfully
│  ├─ describe('createWarehouse')
│  │  └─ create a new warehouse successfully
│  ├─ describe('updateWarehouse')
│  │  └─ update an existing warehouse successfully
│  ├─ describe('deleteWarehouse')
│  │  └─ delete a warehouse successfully
│  └─ describe('error handling')
│     ├─ handle API errors when fetching
│     └─ handle API errors when creating
└─ TOTAL: 8 tests
```

### useCustomer.test.ts (NEW)
```
├─ describe('useCustomer')
│  ├─ beforeEach() - Reset mocks
│  ├─ describe('getAllCustomers')
│  │  ├─ fetch all customers successfully
│  │  ├─ fetch with searchTerm filter
│  │  ├─ fetch with multiple filters
│  │  ├─ fetch with email and phone filters
│  │  ├─ fetch with taxId filter
│  │  ├─ fetch inactive customers
│  │  └─ handle empty customer list
│  ├─ describe('getCustomerById')
│  │  └─ fetch a customer by id successfully
│  ├─ describe('createCustomer')
│  │  ├─ create with all fields
│  │  └─ create with minimal fields
│  ├─ describe('updateCustomer')
│  │  ├─ update an existing customer successfully
│  │  └─ update customer status to inactive
│  ├─ describe('deleteCustomer')
│  │  ├─ delete a customer successfully
│  │  └─ delete by different id
│  └─ describe('error handling')
│     ├─ handle API errors when fetching
│     ├─ handle API errors when creating
│     ├─ handle API errors when updating
│     └─ handle API errors when deleting
└─ TOTAL: 18 tests
```

## Code Pattern Comparison

### 1. Imports (Identical Pattern)

**useWarehouse.test.ts**
```typescript
import { beforeEach, describe, expect, it } from 'vitest'
import { mockApiFetch } from '../setup'
import { useWarehouse } from '~/composables/useWarehouse'
import type { Warehouse } from '~/types/inventory'
```

**useCustomer.test.ts**
```typescript
import { beforeEach, describe, expect, it } from 'vitest'
import { mockApiFetch } from '../setup'
import { useCustomer } from '~/composables/useCustomer'
import type { Customer } from '~/types/billing'
```

### 2. Setup (Identical Pattern)

**Both Files**
```typescript
describe('useXxx', () => {
  beforeEach(() => {
    // Reset all mocks before each test
    mockApiFetch.mockReset()
  })
  // ... tests
})
```

### 3. GetAll Test (Same Pattern)

**useWarehouse.test.ts**
```typescript
it('should fetch all warehouses successfully', async () => {
  const mockWarehouses: Warehouse[] = [/* mock data */]
  
  mockApiFetch.mockResolvedValue({ data: mockWarehouses, success: true })
  
  const { getAllWarehouses } = useWarehouse()
  const result = await getAllWarehouses()
  
  expect(mockApiFetch).toHaveBeenCalledWith('/warehouses', {
    method: 'GET',
  })
  expect(result).toEqual(mockWarehouses)
  expect(result).toHaveLength(2)
})
```

**useCustomer.test.ts**
```typescript
it('should fetch all customers successfully', async () => {
  const mockCustomers: Customer[] = [/* mock data */]
  
  mockApiFetch.mockResolvedValue({ data: mockCustomers, success: true })
  
  const { getAllCustomers } = useCustomer()
  const result = await getAllCustomers()
  
  expect(mockApiFetch).toHaveBeenCalledWith('/customers', {
    method: 'GET',
  })
  expect(result).toEqual(mockCustomers)
  expect(result).toHaveLength(2)
})
```

### 4. GetById Test (Same Pattern)

**useWarehouse.test.ts**
```typescript
it('should fetch a warehouse by id successfully', async () => {
  const mockWarehouse: Warehouse = {/* mock data */}
  
  mockApiFetch.mockResolvedValue({ data: mockWarehouse, success: true })
  
  const { getWarehouseById } = useWarehouse()
  const result = await getWarehouseById('1')
  
  expect(mockApiFetch).toHaveBeenCalledWith('/warehouses/1', {
    method: 'GET',
  })
  expect(result).toEqual(mockWarehouse)
  expect(result.id).toBe('1')
  expect(result.name).toBe('Main Warehouse')
})
```

**useCustomer.test.ts**
```typescript
it('should fetch a customer by id successfully', async () => {
  const mockCustomer: Customer = {/* mock data */}
  
  mockApiFetch.mockResolvedValue({ data: mockCustomer, success: true })
  
  const { getCustomerById } = useCustomer()
  const result = await getCustomerById('1')
  
  expect(mockApiFetch).toHaveBeenCalledWith('/customers/1', {
    method: 'GET',
  })
  expect(result).toEqual(mockCustomer)
  expect(result.id).toBe('1')
  expect(result.name).toBe('Acme Corporation')
})
```

### 5. Create Test (Same Pattern)

**useWarehouse.test.ts**
```typescript
it('should create a new warehouse successfully', async () => {
  const newWarehouseData = {/* create data */}
  const mockCreatedWarehouse: Warehouse = {
    id: '3',
    tenantId: 'tenant-1',
    ...newWarehouseData,
    createdAt: '2024-01-03T00:00:00Z',
    updatedAt: '2024-01-03T00:00:00Z',
  }
  
  mockApiFetch.mockResolvedValue({ data: mockCreatedWarehouse, success: true })
  
  const { createWarehouse } = useWarehouse()
  const result = await createWarehouse(newWarehouseData)
  
  expect(mockApiFetch).toHaveBeenCalledWith('/warehouses', {
    method: 'POST',
    body: newWarehouseData,
  })
  expect(result).toEqual(mockCreatedWarehouse)
})
```

**useCustomer.test.ts**
```typescript
it('should create a new customer successfully', async () => {
  const newCustomerData = {/* create data */}
  const mockCreatedCustomer: Customer = {
    id: '3',
    tenantId: 'tenant-1',
    ...newCustomerData,
    createdAt: '2024-01-03T00:00:00Z',
    updatedAt: '2024-01-03T00:00:00Z',
  }
  
  mockApiFetch.mockResolvedValue({ data: mockCreatedCustomer, success: true })
  
  const { createCustomer } = useCustomer()
  const result = await createCustomer(newCustomerData)
  
  expect(mockApiFetch).toHaveBeenCalledWith('/customers', {
    method: 'POST',
    body: newCustomerData,
  })
  expect(result).toEqual(mockCreatedCustomer)
})
```

### 6. Update Test (Same Pattern)

**useWarehouse.test.ts**
```typescript
it('should update an existing warehouse successfully', async () => {
  const updateData = { id: '1', /* update data */ }
  const mockUpdatedWarehouse: Warehouse = {
    ...updateData,
    tenantId: 'tenant-1',
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-05T00:00:00Z',
  }
  
  mockApiFetch.mockResolvedValue({ data: mockUpdatedWarehouse, success: true })
  
  const { updateWarehouse } = useWarehouse()
  const result = await updateWarehouse(updateData)
  
  expect(mockApiFetch).toHaveBeenCalledWith('/warehouses/1', {
    method: 'PUT',
    body: updateData,
  })
  expect(result).toEqual(mockUpdatedWarehouse)
})
```

**useCustomer.test.ts**
```typescript
it('should update an existing customer successfully', async () => {
  const updateData = { id: '1', /* update data */ }
  const mockUpdatedCustomer: Customer = {
    ...updateData,
    tenantId: 'tenant-1',
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-05T00:00:00Z',
  }
  
  mockApiFetch.mockResolvedValue({ data: mockUpdatedCustomer, success: true })
  
  const { updateCustomer } = useCustomer()
  const result = await updateCustomer(updateData)
  
  expect(mockApiFetch).toHaveBeenCalledWith('/customers/1', {
    method: 'PUT',
    body: updateData,
  })
  expect(result).toEqual(mockUpdatedCustomer)
})
```

### 7. Delete Test (Same Pattern)

**useWarehouse.test.ts**
```typescript
it('should delete a warehouse successfully', async () => {
  mockApiFetch.mockResolvedValue(undefined)
  
  const { deleteWarehouse } = useWarehouse()
  await deleteWarehouse('1')
  
  expect(mockApiFetch).toHaveBeenCalledWith('/warehouses/1', {
    method: 'DELETE',
  })
})
```

**useCustomer.test.ts**
```typescript
it('should delete a customer successfully', async () => {
  mockApiFetch.mockResolvedValue(undefined)
  
  const { deleteCustomer } = useCustomer()
  await deleteCustomer('1')
  
  expect(mockApiFetch).toHaveBeenCalledWith('/customers/1', {
    method: 'DELETE',
  })
})
```

### 8. Error Handling (Same Pattern)

**useWarehouse.test.ts**
```typescript
describe('error handling', () => {
  it('should handle API errors when fetching warehouses', async () => {
    const mockError = new Error('Network error')
    mockApiFetch.mockRejectedValue(mockError)
    
    const { getAllWarehouses } = useWarehouse()
    
    await expect(getAllWarehouses()).rejects.toThrow('Network error')
  })
  
  it('should handle API errors when creating warehouse', async () => {
    const mockError = new Error('Validation error')
    mockApiFetch.mockRejectedValue(mockError)
    
    const { createWarehouse } = useWarehouse()
    
    await expect(createWarehouse(data)).rejects.toThrow('Validation error')
  })
})
```

**useCustomer.test.ts**
```typescript
describe('error handling', () => {
  it('should handle API errors when fetching customers', async () => {
    const mockError = new Error('Network error')
    mockApiFetch.mockRejectedValue(mockError)
    
    const { getAllCustomers } = useCustomer()
    
    await expect(getAllCustomers()).rejects.toThrow('Network error')
  })
  
  it('should handle API errors when creating customer', async () => {
    const mockError = new Error('Validation error: email already exists')
    mockApiFetch.mockRejectedValue(mockError)
    
    const { createCustomer } = useCustomer()
    
    await expect(createCustomer(data)).rejects.toThrow('Validation error')
  })
})
```

## Enhanced Coverage in useCustomer

While following the same pattern, useCustomer includes **additional tests** for:

### Filter Tests (7 variations)
```typescript
// useCustomer has comprehensive filter testing
✅ searchTerm filter
✅ multiple filters (name, city, country, isActive)
✅ email and phone filters
✅ taxId filter
✅ isActive=false filter
✅ query string building with URL encoding
```

### Additional CRUD Variations
```typescript
// useCustomer has more edge cases
✅ Create with minimal data (only required fields)
✅ Update status to inactive
✅ Delete by different IDs
✅ Error handling for all CRUD operations (4 tests)
```

## Code Quality Checklist

Both test files follow these standards:

- ✅ Single quotes (ESLint)
- ✅ No semicolons (ESLint)
- ✅ Trailing commas (ESLint)
- ✅ 2-space indentation
- ✅ Mock reset in beforeEach
- ✅ Clear describe blocks
- ✅ Descriptive test names with "should..."
- ✅ Proper TypeScript typing
- ✅ Comprehensive assertions
- ✅ Error scenario testing

## Test Results Comparison

| File | Tests | Status | Coverage |
|------|-------|--------|----------|
| useWarehouse.test.ts | 8 | ✅ | Basic CRUD + Errors |
| useCustomer.test.ts | 18 | ✅ | CRUD + All Filters + Errors |

## Summary

The `useCustomer.test.ts` file:
1. **Follows the exact same pattern** as `useWarehouse.test.ts`
2. **Extends the pattern** with comprehensive filter testing
3. **Maintains consistency** with code style and conventions
4. **Provides enhanced coverage** with 18 tests vs 8 tests
5. **All tests pass** with no breaking changes

This demonstrates a scalable testing pattern that can be applied to other composables in the project.
