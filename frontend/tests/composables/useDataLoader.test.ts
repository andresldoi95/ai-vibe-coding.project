import { beforeEach, describe, expect, it, vi } from 'vitest'
import { useDataLoader } from '~/composables/useDataLoader'

// Mock useNotification
const mockShowError = vi.fn()
const mockShowSuccess = vi.fn()

globalThis.useNotification = vi.fn(() => ({
  showError: mockShowError,
  showSuccess: mockShowSuccess,
}))

// Mock i18n t function (already available from setup.ts, but override for custom translation)
const mockT = vi.fn((key: string) => {
  const translations: Record<string, string> = {
    'messages.error_load': 'Failed to load data',
  }
  return translations[key] || key
})

globalThis.useI18n = vi.fn(() => ({
  t: mockT,
}))

describe('useDataLoader', () => {
  beforeEach(() => {
    // Reset all mocks before each test
    vi.clearAllMocks()
  })

  describe('initialization', () => {
    it('should initialize with null data, false loading, and null error', () => {
      const { data, loading, error } = useDataLoader<string>()

      expect(data.value).toBeNull()
      expect(loading.value).toBe(false)
      expect(error.value).toBeNull()
    })
  })

  describe('load - successful data loading', () => {
    it('should load data successfully and update state', async () => {
      const { data, loading, error, load } = useDataLoader<string>()
      const mockLoader = vi.fn().mockResolvedValue('Test data')

      const loadPromise = load(mockLoader)
      
      // Loading should be true while loading
      expect(loading.value).toBe(true)
      
      await loadPromise
      
      // After loading completes
      expect(data.value).toBe('Test data')
      expect(loading.value).toBe(false)
      expect(error.value).toBeNull()
      expect(mockLoader).toHaveBeenCalledTimes(1)
    })

    it('should load complex object data successfully', async () => {
      interface User {
        id: number
        name: string
      }
      
      const { data, loading, error, load } = useDataLoader<User>()
      const mockUser = { id: 1, name: 'John Doe' }
      const mockLoader = vi.fn().mockResolvedValue(mockUser)

      await load(mockLoader)

      expect(data.value).toEqual(mockUser)
      expect(loading.value).toBe(false)
      expect(error.value).toBeNull()
    })

    it('should not show toast on successful load by default', async () => {
      const { load } = useDataLoader<string>()
      const mockLoader = vi.fn().mockResolvedValue('Success')

      await load(mockLoader)

      expect(mockShowError).not.toHaveBeenCalled()
      expect(mockShowSuccess).not.toHaveBeenCalled()
    })
  })

  describe('load - error handling', () => {
    it('should handle errors and update error state', async () => {
      const { data, loading, error, load } = useDataLoader<string>()
      const testError = new Error('Network error')
      const mockLoader = vi.fn().mockRejectedValue(testError)

      await load(mockLoader)

      expect(data.value).toBeNull()
      expect(loading.value).toBe(false)
      expect(error.value).toBe(testError)
      expect(mockShowError).toHaveBeenCalledTimes(1)
      expect(mockShowError).toHaveBeenCalledWith('Failed to load data', 'Network error')
    })

    it('should convert non-Error objects to Error instances', async () => {
      const { error, load } = useDataLoader<string>()
      const mockLoader = vi.fn().mockRejectedValue('String error')

      await load(mockLoader)

      expect(error.value).toBeInstanceOf(Error)
      expect(error.value?.message).toBe('String error')
    })

    it('should use custom error message when provided', async () => {
      const { load } = useDataLoader<string>()
      const testError = new Error('API Error')
      const mockLoader = vi.fn().mockRejectedValue(testError)

      await load(mockLoader, {
        errorMessage: 'Custom error message',
      })

      expect(mockShowError).toHaveBeenCalledTimes(1)
      expect(mockShowError).toHaveBeenCalledWith('Custom error message', 'API Error')
    })

    it('should not show toast when showToast is false', async () => {
      const { load } = useDataLoader<string>()
      const testError = new Error('Silent error')
      const mockLoader = vi.fn().mockRejectedValue(testError)

      await load(mockLoader, {
        showToast: false,
      })

      expect(mockShowError).not.toHaveBeenCalled()
    })
  })

  describe('onSuccess callback', () => {
    it('should call onSuccess callback after successful load', async () => {
      const { load } = useDataLoader<string>()
      const mockLoader = vi.fn().mockResolvedValue('Data')
      const mockOnSuccess = vi.fn()

      await load(mockLoader, {
        onSuccess: mockOnSuccess,
      })

      expect(mockOnSuccess).toHaveBeenCalledTimes(1)
    })

    it('should not call onSuccess callback on error', async () => {
      const { load } = useDataLoader<string>()
      const mockLoader = vi.fn().mockRejectedValue(new Error('Error'))
      const mockOnSuccess = vi.fn()

      await load(mockLoader, {
        onSuccess: mockOnSuccess,
        showToast: false,
      })

      expect(mockOnSuccess).not.toHaveBeenCalled()
    })
  })

  describe('onError callback', () => {
    it('should call onError callback with error object on failure', async () => {
      const { load } = useDataLoader<string>()
      const testError = new Error('Test error')
      const mockLoader = vi.fn().mockRejectedValue(testError)
      const mockOnError = vi.fn()

      await load(mockLoader, {
        onError: mockOnError,
        showToast: false,
      })

      expect(mockOnError).toHaveBeenCalledTimes(1)
      expect(mockOnError).toHaveBeenCalledWith(testError)
    })

    it('should not call onError callback on success', async () => {
      const { load } = useDataLoader<string>()
      const mockLoader = vi.fn().mockResolvedValue('Data')
      const mockOnError = vi.fn()

      await load(mockLoader, {
        onError: mockOnError,
      })

      expect(mockOnError).not.toHaveBeenCalled()
    })
  })

  describe('reload function', () => {
    it('should reload using the last loader and options', async () => {
      const { load, reload } = useDataLoader<string>()
      const mockLoader = vi.fn()
        .mockResolvedValueOnce('First load')
        .mockResolvedValueOnce('Second load')

      // First load
      await load(mockLoader)
      expect(mockLoader).toHaveBeenCalledTimes(1)

      // Reload
      await reload()
      expect(mockLoader).toHaveBeenCalledTimes(2)
    })

    it('should preserve options when reloading', async () => {
      const { load, reload } = useDataLoader<string>()
      const mockLoader = vi.fn()
        .mockResolvedValueOnce('First')
        .mockResolvedValueOnce('Second')
      const mockOnSuccess = vi.fn()

      // First load with options
      await load(mockLoader, {
        onSuccess: mockOnSuccess,
      })

      expect(mockOnSuccess).toHaveBeenCalledTimes(1)

      // Reload should use same options
      await reload()
      expect(mockOnSuccess).toHaveBeenCalledTimes(2)
    })

    it('should do nothing if no loader has been called yet', async () => {
      const { data, reload } = useDataLoader<string>()

      await reload()

      expect(data.value).toBeNull()
    })

    it('should update data on reload', async () => {
      const { data, load, reload } = useDataLoader<number>()
      const mockLoader = vi.fn()
        .mockResolvedValueOnce(1)
        .mockResolvedValueOnce(2)

      await load(mockLoader)
      expect(data.value).toBe(1)

      await reload()
      expect(data.value).toBe(2)
    })
  })

  describe('showToast option', () => {
    it('should show error toast by default when loading fails', async () => {
      const { load } = useDataLoader<string>()
      const testError = new Error('Default toast error')
      const mockLoader = vi.fn().mockRejectedValue(testError)

      await load(mockLoader)

      expect(mockShowError).toHaveBeenCalledTimes(1)
    })

    it('should show error toast when showToast is explicitly true', async () => {
      const { load } = useDataLoader<string>()
      const testError = new Error('Explicit toast error')
      const mockLoader = vi.fn().mockRejectedValue(testError)

      await load(mockLoader, {
        showToast: true,
      })

      expect(mockShowError).toHaveBeenCalledTimes(1)
    })

    it('should not show error toast when showToast is false', async () => {
      const { load } = useDataLoader<string>()
      const testError = new Error('No toast error')
      const mockLoader = vi.fn().mockRejectedValue(testError)

      await load(mockLoader, {
        showToast: false,
      })

      expect(mockShowError).not.toHaveBeenCalled()
    })
  })

  describe('state reactivity', () => {
    it('should clear error on new successful load', async () => {
      const { error, load } = useDataLoader<string>()
      
      // First load fails
      const mockLoader1 = vi.fn().mockRejectedValue(new Error('First error'))
      await load(mockLoader1, { showToast: false })
      expect(error.value).not.toBeNull()

      // Second load succeeds
      const mockLoader2 = vi.fn().mockResolvedValue('Success')
      await load(mockLoader2)
      expect(error.value).toBeNull()
    })

    it('should set loading to false after both success and error', async () => {
      const { loading, load } = useDataLoader<string>()

      // Success case
      const mockLoader1 = vi.fn().mockResolvedValue('Success')
      await load(mockLoader1)
      expect(loading.value).toBe(false)

      // Error case
      const mockLoader2 = vi.fn().mockRejectedValue(new Error('Error'))
      await load(mockLoader2, { showToast: false })
      expect(loading.value).toBe(false)
    })
  })

  describe('integration scenarios', () => {
    it('should handle complete success workflow with all callbacks', async () => {
      const { data, loading, error, load } = useDataLoader<string>()
      const mockLoader = vi.fn().mockResolvedValue('Complete data')
      const mockOnSuccess = vi.fn()

      await load(mockLoader, {
        onSuccess: mockOnSuccess,
      })

      expect(data.value).toBe('Complete data')
      expect(loading.value).toBe(false)
      expect(error.value).toBeNull()
      expect(mockLoader).toHaveBeenCalledTimes(1)
      expect(mockOnSuccess).toHaveBeenCalledTimes(1)
      expect(mockShowError).not.toHaveBeenCalled()
    })

    it('should handle complete error workflow with all callbacks', async () => {
      const { data, loading, error, load } = useDataLoader<string>()
      const testError = new Error('Complete error')
      const mockLoader = vi.fn().mockRejectedValue(testError)
      const mockOnError = vi.fn()

      await load(mockLoader, {
        errorMessage: 'Operation failed',
        onError: mockOnError,
      })

      expect(data.value).toBeNull()
      expect(loading.value).toBe(false)
      expect(error.value).toBe(testError)
      expect(mockLoader).toHaveBeenCalledTimes(1)
      expect(mockOnError).toHaveBeenCalledTimes(1)
      expect(mockOnError).toHaveBeenCalledWith(testError)
      expect(mockShowError).toHaveBeenCalledWith('Operation failed', 'Complete error')
    })
  })
})
