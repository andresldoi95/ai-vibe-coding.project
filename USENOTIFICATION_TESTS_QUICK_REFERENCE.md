# useNotification Tests - Quick Reference

## 📊 Test Summary
- **File**: `/frontend/tests/composables/useNotification.test.ts`
- **Tests**: 10 passing
- **Coverage**: 100% of useNotification methods
- **Status**: ✅ All tests passing, ESLint compliant

## 🧪 Test Cases

### showSuccess (2 tests)
```typescript
showSuccess('Operation successful')
showSuccess('Operation successful', 'The item was created successfully')
```
- Severity: `'success'`
- Life: `3000ms`

### showError (2 tests)
```typescript
showError('Operation failed')
showError('Operation failed', 'Unable to connect to server')
```
- Severity: `'error'`
- Life: `5000ms`

### showWarning (2 tests)
```typescript
showWarning('Warning message')
showWarning('Warning message', 'This action may have consequences')
```
- Severity: `'warn'`
- Life: `4000ms`

### showInfo (2 tests)
```typescript
showInfo('Information message')
showInfo('Information message', 'Here is some additional information')
```
- Severity: `'info'`
- Life: `3000ms`

### showPermissionError (1 test)
```typescript
showPermissionError()
```
- Severity: `'error'`
- Summary: `'Insufficient Permissions'`
- Detail: `'You do not have permission to perform this action. Contact your administrator.'`
- Life: `5000ms`

### Toast Instance (1 test)
```typescript
const { toast } = useNotification()
// Verifies toast instance is returned
```

## 🔧 Mock Setup
```typescript
const mockAdd = vi.fn()
vi.mock('primevue/usetoast', () => ({
  useToast: () => ({
    add: mockAdd,
  }),
}))
```

## ✅ Verification Pattern
Each test verifies:
1. `mockAdd` is called exactly once
2. Correct parameters are passed:
   - `severity`: notification type
   - `summary`: message string
   - `detail`: optional detail or undefined
   - `life`: duration in milliseconds

## 📈 Test Results
```
✓ tests/composables/useNotification.test.ts (10 tests) 8ms
  ✓ showSuccess
    ✓ should call toast.add with success severity and message
    ✓ should call toast.add with success severity, message, and detail
  ✓ showError
    ✓ should call toast.add with error severity and message
    ✓ should call toast.add with error severity, message, and detail
  ✓ showWarning
    ✓ should call toast.add with warn severity and message
    ✓ should call toast.add with warn severity, message, and detail
  ✓ showInfo
    ✓ should call toast.add with info severity and message
    ✓ should call toast.add with info severity, message, and detail
  ✓ showPermissionError
    ✓ should call toast.add with fixed permission error message
  ✓ toast instance
    ✓ should return toast instance
```

## 🎯 Code Quality
- ✅ Single quotes (ESLint compliant)
- ✅ No semicolons
- ✅ Trailing commas
- ✅ TypeScript types
- ✅ Proper mock reset in beforeEach
- ✅ Clear test descriptions
- ✅ Follows project patterns

## 🚀 Run Tests
```bash
# Run only useNotification tests
npm test -- useNotification.test.ts

# Run all tests
npm test -- --run

# Run with coverage
npm test -- --coverage
```

## 📦 Total Project Tests
- **Test Files**: 8
- **Total Tests**: 130 ✅
- **Duration**: ~2.2s

---
*All requirements met ✓*
