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
// Create global mock for useI18n
const mockI18nLocale = ref('en-US')
const mockI18nLocales = ref([
  { code: 'en-US', name: 'English (US)' },
  { code: 'es-ES', name: 'Español' },
])
const mockSetLocale = vi.fn((locale: string) => {
  mockI18nLocale.value = locale
})

const useI18n = vi.fn(() => ({
  locale: computed({
    get: () => mockI18nLocale.value,
    set: (value: string) => mockSetLocale(value),
  }),
  locales: mockI18nLocales,
  setLocale: mockSetLocale,
  t: (key: string) => key,
}))

// Create global mock for useColorMode (Nuxt Color Mode)
const mockColorModePreference = ref<string>('system')
const useColorMode = vi.fn(() => ({
  preference: computed({
    get: () => mockColorModePreference.value,
    set: (value: string) => { mockColorModePreference.value = value },
  }),
  value: computed(() => mockColorModePreference.value === 'system' ? 'light' : mockColorModePreference.value),
}))

// Make them available globally
globalThis.useApi = useApi
globalThis.useNuxtApp = useNuxtApp
globalThis.useRuntimeConfig = useRuntimeConfig
globalThis.useAuthStore = useAuthStore
globalThis.useTenantStore = useTenantStore
globalThis.useI18n = useI18n
globalThis.useColorMode = useColorMode

// Create global mock for persistedState (used by Pinia stores with persistence)
const persistedState = {
  localStorage: {
    getItem: vi.fn(),
    setItem: vi.fn(),
    removeItem: vi.fn(),
    clear: vi.fn(),
  },
  sessionStorage: {
    getItem: vi.fn(),
    setItem: vi.fn(),
    removeItem: vi.fn(),
    clear: vi.fn(),
  },
  cookieOptions: {},
}

globalThis.persistedState = persistedState

// Make Vue reactive functions available globally
globalThis.ref = ref
globalThis.reactive = reactive
globalThis.computed = computed
globalThis.watch = watch
globalThis.nextTick = nextTick
globalThis.onMounted = onMounted

// Export for test files that need to manipulate the mock
export {
  computed,
  mockApiFetch,
  mockAuthStoreData,
  mockColorModePreference,
  mockI18nLocale,
  mockI18nLocales,
  mockSetLocale,
  mockTenantStoreData,
  nextTick,
  onMounted,
  reactive,
  ref,
  useApi,
  useAuthStore,
  useColorMode,
  useI18n,
  useNuxtApp,
  useRuntimeConfig,
  useTenantStore,
  watch,
}
