import { beforeEach, describe, expect, it } from 'vitest'
import { mockApiFetch } from '../setup'
import { useTaxRate } from '~/composables/useTaxRate'
import type { CreateTaxRateDto, TaxRate, UpdateTaxRateDto } from '~/types/billing'

describe('useTaxRate', () => {
  beforeEach(() => {
    // Reset all mocks before each test
    mockApiFetch.mockReset()
  })

  describe('getAllTaxRates', () => {
    it('should fetch all tax rates successfully', async () => {
      const mockTaxRates: TaxRate[] = [
        {
          id: '1',
          tenantId: 'tenant-1',
          code: 'IVA12',
          name: 'IVA 12%',
          rate: 0.12,
          isDefault: true,
          isActive: true,
          countryId: 'ECU',
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
        {
          id: '2',
          tenantId: 'tenant-1',
          code: 'IVA0',
          name: 'IVA 0%',
          rate: 0.00,
          isDefault: false,
          isActive: true,
          countryId: 'ECU',
          createdAt: '2024-01-02T00:00:00Z',
          updatedAt: '2024-01-02T00:00:00Z',
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockTaxRates, success: true })

      const { getAllTaxRates } = useTaxRate()
      const result = await getAllTaxRates()

      expect(mockApiFetch).toHaveBeenCalledWith('/taxrates', {
        method: 'GET',
      })
      expect(result).toEqual(mockTaxRates)
      expect(result).toHaveLength(2)
    })

    it('should handle empty tax rate list', async () => {
      mockApiFetch.mockResolvedValue({ data: [], success: true })

      const { getAllTaxRates } = useTaxRate()
      const result = await getAllTaxRates()

      expect(result).toEqual([])
      expect(result).toHaveLength(0)
    })
  })

  describe('getTaxRateById', () => {
    it('should fetch a tax rate by id successfully', async () => {
      const mockTaxRate: TaxRate = {
        id: '1',
        tenantId: 'tenant-1',
        code: 'IVA12',
        name: 'IVA 12%',
        rate: 0.12,
        isDefault: true,
        isActive: true,
        countryId: 'ECU',
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockTaxRate, success: true })

      const { getTaxRateById } = useTaxRate()
      const result = await getTaxRateById('1')

      expect(mockApiFetch).toHaveBeenCalledWith('/taxrates/1', {
        method: 'GET',
      })
      expect(result).toEqual(mockTaxRate)
      expect(result.id).toBe('1')
      expect(result.code).toBe('IVA12')
      expect(result.rate).toBe(0.12)
    })
  })

  describe('createTaxRate', () => {
    it('should create a new tax rate successfully', async () => {
      const newTaxRateData: CreateTaxRateDto = {
        code: 'VAT20',
        name: 'VAT 20%',
        rate: 0.20,
        isDefault: false,
        isActive: true,
        countryId: 'GBR',
      }

      const mockCreatedTaxRate: TaxRate = {
        id: '3',
        tenantId: 'tenant-1',
        ...newTaxRateData,
        createdAt: '2024-01-03T00:00:00Z',
        updatedAt: '2024-01-03T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockCreatedTaxRate, success: true })

      const { createTaxRate } = useTaxRate()
      const result = await createTaxRate(newTaxRateData)

      expect(mockApiFetch).toHaveBeenCalledWith('/taxrates', {
        method: 'POST',
        body: newTaxRateData,
      })
      expect(result).toEqual(mockCreatedTaxRate)
      expect(result.id).toBe('3')
      expect(result.code).toBe('VAT20')
      expect(result.rate).toBe(0.20)
    })
  })

  describe('updateTaxRate', () => {
    it('should update an existing tax rate successfully', async () => {
      const updateData: UpdateTaxRateDto = {
        id: '1',
        code: 'IVA15',
        name: 'IVA 15%',
        rate: 0.15,
        isDefault: true,
        isActive: true,
        countryId: 'ECU',
      }

      const mockUpdatedTaxRate: TaxRate = {
        ...updateData,
        tenantId: 'tenant-1',
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-05T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockUpdatedTaxRate, success: true })

      const { updateTaxRate } = useTaxRate()
      const result = await updateTaxRate(updateData)

      expect(mockApiFetch).toHaveBeenCalledWith('/taxrates/1', {
        method: 'PUT',
        body: updateData,
      })
      expect(result).toEqual(mockUpdatedTaxRate)
      expect(result.code).toBe('IVA15')
      expect(result.rate).toBe(0.15)
    })
  })

  describe('deleteTaxRate', () => {
    it('should delete a tax rate successfully', async () => {
      mockApiFetch.mockResolvedValue(undefined)

      const { deleteTaxRate } = useTaxRate()
      await deleteTaxRate('1')

      expect(mockApiFetch).toHaveBeenCalledWith('/taxrates/1', {
        method: 'DELETE',
      })
    })
  })

  describe('error handling', () => {
    it('should handle API errors when fetching tax rates', async () => {
      const mockError = new Error('Network error')
      mockApiFetch.mockRejectedValue(mockError)

      const { getAllTaxRates } = useTaxRate()

      await expect(getAllTaxRates()).rejects.toThrow('Network error')
    })

    it('should handle API errors when creating tax rate', async () => {
      const mockError = new Error('Validation error')
      mockApiFetch.mockRejectedValue(mockError)

      const newTaxRateData: CreateTaxRateDto = {
        code: 'TEST',
        name: 'Test Tax Rate',
        rate: 0.10,
        isDefault: false,
        isActive: true,
      }

      const { createTaxRate } = useTaxRate()

      await expect(createTaxRate(newTaxRateData)).rejects.toThrow('Validation error')
    })
  })
})
