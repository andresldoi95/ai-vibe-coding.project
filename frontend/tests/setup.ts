import { computed, nextTick, onMounted, reactive, ref, watch } from 'vue'
import { vi } from 'vitest'

// Create a mock function that can be reused across tests
const mockApiFetch = vi.fn()

// Create global mock for useApi that will be available in all tests
const useApi = vi.fn(() => ({
  apiFetch: mockApiFetch,
}))

// Create global mock for useNuxtApp (used by some composables like useWarehouseInventory)
const useNuxtApp = vi.fn(() => ({
  $apiFetch: mockApiFetch,
}))

// Create global mock for useRuntimeConfig
const useRuntimeConfig = vi.fn(() => ({
  public: {
    apiBase: 'http://localhost:5000/api',
  },
}))

// Create mutable mock stores that can be modified in tests
const mockAuthStoreData = {
  token: 'test-auth-token',
  user: { id: 'user-1', email: 'test@example.com', name: 'Test User', isActive: true, emailConfirmed: true },
}

const mockTenantStoreData = {
  currentTenantId: 'tenant-123',
  currentTenant: { id: 'tenant-123', name: 'Test Tenant' },
}

// Create global mocks for stores that return the same object reference
const useAuthStore = vi.fn(() => mockAuthStoreData)
const useTenantStore = vi.fn(() => mockTenantStoreData)

// Create global mock for useI18n
const useI18n = vi.fn(() => ({
  locale: ref('en-US'),
  t: (key: string) => key,
}))

// Make them available globally
globalThis.useApi = useApi
globalThis.useNuxtApp = useNuxtApp
globalThis.useRuntimeConfig = useRuntimeConfig
globalThis.useAuthStore = useAuthStore
globalThis.useTenantStore = useTenantStore
globalThis.useI18n = useI18n

// Make Vue reactive functions available globally
globalThis.ref = ref
globalThis.reactive = reactive
globalThis.computed = computed
globalThis.watch = watch
globalThis.nextTick = nextTick
globalThis.onMounted = onMounted

// Export for test files that need to manipulate the mock
export { computed, mockApiFetch, mockAuthStoreData, mockTenantStoreData, nextTick, onMounted, reactive, ref, useApi, useAuthStore, useI18n, useNuxtApp, useRuntimeConfig, useTenantStore, watch }
