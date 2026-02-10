import { beforeEach, describe, expect, it } from 'vitest'
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
