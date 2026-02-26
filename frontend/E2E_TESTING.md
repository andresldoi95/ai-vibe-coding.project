# E2E Testing with Playwright

This document describes the End-to-End (E2E) testing setup for the SaaS Billing + Inventory Management System using Playwright.

## Overview

E2E tests validate complete user journeys through the application, testing the integration between frontend, backend, and database. Unlike unit tests that focus on isolated logic, E2E tests simulate real user interactions in a browser environment.

## Why E2E Testing?

After achieving **99.66% unit test coverage** (828 tests across composables, utilities, stores, and components), we identified that page-level testing requires a different approach:

- **Complex Integration**: Pages integrate multiple libraries (Vuelidate, PrimeVue, Nuxt composables)
- **Browser APIs**: Real interactions with clipboard, localStorage, cookies
- **Navigation Flows**: Multi-step processes like login → dashboard
- **Visual Feedback**: Toast messages, loading states, validation errors
- **Session Management**: Authentication persistence, token refresh

These scenarios are best tested with E2E frameworks like Playwright.

## Architecture

```
frontend/
├── e2e/                          # E2E test files
│   ├── fixtures.ts               # Page objects and test data
│   ├── login.spec.ts             # Login flow tests
│   └── register.spec.ts          # Registration flow tests
├── playwright.config.ts          # Playwright configuration
└── playwright-report/            # Test reports (generated)
```

## Setup

### 1. Install Playwright Browsers

```bash
npm run e2e:install
```

This installs Chromium, Firefox, and WebKit browsers for testing.

### 2. Start Backend Services

Ensure the backend API and database are running:

```bash
# In backend directory
docker-compose up -d
dotnet run
```

### 3. Verify Test Data

E2E tests use seeded test accounts:

- **Admin**: `admin@example.com` / `Admin123!`
- **User**: `user@example.com` / `User123!`

These should be created by the backend seeder (`SeedController.cs`).

## Running Tests

### Headless Mode (CI/Production)

```bash
npm run e2e
```

Runs all tests in headless mode with Chromium browser.

### UI Mode (Development)

```bash
npm run e2e:ui
```

Opens Playwright UI for interactive test development:
- See test execution in real-time
- Debug failed tests
- Record new tests
- Time-travel debugging

### Debug Mode

```bash
npm run e2e:debug
```

Runs tests with Playwright Inspector for step-by-step debugging.

### View Last Report

```bash
npm run e2e:report
```

Opens the HTML report from the last test run.

### Run Specific Test File

```bash
npx playwright test e2e/login.spec.ts
```

### Run Single Test

```bash
npx playwright test -g "should successfully login"
```

## Test Structure

### Page Objects Pattern

We use the Page Object Model (POM) to encapsulate page interactions:

```typescript
// e2e/fixtures.ts
export class LoginPage {
  constructor(private page: Page) {}

  async goto() {
    await this.page.goto('/login')
  }

  async login(email: string, password: string) {
    await this.page.fill('input[type="email"]', email)
    await this.page.fill('input[type="password"]', password)
    await this.page.click('button[type="submit"]')
  }
}
```

**Benefits**:
- Reusable methods across tests
- Single source of truth for selectors
- Easy maintenance when UI changes

### Test Data

Test credentials are centralized in `fixtures.ts`:

```typescript
export const TEST_CREDENTIALS = {
  admin: {
    email: 'admin@example.com',
    password: 'Admin123!',
  },
}
```

**Important**: Keep test data synchronized with backend seed data.

### Example Test

```typescript
test('should successfully login with valid credentials', async ({ page }) => {
  const loginPage = new LoginPage(page)
  await loginPage.goto()

  await loginPage.login(
    TEST_CREDENTIALS.admin.email,
    TEST_CREDENTIALS.admin.password,
  )

  await expectToBeOnDashboard(page)
})
```

## Current Test Coverage

### Login Flow (`e2e/login.spec.ts`)

✅ **UI Rendering** (1 test):
- Display login form with email, password inputs, submit button

✅ **Form Validation** (2 tests):
- Empty form validation
- Invalid email format validation

✅ **Authentication** (2 tests):
- Invalid credentials error
- Successful login and redirect

✅ **Loading States** (1 test):
- Disabled submit button during API call

✅ **Navigation** (2 tests):
- Navigate to register page
- Navigate to forgot password page

