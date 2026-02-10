import { beforeEach, describe, expect, it, vi } from 'vitest'
import { mockApiFetch, mockAuthStore, mockRuntimeConfig, mockTenantStore } from '../setup'
import { useStockMovement } from '~/composables/useStockMovement'
import { MovementType, type StockMovement } from '~/types/inventory'

describe('useStockMovement', () => {
  beforeEach(() => {
    // Reset all mocks before each test
    mockApiFetch.mockReset()
    mockAuthStore.token = 'mock-token-123'
    mockTenantStore.currentTenantId = 'tenant-123'
  })

  describe('getAllStockMovements', () => {
    it('should fetch all stock movements successfully', async () => {
      const mockStockMovements: StockMovement[] = [
        {
          id: '1',
          tenantId: 'tenant-123',
          productId: 'product-1',
          warehouseId: 'warehouse-1',
          movementType: MovementType.Purchase,
          quantity: 100,
          unitCost: 10.5,
          totalCost: 1050,
          reference: 'PO-001',
          notes: 'Initial purchase',
          movementDate: '2024-01-01T00:00:00Z',
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
        {
          id: '2',
          tenantId: 'tenant-123',
          productId: 'product-2',
          warehouseId: 'warehouse-1',
          destinationWarehouseId: 'warehouse-2',
          movementType: MovementType.Transfer,
          quantity: 50,
          reference: 'TRF-001',
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
  })

  describe('getStockMovementById', () => {
    it('should fetch a stock movement by id successfully', async () => {
      const mockStockMovement: StockMovement = {
        id: '1',
        tenantId: 'tenant-123',
        productId: 'product-1',
        warehouseId: 'warehouse-1',
        movementType: MovementType.Sale,
        quantity: 25,
        unitCost: 15.0,
        totalCost: 375,
        reference: 'SO-001',
        notes: 'Customer order',
        movementDate: '2024-01-03T00:00:00Z',
        createdAt: '2024-01-03T00:00:00Z',
        updatedAt: '2024-01-03T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockStockMovement, success: true })

      const { getStockMovementById } = useStockMovement()
      const result = await getStockMovementById('1')

      expect(mockApiFetch).toHaveBeenCalledWith('/stock-movements/1', {
        method: 'GET',
      })
      expect(result).toEqual(mockStockMovement)
      expect(result.id).toBe('1')
      expect(result.movementType).toBe(MovementType.Sale)
    })
  })

  describe('createStockMovement', () => {
    it('should create a new stock movement successfully', async () => {
      const newStockMovementData = {
        productId: 'product-1',
        warehouseId: 'warehouse-1',
        movementType: MovementType.Purchase,
        quantity: 200,
        unitCost: 12.0,
        totalCost: 2400,
        reference: 'PO-002',
        notes: 'New purchase order',
        movementDate: '2024-01-04T00:00:00Z',
      }

      const mockCreatedStockMovement: StockMovement = {
        id: '3',
        tenantId: 'tenant-123',
        ...newStockMovementData,
        createdAt: '2024-01-04T00:00:00Z',
        updatedAt: '2024-01-04T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockCreatedStockMovement, success: true })

      const { createStockMovement } = useStockMovement()
      const result = await createStockMovement(newStockMovementData)

      expect(mockApiFetch).toHaveBeenCalledWith('/stock-movements', {
        method: 'POST',
        body: newStockMovementData,
      })
      expect(result).toEqual(mockCreatedStockMovement)
      expect(result.id).toBe('3')
      expect(result.quantity).toBe(200)
    })

    it('should create a transfer stock movement with destination warehouse', async () => {
      const transferData = {
        productId: 'product-1',
        warehouseId: 'warehouse-1',
        destinationWarehouseId: 'warehouse-2',
        movementType: MovementType.Transfer,
        quantity: 75,
        reference: 'TRF-002',
        notes: 'Transfer between warehouses',
        movementDate: '2024-01-05T00:00:00Z',
      }

      const mockCreatedTransfer: StockMovement = {
        id: '4',
        tenantId: 'tenant-123',
        ...transferData,
        createdAt: '2024-01-05T00:00:00Z',
        updatedAt: '2024-01-05T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockCreatedTransfer, success: true })

      const { createStockMovement } = useStockMovement()
      const result = await createStockMovement(transferData)

      expect(mockApiFetch).toHaveBeenCalledWith('/stock-movements', {
        method: 'POST',
        body: transferData,
      })
      expect(result).toEqual(mockCreatedTransfer)
      expect(result.destinationWarehouseId).toBe('warehouse-2')
    })
  })

  describe('updateStockMovement', () => {
    it('should update an existing stock movement successfully', async () => {
      const updateData = {
        id: '1',
        productId: 'product-1',
        warehouseId: 'warehouse-1',
        movementType: MovementType.Adjustment,
        quantity: 150,
        unitCost: 11.0,
        totalCost: 1650,
        reference: 'ADJ-001',
        notes: 'Inventory adjustment',
        movementDate: '2024-01-06T00:00:00Z',
      }

      const mockUpdatedStockMovement: StockMovement = {
        ...updateData,
        tenantId: 'tenant-123',
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-06T00:00:00Z',
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
      expect(result.reference).toBe('ADJ-001')
    })
  })

  describe('deleteStockMovement', () => {
    it('should delete a stock movement successfully', async () => {
      mockApiFetch.mockResolvedValue(undefined)

      const { deleteStockMovement } = useStockMovement()
      await deleteStockMovement('1')

      expect(mockApiFetch).toHaveBeenCalledWith('/stock-movements/1', {
        method: 'DELETE',
      })
    })
  })

  describe('exportStockMovements', () => {
    let mockFetch: any
    let mockBlob: any
    let mockURL: any
    let mockCreateElement: any
    let mockAppendChild: any
    let mockRemoveChild: any
    let mockClick: any

    beforeEach(() => {
      // Mock fetch
      mockBlob = { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' }
      mockFetch = vi.fn().mockResolvedValue({
        ok: true,
        headers: {
          get: vi.fn((header: string) => {
            if (header === 'Content-Disposition') {
              return 'attachment; filename="stock-movements-export.xlsx"'
            }
            return null
          }),
        },
        blob: vi.fn().mockResolvedValue(mockBlob),
      })
      global.fetch = mockFetch

      // Mock URL.createObjectURL and revokeObjectURL
      mockURL = {
        createObjectURL: vi.fn().mockReturnValue('blob:mock-url'),
        revokeObjectURL: vi.fn(),
      }
      global.URL = mockURL as any

      // Mock document.createElement
      mockClick = vi.fn()
      const mockAnchor = {
        href: '',
        download: '',
        click: mockClick,
      }
      mockCreateElement = vi.fn().mockReturnValue(mockAnchor)
      global.document = {
        createElement: mockCreateElement,
        body: {
          appendChild: vi.fn(),
          removeChild: vi.fn(),
        },
      } as any

      mockAppendChild = global.document.body.appendChild
      mockRemoveChild = global.document.body.removeChild
    })

    it('should export stock movements with default format', async () => {
      const { exportStockMovements } = useStockMovement()
      await exportStockMovements()

      expect(mockFetch).toHaveBeenCalledWith(
        'http://localhost:3001/stock-movements/export',
        {
          headers: {
            'Authorization': 'Bearer mock-token-123',
            'X-Tenant-Id': 'tenant-123',
          },
        },
      )
      expect(mockURL.createObjectURL).toHaveBeenCalledWith(mockBlob)
      expect(mockCreateElement).toHaveBeenCalledWith('a')
      expect(mockClick).toHaveBeenCalled()
      expect(mockAppendChild).toHaveBeenCalled()
      expect(mockRemoveChild).toHaveBeenCalled()
      expect(mockURL.revokeObjectURL).toHaveBeenCalledWith('blob:mock-url')
    })

    it('should export stock movements with CSV format', async () => {
      const { exportStockMovements } = useStockMovement()
      await exportStockMovements({ format: 'csv' })

      expect(mockFetch).toHaveBeenCalledWith(
        'http://localhost:3001/stock-movements/export?format=csv',
        expect.objectContaining({
          headers: {
            'Authorization': 'Bearer mock-token-123',
            'X-Tenant-Id': 'tenant-123',
          },
        }),
      )
    })

    it('should export stock movements with all filter parameters', async () => {
      const filters = {
        format: 'excel' as const,
        brand: 'Apple',
        category: 'Electronics',
        warehouseId: 'warehouse-1',
        fromDate: '2024-01-01',
        toDate: '2024-01-31',
      }

      const { exportStockMovements } = useStockMovement()
      await exportStockMovements(filters)

      expect(mockFetch).toHaveBeenCalledWith(
        'http://localhost:3001/stock-movements/export?format=excel&brand=Apple&category=Electronics&warehouseId=warehouse-1&fromDate=2024-01-01&toDate=2024-01-31',
        expect.objectContaining({
          headers: {
            'Authorization': 'Bearer mock-token-123',
            'X-Tenant-Id': 'tenant-123',
          },
        }),
      )
    })

    it('should throw error when no tenant is selected', async () => {
      mockTenantStore.currentTenantId = ''

      const { exportStockMovements } = useStockMovement()

      await expect(exportStockMovements()).rejects.toThrow('No tenant selected')
      expect(mockFetch).not.toHaveBeenCalled()
    })

    it('should throw error when not authenticated', async () => {
      mockAuthStore.token = ''

      const { exportStockMovements } = useStockMovement()

      await expect(exportStockMovements()).rejects.toThrow('Not authenticated')
      expect(mockFetch).not.toHaveBeenCalled()
    })

    it('should throw error when export fails', async () => {
      mockFetch.mockResolvedValue({
        ok: false,
        status: 500,
      })

      const { exportStockMovements } = useStockMovement()

      await expect(exportStockMovements()).rejects.toThrow('Export failed')
      expect(mockURL.createObjectURL).not.toHaveBeenCalled()
    })

    it('should use default filename when Content-Disposition header is missing', async () => {
      mockFetch.mockResolvedValue({
        ok: true,
        headers: {
          get: vi.fn().mockReturnValue(null),
        },
        blob: vi.fn().mockResolvedValue(mockBlob),
      })

      const mockAnchor = {
        href: '',
        download: '',
        click: mockClick,
      }
      mockCreateElement.mockReturnValue(mockAnchor)

      const { exportStockMovements } = useStockMovement()
      await exportStockMovements({ format: 'csv' })

      // Check that download attribute was set to a filename matching the pattern
      expect(mockAnchor.download).toMatch(/stock-movements-\d{4}-\d{2}-\d{2}\.csv/)
    })
  })

  describe('error handling', () => {
    it('should handle API errors when fetching stock movements', async () => {
      const mockError = new Error('Network error')
      mockApiFetch.mockRejectedValue(mockError)

      const { getAllStockMovements } = useStockMovement()

      await expect(getAllStockMovements()).rejects.toThrow('Network error')
    })

    it('should handle API errors when creating stock movement', async () => {
      const mockError = new Error('Validation error')
      mockApiFetch.mockRejectedValue(mockError)

      const newStockMovementData = {
        productId: 'product-1',
        warehouseId: 'warehouse-1',
        movementType: MovementType.Purchase,
        quantity: 100,
      }

      const { createStockMovement } = useStockMovement()

      await expect(createStockMovement(newStockMovementData)).rejects.toThrow('Validation error')
    })

    it('should handle API errors when updating stock movement', async () => {
      const mockError = new Error('Not found')
      mockApiFetch.mockRejectedValue(mockError)

      const updateData = {
        id: 'non-existent-id',
        productId: 'product-1',
        warehouseId: 'warehouse-1',
        movementType: MovementType.Adjustment,
        quantity: 50,
      }

      const { updateStockMovement } = useStockMovement()

      await expect(updateStockMovement(updateData)).rejects.toThrow('Not found')
    })

    it('should handle API errors when deleting stock movement', async () => {
      const mockError = new Error('Cannot delete')
      mockApiFetch.mockRejectedValue(mockError)

      const { deleteStockMovement } = useStockMovement()

      await expect(deleteStockMovement('1')).rejects.toThrow('Cannot delete')
    })
  })
})
