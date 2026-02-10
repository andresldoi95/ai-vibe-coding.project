import { beforeEach, describe, expect, it } from 'vitest'
import { mockApiFetch } from '../setup'
import { useWarehouseInventory } from '~/composables/useWarehouseInventory'
import type { InventoryLevel } from '~/types/inventory'

describe('useWarehouseInventory', () => {
  beforeEach(() => {
    // Reset all mocks before each test
    mockApiFetch.mockReset()
  })

  // Helper function to create mock inventory data
  const createMockInventory = (overrides: Partial<InventoryLevel>[] = []): InventoryLevel[] => {
    const defaults: InventoryLevel[] = [
      {
        id: '1',
        tenantId: 'tenant-123',
        productId: 'product-1',
        productName: 'Laptop',
        productCode: 'PROD-001',
        warehouseId: 'warehouse-1',
        warehouseName: 'Main Warehouse',
        warehouseCode: 'WH-001',
        quantity: 100,
        reservedQuantity: 20,
        availableQuantity: 80,
        lastMovementDate: '2024-01-15T10:30:00Z',
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-15T10:30:00Z',
      },
      {
        id: '2',
        tenantId: 'tenant-123',
        productId: 'product-1',
        productName: 'Laptop',
        productCode: 'PROD-001',
        warehouseId: 'warehouse-2',
        warehouseName: 'Secondary Warehouse',
        warehouseCode: 'WH-002',
        quantity: 50,
        reservedQuantity: 10,
        availableQuantity: 40,
        lastMovementDate: '2024-01-14T14:20:00Z',
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-14T14:20:00Z',
      },
      {
        id: '3',
        tenantId: 'tenant-123',
        productId: 'product-1',
        productName: 'Laptop',
        productCode: 'PROD-001',
        warehouseId: 'warehouse-3',
        warehouseName: 'Regional Warehouse',
        warehouseCode: 'WH-003',
        quantity: 30,
        reservedQuantity: 5,
        availableQuantity: 25,
        lastMovementDate: '2024-01-13T09:15:00Z',
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-13T09:15:00Z',
      },
    ]

    if (overrides.length > 0) {
      return overrides.map((override, index) => ({ ...defaults[index], ...override }))
    }

    return defaults
  }

  describe('getProductInventory', () => {
    it('should fetch product inventory across all warehouses', async () => {
      const mockInventory = createMockInventory()
      mockApiFetch.mockResolvedValue({ data: mockInventory })

      const { getProductInventory } = useWarehouseInventory()
      const result = await getProductInventory('product-1')

      expect(mockApiFetch).toHaveBeenCalledWith('/products/product-1/inventory')
      expect(result).toEqual(mockInventory)
      expect(result).toHaveLength(3)
      expect(result[0].productId).toBe('product-1')
      expect(result[0].warehouseName).toBe('Main Warehouse')
    })

    it('should handle empty inventory', async () => {
      mockApiFetch.mockResolvedValue({ data: [] })

      const { getProductInventory } = useWarehouseInventory()
      const result = await getProductInventory('product-999')

      expect(mockApiFetch).toHaveBeenCalledWith('/products/product-999/inventory')
      expect(result).toEqual([])
      expect(result).toHaveLength(0)
    })

    it('should handle API errors', async () => {
      const mockError = new Error('Product not found')
      mockApiFetch.mockRejectedValue(mockError)

      const { getProductInventory } = useWarehouseInventory()

      await expect(getProductInventory('product-999')).rejects.toThrow('Product not found')
    })
  })

  describe('getTotalStock', () => {
    it('should calculate total stock across all warehouses', () => {
      const mockInventory = createMockInventory()

      const { getTotalStock } = useWarehouseInventory()
      const result = getTotalStock(mockInventory)

      // 100 + 50 + 30 = 180
      expect(result).toBe(180)
    })

    it('should return 0 for empty inventory', () => {
      const { getTotalStock } = useWarehouseInventory()
      const result = getTotalStock([])

      expect(result).toBe(0)
    })

    it('should handle single warehouse', () => {
      const mockInventory = createMockInventory([
        {
          id: '1',
          tenantId: 'tenant-123',
          productId: 'product-1',
          productName: 'Laptop',
          productCode: 'PROD-001',
          warehouseId: 'warehouse-1',
          warehouseName: 'Main Warehouse',
          warehouseCode: 'WH-001',
          quantity: 100,
          reservedQuantity: 20,
          availableQuantity: 80,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-15T10:30:00Z',
        },
      ])

      const { getTotalStock } = useWarehouseInventory()
      const result = getTotalStock(mockInventory)

      expect(result).toBe(100)
    })

    it('should handle zero quantities', () => {
      const mockInventory = createMockInventory([
        {
          id: '1',
          tenantId: 'tenant-123',
          productId: 'product-1',
          productName: 'Laptop',
          productCode: 'PROD-001',
          warehouseId: 'warehouse-1',
          warehouseName: 'Main Warehouse',
          warehouseCode: 'WH-001',
          quantity: 0,
          reservedQuantity: 0,
          availableQuantity: 0,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-15T10:30:00Z',
        },
        {
          id: '2',
          tenantId: 'tenant-123',
          productId: 'product-1',
          productName: 'Laptop',
          productCode: 'PROD-001',
          warehouseId: 'warehouse-2',
          warehouseName: 'Secondary Warehouse',
          warehouseCode: 'WH-002',
          quantity: 0,
          reservedQuantity: 0,
          availableQuantity: 0,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-14T14:20:00Z',
        },
      ])

      const { getTotalStock } = useWarehouseInventory()
      const result = getTotalStock(mockInventory)

      expect(result).toBe(0)
    })
  })

  describe('getTotalAvailable', () => {
    it('should calculate total available stock across all warehouses', () => {
      const mockInventory = createMockInventory()

      const { getTotalAvailable } = useWarehouseInventory()
      const result = getTotalAvailable(mockInventory)

      // 80 + 40 + 25 = 145
      expect(result).toBe(145)
    })

    it('should return 0 for empty inventory', () => {
      const { getTotalAvailable } = useWarehouseInventory()
      const result = getTotalAvailable([])

      expect(result).toBe(0)
    })

    it('should handle single warehouse', () => {
      const mockInventory = createMockInventory([
        {
          id: '1',
          tenantId: 'tenant-123',
          productId: 'product-1',
          productName: 'Laptop',
          productCode: 'PROD-001',
          warehouseId: 'warehouse-1',
          warehouseName: 'Main Warehouse',
          warehouseCode: 'WH-001',
          quantity: 100,
          reservedQuantity: 20,
          availableQuantity: 80,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-15T10:30:00Z',
        },
      ])

      const { getTotalAvailable } = useWarehouseInventory()
      const result = getTotalAvailable(mockInventory)

      expect(result).toBe(80)
    })

    it('should handle all reserved stock', () => {
      const mockInventory = createMockInventory([
        {
          id: '1',
          tenantId: 'tenant-123',
          productId: 'product-1',
          productName: 'Laptop',
          productCode: 'PROD-001',
          warehouseId: 'warehouse-1',
          warehouseName: 'Main Warehouse',
          warehouseCode: 'WH-001',
          quantity: 100,
          reservedQuantity: 100,
          availableQuantity: 0,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-15T10:30:00Z',
        },
        {
          id: '2',
          tenantId: 'tenant-123',
          productId: 'product-1',
          productName: 'Laptop',
          productCode: 'PROD-001',
          warehouseId: 'warehouse-2',
          warehouseName: 'Secondary Warehouse',
          warehouseCode: 'WH-002',
          quantity: 50,
          reservedQuantity: 50,
          availableQuantity: 0,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-14T14:20:00Z',
        },
      ])

      const { getTotalAvailable } = useWarehouseInventory()
      const result = getTotalAvailable(mockInventory)

      expect(result).toBe(0)
    })
  })

  describe('getWarehouseStock', () => {
    it('should find inventory for specific warehouse', () => {
      const mockInventory = createMockInventory()

      const { getWarehouseStock } = useWarehouseInventory()
      const result = getWarehouseStock(mockInventory, 'warehouse-2')

      expect(result).toBeDefined()
      expect(result?.warehouseId).toBe('warehouse-2')
      expect(result?.warehouseName).toBe('Secondary Warehouse')
      expect(result?.quantity).toBe(50)
    })

    it('should return undefined when warehouse not found', () => {
      const mockInventory = createMockInventory()

      const { getWarehouseStock } = useWarehouseInventory()
      const result = getWarehouseStock(mockInventory, 'warehouse-999')

      expect(result).toBeUndefined()
    })

    it('should return undefined for empty inventory', () => {
      const { getWarehouseStock } = useWarehouseInventory()
      const result = getWarehouseStock([], 'warehouse-1')

      expect(result).toBeUndefined()
    })

    it('should find first warehouse in inventory', () => {
      const mockInventory = createMockInventory()

      const { getWarehouseStock } = useWarehouseInventory()
      const result = getWarehouseStock(mockInventory, 'warehouse-1')

      expect(result).toBeDefined()
      expect(result?.warehouseId).toBe('warehouse-1')
      expect(result?.warehouseName).toBe('Main Warehouse')
      expect(result?.quantity).toBe(100)
      expect(result?.availableQuantity).toBe(80)
    })

    it('should find last warehouse in inventory', () => {
      const mockInventory = createMockInventory()

      const { getWarehouseStock } = useWarehouseInventory()
      const result = getWarehouseStock(mockInventory, 'warehouse-3')

      expect(result).toBeDefined()
      expect(result?.warehouseId).toBe('warehouse-3')
      expect(result?.warehouseName).toBe('Regional Warehouse')
      expect(result?.quantity).toBe(30)
      expect(result?.availableQuantity).toBe(25)
    })
  })

  describe('isLowStock', () => {
    it('should return true when total stock is below minimum level', () => {
      const mockInventory = createMockInventory()

      const { isLowStock } = useWarehouseInventory()
      const result = isLowStock(mockInventory, 200)

      // Total stock is 180, minimum is 200
      expect(result).toBe(true)
    })

    it('should return false when total stock is above minimum level', () => {
      const mockInventory = createMockInventory()

      const { isLowStock } = useWarehouseInventory()
      const result = isLowStock(mockInventory, 150)

      // Total stock is 180, minimum is 150
      expect(result).toBe(false)
    })

    it('should return false when total stock equals minimum level', () => {
      const mockInventory = createMockInventory()

      const { isLowStock } = useWarehouseInventory()
      const result = isLowStock(mockInventory, 180)

      // Total stock is 180, minimum is 180
      expect(result).toBe(false)
    })

    it('should return true for empty inventory', () => {
      const { isLowStock } = useWarehouseInventory()
      const result = isLowStock([], 10)

      // Total stock is 0, minimum is 10
      expect(result).toBe(true)
    })

    it('should return false when minimum level is zero', () => {
      const mockInventory = createMockInventory()

      const { isLowStock } = useWarehouseInventory()
      const result = isLowStock(mockInventory, 0)

      // Total stock is 180, minimum is 0
      expect(result).toBe(false)
    })

    it('should return true when stock is critically low', () => {
      const mockInventory = createMockInventory([
        {
          id: '1',
          tenantId: 'tenant-123',
          productId: 'product-1',
          productName: 'Laptop',
          productCode: 'PROD-001',
          warehouseId: 'warehouse-1',
          warehouseName: 'Main Warehouse',
          warehouseCode: 'WH-001',
          quantity: 5,
          reservedQuantity: 2,
          availableQuantity: 3,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-15T10:30:00Z',
        },
      ])

      const { isLowStock } = useWarehouseInventory()
      const result = isLowStock(mockInventory, 50)

      // Total stock is 5, minimum is 50
      expect(result).toBe(true)
    })

    it('should return true when stock is zero', () => {
      const mockInventory = createMockInventory([
        {
          id: '1',
          tenantId: 'tenant-123',
          productId: 'product-1',
          productName: 'Laptop',
          productCode: 'PROD-001',
          warehouseId: 'warehouse-1',
          warehouseName: 'Main Warehouse',
          warehouseCode: 'WH-001',
          quantity: 0,
          reservedQuantity: 0,
          availableQuantity: 0,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-15T10:30:00Z',
        },
      ])

      const { isLowStock } = useWarehouseInventory()
      const result = isLowStock(mockInventory, 10)

      // Total stock is 0, minimum is 10
      expect(result).toBe(true)
    })
  })

  describe('integration scenarios', () => {
    it('should work with all functions together', async () => {
      const mockInventory = createMockInventory()
      mockApiFetch.mockResolvedValue({ data: mockInventory })

      const {
        getProductInventory,
        getTotalStock,
        getTotalAvailable,
        getWarehouseStock,
        isLowStock,
      } = useWarehouseInventory()

      // Fetch inventory
      const inventory = await getProductInventory('product-1')
      expect(inventory).toHaveLength(3)

      // Calculate totals
      const totalStock = getTotalStock(inventory)
      expect(totalStock).toBe(180)

      const totalAvailable = getTotalAvailable(inventory)
      expect(totalAvailable).toBe(145)

      // Get specific warehouse
      const warehouse2Stock = getWarehouseStock(inventory, 'warehouse-2')
      expect(warehouse2Stock?.quantity).toBe(50)
      expect(warehouse2Stock?.availableQuantity).toBe(40)

      // Check stock levels
      const isLow = isLowStock(inventory, 200)
      expect(isLow).toBe(true)

      const isNotLow = isLowStock(inventory, 100)
      expect(isNotLow).toBe(false)
    })

    it('should handle product with no inventory', async () => {
      mockApiFetch.mockResolvedValue({ data: [] })

      const {
        getProductInventory,
        getTotalStock,
        getTotalAvailable,
        getWarehouseStock,
        isLowStock,
      } = useWarehouseInventory()

      const inventory = await getProductInventory('product-new')
      expect(inventory).toHaveLength(0)

      const totalStock = getTotalStock(inventory)
      expect(totalStock).toBe(0)

      const totalAvailable = getTotalAvailable(inventory)
      expect(totalAvailable).toBe(0)

      const warehouseStock = getWarehouseStock(inventory, 'warehouse-1')
      expect(warehouseStock).toBeUndefined()

      const isLow = isLowStock(inventory, 10)
      expect(isLow).toBe(true)
    })
  })
})
