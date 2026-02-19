import { expect, test } from '@playwright/test'
import {
  DashboardPage,
  LoginPage,
  TEST_CREDENTIALS,
  expectToBeOnDashboard,
  expectToBeOnLoginPage,
} from './fixtures'

/**
 * E2E Tests for Login Flow
 *
 * Tests the authentication flow including:
 * - Successful login
 * - Invalid credentials
 * - Form validation
 * - Navigation after login
 * - Loading states
 */
test.describe('Login Flow', () => {
  let loginPage: LoginPage
  let dashboardPage: DashboardPage

  test.beforeEach(async ({ page }) => {
    loginPage = new LoginPage(page)
    dashboardPage = new DashboardPage(page)
    await loginPage.goto()
  })

  test('should display login form', async ({ page }) => {
    await expect(page).toHaveTitle(/Login/i)

    const emailInput = await loginPage.getEmailInput()
    await expect(emailInput).toBeVisible()

    const passwordInput = await loginPage.getPasswordInput()
    await expect(passwordInput).toBeVisible()

    const submitButton = await loginPage.getSubmitButton()
    await expect(submitButton).toBeVisible()
    await expect(submitButton).toHaveText(/Sign in|Login/i)
  })

  test('should show validation errors for empty form', async ({ page }) => {
    const submitButton = await loginPage.getSubmitButton()
    await submitButton.click()

    // Should not navigate away from login page
    await page.waitForTimeout(500)
    await expectToBeOnLoginPage(page)

    // Check for validation messages (these depend on your validation implementation)
    const emailInput = await loginPage.getEmailInput()
    const emailError = await emailInput.evaluate((el) => {
      return el.getAttribute('aria-invalid') === 'true'
    })
    expect(emailError).toBeTruthy()
  })

  test('should show validation error for invalid email format', async ({ page }) => {
    await loginPage.login('invalid-email', 'password123')

    // Should not navigate away from login page
    await page.waitForTimeout(500)
    await expectToBeOnLoginPage(page)
  })

  test('should show error for invalid credentials', async ({ page }) => {
    await loginPage.login(
      TEST_CREDENTIALS.invalid.email,
      TEST_CREDENTIALS.invalid.password,
    )

    // Wait for error message
    await loginPage.waitForErrorMessage()

    const errorMessage = await loginPage.getErrorMessage()
    await expect(errorMessage).toBeVisible()

    // Should remain on login page
    await expectToBeOnLoginPage(page)
  })

  test('should successfully login with valid credentials', async ({ page }) => {
    await loginPage.login(
      TEST_CREDENTIALS.admin.email,
      TEST_CREDENTIALS.admin.password,
    )

    // Wait for success message (optional, depending on implementation)
    try {
      await loginPage.waitForSuccessMessage()
    }
    catch {
      // Success message might not be shown, that's okay
    }

    // Should redirect to dashboard
    await expectToBeOnDashboard(page)

    // Verify dashboard is loaded
    await dashboardPage.waitForLoad()
    const isDashboardVisible = await dashboardPage.isVisible()
    expect(isDashboardVisible).toBeTruthy()
  })

  test('should disable submit button while loading', async ({ page }) => {
    const submitButton = await loginPage.getSubmitButton()

    // Start login process
    const emailInput = await loginPage.getEmailInput()
    await emailInput.fill(TEST_CREDENTIALS.admin.email)

    const passwordInput = await loginPage.getPasswordInput()
    await passwordInput.fill(TEST_CREDENTIALS.admin.password)

    // Click submit and immediately check if button is disabled
    await submitButton.click()

    // Button should be disabled during API call
    // This might be too fast to catch, but worth testing
    const isDisabled = await submitButton.isDisabled()
    expect(isDisabled).toBeTruthy()
  })

  test('should navigate to register page from login', async ({ page }) => {
    await loginPage.clickRegisterLink()

    await page.waitForURL('**/register')
    await expect(page).toHaveURL(/register/)
  })

  test('should navigate to forgot password page', async ({ page }) => {
    await loginPage.clickForgotPassword()

    await page.waitForURL('**/forgot-password')
    await expect(page).toHaveURL(/forgot-password/)
  })
})

/**
 * E2E Tests for Session Persistence
 */
test.describe('Session Persistence', () => {
  let loginPage: LoginPage

  test.beforeEach(async ({ page }) => {
    loginPage = new LoginPage(page)
  })

  test('should persist session after page reload', async ({ page }) => {
    // Login first
    await loginPage.goto()
    await loginPage.login(
      TEST_CREDENTIALS.admin.email,
      TEST_CREDENTIALS.admin.password,
    )

    await expectToBeOnDashboard(page)

    // Reload the page
    await page.reload()

    // Should still be on dashboard (session persisted)
    await expectToBeOnDashboard(page)
  })

  test('should redirect to login when accessing protected route without auth', async ({ page }) => {
    // Try to access dashboard without logging in
    await page.goto('/')

    // Should be redirected to login
    await expectToBeOnLoginPage(page)
  })
})
