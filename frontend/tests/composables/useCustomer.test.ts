import { beforeEach, describe, expect, it } from 'vitest'
import { mockApiFetch } from '../setup'
import { useCustomer } from '~/composables/useCustomer'
import type { Customer, CustomerFilters } from '~/types/billing'

describe('useCustomer', () => {
  beforeEach(() => {
    // Reset all mocks before each test
    mockApiFetch.mockReset()
  })

  describe('getAllCustomers', () => {
    it('should fetch all customers successfully', async () => {
      const mockCustomers: Customer[] = [
        {
          id: '1',
          tenantId: 'tenant-1',
          name: 'Acme Corp',
          email: 'contact@acme.com',
          phone: '555-0100',
          identificationType: 1,
          taxId: '1234567890',
          contactPerson: 'John Doe',
          billingStreet: '123 Main St',
          billingCity: 'New York',
          billingState: 'NY',
          billingPostalCode: '10001',
          billingCountry: 'USA',
          shippingStreet: '123 Main St',
          shippingCity: 'New York',
          shippingState: 'NY',
          shippingPostalCode: '10001',
          shippingCountry: 'USA',
          notes: 'Premium customer',
          website: 'https://acme.com',
          isActive: true,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
        {
          id: '2',
          tenantId: 'tenant-1',
          name: 'Tech Solutions Inc',
          email: 'info@techsolutions.com',
          phone: '555-0200',
          identificationType: 1,
          taxId: '9876543210',
          isActive: true,
          createdAt: '2024-01-02T00:00:00Z',
          updatedAt: '2024-01-02T00:00:00Z',
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockCustomers, success: true })

      const { getAllCustomers } = useCustomer()
      const result = await getAllCustomers()

      expect(mockApiFetch).toHaveBeenCalledWith('/customers', {
        method: 'GET',
      })
      expect(result).toEqual(mockCustomers)
      expect(result).toHaveLength(2)
    })

    it('should fetch customers with filters', async () => {
      const mockCustomers: Customer[] = [
        {
          id: '1',
          tenantId: 'tenant-1',
          name: 'Acme Corp',
          email: 'contact@acme.com',
          identificationType: 1,
          isActive: true,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockCustomers, success: true })

      const filters: CustomerFilters = {
        searchTerm: 'Acme',
        isActive: true,
        city: 'New York',
      }

      const { getAllCustomers } = useCustomer()
      const result = await getAllCustomers(filters)

      expect(mockApiFetch).toHaveBeenCalledWith(
        '/customers?searchTerm=Acme&city=New+York&isActive=true',
        {
          method: 'GET',
        },
      )
      expect(result).toEqual(mockCustomers)
      expect(result).toHaveLength(1)
    })

    it('should handle empty customer list', async () => {
      mockApiFetch.mockResolvedValue({ data: [], success: true })

      const { getAllCustomers } = useCustomer()
      const result = await getAllCustomers()

      expect(result).toEqual([])
      expect(result).toHaveLength(0)
    })

    it('should fetch customers with all filter options', async () => {
      const mockCustomers: Customer[] = [
        {
          id: '1',
          tenantId: 'tenant-1',
          name: 'Tech Corp',
          email: 'tech@example.com',
          phone: '555-1234',
          identificationType: 1,
          taxId: '1234567890',
          billingCountry: 'USA',
          isActive: true,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockCustomers, success: true })

      const filters: CustomerFilters = {
        name: 'Tech Corp',
        email: 'tech@example.com',
        phone: '555-1234',
        taxId: '1234567890',
        country: 'USA',
      }

      const { getAllCustomers } = useCustomer()
      const result = await getAllCustomers(filters)

      expect(mockApiFetch).toHaveBeenCalledWith(
        '/customers?name=Tech+Corp&email=tech%40example.com&phone=555-1234&taxId=1234567890&country=USA',
        {
          method: 'GET',
        },
      )
      expect(result).toEqual(mockCustomers)
    })
  })

  describe('getCustomerById', () => {
    it('should fetch a customer by id successfully', async () => {
      const mockCustomer: Customer = {
        id: '1',
        tenantId: 'tenant-1',
        name: 'Acme Corp',
        email: 'contact@acme.com',
        phone: '555-0100',
        identificationType: 1,
        taxId: '1234567890',
        contactPerson: 'John Doe',
        billingStreet: '123 Main St',
        billingCity: 'New York',
        billingState: 'NY',
        billingPostalCode: '10001',
        billingCountry: 'USA',
        isActive: true,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockCustomer, success: true })

      const { getCustomerById } = useCustomer()
      const result = await getCustomerById('1')

      expect(mockApiFetch).toHaveBeenCalledWith('/customers/1', {
        method: 'GET',
      })
      expect(result).toEqual(mockCustomer)
      expect(result.id).toBe('1')
      expect(result.name).toBe('Acme Corp')
    })
  })

  describe('createCustomer', () => {
    it('should create a new customer successfully', async () => {
      const newCustomerData = {
        name: 'New Company Ltd',
        email: 'contact@newcompany.com',
        phone: '555-0300',
        taxId: '5555555555',
        contactPerson: 'Jane Smith',
        billingStreet: '456 Oak Ave',
        billingCity: 'Los Angeles',
        billingState: 'CA',
        billingPostalCode: '90001',
        billingCountry: 'USA',
        isActive: true,
      }

      const mockCreatedCustomer: Customer = {
        id: '3',
        tenantId: 'tenant-1',
        identificationType: 1,
        ...newCustomerData,
        createdAt: '2024-01-03T00:00:00Z',
        updatedAt: '2024-01-03T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockCreatedCustomer, success: true })

      const { createCustomer } = useCustomer()
      const result = await createCustomer(newCustomerData)

      expect(mockApiFetch).toHaveBeenCalledWith('/customers', {
        method: 'POST',
        body: newCustomerData,
      })
      expect(result).toEqual(mockCreatedCustomer)
      expect(result.id).toBe('3')
      expect(result.name).toBe('New Company Ltd')
    })
  })

  describe('updateCustomer', () => {
    it('should update an existing customer successfully', async () => {
      const updateData = {
        id: '1',
        name: 'Updated Corp',
        email: 'updated@corp.com',
        phone: '555-9999',
        taxId: '1234567890',
        contactPerson: 'Updated Contact',
        billingCity: 'San Francisco',
        billingState: 'CA',
        isActive: true,
      }

      const mockUpdatedCustomer: Customer = {
        ...updateData,
        tenantId: 'tenant-1',
        identificationType: 1,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-05T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockUpdatedCustomer, success: true })

      const { updateCustomer } = useCustomer()
      const result = await updateCustomer(updateData)

      expect(mockApiFetch).toHaveBeenCalledWith('/customers/1', {
        method: 'PUT',
        body: updateData,
      })
      expect(result).toEqual(mockUpdatedCustomer)
      expect(result.name).toBe('Updated Corp')
      expect(result.billingCity).toBe('San Francisco')
    })
  })

  describe('deleteCustomer', () => {
    it('should delete a customer successfully', async () => {
      mockApiFetch.mockResolvedValue(undefined)

      const { deleteCustomer } = useCustomer()
      await deleteCustomer('1')

      expect(mockApiFetch).toHaveBeenCalledWith('/customers/1', {
        method: 'DELETE',
      })
    })
  })

  describe('error handling', () => {
    it('should handle API errors when fetching customers', async () => {
      const mockError = new Error('Network error')
      mockApiFetch.mockRejectedValue(mockError)

      const { getAllCustomers } = useCustomer()

      await expect(getAllCustomers()).rejects.toThrow('Network error')
    })

    it('should handle API errors when creating customer', async () => {
      const mockError = new Error('Validation error')
      mockApiFetch.mockRejectedValue(mockError)

      const newCustomerData = {
        name: 'Test Customer',
        email: 'test@example.com',
      }

      const { createCustomer } = useCustomer()

      await expect(createCustomer(newCustomerData)).rejects.toThrow('Validation error')
    })
  })
})
