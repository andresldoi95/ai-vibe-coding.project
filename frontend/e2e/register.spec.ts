import { expect, test } from '@playwright/test'
import {
  LoginPage,
  RegisterPage,
  expectToBeOnLoginPage,
} from './fixtures'

/**
 * E2E Tests for Registration Flow
 *
 * Tests user registration including:
 * - Form validation
 * - Successful registration
 * - Error handling
 * - Navigation to login
 */
test.describe('Registration Flow', () => {
  let registerPage: RegisterPage

  test.beforeEach(async ({ page }) => {
    registerPage = new RegisterPage(page)
    await registerPage.goto()
  })

  test('should display registration form', async ({ page }) => {
    await expect(page).toHaveTitle(/Register|Sign up/i)

    // Check all required fields are present
    await expect(page.locator('input[name="name"]')).toBeVisible()
    await expect(page.locator('input[type="email"]')).toBeVisible()
    await expect(page.locator('input[name="password"]')).toBeVisible()
    await expect(page.locator('input[name="confirmPassword"]')).toBeVisible()
    await expect(page.locator('input[name="companyName"]')).toBeVisible()

    const submitButton = page.locator('button[type="submit"]')
    await expect(submitButton).toBeVisible()
  })

  test('should show validation errors for empty form', async ({ page }) => {
    const submitButton = page.locator('button[type="submit"]')
    await submitButton.click()

    // Should not navigate away
    await page.waitForTimeout(500)
    await expect(page).toHaveURL(/register/)

    // Check for validation errors (implementation dependent)
    const nameInput = page.locator('input[name="name"]')
    const hasError = await nameInput.evaluate((el) => {
      return el.getAttribute('aria-invalid') === 'true'
    })
    expect(hasError).toBeTruthy()
  })

  test('should show error when passwords do not match', async ({ page }) => {
    await registerPage.register({
      name: 'Test User',
      email: `test${Date.now()}@example.com`,
      password: 'Password123!',
      confirmPassword: 'DifferentPassword123!',
      companyName: 'Test Company',
    })

    // Should remain on register page
    await page.waitForTimeout(500)
    await expect(page).toHaveURL(/register/)
  })

  test('should show error for invalid email format', async ({ page }) => {
    await registerPage.register({
      name: 'Test User',
      email: 'invalid-email',
      password: 'Password123!',
      confirmPassword: 'Password123!',
      companyName: 'Test Company',
    })

    // Should remain on register page
    await page.waitForTimeout(500)
    await expect(page).toHaveURL(/register/)
  })

  test('should show error for weak password', async ({ page }) => {
    await registerPage.register({
      name: 'Test User',
      email: `test${Date.now()}@example.com`,
      password: '123',
      confirmPassword: '123',
      companyName: 'Test Company',
    })

    // Should remain on register page
    await page.waitForTimeout(500)
    await expect(page).toHaveURL(/register/)
  })

  test('should successfully register with valid data', async ({ page }) => {
    const timestamp = Date.now()
    await registerPage.register({
      name: 'New Test User',
      email: `newuser${timestamp}@example.com`,
      password: 'NewPassword123!',
      confirmPassword: 'NewPassword123!',
      companyName: `Test Company ${timestamp}`,
    })

    // Wait for success message
    await registerPage.waitForSuccessMessage()

    // Should redirect to login page
    await expectToBeOnLoginPage(page)
  })

  test('should show error when registering with existing email', async ({ page }) => {
    // Try to register with an email that already exists
    await registerPage.register({
      name: 'Duplicate User',
      email: 'admin@example.com', // This should already exist from seed data
      password: 'Password123!',
      confirmPassword: 'Password123!',
      companyName: 'Duplicate Company',
    })

    // Wait for error message
    await page.waitForSelector('.p-toast-message-error', { timeout: 5000 })

    // Should remain on register page
    await expect(page).toHaveURL(/register/)
  })

  test('should navigate to login page from register', async ({ page }) => {
    // Click on login link
    await page.click('text=Already have an account?')

    // Should navigate to login
    await expectToBeOnLoginPage(page)
  })
})

/**
 * E2E Tests for Registration Form Validation
 */
test.describe('Registration Form Validation', () => {
  let registerPage: RegisterPage

  test.beforeEach(async ({ page }) => {
    registerPage = new RegisterPage(page)
    await registerPage.goto()
  })

  test('should validate name field is required', async ({ page }) => {
    const nameInput = page.locator('input[name="name"]')
    await nameInput.fill('Test')
    await nameInput.clear()
    await nameInput.blur()

    // Should show validation error
    await page.waitForTimeout(200)
    const hasError = await nameInput.evaluate((el) => {
      return el.getAttribute('aria-invalid') === 'true'
    })
    expect(hasError).toBeTruthy()
  })

  test('should validate email format', async ({ page }) => {
    const emailInput = page.locator('input[type="email"]')
    await emailInput.fill('invalid')
    await emailInput.blur()

    // Should show validation error
    await page.waitForTimeout(200)
    const hasError = await emailInput.evaluate((el) => {
      return el.getAttribute('aria-invalid') === 'true'
    })
    expect(hasError).toBeTruthy()
  })

  test('should validate password strength requirements', async ({ page }) => {
    const passwordInput = page.locator('input[name="password"]')

    // Try weak password
    await passwordInput.fill('weak')
    await passwordInput.blur()

    // Should show validation error
    await page.waitForTimeout(200)
    const hasError = await passwordInput.evaluate((el) => {
      return el.getAttribute('aria-invalid') === 'true'
    })
    expect(hasError).toBeTruthy()
  })

  test('should validate company name is required', async ({ page }) => {
    const companyInput = page.locator('input[name="companyName"]')
    await companyInput.fill('Company')
    await companyInput.clear()
    await companyInput.blur()

    // Should show validation error
    await page.waitForTimeout(200)
    const hasError = await companyInput.evaluate((el) => {
      return el.getAttribute('aria-invalid') === 'true'
    })
    expect(hasError).toBeTruthy()
  })
})
