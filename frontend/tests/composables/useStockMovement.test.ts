import { beforeEach, describe, expect, it, vi } from 'vitest'
import { mockApiFetch, mockAuthStoreData, mockTenantStoreData } from '../setup'
import { useStockMovement } from '~/composables/useStockMovement'
import { MovementType, type StockMovement } from '~/types/inventory'

// Mock global fetch for export functionality
globalThis.fetch = vi.fn()
globalThis.window = {
  URL: {
    createObjectURL: vi.fn(() => 'blob:mock-url'),
    revokeObjectURL: vi.fn(),
  },
} as unknown as Window & typeof globalThis

// Mock document methods
globalThis.document = {
  createElement: vi.fn(() => ({
    click: vi.fn(),
    href: '',
    download: '',
  })),
  body: {
    appendChild: vi.fn(),
    removeChild: vi.fn(),
  },
} as unknown as Document

describe('useStockMovement', () => {
  beforeEach(() => {
    // Reset all mocks before each test
    mockApiFetch.mockReset()
    vi.clearAllMocks()
  })

  describe('getAllStockMovements', () => {
    it('should fetch all stock movements successfully', async () => {
      const mockStockMovements: StockMovement[] = [
        {
          id: '1',
          tenantId: 'tenant-1',
          productId: 'product-1',
          warehouseId: 'warehouse-1',
          movementType: MovementType.Purchase,
          quantity: 100,
          unitCost: 10.00,
          totalCost: 1000.00,
          reference: 'PO-001',
          notes: 'Initial purchase',
          movementDate: '2024-01-01T00:00:00Z',
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
        {
          id: '2',
          tenantId: 'tenant-1',
          productId: 'product-2',
          warehouseId: 'warehouse-1',
          destinationWarehouseId: 'warehouse-2',
          movementType: MovementType.Transfer,
          quantity: 20,
          reference: 'TRANS-001',
          notes: 'Transfer to secondary warehouse',
          movementDate: '2024-01-02T00:00:00Z',
          createdAt: '2024-01-02T00:00:00Z',
          updatedAt: '2024-01-02T00:00:00Z',
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockStockMovements, success: true })

      const { getAllStockMovements } = useStockMovement()
      const result = await getAllStockMovements()

      expect(mockApiFetch).toHaveBeenCalledWith('/stock-movements', {
        method: 'GET',
      })
      expect(result).toEqual(mockStockMovements)
      expect(result).toHaveLength(2)
    })

    it('should handle empty stock movements list', async () => {
      mockApiFetch.mockResolvedValue({ data: [], success: true })

      const { getAllStockMovements } = useStockMovement()
      const result = await getAllStockMovements()

      expect(result).toEqual([])
      expect(result).toHaveLength(0)
    })

    it('should handle API errors when fetching stock movements', async () => {
      mockApiFetch.mockRejectedValue(new Error('Network error'))

      const { getAllStockMovements } = useStockMovement()

      await expect(getAllStockMovements()).rejects.toThrow('Network error')
    })
  })

  describe('getStockMovementById', () => {
    it('should fetch a stock movement by id successfully', async () => {
      const mockStockMovement: StockMovement = {
        id: '1',
        tenantId: 'tenant-1',
        productId: 'product-1',
        warehouseId: 'warehouse-1',
        movementType: MovementType.Purchase,
        quantity: 100,
        unitCost: 10.00,
        totalCost: 1000.00,
        reference: 'PO-001',
        notes: 'Initial purchase',
        movementDate: '2024-01-01T00:00:00Z',
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockStockMovement, success: true })

      const { getStockMovementById } = useStockMovement()
      const result = await getStockMovementById('1')

      expect(mockApiFetch).toHaveBeenCalledWith('/stock-movements/1', {
        method: 'GET',
      })
      expect(result).toEqual(mockStockMovement)
      expect(result.id).toBe('1')
      expect(result.movementType).toBe(MovementType.Purchase)
    })

    it('should handle API errors when fetching stock movement by id', async () => {
      mockApiFetch.mockRejectedValue(new Error('Stock movement not found'))

      const { getStockMovementById } = useStockMovement()

      await expect(getStockMovementById('999')).rejects.toThrow('Stock movement not found')
    })
  })

  describe('createStockMovement', () => {
    it('should create a stock movement successfully', async () => {
      const createData = {
        productId: 'product-1',
        warehouseId: 'warehouse-1',
        movementType: MovementType.Purchase,
        quantity: 50,
        unitCost: 15.00,
        totalCost: 750.00,
        reference: 'PO-002',
        notes: 'New purchase order',
        movementDate: '2024-01-03T00:00:00Z',
      }

      const mockCreatedStockMovement: StockMovement = {
        id: '3',
        tenantId: 'tenant-1',
        ...createData,
        createdAt: '2024-01-03T00:00:00Z',
        updatedAt: '2024-01-03T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockCreatedStockMovement, success: true })

      const { createStockMovement } = useStockMovement()
      const result = await createStockMovement(createData)

      expect(mockApiFetch).toHaveBeenCalledWith('/stock-movements', {
        method: 'POST',
        body: createData,
      })
      expect(result).toEqual(mockCreatedStockMovement)
      expect(result.id).toBe('3')
      expect(result.quantity).toBe(50)
    })

    it('should create a transfer stock movement with destination warehouse', async () => {
      const createData = {
        productId: 'product-1',
        warehouseId: 'warehouse-1',
        destinationWarehouseId: 'warehouse-2',
        movementType: MovementType.Transfer,
        quantity: 30,
        reference: 'TRANS-002',
        notes: 'Transfer between warehouses',
        movementDate: '2024-01-04T00:00:00Z',
      }

      const mockCreatedStockMovement: StockMovement = {
        id: '4',
        tenantId: 'tenant-1',
        ...createData,
        createdAt: '2024-01-04T00:00:00Z',
        updatedAt: '2024-01-04T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockCreatedStockMovement, success: true })

      const { createStockMovement } = useStockMovement()
      const result = await createStockMovement(createData)

      expect(mockApiFetch).toHaveBeenCalledWith('/stock-movements', {
        method: 'POST',
        body: createData,
      })
      expect(result.destinationWarehouseId).toBe('warehouse-2')
    })

    it('should handle API errors when creating stock movement', async () => {
      const createData = {
        productId: 'invalid-product',
        warehouseId: 'warehouse-1',
        movementType: MovementType.Purchase,
        quantity: 10,
      }

      mockApiFetch.mockRejectedValue(new Error('Product not found'))

      const { createStockMovement } = useStockMovement()

      await expect(createStockMovement(createData)).rejects.toThrow('Product not found')
    })
  })

  describe('updateStockMovement', () => {
    it('should update a stock movement successfully', async () => {
      const updateData = {
        id: '1',
        productId: 'product-1',
        warehouseId: 'warehouse-1',
        movementType: MovementType.Purchase,
        quantity: 150,
        unitCost: 12.00,
        totalCost: 1800.00,
        reference: 'PO-001-UPDATED',
        notes: 'Updated purchase order',
        movementDate: '2024-01-01T00:00:00Z',
      }

      const mockUpdatedStockMovement: StockMovement = {
        tenantId: 'tenant-1',
        ...updateData,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-05T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockUpdatedStockMovement, success: true })

      const { updateStockMovement } = useStockMovement()
      const result = await updateStockMovement(updateData)

      expect(mockApiFetch).toHaveBeenCalledWith('/stock-movements/1', {
        method: 'PUT',
        body: updateData,
      })
      expect(result).toEqual(mockUpdatedStockMovement)
      expect(result.quantity).toBe(150)
    })

    it('should handle API errors when updating stock movement', async () => {
      const updateData = {
        id: '999',
        productId: 'product-1',
        warehouseId: 'warehouse-1',
        movementType: MovementType.Purchase,
        quantity: 10,
      }

      mockApiFetch.mockRejectedValue(new Error('Stock movement not found'))

      const { updateStockMovement } = useStockMovement()

      await expect(updateStockMovement(updateData)).rejects.toThrow('Stock movement not found')
    })
  })

  describe('deleteStockMovement', () => {
    it('should delete a stock movement successfully', async () => {
      mockApiFetch.mockResolvedValue({ success: true })

      const { deleteStockMovement } = useStockMovement()
      await deleteStockMovement('1')

      expect(mockApiFetch).toHaveBeenCalledWith('/stock-movements/1', {
        method: 'DELETE',
      })
    })

    it('should handle API errors when deleting stock movement', async () => {
      mockApiFetch.mockRejectedValue(new Error('Cannot delete stock movement'))

      const { deleteStockMovement } = useStockMovement()

      await expect(deleteStockMovement('1')).rejects.toThrow('Cannot delete stock movement')
    })
  })

  describe('exportStockMovements', () => {
    it('should export stock movements with default format', async () => {
      const mockBlob = new Blob(['test data'], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' })

      ;(globalThis.fetch as ReturnType<typeof vi.fn>).mockResolvedValue({
        ok: true,
        headers: {
          get: () => 'attachment; filename="stock-movements-export.xlsx"',
        },
        blob: async () => mockBlob,
      })

      const { exportStockMovements } = useStockMovement()
      await exportStockMovements()

      expect(globalThis.fetch).toHaveBeenCalledWith(
        'http://localhost:5000/api/stock-movements/export',
        {
          headers: {
            'Authorization': 'Bearer test-auth-token',
            'X-Tenant-Id': 'tenant-123',
          },
        },
      )
    })

    it('should export stock movements with filters', async () => {
      const mockBlob = new Blob(['test data'], { type: 'text/csv' })

      ;(globalThis.fetch as ReturnType<typeof vi.fn>).mockResolvedValue({
        ok: true,
        headers: {
          get: () => 'attachment; filename="stock-movements-filtered.csv"',
        },
        blob: async () => mockBlob,
      })

      const { exportStockMovements } = useStockMovement()
      await exportStockMovements({
        format: 'csv',
        brand: 'TechBrand',
        category: 'Electronics',
        warehouseId: 'warehouse-1',
        fromDate: '2024-01-01',
        toDate: '2024-01-31',
      })

      expect(globalThis.fetch).toHaveBeenCalledWith(
        'http://localhost:5000/api/stock-movements/export?format=csv&brand=TechBrand&category=Electronics&warehouseId=warehouse-1&fromDate=2024-01-01&toDate=2024-01-31',
        {
          headers: {
            'Authorization': 'Bearer test-auth-token',
            'X-Tenant-Id': 'tenant-123',
          },
        },
      )
    })

    it('should throw error when tenant is not selected', async () => {
      const originalTenantId = mockTenantStoreData.currentTenantId
      mockTenantStoreData.currentTenantId = ''

      const { exportStockMovements } = useStockMovement()

      await expect(exportStockMovements()).rejects.toThrow('No tenant selected')

      // Restore for other tests
      mockTenantStoreData.currentTenantId = originalTenantId
    })

    it('should throw error when not authenticated', async () => {
      const originalToken = mockAuthStoreData.token
      mockAuthStoreData.token = ''

      const { exportStockMovements } = useStockMovement()

      await expect(exportStockMovements()).rejects.toThrow('Not authenticated')

      // Restore for other tests
      mockAuthStoreData.token = originalToken
    })

    it('should throw error when export fails', async () => {
      ;(globalThis.fetch as ReturnType<typeof vi.fn>).mockResolvedValue({
        ok: false,
      })

      const { exportStockMovements } = useStockMovement()

      await expect(exportStockMovements()).rejects.toThrow('Export failed')
    })

    it('should use default filename when Content-Disposition is not provided', async () => {
      const mockBlob = new Blob(['test data'])
      const mockDate = '2024-01-15'

      // Mock Date.toISOString
      const originalDate = Date
      globalThis.Date = class extends originalDate {
        toISOString() {
          return `${mockDate}T00:00:00.000Z`
        }
      } as typeof Date

      ;(globalThis.fetch as ReturnType<typeof vi.fn>).mockResolvedValue({
        ok: true,
        headers: {
          get: () => null,
        },
        blob: async () => mockBlob,
      })

      const { exportStockMovements } = useStockMovement()
      await exportStockMovements({ format: 'excel' })

      expect(globalThis.document.createElement).toHaveBeenCalledWith('a')

      // Restore Date
      globalThis.Date = originalDate
    })
  })
})
