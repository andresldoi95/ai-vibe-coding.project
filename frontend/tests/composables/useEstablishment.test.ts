import { beforeEach, describe, expect, it } from 'vitest'
import { mockApiFetch } from '../setup'
import { useEstablishment } from '~/composables/useEstablishment'
import type { CreateEstablishmentDto, Establishment, UpdateEstablishmentDto } from '~/types/establishment'

describe('useEstablishment', () => {
  beforeEach(() => {
    mockApiFetch.mockReset()
  })

  describe('getAllEstablishments', () => {
    it('should fetch all establishments successfully', async () => {
      const mockEstablishments: Establishment[] = [
        {
          id: '1',
          establishmentCode: '001',
          name: 'Main Office',
          address: '123 Main St, Quito, Ecuador',
          phone: '+593-2-123-4567',
          email: 'main@company.com',
          isActive: true,
          tenantId: 'tenant-1',
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
        {
          id: '2',
          establishmentCode: '002',
          name: 'Branch Office',
          address: '456 Secondary Ave, Guayaquil, Ecuador',
          phone: '+593-4-765-4321',
          email: 'branch@company.com',
          isActive: true,
          tenantId: 'tenant-1',
          createdAt: '2024-01-02T00:00:00Z',
          updatedAt: '2024-01-02T00:00:00Z',
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockEstablishments, success: true })

      const { getAllEstablishments } = useEstablishment()
      const result = await getAllEstablishments()

      expect(mockApiFetch).toHaveBeenCalledWith('/establishments', {
        method: 'GET',
      })
      expect(result).toEqual(mockEstablishments)
      expect(result).toHaveLength(2)
    })

    it('should handle empty establishments list', async () => {
      mockApiFetch.mockResolvedValue({ data: [], success: true })

      const { getAllEstablishments } = useEstablishment()
      const result = await getAllEstablishments()

      expect(result).toEqual([])
      expect(result).toHaveLength(0)
    })

    it('should handle API errors when fetching establishments', async () => {
      mockApiFetch.mockRejectedValue(new Error('Network error'))

      const { getAllEstablishments } = useEstablishment()

      await expect(getAllEstablishments()).rejects.toThrow('Network error')
    })
  })

  describe('getEstablishmentById', () => {
    it('should fetch an establishment by id successfully', async () => {
      const mockEstablishment: Establishment = {
        id: '1',
        establishmentCode: '001',
        name: 'Main Office',
        address: '123 Main St, Quito, Ecuador',
        phone: '+593-2-123-4567',
        email: 'main@company.com',
        isActive: true,
        tenantId: 'tenant-1',
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockEstablishment, success: true })

      const { getEstablishmentById } = useEstablishment()
      const result = await getEstablishmentById('1')

      expect(mockApiFetch).toHaveBeenCalledWith('/establishments/1', {
        method: 'GET',
      })
      expect(result).toEqual(mockEstablishment)
      expect(result.id).toBe('1')
      expect(result.establishmentCode).toBe('001')
    })

    it('should handle API errors when fetching establishment by id', async () => {
      mockApiFetch.mockRejectedValue(new Error('Establishment not found'))

      const { getEstablishmentById } = useEstablishment()

      await expect(getEstablishmentById('999')).rejects.toThrow('Establishment not found')
    })
  })

  describe('createEstablishment', () => {
    it('should create an establishment successfully', async () => {
      const createData: CreateEstablishmentDto = {
        establishmentCode: '003',
        name: 'New Branch',
        address: '789 New St, Cuenca, Ecuador',
        phone: '+593-7-888-9999',
        email: 'newbranch@company.com',
        isActive: true,
      }

      const mockCreatedEstablishment: Establishment = {
        id: '3',
        tenantId: 'tenant-1',
        ...createData,
        createdAt: '2024-01-03T00:00:00Z',
        updatedAt: '2024-01-03T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockCreatedEstablishment, success: true })

      const { createEstablishment } = useEstablishment()
      const result = await createEstablishment(createData)

      expect(mockApiFetch).toHaveBeenCalledWith('/establishments', {
        method: 'POST',
        body: createData,
      })
      expect(result).toEqual(mockCreatedEstablishment)
      expect(result.id).toBe('3')
      expect(result.name).toBe('New Branch')
    })

    it('should create an establishment without optional fields', async () => {
      const createData: CreateEstablishmentDto = {
        establishmentCode: '004',
        name: 'Minimal Branch',
        address: '999 Basic St, Loja, Ecuador',
        isActive: false,
      }

      const mockCreatedEstablishment: Establishment = {
        id: '4',
        tenantId: 'tenant-1',
        ...createData,
        createdAt: '2024-01-04T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockCreatedEstablishment, success: true })

      const { createEstablishment } = useEstablishment()
      const result = await createEstablishment(createData)

      expect(mockApiFetch).toHaveBeenCalledWith('/establishments', {
        method: 'POST',
        body: createData,
      })
      expect(result.phone).toBeUndefined()
      expect(result.email).toBeUndefined()
      expect(result.isActive).toBe(false)
    })

    it('should handle API errors when creating establishment', async () => {
      const createData: CreateEstablishmentDto = {
        establishmentCode: '001', // Duplicate code
        name: 'Duplicate',
        address: 'Some Address',
        isActive: true,
      }

      mockApiFetch.mockRejectedValue(new Error('Establishment code already exists'))

      const { createEstablishment } = useEstablishment()

      await expect(createEstablishment(createData)).rejects.toThrow('Establishment code already exists')
    })
  })

  describe('updateEstablishment', () => {
    it('should update an establishment successfully', async () => {
      const updateData: UpdateEstablishmentDto = {
        establishmentCode: '001',
        name: 'Main Office Updated',
        address: '123 Main St Updated, Quito, Ecuador',
        phone: '+593-2-000-0000',
        email: 'main.updated@company.com',
        isActive: true,
      }

      const mockUpdatedEstablishment: Establishment = {
        id: '1',
        tenantId: 'tenant-1',
        ...updateData,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-05T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockUpdatedEstablishment, success: true })

      const { updateEstablishment } = useEstablishment()
      const result = await updateEstablishment('1', updateData)

      expect(mockApiFetch).toHaveBeenCalledWith('/establishments/1', {
        method: 'PUT',
        body: updateData,
      })
      expect(result).toEqual(mockUpdatedEstablishment)
      expect(result.name).toBe('Main Office Updated')
    })

    it('should update establishment status to inactive', async () => {
      const updateData: UpdateEstablishmentDto = {
        establishmentCode: '001',
        name: 'Main Office',
        address: '123 Main St, Quito, Ecuador',
        isActive: false,
      }

      const mockUpdatedEstablishment: Establishment = {
        id: '1',
        tenantId: 'tenant-1',
        ...updateData,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-06T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockUpdatedEstablishment, success: true })

      const { updateEstablishment } = useEstablishment()
      const result = await updateEstablishment('1', updateData)

      expect(result.isActive).toBe(false)
    })

    it('should handle API errors when updating establishment', async () => {
      const updateData: UpdateEstablishmentDto = {
        establishmentCode: '002', // Duplicate code
        name: 'Test',
        address: 'Test Address',
        isActive: true,
      }

      mockApiFetch.mockRejectedValue(new Error('Establishment code already in use'))

      const { updateEstablishment } = useEstablishment()

      await expect(updateEstablishment('1', updateData)).rejects.toThrow('Establishment code already in use')
    })
  })

  describe('deleteEstablishment', () => {
    it('should delete an establishment successfully', async () => {
      mockApiFetch.mockResolvedValue({ success: true })

      const { deleteEstablishment } = useEstablishment()
      await deleteEstablishment('1')

      expect(mockApiFetch).toHaveBeenCalledWith('/establishments/1', {
        method: 'DELETE',
      })
    })

    it('should handle API errors when deleting establishment', async () => {
      mockApiFetch.mockRejectedValue(new Error('Cannot delete establishment with emission points'))

      const { deleteEstablishment } = useEstablishment()

      await expect(deleteEstablishment('1')).rejects.toThrow('Cannot delete establishment with emission points')
    })
  })
})
