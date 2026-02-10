# Frontend Testing Guide

## Overview

This document describes the testing infrastructure and practices for the frontend application. We use [Vitest](https://vitest.dev/) as our testing framework, which is the recommended testing solution for Nuxt 3 projects.

## Testing Stack

- **Framework**: Vitest 4.x
- **Component Testing**: @vue/test-utils 2.x
- **DOM Environment**: happy-dom
- **Nuxt Integration**: @nuxt/test-utils (installed for future use)
- **UI**: @vitest/ui for visual test runner

## Setup

### Installation

All testing dependencies are already installed. To verify, check `package.json`:

```bash
npm list vitest @vue/test-utils happy-dom
```

### Configuration

Test configuration is in `vitest.config.ts`:

```typescript
import { resolve } from 'node:path'
import { defineConfig } from 'vitest/config'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [vue()],
  test: {
    globals: true,
    environment: 'happy-dom',
    setupFiles: ['./tests/setup.ts'],
    // ... coverage settings
  },
  resolve: {
    alias: {
      '~': resolve(__dirname, './'),
      '@': resolve(__dirname, './'),
    },
  },
})
```

### Test Setup File

The `tests/setup.ts` file provides global mocks for Nuxt auto-imports:

```typescript
import { vi } from 'vitest'

const mockApiFetch = vi.fn()

const useApi = vi.fn(() => ({
  apiFetch: mockApiFetch,
}))

globalThis.useApi = useApi

export { mockApiFetch, useApi }
```

## Running Tests

### Available Commands

```bash
# Run tests in watch mode
npm run test

# Run tests with UI
npm run test:ui

# Run tests with coverage
npm run test:coverage
```

### Running Specific Tests

```bash
# Run a specific test file
npm run test -- tests/composables/useWarehouse.test.ts

# Run tests matching a pattern
npm run test -- --grep "getAllWarehouses"
```

## Writing Tests

### Composable Tests

Composables are the easiest to test since they are pure TypeScript functions. Example:

```typescript
import { beforeEach, describe, expect, it } from 'vitest'
import { mockApiFetch } from '../setup'
import { useWarehouse } from '~/composables/useWarehouse'

describe('useWarehouse', () => {
  beforeEach(() => {
    mockApiFetch.mockReset()
  })

  it('should fetch all warehouses', async () => {
    const mockData = [{ id: '1', name: 'Test' }]
    mockApiFetch.mockResolvedValue({ data: mockData, success: true })

    const { getAllWarehouses } = useWarehouse()
    const result = await getAllWarehouses()

    expect(result).toEqual(mockData)
    expect(mockApiFetch).toHaveBeenCalledWith('/warehouses', {
      method: 'GET',
    })
  })
})
```

### Component Tests (Future)

Component testing in Nuxt 3 requires additional setup for:
- i18n mocking
- Pinia store mocking
- Router mocking
- PrimeVue component mocking

This is planned for future implementation when component testing becomes necessary.

## Test Coverage

### Current Coverage

- ✅ **Composables**: `useWarehouse` (8 tests)
  - getAllWarehouses
  - getWarehouseById
  - createWarehouse
  - updateWarehouse
  - deleteWarehouse
  - Error handling

### Future Coverage

- ⏳ **Composables**:
  - useWarehouseInventory
  - useProduct
  - useStockMovement
  - Other business logic composables

- ⏳ **Components**:
  - Warehouse list page
  - Warehouse form components
  - Other critical UI components

## Best Practices

### 1. Test Organization

- Place tests in `tests/` directory
- Mirror the source structure: `tests/composables/`, `tests/components/`
- Name test files with `.test.ts` or `.spec.ts` suffix

### 2. Test Structure

Use the AAA pattern (Arrange, Act, Assert):

```typescript
it('should do something', async () => {
  // Arrange
  mockApiFetch.mockResolvedValue({ data: mockData })

  // Act
  const result = await someFunction()

  // Assert
  expect(result).toEqual(expected)
})
```

### 3. Mocking

- Use `mockApiFetch` from setup file for API calls
- Reset mocks in `beforeEach` to ensure test isolation
- Mock external dependencies at the top of test files

### 4. Assertions

- Use descriptive assertions: `expect(result).toHaveLength(2)` instead of `expect(result.length).toBe(2)`
- Test both success and error cases
- Verify that functions are called with correct parameters

## Troubleshooting

### Common Issues

#### 1. "useApi is not defined"

Make sure the setup file is imported and the mock is properly configured:

```typescript
import { mockApiFetch } from '../setup'
```

#### 2. Tests not running

Ensure `.nuxt` directory exists by running:

```bash
npm run postinstall
```

#### 3. TypeScript errors

Make sure types are available:

```bash
npm run typecheck
```

## CI/CD Integration

Tests should be run in the CI pipeline. Add to `.github/workflows/`:

```yaml
- name: Run tests
  run: |
    cd frontend
    npm run test
```

## Future Improvements

1. **Increase Coverage**: Add tests for more composables
2. **Component Tests**: Implement component testing with proper mocking
3. **E2E Tests**: Consider Playwright for end-to-end testing
4. **Coverage Thresholds**: Enforce minimum coverage percentages
5. **Visual Regression**: Consider adding visual regression testing

## Resources

- [Vitest Documentation](https://vitest.dev/)
- [Vue Test Utils](https://test-utils.vuejs.org/)
- [Nuxt Testing](https://nuxt.com/docs/getting-started/testing)
- [Testing Library](https://testing-library.com/docs/vue-testing-library/intro/)

## Example: Complete Test File

See `tests/composables/useWarehouse.test.ts` for a complete example demonstrating:
- Mock setup and reset
- Multiple test scenarios
- Success and error cases
- API call verification
- Type-safe assertions
