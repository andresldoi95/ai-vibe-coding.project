import { beforeEach, describe, expect, it, vi } from 'vitest'
import { usePermissions } from '~/composables/usePermissions'

// Mock useAuthStore
const mockAuthStore = {
  hasPermission: vi.fn(),
  hasAnyPermission: vi.fn(),
  hasAllPermissions: vi.fn(),
}

globalThis.useAuthStore = vi.fn(() => mockAuthStore)

describe('usePermissions', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  describe('hasPermission', () => {
    it('should check single permission', () => {
      mockAuthStore.hasPermission.mockReturnValue(true)

      const { hasPermission } = usePermissions()
      const result = hasPermission('warehouses.read')

      expect(result).toBe(true)
      expect(mockAuthStore.hasPermission).toHaveBeenCalledWith('warehouses.read')
    })

    it('should return false when permission is denied', () => {
      mockAuthStore.hasPermission.mockReturnValue(false)

      const { hasPermission } = usePermissions()
      const result = hasPermission('warehouses.delete')

      expect(result).toBe(false)
      expect(mockAuthStore.hasPermission).toHaveBeenCalledWith('warehouses.delete')
    })
  })

  describe('hasAnyPermission', () => {
    it('should check if user has any of the permissions', () => {
      mockAuthStore.hasAnyPermission.mockReturnValue(true)

      const { hasAnyPermission } = usePermissions()
      const permissions = ['warehouses.read', 'warehouses.create']
      const result = hasAnyPermission(permissions)

      expect(result).toBe(true)
      expect(mockAuthStore.hasAnyPermission).toHaveBeenCalledWith(permissions)
    })

    it('should return false when user has none of the permissions', () => {
      mockAuthStore.hasAnyPermission.mockReturnValue(false)

      const { hasAnyPermission } = usePermissions()
      const permissions = ['warehouses.delete', 'products.delete']
      const result = hasAnyPermission(permissions)

      expect(result).toBe(false)
      expect(mockAuthStore.hasAnyPermission).toHaveBeenCalledWith(permissions)
    })
  })

  describe('hasAllPermissions', () => {
    it('should check if user has all permissions', () => {
      mockAuthStore.hasAllPermissions.mockReturnValue(true)

      const { hasAllPermissions } = usePermissions()
      const permissions = ['warehouses.read', 'warehouses.update']
      const result = hasAllPermissions(permissions)

      expect(result).toBe(true)
      expect(mockAuthStore.hasAllPermissions).toHaveBeenCalledWith(permissions)
    })

    it('should return false when user lacks any permission', () => {
      mockAuthStore.hasAllPermissions.mockReturnValue(false)

      const { hasAllPermissions } = usePermissions()
      const permissions = ['warehouses.read', 'warehouses.delete']
      const result = hasAllPermissions(permissions)

      expect(result).toBe(false)
      expect(mockAuthStore.hasAllPermissions).toHaveBeenCalledWith(permissions)
    })
  })

  describe('can - warehouses', () => {
    it('should check viewWarehouses permission', () => {
      mockAuthStore.hasPermission.mockReturnValue(true)

      const { can } = usePermissions()
      const result = can.viewWarehouses()

      expect(result).toBe(true)
      expect(mockAuthStore.hasPermission).toHaveBeenCalledWith('warehouses.read')
    })

    it('should check createWarehouse permission', () => {
      mockAuthStore.hasPermission.mockReturnValue(false)

      const { can } = usePermissions()
      const result = can.createWarehouse()

      expect(result).toBe(false)
      expect(mockAuthStore.hasPermission).toHaveBeenCalledWith('warehouses.create')
    })

    it('should check editWarehouse permission', () => {
      mockAuthStore.hasPermission.mockReturnValue(true)

      const { can } = usePermissions()
      const result = can.editWarehouse()

      expect(result).toBe(true)
      expect(mockAuthStore.hasPermission).toHaveBeenCalledWith('warehouses.update')
    })

    it('should check deleteWarehouse permission', () => {
      mockAuthStore.hasPermission.mockReturnValue(false)

      const { can } = usePermissions()
      const result = can.deleteWarehouse()

      expect(result).toBe(false)
      expect(mockAuthStore.hasPermission).toHaveBeenCalledWith('warehouses.delete')
    })
  })

  describe('can - products', () => {
    it('should check viewProducts permission', () => {
      mockAuthStore.hasPermission.mockReturnValue(true)

      const { can } = usePermissions()
      const result = can.viewProducts()

      expect(result).toBe(true)
      expect(mockAuthStore.hasPermission).toHaveBeenCalledWith('products.read')
    })

    it('should check createProduct permission', () => {
      mockAuthStore.hasPermission.mockReturnValue(true)

      const { can } = usePermissions()
      const result = can.createProduct()

      expect(result).toBe(true)
      expect(mockAuthStore.hasPermission).toHaveBeenCalledWith('products.create')
    })

    it('should check editProduct permission', () => {
      mockAuthStore.hasPermission.mockReturnValue(false)

      const { can } = usePermissions()
      const result = can.editProduct()

      expect(result).toBe(false)
      expect(mockAuthStore.hasPermission).toHaveBeenCalledWith('products.update')
    })

    it('should check deleteProduct permission', () => {
      mockAuthStore.hasPermission.mockReturnValue(false)

      const { can } = usePermissions()
      const result = can.deleteProduct()

      expect(result).toBe(false)
      expect(mockAuthStore.hasPermission).toHaveBeenCalledWith('products.delete')
    })
  })

  describe('can - users', () => {
    it('should check viewUsers permission', () => {
      mockAuthStore.hasPermission.mockReturnValue(true)

      const { can } = usePermissions()
      const result = can.viewUsers()

      expect(result).toBe(true)
      expect(mockAuthStore.hasPermission).toHaveBeenCalledWith('users.read')
    })

    it('should check createUser permission', () => {
      mockAuthStore.hasPermission.mockReturnValue(true)

      const { can } = usePermissions()
      const result = can.createUser()

      expect(result).toBe(true)
      expect(mockAuthStore.hasPermission).toHaveBeenCalledWith('users.create')
    })

    it('should check inviteUser permission', () => {
      mockAuthStore.hasPermission.mockReturnValue(false)

      const { can } = usePermissions()
      const result = can.inviteUser()

      expect(result).toBe(false)
      expect(mockAuthStore.hasPermission).toHaveBeenCalledWith('users.invite')
    })

    it('should check removeUser permission', () => {
      mockAuthStore.hasPermission.mockReturnValue(true)

      const { can } = usePermissions()
      const result = can.removeUser()

      expect(result).toBe(true)
      expect(mockAuthStore.hasPermission).toHaveBeenCalledWith('users.remove')
    })
  })

  describe('can - invoices', () => {
    it('should check viewInvoices permission', () => {
      mockAuthStore.hasPermission.mockReturnValue(true)

      const { can } = usePermissions()
      const result = can.viewInvoices()

      expect(result).toBe(true)
      expect(mockAuthStore.hasPermission).toHaveBeenCalledWith('invoices.read')
    })

    it('should check createInvoice permission', () => {
      mockAuthStore.hasPermission.mockReturnValue(false)

      const { can } = usePermissions()
      const result = can.createInvoice()

      expect(result).toBe(false)
      expect(mockAuthStore.hasPermission).toHaveBeenCalledWith('invoices.create')
    })

    it('should check updateInvoice permission', () => {
      mockAuthStore.hasPermission.mockReturnValue(true)

      const { can } = usePermissions()
      const result = can.updateInvoice()

      expect(result).toBe(true)
      expect(mockAuthStore.hasPermission).toHaveBeenCalledWith('invoices.update')
    })
  })

  describe('can - payments', () => {
    it('should check viewPayments permission', () => {
      mockAuthStore.hasPermission.mockReturnValue(true)

      const { can } = usePermissions()
      const result = can.viewPayments()

      expect(result).toBe(true)
      expect(mockAuthStore.hasPermission).toHaveBeenCalledWith('payments.read')
    })

    it('should check voidPayment permission', () => {
      mockAuthStore.hasPermission.mockReturnValue(false)

      const { can } = usePermissions()
      const result = can.voidPayment()

      expect(result).toBe(false)
      expect(mockAuthStore.hasPermission).toHaveBeenCalledWith('payments.void')
    })

    it('should check completePayment permission', () => {
      mockAuthStore.hasPermission.mockReturnValue(true)

      const { can } = usePermissions()
      const result = can.completePayment()

      expect(result).toBe(true)
      expect(mockAuthStore.hasPermission).toHaveBeenCalledWith('payments.complete')
    })
  })

  describe('can - SRI Ecuador', () => {
    it('should check viewEstablishments permission', () => {
      mockAuthStore.hasPermission.mockReturnValue(true)

      const { can } = usePermissions()
      const result = can.viewEstablishments()

      expect(result).toBe(true)
      expect(mockAuthStore.hasPermission).toHaveBeenCalledWith('establishments.read')
    })

    it('should check viewEmissionPoints permission', () => {
      mockAuthStore.hasPermission.mockReturnValue(false)

      const { can } = usePermissions()
      const result = can.viewEmissionPoints()

      expect(result).toBe(false)
      expect(mockAuthStore.hasPermission).toHaveBeenCalledWith('emission_points.read')
    })

    it('should check viewSriConfiguration permission', () => {
      mockAuthStore.hasPermission.mockReturnValue(true)

      const { can } = usePermissions()
      const result = can.viewSriConfiguration()

      expect(result).toBe(true)
      expect(mockAuthStore.hasPermission).toHaveBeenCalledWith('sri_configuration.read')
    })

    it('should check updateSriConfiguration permission', () => {
      mockAuthStore.hasPermission.mockReturnValue(false)

      const { can } = usePermissions()
      const result = can.updateSriConfiguration()

      expect(result).toBe(false)
      expect(mockAuthStore.hasPermission).toHaveBeenCalledWith('sri_configuration.update')
    })
  })
})
