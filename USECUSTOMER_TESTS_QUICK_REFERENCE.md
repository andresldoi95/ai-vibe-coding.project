# useCustomer Tests - Quick Reference

## 📋 Test Summary
- **Location**: `/frontend/tests/composables/useCustomer.test.ts`
- **Total Tests**: 18 ✅
- **Coverage**: All CRUD + All Filters + Error Handling

## 🧪 Test Breakdown

### getAllCustomers (7 tests)
```typescript
✅ Fetch all customers
✅ Filter by searchTerm
✅ Filter by multiple fields (name, city, country, isActive)
✅ Filter by email and phone
✅ Filter by taxId
✅ Filter by isActive=false
✅ Handle empty list
```

### getCustomerById (1 test)
```typescript
✅ Fetch by ID
```

### createCustomer (2 tests)
```typescript
✅ Create with full data
✅ Create with minimal data (name, email only)
```

### updateCustomer (2 tests)
```typescript
✅ Update with full data
✅ Update status to inactive
```

### deleteCustomer (2 tests)
```typescript
✅ Delete by ID
✅ Delete different ID
```

### Error Handling (4 tests)
```typescript
✅ Network error on fetch
✅ Validation error on create
✅ Not found error on update
✅ Business rule error on delete
```

## 🔍 Filter Parameters Tested

| Filter | Type | Example |
|--------|------|---------|
| searchTerm | string | "Acme" |
| name | string | "Acme Corporation" |
| email | string | "acme.com" |
| phone | string | "555-0100" |
| taxId | string | "TAX-12345" |
| city | string | "New York" |
| country | string | "USA" |
| isActive | boolean | true / false |

## 🚀 Run Commands

```bash
# Customer tests only
npm test -- tests/composables/useCustomer.test.ts

# All composable tests
npm test -- tests/composables/

# All tests
npm test

# Watch mode
npm test -- --watch

# Coverage
npm test -- --coverage
```

## 📊 Test Results

```
✓ tests/composables/useCustomer.test.ts (18 tests) 12ms

Test Files  1 passed (1)
     Tests  18 passed (18)
  Duration  ~12ms
```

## 🎯 Key Test Patterns

### Basic Test
```typescript
it('should fetch all customers successfully', async () => {
  mockApiFetch.mockResolvedValue({ data: mockCustomers, success: true })
  
  const { getAllCustomers } = useCustomer()
  const result = await getAllCustomers()
  
  expect(mockApiFetch).toHaveBeenCalledWith('/customers', { method: 'GET' })
  expect(result).toEqual(mockCustomers)
})
```

### Filter Test
```typescript
it('should fetch customers with multiple filters', async () => {
  mockApiFetch.mockResolvedValue({ data: mockCustomers, success: true })
  
  const { getAllCustomers } = useCustomer()
  await getAllCustomers({ name: 'Acme', city: 'New York', isActive: true })
  
  expect(mockApiFetch).toHaveBeenCalledWith(
    '/customers?name=Acme&city=New+York&isActive=true',
    { method: 'GET' }
  )
})
```

### Error Test
```typescript
it('should handle API errors when creating customer', async () => {
  mockApiFetch.mockRejectedValue(new Error('Validation error'))
  
  const { createCustomer } = useCustomer()
  
  await expect(createCustomer(data)).rejects.toThrow('Validation error')
})
```

## 📦 Mock Data

### Full Customer
```typescript
{
  id: '1',
  tenantId: 'tenant-1',
  name: 'Acme Corporation',
  email: 'contact@acme.com',
  phone: '+1-555-0100',
  taxId: 'TAX-12345',
  contactPerson: 'John Doe',
  billingStreet: '123 Business Ave',
  billingCity: 'New York',
  billingState: 'NY',
  billingPostalCode: '10001',
  billingCountry: 'USA',
  // ... shipping address
  notes: 'VIP customer',
  website: 'https://acme.com',
  isActive: true,
  createdAt: '2024-01-01T00:00:00Z',
  updatedAt: '2024-01-01T00:00:00Z',
}
```

### Minimal Customer
```typescript
{
  id: '4',
  tenantId: 'tenant-1',
  name: 'Minimal Customer',
  email: 'minimal@customer.com',
  isActive: true,
  createdAt: '2024-01-04T00:00:00Z',
  updatedAt: '2024-01-04T00:00:00Z',
}
```

## ✅ Checklist

- [x] All CRUD operations tested
- [x] All 8 filter parameters tested
- [x] Error handling for all operations
- [x] Edge cases covered
- [x] TypeScript types from ~/types/billing
- [x] ESLint compliant (single quotes, no semicolons, trailing commas)
- [x] Mock setup in beforeEach
- [x] Follows useWarehouse.test.ts pattern
- [x] All tests passing
- [x] No breaking changes

## 🎉 Status: COMPLETE ✅

All tests implemented and passing!
