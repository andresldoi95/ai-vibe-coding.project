# E2E Tests

This directory contains End-to-End (E2E) tests for the SaaS Billing + Inventory Management System using Playwright.

## Quick Start

```bash
# Install Playwright browsers (first time only)
npm run e2e:install

# Run tests in UI mode (recommended for development)
npm run e2e:ui

# Run tests headless (CI/production)
npm run e2e
```

## Test Files

- **`fixtures.ts`**: Page objects, test data, and common utilities
- **`login.spec.ts`**: Login flow tests (10 tests)
- **`register.spec.ts`**: Registration flow tests (11 tests)

## Test Organization

### Page Objects

Page objects encapsulate page interactions and selectors:

```typescript
const loginPage = new LoginPage(page)
await loginPage.goto()
await loginPage.login(email, password)
```

Available page objects:
- `LoginPage`: Login page interactions
- `RegisterPage`: Registration page interactions
- `DashboardPage`: Dashboard validations

### Test Data

Test credentials are defined in `fixtures.ts`:

```typescript
TEST_CREDENTIALS.admin // admin@example.com / Admin123!
TEST_CREDENTIALS.user  // user@example.com / User123!
```

**Important**: These must match backend seed data.

## Writing New Tests

1. Create/update page object in `fixtures.ts`
2. Create test file `*.spec.ts`
3. Write tests using page objects
4. Run with `npm run e2e:ui`

Example:

```typescript
import { test, expect } from '@playwright/test'
import { LoginPage } from './fixtures'

test('should login successfully', async ({ page }) => {
  const loginPage = new LoginPage(page)
  await loginPage.goto()
  await loginPage.login('admin@example.com', 'Admin123!')

  await expect(page).toHaveURL('/')
})
```

## Best Practices

✅ **Use page objects** for all page interactions
✅ **Wait properly** with `waitForSelector`, not `waitForTimeout`
✅ **Keep tests independent** - each test should run in isolation
✅ **Use semantic selectors** - `text=`, `role=`, `data-testid=`
✅ **Test user journeys**, not implementation details

❌ **Don't hard-code waits** - use explicit waits
❌ **Don't use brittle selectors** - avoid CSS classes
❌ **Don't test unit logic** - E2E is for integration

## Debugging

### UI Mode (Visual)
```bash
npm run e2e:ui
```
See tests execute in real-time with time-travel debugging.

### Debug Mode (Step-by-Step)
```bash
npm run e2e:debug
```
Step through tests line by line with Playwright Inspector.

### View Last Report
```bash
npm run e2e:report
```
See detailed HTML report with screenshots and traces.

## Current Coverage

**Authentication Flows**: 21 tests
- Login (10 tests): UI, validation, success/error, navigation, session
- Registration (11 tests): UI, validation, success/error, duplicate email

**Coverage**: 100% of critical authentication user journeys

## Documentation

See [E2E_TESTING.md](../E2E_TESTING.md) for comprehensive documentation including:
- Setup instructions
- Test architecture
- CI/CD integration
- Troubleshooting guide
- Best practices

## Requirements

- Node.js 18+
- Playwright browsers (install with `npm run e2e:install`)
- Backend running on `http://localhost:5000`
- Frontend dev server (auto-started by Playwright)

## Support

For issues or questions about E2E testing:
1. Check [E2E_TESTING.md](../E2E_TESTING.md)
2. Review [Playwright docs](https://playwright.dev)
3. Check test results with `npm run e2e:report`
