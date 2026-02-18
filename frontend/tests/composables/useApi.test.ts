import { beforeEach, describe, expect, it, vi } from 'vitest'
import { useApi } from '~/composables/useApi'

// Mock useNuxtApp
const mockApiFetch = vi.fn()
const mockUseNuxtApp = vi.fn(() => ({
  $apiFetch: mockApiFetch,
}))

globalThis.useNuxtApp = mockUseNuxtApp

describe('useApi', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  describe('apiFetch', () => {
    it('should expose apiFetch from useNuxtApp', () => {
      const { apiFetch } = useApi()

      expect(apiFetch).toBe(mockApiFetch)
      expect(mockUseNuxtApp).toHaveBeenCalled()
    })

    it('should allow making API calls through apiFetch', async () => {
      mockApiFetch.mockResolvedValue({ data: 'test response' })

      const { apiFetch } = useApi()
      const result = await apiFetch('/test-endpoint')

      expect(result).toEqual({ data: 'test response' })
      expect(mockApiFetch).toHaveBeenCalledWith('/test-endpoint')
    })

    it('should pass through all apiFetch arguments', async () => {
      mockApiFetch.mockResolvedValue({ success: true })

      const { apiFetch } = useApi()
      const options = { method: 'POST', body: { name: 'Test' } }
      await apiFetch('/api/items', options)

      expect(mockApiFetch).toHaveBeenCalledWith('/api/items', options)
    })

    it('should handle API errors', async () => {
      const error = new Error('Network error')
      mockApiFetch.mockRejectedValue(error)

      const { apiFetch } = useApi()

      await expect(apiFetch('/api/error')).rejects.toThrow('Network error')
      expect(mockApiFetch).toHaveBeenCalledWith('/api/error')
    })

    it('should return the same apiFetch instance from useNuxtApp', () => {
      const { apiFetch: apiFetch1 } = useApi()
      const { apiFetch: apiFetch2 } = useApi()

      expect(apiFetch1).toBe(apiFetch2)
      expect(apiFetch1).toBe(mockApiFetch)
    })
  })

  describe('integration with useNuxtApp', () => {
    it('should call useNuxtApp to get $apiFetch', () => {
      mockUseNuxtApp.mockClear()

      useApi()

      expect(mockUseNuxtApp).toHaveBeenCalledTimes(1)
    })

    it('should work with different $apiFetch implementations', async () => {
      const customApiFetch = vi.fn().mockResolvedValue({ custom: true })
      globalThis.useNuxtApp = vi.fn(() => ({ $apiFetch: customApiFetch }))

      const { apiFetch } = useApi()
      const result = await apiFetch('/custom')

      expect(result).toEqual({ custom: true })
      expect(customApiFetch).toHaveBeenCalledWith('/custom')

      // Restore original mock
      globalThis.useNuxtApp = mockUseNuxtApp
    })
  })
})
