# useStatus Tests - Quick Reference

## Run Tests
```bash
# Run useStatus tests only
npm test -- tests/composables/useStatus.test.ts

# Run in watch mode
npm test -- tests/composables/useStatus.test.ts --watch

# Run with coverage
npm test -- tests/composables/useStatus.test.ts --coverage
```

## Test Structure

### Setup
```typescript
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { useI18n } from '../setup'
import { useStatus } from '~/composables/useStatus'

describe('useStatus', () => {
  const mockT = vi.fn()
  
  beforeEach(() => {
    mockT.mockReset()
    useI18n.mockReturnValue({
      locale: { value: 'en-US' },
      t: mockT,
    })
  })
})
```

### Test Pattern
```typescript
it('should return active label when status is true', () => {
  // Arrange
  mockT.mockReturnValue('Active')
  const { getStatusLabel } = useStatus()

  // Act
  const result = getStatusLabel(true)

  // Assert
  expect(mockT).toHaveBeenCalledWith('common.active')
  expect(result).toBe('Active')
})
```

## Coverage Summary

| Function | Tests | Coverage |
|----------|-------|----------|
| getStatusLabel | 2 | 100% |
| getStatusSeverity | 2 | 100% |
| getStatusBadge | 2 | 100% |
| **Total** | **6** | **100%** |

## Test Results
```
✓ getStatusLabel > should return active label when status is true
✓ getStatusLabel > should return inactive label when status is false
✓ getStatusSeverity > should return success severity when status is true
✓ getStatusSeverity > should return danger severity when status is false
✓ getStatusBadge > should return badge with active label and success severity when status is true
✓ getStatusBadge > should return badge with inactive label and danger severity when status is false

Test Files  1 passed (1)
     Tests  6 passed (6)
  Duration  ~6ms
```

## Common Assertions

### String Return
```typescript
expect(result).toBe('Active')
```

### Severity Type
```typescript
expect(result).toBe('success') // or 'danger'
```

### Object Shape
```typescript
expect(result).toEqual({
  label: 'Active',
  severity: 'success',
})
```

### Translation Call
```typescript
expect(mockT).toHaveBeenCalledWith('common.active')
```

## Key Features

✅ **Mock Reset** - Mocks cleared before each test
✅ **Translation Mock** - i18n function properly mocked
✅ **Type Safety** - Return types verified
✅ **Both States** - True and false tested for each function
✅ **ESLint Compliant** - Follows all code style rules
