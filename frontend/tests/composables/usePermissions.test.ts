import { beforeEach, describe, expect, it, vi } from 'vitest'
import { usePermissions } from '~/composables/usePermissions'

// Mock useAuthStore methods
const mockHasPermission = vi.fn()
const mockHasAnyPermission = vi.fn()
const mockHasAllPermissions = vi.fn()

// Override global useAuthStore mock for this test
globalThis.useAuthStore = vi.fn(() => ({
  hasPermission: mockHasPermission,
  hasAnyPermission: mockHasAnyPermission,
  hasAllPermissions: mockHasAllPermissions,
}))

describe('usePermissions', () => {
  beforeEach(() => {
    // Reset all mocks before each test
    vi.clearAllMocks()
  })

  describe('hasPermission', () => {
    it('should call authStore.hasPermission with correct permission', () => {
      mockHasPermission.mockReturnValue(true)
      const { hasPermission } = usePermissions()

      const result = hasPermission('warehouses.read')

      expect(mockHasPermission).toHaveBeenCalledTimes(1)
      expect(mockHasPermission).toHaveBeenCalledWith('warehouses.read')
      expect(result).toBe(true)
    })

    it('should return false when user does not have permission', () => {
      mockHasPermission.mockReturnValue(false)
      const { hasPermission } = usePermissions()

      const result = hasPermission('warehouses.delete')

      expect(mockHasPermission).toHaveBeenCalledTimes(1)
      expect(mockHasPermission).toHaveBeenCalledWith('warehouses.delete')
      expect(result).toBe(false)
    })
  })

  describe('hasAnyPermission', () => {
    it('should call authStore.hasAnyPermission with correct permissions array', () => {
      mockHasAnyPermission.mockReturnValue(true)
      const { hasAnyPermission } = usePermissions()

      const permissions = ['warehouses.read', 'warehouses.create']
      const result = hasAnyPermission(permissions)

      expect(mockHasAnyPermission).toHaveBeenCalledTimes(1)
      expect(mockHasAnyPermission).toHaveBeenCalledWith(permissions)
      expect(result).toBe(true)
    })

    it('should return false when user has none of the permissions', () => {
      mockHasAnyPermission.mockReturnValue(false)
      const { hasAnyPermission } = usePermissions()

      const permissions = ['products.delete', 'users.delete']
      const result = hasAnyPermission(permissions)

      expect(mockHasAnyPermission).toHaveBeenCalledTimes(1)
      expect(mockHasAnyPermission).toHaveBeenCalledWith(permissions)
      expect(result).toBe(false)
    })
  })

  describe('hasAllPermissions', () => {
    it('should call authStore.hasAllPermissions with correct permissions array', () => {
      mockHasAllPermissions.mockReturnValue(true)
      const { hasAllPermissions } = usePermissions()

      const permissions = ['warehouses.read', 'warehouses.create', 'warehouses.update']
      const result = hasAllPermissions(permissions)

      expect(mockHasAllPermissions).toHaveBeenCalledTimes(1)
      expect(mockHasAllPermissions).toHaveBeenCalledWith(permissions)
      expect(result).toBe(true)
    })

    it('should return false when user does not have all permissions', () => {
      mockHasAllPermissions.mockReturnValue(false)
      const { hasAllPermissions } = usePermissions()

      const permissions = ['products.read', 'products.create', 'products.delete']
      const result = hasAllPermissions(permissions)

      expect(mockHasAllPermissions).toHaveBeenCalledTimes(1)
      expect(mockHasAllPermissions).toHaveBeenCalledWith(permissions)
      expect(result).toBe(false)
    })
  })

  describe('can.viewWarehouses', () => {
    it('should call hasPermission with warehouses.read permission', () => {
      mockHasPermission.mockReturnValue(true)
      const { can } = usePermissions()

      const result = can.viewWarehouses()

      expect(mockHasPermission).toHaveBeenCalledTimes(1)
      expect(mockHasPermission).toHaveBeenCalledWith('warehouses.read')
      expect(result).toBe(true)
    })

    it('should return false when user cannot view warehouses', () => {
      mockHasPermission.mockReturnValue(false)
      const { can } = usePermissions()

      const result = can.viewWarehouses()

      expect(result).toBe(false)
    })
  })

  describe('can.createProduct', () => {
    it('should call hasPermission with products.create permission', () => {
      mockHasPermission.mockReturnValue(true)
      const { can } = usePermissions()

      const result = can.createProduct()

      expect(mockHasPermission).toHaveBeenCalledTimes(1)
      expect(mockHasPermission).toHaveBeenCalledWith('products.create')
      expect(result).toBe(true)
    })

    it('should return false when user cannot create products', () => {
      mockHasPermission.mockReturnValue(false)
      const { can } = usePermissions()

      const result = can.createProduct()

      expect(result).toBe(false)
    })
  })

  describe('can.editCustomer', () => {
    it('should call hasPermission with customers.update permission', () => {
      mockHasPermission.mockReturnValue(true)
      const { can } = usePermissions()

      const result = can.editCustomer()

      expect(mockHasPermission).toHaveBeenCalledTimes(1)
      expect(mockHasPermission).toHaveBeenCalledWith('customers.update')
      expect(result).toBe(true)
    })

    it('should return false when user cannot edit customers', () => {
      mockHasPermission.mockReturnValue(false)
      const { can } = usePermissions()

      const result = can.editCustomer()

      expect(result).toBe(false)
    })
  })

  describe('can.deleteStock', () => {
    it('should call hasPermission with stock.delete permission', () => {
      mockHasPermission.mockReturnValue(true)
      const { can } = usePermissions()

      const result = can.deleteStock()

      expect(mockHasPermission).toHaveBeenCalledTimes(1)
      expect(mockHasPermission).toHaveBeenCalledWith('stock.delete')
      expect(result).toBe(true)
    })

    it('should return false when user cannot delete stock', () => {
      mockHasPermission.mockReturnValue(false)
      const { can } = usePermissions()

      const result = can.deleteStock()

      expect(result).toBe(false)
    })
  })

  describe('can.manageRoles', () => {
    it('should call hasPermission with roles.manage permission', () => {
      mockHasPermission.mockReturnValue(true)
      const { can } = usePermissions()

      const result = can.manageRoles()

      expect(mockHasPermission).toHaveBeenCalledTimes(1)
      expect(mockHasPermission).toHaveBeenCalledWith('roles.manage')
      expect(result).toBe(true)
    })

    it('should return false when user cannot manage roles', () => {
      mockHasPermission.mockReturnValue(false)
      const { can } = usePermissions()

      const result = can.manageRoles()

      expect(result).toBe(false)
    })
  })
})
