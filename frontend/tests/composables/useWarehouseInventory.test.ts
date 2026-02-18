import { beforeEach, describe, expect, it } from 'vitest'
import { mockApiFetch } from '../setup'
import { useWarehouseInventory } from '~/composables/useWarehouseInventory'
import type { InventoryLevel } from '~/types/inventory'

describe('useWarehouseInventory', () => {
  beforeEach(() => {
    // Reset all mocks before each test
    mockApiFetch.mockReset()
  })

  describe('getProductInventory', () => {
    it('should fetch inventory for a specific product across all warehouses', async () => {
      const mockInventory: InventoryLevel[] = [
        {
          id: '1',
          tenantId: 'tenant-1',
          productId: 'product-1',
          productName: 'Laptop',
          productCode: 'LAP-001',
          warehouseId: 'warehouse-1',
          warehouseName: 'Main Warehouse',
          warehouseCode: 'WH-001',
          quantity: 50,
          reservedQuantity: 10,
          availableQuantity: 40,
          lastMovementDate: '2024-01-15T00:00:00Z',
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-15T00:00:00Z',
        },
        {
          id: '2',
          tenantId: 'tenant-1',
          productId: 'product-1',
          productName: 'Laptop',
          productCode: 'LAP-001',
          warehouseId: 'warehouse-2',
          warehouseName: 'Secondary Warehouse',
          warehouseCode: 'WH-002',
          quantity: 30,
          reservedQuantity: 5,
          availableQuantity: 25,
          lastMovementDate: '2024-01-10T00:00:00Z',
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-10T00:00:00Z',
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockInventory })

      const { getProductInventory } = useWarehouseInventory()
      const result = await getProductInventory('product-1')

      expect(mockApiFetch).toHaveBeenCalledWith('/products/product-1/inventory')
      expect(result).toEqual(mockInventory)
      expect(result).toHaveLength(2)
      expect(result[0].productId).toBe('product-1')
      expect(result[1].productId).toBe('product-1')
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
      mockApiFetch.mockRejectedValue(new Error('Product not found'))

      const { getProductInventory } = useWarehouseInventory()

      await expect(getProductInventory('invalid-product')).rejects.toThrow('Product not found')
    })
  })

  describe('getTotalStock', () => {
    it('should calculate total stock across all warehouses', () => {
      const mockInventory: InventoryLevel[] = [
        {
          id: '1',
          tenantId: 'tenant-1',
          productId: 'product-1',
          productName: 'Laptop',
          productCode: 'LAP-001',
          warehouseId: 'warehouse-1',
          warehouseName: 'Main Warehouse',
          warehouseCode: 'WH-001',
          quantity: 50,
          reservedQuantity: 10,
          availableQuantity: 40,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
        {
          id: '2',
          tenantId: 'tenant-1',
          productId: 'product-1',
          productName: 'Laptop',
          productCode: 'LAP-001',
          warehouseId: 'warehouse-2',
          warehouseName: 'Secondary Warehouse',
          warehouseCode: 'WH-002',
          quantity: 30,
          reservedQuantity: 5,
          availableQuantity: 25,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
      ]

      const { getTotalStock } = useWarehouseInventory()
      const total = getTotalStock(mockInventory)

      expect(total).toBe(80)
    })

    it('should return 0 for empty inventory', () => {
      const { getTotalStock } = useWarehouseInventory()
      const total = getTotalStock([])

      expect(total).toBe(0)
    })

    it('should handle single warehouse inventory', () => {
      const mockInventory: InventoryLevel[] = [
        {
          id: '1',
          tenantId: 'tenant-1',
          productId: 'product-1',
          productName: 'Laptop',
          productCode: 'LAP-001',
          warehouseId: 'warehouse-1',
          warehouseName: 'Main Warehouse',
          warehouseCode: 'WH-001',
          quantity: 100,
          reservedQuantity: 20,
          availableQuantity: 80,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
      ]

      const { getTotalStock } = useWarehouseInventory()
      const total = getTotalStock(mockInventory)

      expect(total).toBe(100)
    })
  })

  describe('getTotalAvailable', () => {
    it('should calculate total available stock across all warehouses', () => {
      const mockInventory: InventoryLevel[] = [
        {
          id: '1',
          tenantId: 'tenant-1',
          productId: 'product-1',
          productName: 'Laptop',
          productCode: 'LAP-001',
          warehouseId: 'warehouse-1',
          warehouseName: 'Main Warehouse',
          warehouseCode: 'WH-001',
          quantity: 50,
          reservedQuantity: 10,
          availableQuantity: 40,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
        {
          id: '2',
          tenantId: 'tenant-1',
          productId: 'product-1',
          productName: 'Laptop',
          productCode: 'LAP-001',
          warehouseId: 'warehouse-2',
          warehouseName: 'Secondary Warehouse',
          warehouseCode: 'WH-002',
          quantity: 30,
          reservedQuantity: 5,
          availableQuantity: 25,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
      ]

      const { getTotalAvailable } = useWarehouseInventory()
      const totalAvailable = getTotalAvailable(mockInventory)

      expect(totalAvailable).toBe(65)
    })

    it('should return 0 for empty inventory', () => {
      const { getTotalAvailable } = useWarehouseInventory()
      const totalAvailable = getTotalAvailable([])

      expect(totalAvailable).toBe(0)
    })

    it('should handle fully reserved inventory', () => {
      const mockInventory: InventoryLevel[] = [
        {
          id: '1',
          tenantId: 'tenant-1',
          productId: 'product-1',
          productName: 'Laptop',
          productCode: 'LAP-001',
          warehouseId: 'warehouse-1',
          warehouseName: 'Main Warehouse',
          warehouseCode: 'WH-001',
          quantity: 50,
          reservedQuantity: 50,
          availableQuantity: 0,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
      ]

      const { getTotalAvailable } = useWarehouseInventory()
      const totalAvailable = getTotalAvailable(mockInventory)

      expect(totalAvailable).toBe(0)
    })
  })

  describe('getWarehouseStock', () => {
    it('should find inventory for a specific warehouse', () => {
      const mockInventory: InventoryLevel[] = [
        {
          id: '1',
          tenantId: 'tenant-1',
          productId: 'product-1',
          productName: 'Laptop',
          productCode: 'LAP-001',
          warehouseId: 'warehouse-1',
          warehouseName: 'Main Warehouse',
          warehouseCode: 'WH-001',
          quantity: 50,
          reservedQuantity: 10,
          availableQuantity: 40,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
        {
          id: '2',
          tenantId: 'tenant-1',
          productId: 'product-1',
          productName: 'Laptop',
          productCode: 'LAP-001',
          warehouseId: 'warehouse-2',
          warehouseName: 'Secondary Warehouse',
          warehouseCode: 'WH-002',
          quantity: 30,
          reservedQuantity: 5,
          availableQuantity: 25,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
      ]

      const { getWarehouseStock } = useWarehouseInventory()
      const warehouseStock = getWarehouseStock(mockInventory, 'warehouse-2')

      expect(warehouseStock).toBeDefined()
      expect(warehouseStock?.warehouseId).toBe('warehouse-2')
      expect(warehouseStock?.quantity).toBe(30)
      expect(warehouseStock?.availableQuantity).toBe(25)
    })

    it('should return undefined for non-existent warehouse', () => {
      const mockInventory: InventoryLevel[] = [
        {
          id: '1',
          tenantId: 'tenant-1',
          productId: 'product-1',
          productName: 'Laptop',
          productCode: 'LAP-001',
          warehouseId: 'warehouse-1',
          warehouseName: 'Main Warehouse',
          warehouseCode: 'WH-001',
          quantity: 50,
          reservedQuantity: 10,
          availableQuantity: 40,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
      ]

      const { getWarehouseStock } = useWarehouseInventory()
      const warehouseStock = getWarehouseStock(mockInventory, 'warehouse-999')

      expect(warehouseStock).toBeUndefined()
    })

    it('should return undefined for empty inventory', () => {
      const { getWarehouseStock } = useWarehouseInventory()
      const warehouseStock = getWarehouseStock([], 'warehouse-1')

      expect(warehouseStock).toBeUndefined()
    })
  })

  describe('isLowStock', () => {
    it('should return true when total stock is below minimum level', () => {
      const mockInventory: InventoryLevel[] = [
        {
          id: '1',
          tenantId: 'tenant-1',
          productId: 'product-1',
          productName: 'Laptop',
          productCode: 'LAP-001',
          warehouseId: 'warehouse-1',
          warehouseName: 'Main Warehouse',
          warehouseCode: 'WH-001',
          quantity: 3,
          reservedQuantity: 0,
          availableQuantity: 3,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
        {
          id: '2',
          tenantId: 'tenant-1',
          productId: 'product-1',
          productName: 'Laptop',
          productCode: 'LAP-001',
          warehouseId: 'warehouse-2',
          warehouseName: 'Secondary Warehouse',
          warehouseCode: 'WH-002',
          quantity: 2,
          reservedQuantity: 0,
          availableQuantity: 2,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
      ]

      const { isLowStock } = useWarehouseInventory()
      const lowStock = isLowStock(mockInventory, 10)

      expect(lowStock).toBe(true)
    })

    it('should return false when total stock is at minimum level', () => {
      const mockInventory: InventoryLevel[] = [
        {
          id: '1',
          tenantId: 'tenant-1',
          productId: 'product-1',
          productName: 'Laptop',
          productCode: 'LAP-001',
          warehouseId: 'warehouse-1',
          warehouseName: 'Main Warehouse',
          warehouseCode: 'WH-001',
          quantity: 10,
          reservedQuantity: 0,
          availableQuantity: 10,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
      ]

      const { isLowStock } = useWarehouseInventory()
      const lowStock = isLowStock(mockInventory, 10)

      expect(lowStock).toBe(false)
    })

    it('should return false when total stock is above minimum level', () => {
      const mockInventory: InventoryLevel[] = [
        {
          id: '1',
          tenantId: 'tenant-1',
          productId: 'product-1',
          productName: 'Laptop',
          productCode: 'LAP-001',
          warehouseId: 'warehouse-1',
          warehouseName: 'Main Warehouse',
          warehouseCode: 'WH-001',
          quantity: 50,
          reservedQuantity: 10,
          availableQuantity: 40,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
        {
          id: '2',
          tenantId: 'tenant-1',
          productId: 'product-1',
          productName: 'Laptop',
          productCode: 'LAP-001',
          warehouseId: 'warehouse-2',
          warehouseName: 'Secondary Warehouse',
          warehouseCode: 'WH-002',
          quantity: 30,
          reservedQuantity: 5,
          availableQuantity: 25,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
      ]

      const { isLowStock } = useWarehouseInventory()
      const lowStock = isLowStock(mockInventory, 50)

      expect(lowStock).toBe(false)
    })

    it('should return true for empty inventory with positive minimum level', () => {
      const { isLowStock } = useWarehouseInventory()
      const lowStock = isLowStock([], 5)

      expect(lowStock).toBe(true)
    })

    it('should handle zero minimum level', () => {
      const mockInventory: InventoryLevel[] = [
        {
          id: '1',
          tenantId: 'tenant-1',
          productId: 'product-1',
          productName: 'Laptop',
          productCode: 'LAP-001',
          warehouseId: 'warehouse-1',
          warehouseName: 'Main Warehouse',
          warehouseCode: 'WH-001',
          quantity: 0,
          reservedQuantity: 0,
          availableQuantity: 0,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
      ]

      const { isLowStock } = useWarehouseInventory()
      const lowStock = isLowStock(mockInventory, 0)

      expect(lowStock).toBe(false)
    })
  })

  describe('integration scenarios', () => {
    it('should work with complete inventory workflow', async () => {
      const mockInventory: InventoryLevel[] = [
        {
          id: '1',
          tenantId: 'tenant-1',
          productId: 'product-1',
          productName: 'Laptop',
          productCode: 'LAP-001',
          warehouseId: 'warehouse-1',
          warehouseName: 'Main Warehouse',
          warehouseCode: 'WH-001',
          quantity: 50,
          reservedQuantity: 10,
          availableQuantity: 40,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
        {
          id: '2',
          tenantId: 'tenant-1',
          productId: 'product-1',
          productName: 'Laptop',
          productCode: 'LAP-001',
          warehouseId: 'warehouse-2',
          warehouseName: 'Secondary Warehouse',
          warehouseCode: 'WH-002',
          quantity: 30,
          reservedQuantity: 5,
          availableQuantity: 25,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
      ]

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
      expect(inventory).toHaveLength(2)

      // Calculate totals
      const totalStock = getTotalStock(inventory)
      expect(totalStock).toBe(80)

      const totalAvailable = getTotalAvailable(inventory)
      expect(totalAvailable).toBe(65)

      // Check specific warehouse
      const mainWarehouseStock = getWarehouseStock(inventory, 'warehouse-1')
      expect(mainWarehouseStock?.quantity).toBe(50)

      // Check low stock status
      const lowStock = isLowStock(inventory, 100)
      expect(lowStock).toBe(true)
    })
  })
})
