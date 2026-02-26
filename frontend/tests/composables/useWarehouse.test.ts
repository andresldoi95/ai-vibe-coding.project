import { beforeEach, describe, expect, it, vi } from 'vitest'
import { mockApiFetch } from '../setup'
import { useWarehouse } from '~/composables/useWarehouse'
import type { Warehouse } from '~/types/inventory'

describe('useWarehouse', () => {
  beforeEach(() => {
    // Reset all mocks before each test
    mockApiFetch.mockReset()
  })

  describe('getAllWarehouses', () => {
    it('should fetch all warehouses successfully', async () => {
      const mockWarehouses: Warehouse[] = [
        {
          id: '1',
          tenantId: 'tenant-1',
          name: 'Main Warehouse',
          code: 'WH-001',
          description: 'Main warehouse location',
          streetAddress: '123 Main St',
          city: 'New York',
          state: 'NY',
          postalCode: '10001',
          country: 'USA',
          phone: '555-0100',
          email: 'main@warehouse.com',
          isActive: true,
          squareFootage: 10000,
          capacity: 5000,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
        {
          id: '2',
          tenantId: 'tenant-1',
          name: 'Secondary Warehouse',
          code: 'WH-002',
          streetAddress: '456 Oak Ave',
          city: 'Los Angeles',
          state: 'CA',
          postalCode: '90001',
          country: 'USA',
          isActive: true,
          createdAt: '2024-01-02T00:00:00Z',
          updatedAt: '2024-01-02T00:00:00Z',
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockWarehouses, success: true })

      const { getAllWarehouses } = useWarehouse()
      const result = await getAllWarehouses()

      expect(mockApiFetch).toHaveBeenCalledWith('/warehouses', {
        method: 'GET',
      })
      expect(result).toEqual(mockWarehouses)
      expect(result).toHaveLength(2)
    })

    it('should handle empty warehouse list', async () => {
      mockApiFetch.mockResolvedValue({ data: [], success: true })

      const { getAllWarehouses } = useWarehouse()
      const result = await getAllWarehouses()

      expect(result).toEqual([])
      expect(result).toHaveLength(0)
    })
  })

  describe('getWarehouseById', () => {
    it('should fetch a warehouse by id successfully', async () => {
      const mockWarehouse: Warehouse = {
        id: '1',
        tenantId: 'tenant-1',
        name: 'Main Warehouse',
        code: 'WH-001',
        description: 'Main warehouse location',
        streetAddress: '123 Main St',
        city: 'New York',
        state: 'NY',
        postalCode: '10001',
        country: 'USA',
        phone: '555-0100',
        email: 'main@warehouse.com',
        isActive: true,
        squareFootage: 10000,
        capacity: 5000,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockWarehouse, success: true })

      const { getWarehouseById } = useWarehouse()
      const result = await getWarehouseById('1')

      expect(mockApiFetch).toHaveBeenCalledWith('/warehouses/1', {
        method: 'GET',
      })
      expect(result).toEqual(mockWarehouse)
      expect(result.id).toBe('1')
      expect(result.name).toBe('Main Warehouse')
    })
  })

  describe('createWarehouse', () => {
    it('should create a new warehouse successfully', async () => {
      const newWarehouseData = {
        name: 'New Warehouse',
        code: 'WH-003',
        description: 'New warehouse location',
        streetAddress: '789 Pine St',
        city: 'Chicago',
        state: 'IL',
        postalCode: '60601',
        country: 'USA',
        phone: '555-0200',
        email: 'new@warehouse.com',
        isActive: true,
        squareFootage: 8000,
        capacity: 4000,
      }

      const mockCreatedWarehouse: Warehouse = {
        id: '3',
        tenantId: 'tenant-1',
        ...newWarehouseData,
        createdAt: '2024-01-03T00:00:00Z',
        updatedAt: '2024-01-03T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockCreatedWarehouse, success: true })

      const { createWarehouse } = useWarehouse()
      const result = await createWarehouse(newWarehouseData)

      expect(mockApiFetch).toHaveBeenCalledWith('/warehouses', {
        method: 'POST',
        body: newWarehouseData,
      })
      expect(result).toEqual(mockCreatedWarehouse)
      expect(result.id).toBe('3')
      expect(result.name).toBe('New Warehouse')
    })
  })

  describe('updateWarehouse', () => {
    it('should update an existing warehouse successfully', async () => {
      const updateData = {
        id: '1',
        name: 'Updated Warehouse',
        code: 'WH-001',
        description: 'Updated description',
        streetAddress: '123 Main St',
        city: 'New York',
        state: 'NY',
        postalCode: '10001',
        country: 'USA',
        phone: '555-0100',
        email: 'updated@warehouse.com',
        isActive: true,
        squareFootage: 12000,
        capacity: 6000,
      }

      const mockUpdatedWarehouse: Warehouse = {
        ...updateData,
        tenantId: 'tenant-1',
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-05T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockUpdatedWarehouse, success: true })

      const { updateWarehouse } = useWarehouse()
      const result = await updateWarehouse(updateData)

      expect(mockApiFetch).toHaveBeenCalledWith('/warehouses/1', {
        method: 'PUT',
        body: updateData,
      })
      expect(result).toEqual(mockUpdatedWarehouse)
      expect(result.name).toBe('Updated Warehouse')
      expect(result.email).toBe('updated@warehouse.com')
    })
  })

  describe('deleteWarehouse', () => {
    it('should delete a warehouse successfully', async () => {
      mockApiFetch.mockResolvedValue(undefined)

      const { deleteWarehouse } = useWarehouse()
      await deleteWarehouse('1')

      expect(mockApiFetch).toHaveBeenCalledWith('/warehouses/1', {
        method: 'DELETE',
      })
    })
  })

  describe('exportWarehouseStockSummary', () => {
    let mockFetch: ReturnType<typeof vi.fn>
    let mockBlob: Blob
    let mockCreateElement: ReturnType<typeof vi.spyOn>
    let mockAppendChild: ReturnType<typeof vi.spyOn>
    let mockRemoveChild: ReturnType<typeof vi.spyOn>
    let mockCreateObjectURL: ReturnType<typeof vi.fn>
    let mockRevokeObjectURL: ReturnType<typeof vi.fn>

    beforeEach(() => {
      // Mock fetch
      mockBlob = new Blob(['test data'])
      mockFetch = vi.fn().mockResolvedValue({
        ok: true,
        headers: {
          get: vi.fn((header: string) => {
            if (header === 'Content-Disposition')
              return 'attachment; filename="warehouse-report.xlsx"'
            return null
          }),
        },
        blob: vi.fn().mockResolvedValue(mockBlob),
      })
      globalThis.fetch = mockFetch

      // Mock DOM APIs
      mockCreateObjectURL = vi.fn().mockReturnValue('blob:mock-url')
      mockRevokeObjectURL = vi.fn()
      globalThis.URL.createObjectURL = mockCreateObjectURL
      globalThis.URL.revokeObjectURL = mockRevokeObjectURL

      const mockAnchor = {
        href: '',
        download: '',
        click: vi.fn(),
      }
      mockCreateElement = vi.spyOn(document, 'createElement').mockReturnValue(mockAnchor as unknown as HTMLAnchorElement)
      mockAppendChild = vi.spyOn(document.body, 'appendChild').mockImplementation(() => mockAnchor as unknown as HTMLAnchorElement)
      mockRemoveChild = vi.spyOn(document.body, 'removeChild').mockImplementation(() => mockAnchor as unknown as HTMLAnchorElement)
    })

    it('should export warehouse stock summary with default format', async () => {
      const { exportWarehouseStockSummary } = useWarehouse()

      await exportWarehouseStockSummary()

      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('/warehouses/export/stock-summary'),
        expect.objectContaining({
          headers: expect.objectContaining({
            'Authorization': expect.any(String),
            'X-Tenant-Id': expect.any(String),
          }),
        }),
      )
      expect(mockCreateObjectURL).toHaveBeenCalledWith(mockBlob)
      expect(mockCreateElement).toHaveBeenCalledWith('a')
      expect(mockRevokeObjectURL).toHaveBeenCalledWith('blob:mock-url')
    })

    it('should export with excel format', async () => {
      const { exportWarehouseStockSummary } = useWarehouse()

      await exportWarehouseStockSummary({ format: 'excel' })

      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('?format=excel'),
        expect.any(Object),
      )
    })

    it('should export with csv format', async () => {
      const { exportWarehouseStockSummary } = useWarehouse()

      await exportWarehouseStockSummary({ format: 'csv' })

      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('?format=csv'),
        expect.any(Object),
      )
    })

    it('should use filename from Content-Disposition header', async () => {
      const { exportWarehouseStockSummary } = useWarehouse()

      await exportWarehouseStockSummary()

      const anchor = mockCreateElement.mock.results[0].value
      expect(anchor.download).toBe('warehouse-report.xlsx')
    })

    it('should generate default filename when Content-Disposition is missing', async () => {
      mockCreateElement.mockClear()
      mockFetch.mockResolvedValue({
        ok: true,
        headers: {
          get: vi.fn().mockReturnValue(null),
        },
        blob: vi.fn().mockResolvedValue(mockBlob),
      })

      const { exportWarehouseStockSummary } = useWarehouse()

      await exportWarehouseStockSummary()

      const anchor = mockCreateElement.mock.results[0].value
      expect(anchor.download).toMatch(/warehouse-stock-summary-.*\.xlsx/)
    })

    it('should use correct file extension for CSV format', async () => {
      mockCreateElement.mockClear()
      mockFetch.mockResolvedValue({
        ok: true,
        headers: {
          get: vi.fn().mockReturnValue(null),
        },
        blob: vi.fn().mockResolvedValue(mockBlob),
      })

      const { exportWarehouseStockSummary } = useWarehouse()

      await exportWarehouseStockSummary({ format: 'csv' })

      const anchor = mockCreateElement.mock.results[0].value
      expect(anchor.download).toMatch(/\.csv$/)
    })

    it('should throw error when no tenant selected', async () => {
      const mockTenantStore = useTenantStore()
      const originalTenantId = mockTenantStore.currentTenantId
      mockTenantStore.currentTenantId = null

      const { exportWarehouseStockSummary } = useWarehouse()

      await expect(exportWarehouseStockSummary()).rejects.toThrow('No tenant selected')

      mockTenantStore.currentTenantId = originalTenantId
    })

    it('should throw error when not authenticated', async () => {
      const mockAuthStore = useAuthStore()
      const originalToken = mockAuthStore.token
      mockAuthStore.token = null

      const { exportWarehouseStockSummary } = useWarehouse()

      await expect(exportWarehouseStockSummary()).rejects.toThrow('Not authenticated')

      mockAuthStore.token = originalToken
    })

    it('should throw error when export fails', async () => {
      mockFetch.mockResolvedValue({
        ok: false,
        status: 500,
        statusText: 'Internal Server Error',
      })

      const { exportWarehouseStockSummary } = useWarehouse()

      await expect(exportWarehouseStockSummary()).rejects.toThrow('Export failed')
    })

    it('should clean up blob URL after download', async () => {
      const { exportWarehouseStockSummary } = useWarehouse()

      await exportWarehouseStockSummary()

      expect(mockRevokeObjectURL).toHaveBeenCalledWith('blob:mock-url')
    })

    it('should append and remove anchor element from DOM', async () => {
      const { exportWarehouseStockSummary } = useWarehouse()

      await exportWarehouseStockSummary()

      expect(mockAppendChild).toHaveBeenCalled()
      expect(mockRemoveChild).toHaveBeenCalled()
    })
  })

  describe('error handling', () => {
    it('should handle API errors when fetching warehouses', async () => {
      const mockError = new Error('Network error')
      mockApiFetch.mockRejectedValue(mockError)

      const { getAllWarehouses } = useWarehouse()

      await expect(getAllWarehouses()).rejects.toThrow('Network error')
    })

    it('should handle API errors when creating warehouse', async () => {
      const mockError = new Error('Validation error')
      mockApiFetch.mockRejectedValue(mockError)

      const newWarehouseData = {
        name: 'Test Warehouse',
        code: 'WH-005',
        streetAddress: '123 Test St',
        city: 'Test City',
        postalCode: '12345',
        country: 'Test Country',
      }

      const { createWarehouse } = useWarehouse()

      await expect(createWarehouse(newWarehouseData)).rejects.toThrow('Validation error')
    })
  })
})
