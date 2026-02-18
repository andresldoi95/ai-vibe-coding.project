import { beforeEach, describe, expect, it, vi } from 'vitest'
import { useDataLoader } from '~/composables/useDataLoader'

// Mock useNotification
const mockShowError = vi.fn()
const mockUseNotification = vi.fn(() => ({
  showError: mockShowError,
}))

globalThis.useNotification = mockUseNotification

describe('useDataLoader', () => {
  beforeEach(() => {
    mockShowError.mockClear()
    mockUseNotification.mockClear()
  })

  describe('initial state', () => {
    it('should have null data initially', () => {
      const { data } = useDataLoader()

      expect(data.value).toBeNull()
    })

    it('should not be loading initially', () => {
      const { loading } = useDataLoader()

      expect(loading.value).toBe(false)
    })

    it('should have no error initially', () => {
      const { error } = useDataLoader()

      expect(error.value).toBeNull()
    })
  })

  describe('load - successful', () => {
    it('should load data successfully', async () => {
      const mockData = { id: '1', name: 'Test Data' }
      const loader = vi.fn().mockResolvedValue(mockData)

      const { data, load } = useDataLoader()

      await load(loader)

      expect(data.value).toEqual(mockData)
      expect(loader).toHaveBeenCalledTimes(1)
    })

    it('should set loading state during load', async () => {
      const mockData = { id: '1', name: 'Test' }
      let resolvePromise: (value: unknown) => void
      const promise = new Promise((resolve) => {
        resolvePromise = resolve
      })
      const loader = vi.fn().mockReturnValue(promise)

      const { loading, load } = useDataLoader()

      const loadPromise = load(loader)
      expect(loading.value).toBe(true)

      resolvePromise!(mockData)
      await loadPromise

      expect(loading.value).toBe(false)
    })

    it('should clear error on successful load', async () => {
      const loader1 = vi.fn().mockRejectedValue(new Error('First error'))
      const loader2 = vi.fn().mockResolvedValue({ data: 'success' })

      const { error, load } = useDataLoader()

      await load(loader1)
      expect(error.value).not.toBeNull()

      await load(loader2)
      expect(error.value).toBeNull()
    })

    it('should call onSuccess callback', async () => {
      const mockData = { id: '1' }
      const loader = vi.fn().mockResolvedValue(mockData)
      const onSuccess = vi.fn()

      const { load } = useDataLoader()

      await load(loader, { onSuccess })

      expect(onSuccess).toHaveBeenCalledTimes(1)
    })

    it('should not show toast on success by default', async () => {
      const mockData = { id: '1' }
      const loader = vi.fn().mockResolvedValue(mockData)

      const { load } = useDataLoader()

      await load(loader)

      expect(mockShowError).not.toHaveBeenCalled()
    })
  })

  describe('load - error handling', () => {
    it('should handle errors and set error state', async () => {
      const mockError = new Error('Load failed')
      const loader = vi.fn().mockRejectedValue(mockError)

      const { error, load } = useDataLoader()

      await load(loader)

      expect(error.value).toEqual(mockError)
    })

    it('should set loading to false after error', async () => {
      const loader = vi.fn().mockRejectedValue(new Error('Failed'))

      const { loading, load } = useDataLoader()

      await load(loader)

      expect(loading.value).toBe(false)
    })

    it('should show error toast by default', async () => {
      const mockError = new Error('Network error')
      const loader = vi.fn().mockRejectedValue(mockError)

      const { load } = useDataLoader()

      await load(loader)

      expect(mockShowError).toHaveBeenCalledWith(
        'messages.error_load',
        'Network error',
      )
    })

    it('should show custom error message when provided', async () => {
      const mockError = new Error('API error')
      const loader = vi.fn().mockRejectedValue(mockError)

      const { load } = useDataLoader()

      await load(loader, { errorMessage: 'Custom error message' })

      expect(mockShowError).toHaveBeenCalledWith(
        'Custom error message',
        'API error',
      )
    })

    it('should not show toast when showToast is false', async () => {
      const loader = vi.fn().mockRejectedValue(new Error('Failed'))

      const { load } = useDataLoader()

      await load(loader, { showToast: false })

      expect(mockShowError).not.toHaveBeenCalled()
    })

    it('should call onError callback', async () => {
      const mockError = new Error('Load error')
      const loader = vi.fn().mockRejectedValue(mockError)
      const onError = vi.fn()

      const { load } = useDataLoader()

      await load(loader, { onError })

      expect(onError).toHaveBeenCalledWith(mockError)
      expect(onError).toHaveBeenCalledTimes(1)
    })

    it('should convert non-Error objects to Error', async () => {
      const loader = vi.fn().mockRejectedValue('String error')

      const { error, load } = useDataLoader()

      await load(loader)

      expect(error.value).toBeInstanceOf(Error)
      expect(error.value?.message).toBe('String error')
    })
  })

  describe('reload', () => {
    it('should reload with same loader', async () => {
      const mockData1 = { id: '1', value: 'first' }
      const mockData2 = { id: '2', value: 'second' }
      const loader = vi.fn()
        .mockResolvedValueOnce(mockData1)
        .mockResolvedValueOnce(mockData2)

      const { data, load, reload } = useDataLoader()

      await load(loader)
      expect(data.value).toEqual(mockData1)

      await reload()
      expect(data.value).toEqual(mockData2)
      expect(loader).toHaveBeenCalledTimes(2)
    })

    it('should reload with same options', async () => {
      const mockData = { id: '1' }
      const loader = vi.fn().mockResolvedValue(mockData)
      const onSuccess = vi.fn()

      const { load, reload } = useDataLoader()

      await load(loader, { onSuccess })
      expect(onSuccess).toHaveBeenCalledTimes(1)

      await reload()
      expect(onSuccess).toHaveBeenCalledTimes(2)
    })

    it('should do nothing if no loader has been called', async () => {
      const { data, reload } = useDataLoader()

      await reload()

      expect(data.value).toBeNull()
    })

    it('should update to latest loader', async () => {
      const loader1 = vi.fn().mockResolvedValue({ id: '1' })
      const loader2 = vi.fn().mockResolvedValue({ id: '2' })

      const { data, load, reload } = useDataLoader()

      await load(loader1)
      expect(data.value).toEqual({ id: '1' })

      await load(loader2)
      expect(data.value).toEqual({ id: '2' })

      await reload()
      expect(data.value).toEqual({ id: '2' })
      expect(loader1).toHaveBeenCalledTimes(1)
      expect(loader2).toHaveBeenCalledTimes(2)
    })
  })

  describe('generic type support', () => {
    it('should work with string data type', async () => {
      const loader = vi.fn().mockResolvedValue('Hello World')

      const { data, load } = useDataLoader<string>()

      await load(loader)

      expect(data.value).toBe('Hello World')
      expect(typeof data.value).toBe('string')
    })

    it('should work with array data type', async () => {
      const mockArray = [1, 2, 3, 4, 5]
      const loader = vi.fn().mockResolvedValue(mockArray)

      const { data, load } = useDataLoader<number[]>()

      await load(loader)

      expect(data.value).toEqual(mockArray)
      expect(Array.isArray(data.value)).toBe(true)
    })

    it('should work with complex object type', async () => {
      interface User {
        id: string
        name: string
        email: string
        roles: string[]
      }

      const mockUser: User = {
        id: '1',
        name: 'John Doe',
        email: 'john@example.com',
        roles: ['admin', 'user'],
      }

      const loader = vi.fn().mockResolvedValue(mockUser)

      const { data, load } = useDataLoader<User>()

      await load(loader)

      expect(data.value).toEqual(mockUser)
      expect(data.value?.roles).toHaveLength(2)
    })
  })

  describe('multiple loads', () => {
    it('should handle multiple sequential loads', async () => {
      const loader1 = vi.fn().mockResolvedValue({ id: '1' })
      const loader2 = vi.fn().mockResolvedValue({ id: '2' })
      const loader3 = vi.fn().mockResolvedValue({ id: '3' })

      const { data, load } = useDataLoader()

      await load(loader1)
      expect(data.value).toEqual({ id: '1' })

      await load(loader2)
      expect(data.value).toEqual({ id: '2' })

      await load(loader3)
      expect(data.value).toEqual({ id: '3' })
    })

    it('should reset error between loads', async () => {
      const loader1 = vi.fn().mockRejectedValue(new Error('Error 1'))
      const loader2 = vi.fn().mockResolvedValue({ success: true })

      const { error, load } = useDataLoader()

      await load(loader1)
      expect(error.value).not.toBeNull()

      await load(loader2)
      expect(error.value).toBeNull()
    })
  })

  describe('edge cases', () => {
    it('should handle null data from loader', async () => {
      const loader = vi.fn().mockResolvedValue(null)

      const { data, load } = useDataLoader()

      await load(loader)

      expect(data.value).toBeNull()
    })

    it('should handle undefined data from loader', async () => {
      const loader = vi.fn().mockResolvedValue(undefined)

      const { data, load } = useDataLoader()

      await load(loader)

      expect(data.value).toBeUndefined()
    })

    it('should handle empty object from loader', async () => {
      const loader = vi.fn().mockResolvedValue({})

      const { data, load } = useDataLoader()

      await load(loader)

      expect(data.value).toEqual({})
    })

    it('should call both onSuccess and onError correctly', async () => {
      const onSuccess = vi.fn()
      const onError = vi.fn()
      const loader1 = vi.fn().mockResolvedValue({ success: true })
      const loader2 = vi.fn().mockRejectedValue(new Error('Failed'))

      const { load } = useDataLoader()

      await load(loader1, { onSuccess, onError })
      expect(onSuccess).toHaveBeenCalledTimes(1)
      expect(onError).not.toHaveBeenCalled()

      await load(loader2, { onSuccess, onError })
      expect(onSuccess).toHaveBeenCalledTimes(1)
      expect(onError).toHaveBeenCalledTimes(1)
    })
  })
})