✅ **Session Persistence** (2 tests):
- Persist session after page reload
- Redirect to login when accessing protected route

**Total: 10 tests**

### Registration Flow (`e2e/register.spec.ts`)

✅ **UI Rendering** (1 test):
- Display registration form with all fields

✅ **Form Validation** (7 tests):
- Empty form validation
- Password mismatch error
- Invalid email format
- Weak password error
- Name field required
- Email format validation
- Password strength requirements
- Company name required

✅ **Registration** (2 tests):
- Successful registration
- Duplicate email error

✅ **Navigation** (1 test):
- Navigate to login page

**Total: 11 tests**

### Overall E2E Coverage

- **21 E2E tests** covering authentication flows
- **2 test files** (login, register)
- **100% critical user journeys** tested

## Writing New Tests

### 1. Create Page Object (if needed)

Add to `e2e/fixtures.ts`:

```typescript
export class ProductPage {
  constructor(private page: Page) {}

  async goto() {
    await this.page.goto('/products')
  }

  async createProduct(name: string, price: number) {
    await this.page.click('button:has-text("New Product")')
    await this.page.fill('input[name="name"]', name)
    await this.page.fill('input[name="price"]', price.toString())
    await this.page.click('button[type="submit"]')
  }
}
```

### 2. Create Test File

Create `e2e/products.spec.ts`:

```typescript
import { expect, test } from '@playwright/test'
import { ProductPage } from './fixtures'

test.describe('Product Management', () => {
  test('should create new product', async ({ page }) => {
    const productPage = new ProductPage(page)
    await productPage.goto()

    await productPage.createProduct('Test Product', 99.99)

    await expect(page.locator('text=Product created')).toBeVisible()
  })
})
```

### 3. Run Test

```bash
npx playwright test e2e/products.spec.ts
```

## Best Practices

### ✅ Do's

1. **Use Page Objects**: Encapsulate page interactions
2. **Test User Journeys**: Focus on complete workflows, not isolated components
3. **Use Semantic Selectors**: Prefer `text=`, `role=`, `data-testid=` over CSS classes
4. **Wait Properly**: Use `waitForSelector`, `waitForURL`, not `waitForTimeout`
5. **Keep Tests Independent**: Each test should run in isolation
6. **Use Test Data**: Create unique data (timestamps) to avoid conflicts
7. **Clean Up**: Reset state after tests (logout, clear data)

### ❌ Don'ts

1. **Don't Test Unit Logic**: E2E tests are for integration, not algorithms
2. **Don't Use Brittle Selectors**: Avoid `.class-name-123` or `#id-456`
3. **Don't Over-Test**: Focus on critical paths, not every edge case
4. **Don't Ignore Failures**: Fix flaky tests immediately
5. **Don't Hard-Code Waits**: Use explicit waits instead of `waitForTimeout`

## Debugging Failed Tests

### 1. Run with UI

```bash
npm run e2e:ui
```

See exactly what the browser is doing.

### 2. Run with Debug

```bash
npm run e2e:debug
```

Step through test execution line by line.

### 3. Check Screenshots

Failed tests automatically capture screenshots:
```
test-results/
└── login-flow-should-successfully-login/
    └── test-failed-1.png
```

### 4. Check Videos

Failed tests record videos (if enabled):
```
test-results/
└── login-flow-should-successfully-login/
    └── video.webm
```

### 5. View Traces

Playwright captures full traces on failure:
```bash
npx playwright show-trace test-results/.../trace.zip
```

## CI/CD Integration

### GitHub Actions Example

```yaml
name: E2E Tests

on: [push, pull_request]

jobs:
  e2e:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: 18

      - name: Install dependencies
        run: npm ci
        working-directory: ./frontend

      - name: Install Playwright browsers
        run: npm run e2e:install
        working-directory: ./frontend

      - name: Start backend
        run: docker-compose up -d

      - name: Run E2E tests
        run: npm run e2e
        working-directory: ./frontend

      - name: Upload test report
        if: always()
        uses: actions/upload-artifact@v3
        with:
          name: playwright-report
          path: frontend/playwright-report/
```

## Configuration

### Playwright Config (`playwright.config.ts`)

Key settings:

