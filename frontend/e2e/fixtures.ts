import type { Page } from '@playwright/test'

/**
 * Test credentials for E2E testing
 * These should match the seeded test data in the backend
 */
export const TEST_CREDENTIALS = {
  admin: {
    email: 'admin@example.com',
    password: 'Admin123!',
  },
  user: {
    email: 'user@example.com',
    password: 'User123!',
  },
  invalid: {
    email: 'invalid@example.com',
    password: 'WrongPassword123!',
  },
}

/**
 * Page Object for Login Page
 * Encapsulates login page interactions
 */
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

  async getEmailInput() {
    return this.page.locator('input[type="email"]')
  }

  async getPasswordInput() {
    return this.page.locator('input[type="password"]')
  }

  async getSubmitButton() {
    return this.page.locator('button[type="submit"]')
  }

  async getErrorMessage() {
    return this.page.locator('.p-toast-message-error')
  }

  async waitForSuccessMessage() {
    await this.page.waitForSelector('.p-toast-message-success', { timeout: 5000 })
  }

  async waitForErrorMessage() {
    await this.page.waitForSelector('.p-toast-message-error', { timeout: 5000 })
  }

  async clickForgotPassword() {
    await this.page.click('text=Forgot password?')
  }

  async clickRegisterLink() {
    await this.page.click('text=Register')
  }
}

/**
 * Page Object for Register Page
 */
export class RegisterPage {
  constructor(private page: Page) {}

  async goto() {
    await this.page.goto('/register')
  }

  async register(data: {
    name: string
    email: string
    password: string
    confirmPassword: string
    companyName: string
  }) {
    await this.page.fill('input[name="name"]', data.name)
    await this.page.fill('input[type="email"]', data.email)
    await this.page.fill('input[name="password"]', data.password)
    await this.page.fill('input[name="confirmPassword"]', data.confirmPassword)
    await this.page.fill('input[name="companyName"]', data.companyName)
    await this.page.click('button[type="submit"]')
  }

  async waitForSuccessMessage() {
    await this.page.waitForSelector('.p-toast-message-success', { timeout: 5000 })
  }
}

/**
 * Page Object for Dashboard Page
 */
export class DashboardPage {
  constructor(private page: Page) {}

  async isVisible() {
    return await this.page.isVisible('h1:has-text("Dashboard")')
  }

  async waitForLoad() {
    await this.page.waitForLoadState('networkidle')
  }
}

/**
 * Common assertions and utilities
 */
export async function expectToBeOnLoginPage(page: Page) {
  await page.waitForURL('**/login')
}

export async function expectToBeOnDashboard(page: Page) {
  await page.waitForURL('**/')
}

export async function expectToBeOnRegisterPage(page: Page) {
  await page.waitForURL('**/register')
}

export async function logout(page: Page) {
  await page.click('[data-testid="user-menu"]')
  await page.click('text=Logout')
  await expectToBeOnLoginPage(page)
}
