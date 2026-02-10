# usePermissions & useDataLoader Tests - Quick Reference

## Test Files
- **usePermissions**: `frontend/tests/composables/usePermissions.test.ts` (16 tests)
- **useDataLoader**: `frontend/tests/composables/useDataLoader.test.ts` (23 tests)

## Run Tests
```bash
cd frontend
npm test tests/composables/usePermissions.test.ts tests/composables/useDataLoader.test.ts
```

## Test Stats
- **Total Tests**: 39
- **All Passing**: ✅ 39/39
- **Duration**: ~620ms

## usePermissions Test Structure

### Tests by Category
```
hasPermission (2 tests)
├── ✅ should call authStore with correct permission
└── ✅ should return false when no permission

hasAnyPermission (2 tests)
├── ✅ should call authStore with permissions array
└── ✅ should return false when has none

hasAllPermissions (2 tests)
├── ✅ should call authStore with permissions array
└── ✅ should return false when missing permissions

can.viewWarehouses (2 tests)
can.createProduct (2 tests)
can.editCustomer (2 tests)
can.deleteStock (2 tests)
can.manageRoles (2 tests)
```

### Key Mocks
```typescript
mockHasPermission = vi.fn()
mockHasAnyPermission = vi.fn()
mockHasAllPermissions = vi.fn()
```

## useDataLoader Test Structure

### Tests by Category
```
initialization (1 test)
├── ✅ initial state null/false/null

load - successful (3 tests)
├── ✅ loads data and updates state
├── ✅ loads complex objects
└── ✅ no toast by default

load - errors (4 tests)
├── ✅ handles errors and updates state
├── ✅ converts non-Error to Error
├── ✅ uses custom error message
└── ✅ respects showToast: false

onSuccess callback (2 tests)
├── ✅ calls on success
└── ✅ doesn't call on error

onError callback (2 tests)
├── ✅ calls with error object
└── ✅ doesn't call on success

reload function (4 tests)
├── ✅ reloads with last loader
├── ✅ preserves options
├── ✅ does nothing if no loader
└── ✅ updates data on reload

showToast option (3 tests)
├── ✅ shows by default on error
├── ✅ shows when true
└── ✅ hides when false

state reactivity (2 tests)
├── ✅ clears error on new success
└── ✅ sets loading false after both

integration (2 tests)
├── ✅ complete success workflow
└── ✅ complete error workflow
```

### Key Mocks
```typescript
mockShowError = vi.fn()
mockShowSuccess = vi.fn()
mockT = vi.fn()
```

## Common Test Patterns

### Testing Composable Return Values
```typescript
const { hasPermission } = usePermissions()
const result = hasPermission('warehouses.read')
expect(result).toBe(true)
```

### Testing Async Data Loading
```typescript
const { data, loading, load } = useDataLoader<string>()
await load(() => Promise.resolve('test'))
expect(data.value).toBe('test')
```

### Verifying Mock Calls
```typescript
expect(mockFunction).toHaveBeenCalledTimes(1)
expect(mockFunction).toHaveBeenCalledWith('expected-arg')
```

### Testing Error Handling
```typescript
const mockLoader = vi.fn().mockRejectedValue(new Error('test'))
await load(mockLoader, { showToast: false })
expect(error.value).toBeInstanceOf(Error)
```

### Testing Callbacks
```typescript
const mockCallback = vi.fn()
await load(mockLoader, { onSuccess: mockCallback })
expect(mockCallback).toHaveBeenCalled()
```

## Modified Files
1. ✅ Created: `frontend/tests/composables/usePermissions.test.ts`
2. ✅ Created: `frontend/tests/composables/useDataLoader.test.ts`
3. ✅ Updated: `frontend/tests/setup.ts` (added `globalThis.ref`)

## Success Criteria
- [x] 16 tests for usePermissions
- [x] 23 tests for useDataLoader
- [x] All tests passing
- [x] Proper mocking of dependencies
- [x] Coverage of success and error paths
- [x] Testing of callbacks and options
- [x] State reactivity verification
- [x] Integration scenarios

## Key Features Tested

### usePermissions
- Permission checking delegation
- Multiple permission checks
- Convenience helper methods
- Mock verification

### useDataLoader
- Data loading lifecycle
- Loading state management
- Error handling and conversion
- Success/Error callbacks
- Toast notifications
- Reload functionality
- State reactivity
- Integration workflows

---
**Status**: ✅ All tests implemented and passing
**Total**: 39 tests (16 + 23)
**Duration**: ~620ms
