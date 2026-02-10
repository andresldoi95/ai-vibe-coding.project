# useTheme & useRole Tests - Quick Reference

## ✅ Status: COMPLETE
**All 19 tests passing** (9 useTheme + 10 useRole)

---

## Test Files

### 1. useTheme.test.ts
**Location:** `/frontend/tests/composables/useTheme.test.ts`  
**Tests:** 9  
**Composable:** `/frontend/composables/useTheme.ts`

#### What It Tests
- ✅ isDark computed property (2 tests)
- ✅ preference computed property (2 tests)
- ✅ toggleTheme function (2 tests)
- ✅ setTheme function (3 tests)

#### Mock Strategy
```typescript
const mockColorMode = {
  value: 'light',
  preference: 'light',
}
globalThis.useColorMode = vi.fn(() => mockColorMode)
globalThis.computed = computed
```

---

### 2. useRole.test.ts
**Location:** `/frontend/tests/composables/useRole.test.ts`  
**Tests:** 10  
**Composable:** `/frontend/composables/useRole.ts`

#### What It Tests
- ✅ getAllRoles (2 tests - success + empty)
- ✅ getRoleById (1 test)
- ✅ getAllPermissions (2 tests - success + empty)
- ✅ createRole (1 test)
- ✅ updateRole (1 test)
- ✅ deleteRole (1 test)
- ✅ Error handling (2 tests)

#### Mock Strategy
```typescript
import { mockApiFetch } from '../setup'

beforeEach(() => {
  mockApiFetch.mockReset()
})

mockApiFetch.mockResolvedValue({ data: mockRoles, success: true })
```

---

## Running Tests

### Both Tests
```bash
cd frontend
npm test -- tests/composables/useTheme.test.ts tests/composables/useRole.test.ts
```

### Individual Tests
```bash
# useTheme only
npm test -- tests/composables/useTheme.test.ts

# useRole only
npm test -- tests/composables/useRole.test.ts
```

### Verbose Output
```bash
npm test -- tests/composables/useTheme.test.ts tests/composables/useRole.test.ts --reporter=verbose
```

---

## Test Output

```
✓ tests/composables/useTheme.test.ts (9 tests)
  ✓ useTheme
    ✓ isDark computed property
      ✓ should return true when color mode value is dark
      ✓ should return false when color mode value is light
    ✓ preference computed property
      ✓ should return the current preference value
      ✓ should return system when preference is set to system
    ✓ toggleTheme function
      ✓ should switch from light to dark when called
      ✓ should switch from dark to light when called
    ✓ setTheme function
      ✓ should set theme to light
      ✓ should set theme to dark
      ✓ should set theme to system

✓ tests/composables/useRole.test.ts (10 tests)
  ✓ useRole
    ✓ getAllRoles
      ✓ should fetch all roles successfully
      ✓ should handle empty role list
    ✓ getRoleById
      ✓ should fetch a role by id successfully
    ✓ getAllPermissions
      ✓ should fetch all permissions successfully
      ✓ should handle empty permissions list
    ✓ createRole
      ✓ should create a new role successfully
    ✓ updateRole
      ✓ should update an existing role successfully
    ✓ deleteRole
      ✓ should delete a role successfully
    ✓ error handling
      ✓ should handle API errors when fetching roles
      ✓ should handle API errors when creating role

Test Files  2 passed (2)
Tests       19 passed (19)
Duration    602ms
```

---

## Key Patterns

### useTheme Pattern (State-based)
```typescript
// Set up mock state
mockColorMode.value = 'dark'

// Call composable
const { isDark } = useTheme()

// Assert reactive state
expect(isDark.value).toBe(true)
```

### useRole Pattern (API-based CRUD)
```typescript
// Mock API response
mockApiFetch.mockResolvedValue({ data: mockRole, success: true })

// Call composable method
const { getRoleById } = useRole()
const result = await getRoleById('1')

// Assert API call and result
expect(mockApiFetch).toHaveBeenCalledWith('/roles/1')
expect(result).toEqual(mockRole)
```

### Error Handling Pattern
```typescript
const mockError = new Error('Network error')
mockApiFetch.mockRejectedValue(mockError)

await expect(getAllRoles()).rejects.toThrow('Network error')
```

---

## Code Quality Checklist

- ✅ ESLint compliant (single quotes, no semicolons, trailing commas)
- ✅ TypeScript types imported from `~/types/auth`
- ✅ Proper mock cleanup in `beforeEach`
- ✅ Descriptive test names
- ✅ Comprehensive coverage (happy path + errors + edge cases)
- ✅ All assertions verify expected behavior

---

## Related Documentation

- [Full Implementation Summary](./USETHEME_USEROLE_TESTS_COMPLETE.md)
- [Setup File](../frontend/tests/setup.ts)
- [Type Definitions](../frontend/types/auth.ts)
