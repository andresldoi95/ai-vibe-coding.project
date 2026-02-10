import { vi } from 'vitest'

// Create a mock function that can be reused across tests
const mockApiFetch = vi.fn()

// Create global mock for useApi that will be available in all tests
const useApi = vi.fn(() => ({
  apiFetch: mockApiFetch,
}))

// Mock stores
const mockAuthStore = {
  token: 'mock-token-123',
}

const mockTenantStore = {
  currentTenantId: 'tenant-123',
}

// Mock runtime config
const mockRuntimeConfig = {
  public: {
    apiBase: 'http://localhost:3001',
  },
}

// Make them available globally
globalThis.useApi = useApi
globalThis.useRuntimeConfig = vi.fn(() => mockRuntimeConfig)
globalThis.useAuthStore = vi.fn(() => mockAuthStore)
globalThis.useTenantStore = vi.fn(() => mockTenantStore)

// Export for test files that need to manipulate the mocks
export { mockApiFetch, mockAuthStore, mockRuntimeConfig, mockTenantStore, useApi }
