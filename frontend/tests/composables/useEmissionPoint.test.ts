import { beforeEach, describe, expect, it } from 'vitest'
import { mockApiFetch } from '../setup'
import { useEmissionPoint } from '~/composables/useEmissionPoint'
import { DocumentType } from '~/types/sri-enums'
import type { CreateEmissionPointDto, EmissionPoint, UpdateEmissionPointDto } from '~/types/emission-point'

describe('useEmissionPoint', () => {
  beforeEach(() => {
    mockApiFetch.mockReset()
  })

  describe('getAllEmissionPoints', () => {
    it('should fetch all emission points successfully', async () => {
      const mockEmissionPoints: EmissionPoint[] = [
        {
          id: '1',
          emissionPointCode: '001',
          name: 'Main Counter',
          isActive: true,
          invoiceSequence: 1,
          creditNoteSequence: 1,
          debitNoteSequence: 1,
          retentionSequence: 1,
          establishmentId: 'est-1',
          establishmentCode: '001',
          establishmentName: 'Main Office',
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
        {
          id: '2',
          emissionPointCode: '002',
          name: 'Secondary Counter',
          isActive: true,
          invoiceSequence: 100,
          creditNoteSequence: 10,
          debitNoteSequence: 5,
          retentionSequence: 8,
          establishmentId: 'est-1',
          establishmentCode: '001',
          establishmentName: 'Main Office',
          createdAt: '2024-01-02T00:00:00Z',
          updatedAt: '2024-01-02T00:00:00Z',
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockEmissionPoints, success: true })

      const { getAllEmissionPoints } = useEmissionPoint()
      const result = await getAllEmissionPoints()

      expect(mockApiFetch).toHaveBeenCalledWith('/emission-points', {
        method: 'GET',
      })
      expect(result).toEqual(mockEmissionPoints)
      expect(result).toHaveLength(2)
    })

    it('should fetch emission points with establishmentId filter', async () => {
      const mockEmissionPoints: EmissionPoint[] = [
        {
          id: '1',
          emissionPointCode: '001',
          name: 'Counter A',
          isActive: true,
          invoiceSequence: 1,
          creditNoteSequence: 1,
          debitNoteSequence: 1,
          retentionSequence: 1,
          establishmentId: 'est-1',
          createdAt: '2024-01-01T00:00:00Z',
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockEmissionPoints, success: true })

      const { getAllEmissionPoints } = useEmissionPoint()
      const result = await getAllEmissionPoints({ establishmentId: 'est-1' })

      expect(mockApiFetch).toHaveBeenCalledWith('/emission-points?establishmentId=est-1', {
        method: 'GET',
      })
      expect(result).toHaveLength(1)
    })

    it('should fetch emission points with isActive filter', async () => {
      const mockEmissionPoints: EmissionPoint[] = [
        {
          id: '1',
          emissionPointCode: '001',
          name: 'Active Counter',
          isActive: true,
          invoiceSequence: 1,
          creditNoteSequence: 1,
          debitNoteSequence: 1,
          retentionSequence: 1,
          establishmentId: 'est-1',
          createdAt: '2024-01-01T00:00:00Z',
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockEmissionPoints, success: true })

      const { getAllEmissionPoints } = useEmissionPoint()
      const result = await getAllEmissionPoints({ isActive: true })

      expect(mockApiFetch).toHaveBeenCalledWith('/emission-points?isActive=true', {
        method: 'GET',
      })
      expect(result.every(ep => ep.isActive)).toBe(true)
    })

    it('should fetch emission points with multiple filters', async () => {
      const mockEmissionPoints: EmissionPoint[] = []

      mockApiFetch.mockResolvedValue({ data: mockEmissionPoints, success: true })

      const { getAllEmissionPoints } = useEmissionPoint()
      const result = await getAllEmissionPoints({ establishmentId: 'est-1', isActive: false })

      expect(mockApiFetch).toHaveBeenCalledWith('/emission-points?establishmentId=est-1&isActive=false', {
        method: 'GET',
      })
      expect(result).toHaveLength(0)
    })

    it('should handle empty emission points list', async () => {
      mockApiFetch.mockResolvedValue({ data: [], success: true })

      const { getAllEmissionPoints } = useEmissionPoint()
      const result = await getAllEmissionPoints()

      expect(result).toEqual([])
      expect(result).toHaveLength(0)
    })

    it('should handle API errors when fetching emission points', async () => {
      mockApiFetch.mockRejectedValue(new Error('Network error'))

      const { getAllEmissionPoints } = useEmissionPoint()

      await expect(getAllEmissionPoints()).rejects.toThrow('Network error')
    })
  })

  describe('getEmissionPointById', () => {
    it('should fetch an emission point by id successfully', async () => {
      const mockEmissionPoint: EmissionPoint = {
        id: '1',
        emissionPointCode: '001',
        name: 'Main Counter',
        isActive: true,
        invoiceSequence: 250,
        creditNoteSequence: 15,
        debitNoteSequence: 8,
        retentionSequence: 30,
        establishmentId: 'est-1',
        establishmentCode: '001',
        establishmentName: 'Main Office',
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockEmissionPoint, success: true })

      const { getEmissionPointById } = useEmissionPoint()
      const result = await getEmissionPointById('1')

      expect(mockApiFetch).toHaveBeenCalledWith('/emission-points/1', {
        method: 'GET',
      })
      expect(result).toEqual(mockEmissionPoint)
      expect(result.id).toBe('1')
      expect(result.emissionPointCode).toBe('001')
    })

    it('should handle API errors when fetching emission point by id', async () => {
      mockApiFetch.mockRejectedValue(new Error('Emission point not found'))

      const { getEmissionPointById } = useEmissionPoint()

      await expect(getEmissionPointById('999')).rejects.toThrow('Emission point not found')
    })
  })

  describe('getEmissionPointsByEstablishment', () => {
    it('should fetch emission points by establishment id', async () => {
      const mockEmissionPoints: EmissionPoint[] = [
        {
          id: '1',
          emissionPointCode: '001',
          name: 'Counter 1',
          isActive: true,
          invoiceSequence: 1,
          creditNoteSequence: 1,
          debitNoteSequence: 1,
          retentionSequence: 1,
          establishmentId: 'est-1',
          createdAt: '2024-01-01T00:00:00Z',
        },
        {
          id: '2',
          emissionPointCode: '002',
          name: 'Counter 2',
          isActive: true,
          invoiceSequence: 1,
          creditNoteSequence: 1,
          debitNoteSequence: 1,
          retentionSequence: 1,
          establishmentId: 'est-1',
          createdAt: '2024-01-02T00:00:00Z',
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockEmissionPoints, success: true })

      const { getEmissionPointsByEstablishment } = useEmissionPoint()
      const result = await getEmissionPointsByEstablishment('est-1')

      expect(mockApiFetch).toHaveBeenCalledWith('/emission-points?establishmentId=est-1', {
        method: 'GET',
      })
      expect(result).toHaveLength(2)
      expect(result.every(ep => ep.establishmentId === 'est-1')).toBe(true)
    })

    it('should handle empty result when establishment has no emission points', async () => {
      mockApiFetch.mockResolvedValue({ data: [], success: true })

      const { getEmissionPointsByEstablishment } = useEmissionPoint()
      const result = await getEmissionPointsByEstablishment('est-999')

      expect(result).toEqual([])
    })
  })

  describe('getNextSequential', () => {
    it('should get next invoice sequential successfully', async () => {
      const mockResponse = {
        documentType: DocumentType.Invoice,
        currentSequence: 123,
        nextSequential: 124,
        formattedSequential: '001-001-000000124',
      }

      mockApiFetch.mockResolvedValue({ data: mockResponse, success: true })

      const { getNextSequential } = useEmissionPoint()
      const result = await getNextSequential('ep-1', DocumentType.Invoice)

      expect(mockApiFetch).toHaveBeenCalledWith('/emission-points/ep-1/next-sequential/1', {
        method: 'GET',
      })
      expect(result).toEqual(mockResponse)
      expect(result.nextSequential).toBe(124)
      expect(result.formattedSequential).toBe('001-001-000000124')
    })

    it('should get next credit note sequential successfully', async () => {
      const mockResponse = {
        documentType: DocumentType.CreditNote,
        currentSequence: 50,
        nextSequential: 51,
        formattedSequential: '001-001-000000051',
      }

      mockApiFetch.mockResolvedValue({ data: mockResponse, success: true })

      const { getNextSequential } = useEmissionPoint()
      const result = await getNextSequential('ep-1', DocumentType.CreditNote)

      expect(mockApiFetch).toHaveBeenCalledWith('/emission-points/ep-1/next-sequential/4', {
        method: 'GET',
      })
      expect(result.documentType).toBe(DocumentType.CreditNote)
      expect(result.nextSequential).toBe(51)
    })

    it('should get next retention sequential successfully', async () => {
      const mockResponse = {
        documentType: DocumentType.Retention,
        currentSequence: 0,
        nextSequential: 1,
        formattedSequential: '001-001-000000001',
      }

      mockApiFetch.mockResolvedValue({ data: mockResponse, success: true })

      const { getNextSequential } = useEmissionPoint()
      const result = await getNextSequential('ep-1', DocumentType.Retention)

      expect(mockApiFetch).toHaveBeenCalledWith('/emission-points/ep-1/next-sequential/7', {
        method: 'GET',
      })
      expect(result.nextSequential).toBe(1)
    })

    it('should handle API errors when getting next sequential', async () => {
      mockApiFetch.mockRejectedValue(new Error('Emission point not found'))

      const { getNextSequential } = useEmissionPoint()

      await expect(getNextSequential('invalid-id', DocumentType.Invoice)).rejects.toThrow('Emission point not found')
    })
  })

  describe('createEmissionPoint', () => {
    it('should create an emission point successfully', async () => {
      const createData: CreateEmissionPointDto = {
        emissionPointCode: '003',
        name: 'New Counter',
        isActive: true,
        establishmentId: 'est-1',
      }

      const mockCreatedEmissionPoint: EmissionPoint = {
        id: '3',
        ...createData,
        invoiceSequence: 1,
        creditNoteSequence: 1,
        debitNoteSequence: 1,
        retentionSequence: 1,
        createdAt: '2024-01-03T00:00:00Z',
        updatedAt: '2024-01-03T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockCreatedEmissionPoint, success: true })

      const { createEmissionPoint } = useEmissionPoint()
      const result = await createEmissionPoint(createData)

      expect(mockApiFetch).toHaveBeenCalledWith('/emission-points', {
        method: 'POST',
        body: createData,
      })
      expect(result).toEqual(mockCreatedEmissionPoint)
      expect(result.id).toBe('3')
      expect(result.name).toBe('New Counter')
      expect(result.invoiceSequence).toBe(1)
    })

    it('should create an inactive emission point', async () => {
      const createData: CreateEmissionPointDto = {
        emissionPointCode: '004',
        name: 'Backup Counter',
        isActive: false,
        establishmentId: 'est-2',
      }

      const mockCreatedEmissionPoint: EmissionPoint = {
        id: '4',
        ...createData,
        invoiceSequence: 1,
        creditNoteSequence: 1,
        debitNoteSequence: 1,
        retentionSequence: 1,
        createdAt: '2024-01-04T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockCreatedEmissionPoint, success: true })

      const { createEmissionPoint } = useEmissionPoint()
      const result = await createEmissionPoint(createData)

      expect(result.isActive).toBe(false)
    })

    it('should handle API errors when creating emission point', async () => {
      const createData: CreateEmissionPointDto = {
        emissionPointCode: '001', // Duplicate
        name: 'Duplicate',
        isActive: true,
        establishmentId: 'est-1',
      }

      mockApiFetch.mockRejectedValue(new Error('Emission point code already exists for this establishment'))

      const { createEmissionPoint } = useEmissionPoint()

      await expect(createEmissionPoint(createData)).rejects.toThrow('Emission point code already exists for this establishment')
    })
  })

  describe('updateEmissionPoint', () => {
    it('should update an emission point successfully', async () => {
      const updateData: UpdateEmissionPointDto = {
        name: 'Updated Counter Name',
        isActive: true,
      }

      const mockUpdatedEmissionPoint: EmissionPoint = {
        id: '1',
        emissionPointCode: '001',
        ...updateData,
        invoiceSequence: 250,
        creditNoteSequence: 15,
        debitNoteSequence: 8,
        retentionSequence: 30,
        establishmentId: 'est-1',
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-05T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockUpdatedEmissionPoint, success: true })

      const { updateEmissionPoint } = useEmissionPoint()
      const result = await updateEmissionPoint('1', updateData)

      expect(mockApiFetch).toHaveBeenCalledWith('/emission-points/1', {
        method: 'PUT',
        body: updateData,
      })
      expect(result).toEqual(mockUpdatedEmissionPoint)
      expect(result.name).toBe('Updated Counter Name')
    })

    it('should deactivate an emission point', async () => {
      const updateData: UpdateEmissionPointDto = {
        name: 'Main Counter',
        isActive: false,
      }

      const mockUpdatedEmissionPoint: EmissionPoint = {
        id: '1',
        emissionPointCode: '001',
        ...updateData,
        invoiceSequence: 100,
        creditNoteSequence: 10,
        debitNoteSequence: 5,
        retentionSequence: 12,
        establishmentId: 'est-1',
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-06T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockUpdatedEmissionPoint, success: true })

      const { updateEmissionPoint } = useEmissionPoint()
      const result = await updateEmissionPoint('1', updateData)

      expect(result.isActive).toBe(false)
    })

    it('should handle API errors when updating emission point', async () => {
      const updateData: UpdateEmissionPointDto = {
        name: 'Updated Name',
        isActive: true,
      }

      mockApiFetch.mockRejectedValue(new Error('Emission point not found'))

      const { updateEmissionPoint } = useEmissionPoint()

      await expect(updateEmissionPoint('999', updateData)).rejects.toThrow('Emission point not found')
    })
  })

  describe('deleteEmissionPoint', () => {
    it('should delete an emission point successfully', async () => {
      mockApiFetch.mockResolvedValue({ success: true })

      const { deleteEmissionPoint } = useEmissionPoint()
      await deleteEmissionPoint('1')

      expect(mockApiFetch).toHaveBeenCalledWith('/emission-points/1', {
        method: 'DELETE',
      })
    })

    it('should handle API errors when deleting emission point', async () => {
      mockApiFetch.mockRejectedValue(new Error('Cannot delete emission point with issued documents'))

      const { deleteEmissionPoint } = useEmissionPoint()

      await expect(deleteEmissionPoint('1')).rejects.toThrow('Cannot delete emission point with issued documents')
    })
  })
})
