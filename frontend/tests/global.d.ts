/**
 * Global type augmentations for test environment.
 * These match the globalThis mock assignments in tests/setup.ts.
 */

/* eslint-disable no-var, vars-on-top */
declare global {
  // Nuxt composables
  var useApi: (...args: unknown[]) => unknown
  var useNuxtApp: (...args: unknown[]) => unknown
  var useRuntimeConfig: (...args: unknown[]) => unknown
  var useI18n: (...args: unknown[]) => unknown
  var useColorMode: (...args: unknown[]) => unknown
  var navigateTo: (...args: unknown[]) => unknown

  // Store mocks
  var useAuthStore: (...args: unknown[]) => unknown
  var useTenantStore: (...args: unknown[]) => unknown
  var useUiStore: (...args: unknown[]) => unknown
  var useNotification: (...args: unknown[]) => unknown

  // Pinia plugin mock
  var persistedState: unknown

  // Vue reactivity globals
  var ref: typeof import('vue').ref
  var reactive: typeof import('vue').reactive
  var computed: typeof import('vue').computed
  var watch: typeof import('vue').watch
  var nextTick: typeof import('vue').nextTick
  var onMounted: typeof import('vue').onMounted
}
/* eslint-enable no-var, vars-on-top */

export {}
