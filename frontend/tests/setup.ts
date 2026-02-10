import { vi } from 'vitest'

// Create a mock function that can be reused across tests
const mockApiFetch = vi.fn()

// Create global mock for useApi that will be available in all tests
const useApi = vi.fn(() => ({
  apiFetch: mockApiFetch,
}))

// Make it available globally
globalThis.useApi = useApi

// Export for test files that need to manipulate the mock
export { mockApiFetch, useApi }
