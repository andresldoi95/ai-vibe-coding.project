# useFormatters Tests - Quick Reference

## 📊 Test Statistics

| Metric | Value |
|--------|-------|
| **Total Tests** | 28 |
| **Passing** | 28 ✅ |
| **Failing** | 0 |
| **Test File** | `tests/composables/useFormatters.test.ts` |
| **Lines of Code** | 226 |
| **Test Duration** | ~28ms |

## 🧪 Test Categories Breakdown

```
useFormatters
├── formatCurrency (8 tests)
│   ├── USD/EUR formatting for en-US
│   ├── EUR formatting for es-ES and fr-FR
│   ├── Negative amounts
│   ├── Zero values
│   ├── Large amounts
│   └── Default currency parameter
│
├── formatDate (7 tests)
│   ├── Short/Long formats for en-US
│   ├── Short format for es-ES
│   ├── Long format for fr-FR
│   ├── String date input
│   ├── Default format parameter
│   └── Invalid date handling
│
├── formatNumber (7 tests)
│   ├── en-US locale (1,234.56)
│   ├── es-ES locale (1.234,56)
│   ├── fr-FR locale (1 234,56)
│   ├── Negative numbers
│   ├── Zero
│   ├── Large numbers
│   └── Integers
│
├── formatDateTime (5 tests)
│   ├── en-US locale
│   ├── es-ES locale
│   ├── fr-FR locale
│   ├── String input
│   └── Invalid date handling
│
└── locale reactivity (1 test)
    └── Dynamic locale changes
```

## 🚀 Running Tests

```bash
# Run all useFormatters tests
npm test -- useFormatters.test.ts

# Run with verbose output
npm test -- useFormatters.test.ts --reporter=verbose

# Run with coverage
npm run test:coverage -- useFormatters.test.ts

# Watch mode
npm test -- useFormatters.test.ts --watch
```

## 🔧 Mock Configuration

The test uses a shared mock setup from `tests/setup.ts`:

```typescript
import { mockLocale } from '../setup'

// Before each test
beforeEach(() => {
  mockLocale.value = 'en-US'  // Reset to default
})

// To test different locales
mockLocale.value = 'es-ES'
mockLocale.value = 'fr-FR'
```

## 📝 Example Test Patterns

### Basic Formatting Test
```typescript
it('should format currency in USD for en-US locale', () => {
  const { formatCurrency } = useFormatters()
  const result = formatCurrency(1234.56, 'USD')
  expect(result).toBe('$1,234.56')
})
```

### Locale-Specific Test
```typescript
it('should format currency in EUR for es-ES locale', () => {
  mockLocale.value = 'es-ES'
  const { formatCurrency } = useFormatters()
  const result = formatCurrency(1234.56, 'EUR')
  expect(result).toBe('1234,56\u00A0€')  // Note: \u00A0 = non-breaking space
})
```

### Error Handling Test
```typescript
it('should handle invalid date gracefully', () => {
  const { formatDate } = useFormatters()
  expect(() => formatDate('invalid-date', 'short')).toThrow(RangeError)
})
```

### Reactivity Test
```typescript
it('should update formatting when locale changes', () => {
  const { formatCurrency } = useFormatters()
  
  // Test en-US
  let result = formatCurrency(100, 'EUR')
  expect(result).toBe('€100.00')
  
  // Change locale
  mockLocale.value = 'fr-FR'
  
  // Test fr-FR
  result = formatCurrency(100, 'EUR')
  expect(result).toBe('100,00\u00A0€')
})
```

## 🌍 Locale-Specific Formatting

### Currency Formatting

| Locale | Format | Example |
|--------|--------|---------|
| en-US  | `$1,234.56` | Standard comma separator |
| es-ES  | `1234,56 €` | Uses `\u00A0` (non-breaking space) |
| fr-FR  | `1 234,56 €` | Uses `\u202F` (narrow non-breaking space) |

### Number Formatting

| Locale | Format | Thousands | Decimal |
|--------|--------|-----------|---------|
| en-US  | `1,234.56` | `,` | `.` |
| es-ES  | `1.234,56` | `.` | `,` |
| fr-FR  | `1 234,56` | ` ` (`\u202F`) | `,` |

### Date Formatting

| Locale | Short | Long |
|--------|-------|------|
| en-US  | `03/15/2024` | `March 15, 2024` |
| es-ES  | `15/03/2024` | `15 de marzo de 2024` |
| fr-FR  | `15/03/2024` | `15 mars 2024` |

## 🎯 Edge Cases Covered

- ✅ **Negative numbers**: `-$500.25`
- ✅ **Zero values**: `$0.00`, `0`
- ✅ **Large numbers**: Millions and billions
- ✅ **Invalid dates**: Throws RangeError
- ✅ **String dates**: ISO date strings
- ✅ **Default parameters**: USD, short format
- ✅ **Unicode characters**: Non-breaking spaces

## 📐 Special Unicode Characters

| Character | Code | Usage | Locales |
|-----------|------|-------|---------|
| Non-breaking space | `\u00A0` | Between number and symbol | es-ES, fr-FR |
| Narrow non-breaking space | `\u202F` | Thousands separator | fr-FR |

## ✅ ESLint Compliance

The test file passes all ESLint rules:
- ✅ Single quotes
- ✅ No semicolons
- ✅ Trailing commas
- ✅ Uppercase Unicode escapes
- ✅ No trailing spaces
- ✅ Proper TypeScript types

## 🔍 Test Output Example

```
✓ tests/composables/useFormatters.test.ts (28 tests) 28ms
  ✓ useFormatters
    ✓ formatCurrency
      ✓ should format currency in USD for en-US locale 15ms
      ✓ should format currency in EUR for en-US locale 0ms
      ✓ should format currency in EUR for es-ES locale 0ms
      ✓ should format currency in EUR for fr-FR locale 0ms
      ✓ should format negative currency amounts 0ms
      ✓ should format zero as currency 0ms
      ✓ should format large currency amounts 0ms
      ✓ should use USD as default currency 0ms
    ✓ formatDate (7 tests)
    ✓ formatNumber (7 tests)
    ✓ formatDateTime (5 tests)
    ✓ locale reactivity (1 test)

Test Files  1 passed (1)
     Tests  28 passed (28)
```

## 🎓 Key Learnings

1. **Mock Setup**: Global mocks in `setup.ts` are available to all tests
2. **Locale Testing**: Use `mockLocale.value` to test different locales
3. **Unicode Characters**: Different locales use different space characters
4. **Error Handling**: Invalid dates throw RangeError, not return "Invalid Date"
5. **Reactivity**: The locale is reactive, so formatting updates automatically
6. **Default Parameters**: Test both explicit and default parameter values

## 📚 Related Files

- **Test File**: `/frontend/tests/composables/useFormatters.test.ts`
- **Composable**: `/frontend/composables/useFormatters.ts`
- **Setup File**: `/frontend/tests/setup.ts`
- **Summary**: `/USEFORMATTERS_TESTS_COMPLETE.md`

---

**Last Updated**: 2024  
**Status**: ✅ All tests passing  
**Maintainer**: Frontend Team
