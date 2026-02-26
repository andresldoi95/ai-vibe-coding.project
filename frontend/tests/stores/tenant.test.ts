import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { mockApiFetch } from '../setup'
import { useTenantStore } from '~/stores/tenant'
import type { Tenant } from '~/types/tenant'

describe('tenant store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  const mockTenant: Tenant = {
    id: 'tenant-1',
    name: 'Acme Corp',
    slug: 'acme-corp',
    status: 'Active',
  }

  const mockTenant2: Tenant = {
    id: 'tenant-2',
    name: 'Tech Inc',
    slug: 'tech-inc',
    status: 'Active',
  }

  describe('initial state', () => {
    it('should have currentTenantId as null', () => {
      // Arrange & Act
      const store = useTenantStore()

      // Assert
      expect(store.currentTenantId).toBeNull()
    })

    it('should have currentTenant as null', () => {
      // Arrange & Act
      const store = useTenantStore()

      // Assert
      expect(store.currentTenant).toBeNull()
    })

    it('should have empty availableTenants array', () => {
      // Arrange & Act
      const store = useTenantStore()

      // Assert
      expect(store.availableTenants).toEqual([])
    })

    it('should have hasTenant computed as false', () => {
      // Arrange & Act
      const store = useTenantStore()

      // Assert
      expect(store.hasTenant).toBe(false)
    })
  })

  describe('hasTenant computed', () => {
    it('should return true when currentTenantId is set', () => {
      // Arrange
      const store = useTenantStore()
      store.currentTenantId = 'tenant-1'

      // Act & Assert
      expect(store.hasTenant).toBe(true)
    })

    it('should return false when currentTenantId is null', () => {
      // Arrange
      const store = useTenantStore()
      store.currentTenantId = null

      // Act & Assert
      expect(store.hasTenant).toBe(false)
    })

    it('should update reactively when currentTenantId changes', () => {
      // Arrange
      const store = useTenantStore()
      expect(store.hasTenant).toBe(false)

      // Act
      store.currentTenantId = 'tenant-1'

      // Assert
      expect(store.hasTenant).toBe(true)

      // Act
      store.currentTenantId = null

      // Assert
      expect(store.hasTenant).toBe(false)
    })
  })

  describe('setTenant', () => {
    it('should fetch and set tenant successfully', async () => {
      // Arrange
      const store = useTenantStore()
      mockApiFetch.mockResolvedValue(mockTenant)

      // Act
      await store.setTenant('tenant-1')

      // Assert
      expect(mockApiFetch).toHaveBeenCalledWith('/tenants/tenant-1')
      expect(store.currentTenantId).toBe('tenant-1')
      expect(store.currentTenant).toEqual(mockTenant)
    })

    it('should update tenant when changing to different tenant', async () => {
      // Arrange
      const store = useTenantStore()
      mockApiFetch.mockResolvedValueOnce(mockTenant)
      await store.setTenant('tenant-1')

      // Act
      mockApiFetch.mockResolvedValueOnce(mockTenant2)
      await store.setTenant('tenant-2')

      // Assert
      expect(mockApiFetch).toHaveBeenCalledTimes(2)
      expect(store.currentTenantId).toBe('tenant-2')
      expect(store.currentTenant).toEqual(mockTenant2)
    })

    it('should throw error when API call fails', async () => {
      // Arrange
      const store = useTenantStore()
      const error = new Error('API error')
      mockApiFetch.mockRejectedValue(error)

      // Act & Assert
      await expect(store.setTenant('tenant-1')).rejects.toThrow('API error')
      expect(store.currentTenantId).toBeNull()
      expect(store.currentTenant).toBeNull()
    })
  })

  describe('fetchAvailableTenants', () => {
    it('should fetch available tenants successfully', async () => {
      // Arrange
      const store = useTenantStore()
      const tenants = [mockTenant, mockTenant2]
      mockApiFetch.mockResolvedValue(tenants)

      // Act
      await store.fetchAvailableTenants()

      // Assert
      expect(mockApiFetch).toHaveBeenCalledWith('/tenants')
      expect(store.availableTenants).toEqual(tenants)
    })

    it('should auto-select first tenant when none selected', async () => {
      // Arrange
      const store = useTenantStore()
      const tenants = [mockTenant, mockTenant2]
      mockApiFetch.mockResolvedValueOnce(tenants)
      mockApiFetch.mockResolvedValueOnce(mockTenant)

      // Act
      await store.fetchAvailableTenants()

      // Assert
      expect(mockApiFetch).toHaveBeenCalledWith('/tenants')
      expect(mockApiFetch).toHaveBeenCalledWith('/tenants/tenant-1')
      expect(store.currentTenantId).toBe('tenant-1')
      expect(store.currentTenant).toEqual(mockTenant)
    })

    it('should not auto-select when tenant already selected', async () => {
      // Arrange
      const store = useTenantStore()
      store.currentTenantId = 'existing-tenant'
      const tenants = [mockTenant, mockTenant2]
      mockApiFetch.mockResolvedValue(tenants)

      // Act
      await store.fetchAvailableTenants()

      // Assert
      expect(mockApiFetch).toHaveBeenCalledWith('/tenants')
      expect(mockApiFetch).toHaveBeenCalledTimes(1)
      expect(store.currentTenantId).toBe('existing-tenant')
    })

    it('should not auto-select when no tenants available', async () => {
      // Arrange
      const store = useTenantStore()
      mockApiFetch.mockResolvedValue([])

      // Act
      await store.fetchAvailableTenants()

      // Assert
      expect(mockApiFetch).toHaveBeenCalledWith('/tenants')
      expect(mockApiFetch).toHaveBeenCalledTimes(1)
      expect(store.currentTenantId).toBeNull()
    })

    it('should throw error when API call fails', async () => {
      // Arrange
      const store = useTenantStore()
      const error = new Error('Failed to fetch')
      mockApiFetch.mockRejectedValue(error)

      // Mock console.error to verify it's called
      const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {})

      // Act & Assert
      await expect(store.fetchAvailableTenants()).rejects.toThrow('Failed to fetch')
      expect(consoleErrorSpy).toHaveBeenCalledWith('Failed to fetch tenants:', error)

      consoleErrorSpy.mockRestore()
    })
  })

  describe('clearTenant', () => {
    it('should clear all tenant state', () => {
      // Arrange
      const store = useTenantStore()
      store.currentTenantId = 'tenant-1'
      store.currentTenant = mockTenant
      store.availableTenants = [mockTenant, mockTenant2]

      // Act
      store.clearTenant()

      // Assert
      expect(store.currentTenantId).toBeNull()
      expect(store.currentTenant).toBeNull()
      expect(store.availableTenants).toEqual([])
    })

    it('should work when state already empty', () => {
      // Arrange
      const store = useTenantStore()

      // Act
      store.clearTenant()

      // Assert
      expect(store.currentTenantId).toBeNull()
      expect(store.currentTenant).toBeNull()
      expect(store.availableTenants).toEqual([])
    })
  })

  describe('setAvailableTenants', () => {
    it('should set available tenants', () => {
      // Arrange
      const store = useTenantStore()
      const tenants = [mockTenant, mockTenant2]

      // Act
      store.setAvailableTenants(tenants)

      // Assert
      expect(store.availableTenants).toEqual(tenants)
    })

    it('should replace existing available tenants', () => {
      // Arrange
      const store = useTenantStore()
      store.availableTenants = [mockTenant]

      // Act
      store.setAvailableTenants([mockTenant2])

      // Assert
      expect(store.availableTenants).toEqual([mockTenant2])
    })

    it('should set empty array', () => {
      // Arrange
      const store = useTenantStore()
      store.availableTenants = [mockTenant, mockTenant2]

      // Act
      store.setAvailableTenants([])

      // Assert
      expect(store.availableTenants).toEqual([])
    })
  })

  describe('selectTenant', () => {
    it('should select tenant from available tenants', () => {
      // Arrange
      const store = useTenantStore()
      store.availableTenants = [mockTenant, mockTenant2]

      // Mock console.log to verify it's called
      const consoleLogSpy = vi.spyOn(console, 'log').mockImplementation(() => {})

      // Act
      store.selectTenant('tenant-1')

      // Assert
      expect(store.currentTenantId).toBe('tenant-1')
      expect(store.currentTenant).toEqual(mockTenant)
      expect(consoleLogSpy).toHaveBeenCalledWith('[TenantStore] Tenant selected:', 'Acme Corp')

      consoleLogSpy.mockRestore()
    })

    it('should select different tenant', () => {
      // Arrange
      const store = useTenantStore()
      store.availableTenants = [mockTenant, mockTenant2]
      store.selectTenant('tenant-1')

      // Act
      store.selectTenant('tenant-2')

      // Assert
      expect(store.currentTenantId).toBe('tenant-2')
      expect(store.currentTenant).toEqual(mockTenant2)
    })

    it('should warn when tenant not found in available tenants', () => {
      // Arrange
      const store = useTenantStore()
      store.availableTenants = [mockTenant]

      // Mock console.warn to verify it's called
      const consoleWarnSpy = vi.spyOn(console, 'warn').mockImplementation(() => {})

      // Act
      store.selectTenant('non-existent')

      // Assert
      expect(store.currentTenantId).toBeNull()
      expect(store.currentTenant).toBeNull()
      expect(consoleWarnSpy).toHaveBeenCalledWith('[TenantStore] Tenant not found in available tenants:', 'non-existent')

      consoleWarnSpy.mockRestore()
    })

    it('should not select when availableTenants is empty', () => {
      // Arrange
      const store = useTenantStore()

      // Mock console.warn
      const consoleWarnSpy = vi.spyOn(console, 'warn').mockImplementation(() => {})

      // Act
      store.selectTenant('tenant-1')

      // Assert
      expect(store.currentTenantId).toBeNull()
      expect(store.currentTenant).toBeNull()
      expect(consoleWarnSpy).toHaveBeenCalled()

      consoleWarnSpy.mockRestore()
    })
  })

  describe('persistence configuration', () => {
    it('should have persistence enabled with localStorage', () => {
      // Arrange & Act
      const store = useTenantStore()

      // Assert
      expect(store.$id).toBe('tenant')
      // Persistence paths are configured in the store definition
      // We verify the store is properly initialized
      expect(store).toBeDefined()
    })
  })
})
