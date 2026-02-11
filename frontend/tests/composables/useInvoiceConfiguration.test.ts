import { beforeEach, describe, expect, it } from 'vitest'
import { mockApiFetch } from '../setup'
import { useInvoiceConfiguration } from '~/composables/useInvoiceConfiguration'
import type { InvoiceConfiguration, UpdateInvoiceConfigurationDto } from '~/types/billing'

describe('useInvoiceConfiguration', () => {
  beforeEach(() => {
    // Reset all mocks before each test
    mockApiFetch.mockReset()
  })

  describe('getInvoiceConfiguration', () => {
    it('should fetch invoice configuration successfully', async () => {
      const mockConfiguration: InvoiceConfiguration = {
        id: '1',
        tenantId: 'tenant-1',
        establishmentCode: '001',
        emissionPointCode: '001',
        nextSequentialNumber: 1,
        defaultTaxRateId: 'tax-1',
        defaultTaxRateName: 'IVA 12%',
        defaultWarehouseId: 'warehouse-1',
        defaultWarehouseName: 'Main Warehouse',
        dueDays: 30,
        requireCustomerTaxId: true,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockConfiguration, success: true })

      const { getInvoiceConfiguration } = useInvoiceConfiguration()
      const result = await getInvoiceConfiguration()

      expect(mockApiFetch).toHaveBeenCalledWith('/invoice-configurations', {
        method: 'GET',
      })
      expect(result).toEqual(mockConfiguration)
      expect(result.id).toBe('1')
      expect(result.establishmentCode).toBe('001')
      expect(result.emissionPointCode).toBe('001')
      expect(result.dueDays).toBe(30)
    })

    it('should fetch configuration without optional fields', async () => {
      const mockConfiguration: InvoiceConfiguration = {
        id: '1',
        tenantId: 'tenant-1',
        establishmentCode: '001',
        emissionPointCode: '001',
        nextSequentialNumber: 1,
        dueDays: 15,
        requireCustomerTaxId: false,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockConfiguration, success: true })

      const { getInvoiceConfiguration } = useInvoiceConfiguration()
      const result = await getInvoiceConfiguration()

      expect(result).toEqual(mockConfiguration)
      expect(result.defaultTaxRateId).toBeUndefined()
      expect(result.defaultWarehouseId).toBeUndefined()
    })
  })

  describe('updateInvoiceConfiguration', () => {
    it('should update invoice configuration successfully', async () => {
      const updateData: UpdateInvoiceConfigurationDto = {
        establishmentCode: '002',
        emissionPointCode: '002',
        defaultTaxRateId: 'tax-2',
        defaultWarehouseId: 'warehouse-2',
        dueDays: 45,
        requireCustomerTaxId: true,
      }

      const mockUpdatedConfiguration: InvoiceConfiguration = {
        id: '1',
        tenantId: 'tenant-1',
        establishmentCode: '002',
        emissionPointCode: '002',
        nextSequentialNumber: 1,
        defaultTaxRateId: 'tax-2',
        defaultTaxRateName: 'IVA 15%',
        defaultWarehouseId: 'warehouse-2',
        defaultWarehouseName: 'Secondary Warehouse',
        dueDays: 45,
        requireCustomerTaxId: true,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-05T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockUpdatedConfiguration, success: true })

      const { updateInvoiceConfiguration } = useInvoiceConfiguration()
      const result = await updateInvoiceConfiguration(updateData)

      expect(mockApiFetch).toHaveBeenCalledWith('/invoice-configurations', {
        method: 'PUT',
        body: updateData,
      })
      expect(result).toEqual(mockUpdatedConfiguration)
      expect(result.establishmentCode).toBe('002')
      expect(result.emissionPointCode).toBe('002')
      expect(result.dueDays).toBe(45)
    })

    it('should update configuration with minimal data', async () => {
      const updateData: UpdateInvoiceConfigurationDto = {
        establishmentCode: '001',
        emissionPointCode: '001',
        dueDays: 30,
        requireCustomerTaxId: false,
      }

      const mockUpdatedConfiguration: InvoiceConfiguration = {
        id: '1',
        tenantId: 'tenant-1',
        establishmentCode: '001',
        emissionPointCode: '001',
        nextSequentialNumber: 1,
        dueDays: 30,
        requireCustomerTaxId: false,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-05T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockUpdatedConfiguration, success: true })

      const { updateInvoiceConfiguration } = useInvoiceConfiguration()
      const result = await updateInvoiceConfiguration(updateData)

      expect(result).toEqual(mockUpdatedConfiguration)
      expect(result.defaultTaxRateId).toBeUndefined()
      expect(result.defaultWarehouseId).toBeUndefined()
    })
  })

  describe('error handling', () => {
    it('should handle API errors when fetching configuration', async () => {
      const mockError = new Error('Network error')
      mockApiFetch.mockRejectedValue(mockError)

      const { getInvoiceConfiguration } = useInvoiceConfiguration()

      await expect(getInvoiceConfiguration()).rejects.toThrow('Network error')
    })

    it('should handle API errors when updating configuration', async () => {
      const mockError = new Error('Validation error')
      mockApiFetch.mockRejectedValue(mockError)

      const updateData: UpdateInvoiceConfigurationDto = {
        establishmentCode: '001',
        emissionPointCode: '001',
        dueDays: 30,
        requireCustomerTaxId: true,
      }

      const { updateInvoiceConfiguration } = useInvoiceConfiguration()

      await expect(updateInvoiceConfiguration(updateData)).rejects.toThrow('Validation error')
    })
  })
})