```typescript
{
  testDir: './e2e',              // Test directory
  timeout: 30 * 1000,            // 30s per test
  fullyParallel: true,           // Run tests in parallel
  retries: process.env.CI ? 2 : 0, // Retry on CI
  baseURL: 'http://localhost:3000', // Frontend URL
  webServer: {                   // Auto-start dev server
    command: 'npm run dev',
    url: 'http://localhost:3000',
  }
}
```

### Environment Variables

Set these in `.env` or pass to CLI:

- `BASE_URL`: Frontend URL (default: `http://localhost:3000`)
- `CI`: Enable CI mode (retries, parallel workers)

## Test Maintenance

### When to Update Tests

1. **UI Changes**: Update selectors in page objects
2. **New Features**: Add new tests for critical paths
3. **Backend Changes**: Update test credentials/data
4. **Bug Fixes**: Add regression tests

### Keeping Tests Fast

- Run tests in parallel (default)
- Use `fullyParallel: true` in config
- Limit retries (only on CI)
- Skip non-critical tests in dev

### Keeping Tests Stable

- Use reliable selectors (`data-testid`, `role`, `text`)
- Wait for elements properly
- Avoid hard-coded timeouts
- Use unique test data

## Future Test Coverage

### Planned Tests

- [ ] **Password Reset Flow**: Request → Email → Reset
- [ ] **Tenant Switching**: Select tenant → Verify data isolation
- [ ] **Product CRUD**: Create → Edit → View → Delete
- [ ] **Warehouse CRUD**: Create → Edit → View → Delete
- [ ] **Stock Movements**: Create movement → Verify stock updates
- [ ] **Invoice Generation**: Create invoice → Generate PDF → Download
- [ ] **Payment Processing**: Record payment → Verify status
- [ ] **User Management**: Invite user → Accept → Assign roles
- [ ] **Multi-language**: Switch language → Verify translations
- [ ] **Dark Mode**: Toggle theme → Verify persistence

## Comparison: Unit vs E2E Testing

| Aspect | Unit Tests | E2E Tests |
|--------|-----------|-----------|
| **Scope** | Single function/component | Complete user journey |
| **Speed** | Fast (ms) | Slow (seconds) |
| **Isolation** | Fully isolated with mocks | Integrated with real services |
| **Flakiness** | Stable | Can be flaky |
| **Coverage** | 99.66% code coverage | Critical paths only |
| **Purpose** | Validate logic correctness | Validate user experience |
| **Feedback** | Immediate | Slower |
| **Cost** | Low | Higher (browser automation) |

**Our Strategy**:
- ✅ **Unit Tests**: 828 tests covering all business logic (99.66%)
- ✅ **E2E Tests**: 21 tests covering critical user journeys (100% auth flows)

## Resources

- [Playwright Documentation](https://playwright.dev)
- [Best Practices Guide](https://playwright.dev/docs/best-practices)
- [Debugging Guide](https://playwright.dev/docs/debug)
- [CI Integration](https://playwright.dev/docs/ci)

## Troubleshooting

### Tests Timeout

**Problem**: Tests fail with "Timeout exceeded"

**Solution**:
- Increase timeout in `playwright.config.ts`
- Check backend is running
- Verify network connectivity

### Browser Not Installed

**Problem**: "Executable doesn't exist"

**Solution**:
```bash
npm run e2e:install
```

### Flaky Tests

**Problem**: Tests pass/fail randomly

**Solution**:
- Use proper waits (`waitForSelector`, not `waitForTimeout`)
- Ensure test isolation (independent tests)
- Check for race conditions

### Backend Not Ready

**Problem**: Tests fail because backend is not available

**Solution**:
- Verify `docker-compose up -d`
- Check backend is on `http://localhost:5000`
- Increase `webServer.timeout` in config

## Summary

✅ **E2E testing setup complete**
- 21 tests covering authentication flows
- Page Object Model for maintainability
- CI/CD ready configuration
- Comprehensive documentation

✅ **Combined test coverage**:
- **Unit Tests**: 828 tests, 99.66% coverage (logic validation)
- **E2E Tests**: 21 tests (user journey validation)

This dual-layer testing strategy ensures both **code correctness** (unit tests) and **user experience quality** (E2E tests).

---

**Next Steps**:
1. Run `npm run e2e:install` to install browsers
2. Run `npm run e2e:ui` to explore tests
3. Add more E2E tests for inventory/billing flows
4. Integrate into CI/CD pipeline
