import { beforeEach, describe, expect, it } from 'vitest'
import { mockApiFetch } from '../setup'
import { useCustomer } from '~/composables/useCustomer'
import type { Customer } from '~/types/billing'

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
          name: 'Acme Corporation',
          email: 'contact@acme.com',
          phone: '+1-555-0100',
          taxId: 'TAX-12345',
          contactPerson: 'John Doe',
          billingStreet: '123 Business Ave',
          billingCity: 'New York',
          billingState: 'NY',
          billingPostalCode: '10001',
          billingCountry: 'USA',
          shippingStreet: '123 Business Ave',
          shippingCity: 'New York',
          shippingState: 'NY',
          shippingPostalCode: '10001',
          shippingCountry: 'USA',
          notes: 'VIP customer',
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
          phone: '+1-555-0200',
          contactPerson: 'Jane Smith',
          billingStreet: '456 Tech Blvd',
          billingCity: 'San Francisco',
          billingState: 'CA',
          billingPostalCode: '94105',
          billingCountry: 'USA',
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

    it('should fetch customers with searchTerm filter', async () => {
      const mockCustomers: Customer[] = [
        {
          id: '1',
          tenantId: 'tenant-1',
          name: 'Acme Corporation',
          email: 'contact@acme.com',
          isActive: true,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockCustomers, success: true })

      const { getAllCustomers } = useCustomer()
      const result = await getAllCustomers({ searchTerm: 'Acme' })

      expect(mockApiFetch).toHaveBeenCalledWith('/customers?searchTerm=Acme', {
        method: 'GET',
      })
      expect(result).toEqual(mockCustomers)
      expect(result).toHaveLength(1)
    })

    it('should fetch customers with multiple filters', async () => {
      const mockCustomers: Customer[] = [
        {
          id: '1',
          tenantId: 'tenant-1',
          name: 'Acme Corporation',
          email: 'contact@acme.com',
          billingCity: 'New York',
          billingCountry: 'USA',
          isActive: true,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockCustomers, success: true })

      const { getAllCustomers } = useCustomer()
      const result = await getAllCustomers({
        name: 'Acme',
        city: 'New York',
        country: 'USA',
        isActive: true,
      })

      expect(mockApiFetch).toHaveBeenCalledWith('/customers?name=Acme&city=New+York&country=USA&isActive=true', {
        method: 'GET',
      })
      expect(result).toEqual(mockCustomers)
    })

    it('should fetch customers with email and phone filters', async () => {
      const mockCustomers: Customer[] = [
        {
          id: '1',
          tenantId: 'tenant-1',
          name: 'Acme Corporation',
          email: 'contact@acme.com',
          phone: '+1-555-0100',
          isActive: true,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockCustomers, success: true })

      const { getAllCustomers } = useCustomer()
      const result = await getAllCustomers({
        email: 'acme.com',
        phone: '555-0100',
      })

      expect(mockApiFetch).toHaveBeenCalledWith('/customers?email=acme.com&phone=555-0100', {
        method: 'GET',
      })
      expect(result).toEqual(mockCustomers)
    })

    it('should fetch customers with taxId filter', async () => {
      const mockCustomers: Customer[] = [
        {
          id: '1',
          tenantId: 'tenant-1',
          name: 'Acme Corporation',
          email: 'contact@acme.com',
          taxId: 'TAX-12345',
          isActive: true,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockCustomers, success: true })

      const { getAllCustomers } = useCustomer()
      const result = await getAllCustomers({ taxId: 'TAX-12345' })

      expect(mockApiFetch).toHaveBeenCalledWith('/customers?taxId=TAX-12345', {
        method: 'GET',
      })
      expect(result).toEqual(mockCustomers)
    })

    it('should fetch only inactive customers when isActive is false', async () => {
      const mockCustomers: Customer[] = [
        {
          id: '3',
          tenantId: 'tenant-1',
          name: 'Inactive Corp',
          email: 'inactive@corp.com',
          isActive: false,
          createdAt: '2024-01-03T00:00:00Z',
          updatedAt: '2024-01-03T00:00:00Z',
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockCustomers, success: true })

      const { getAllCustomers } = useCustomer()
      const result = await getAllCustomers({ isActive: false })

      expect(mockApiFetch).toHaveBeenCalledWith('/customers?isActive=false', {
        method: 'GET',
      })
      expect(result).toEqual(mockCustomers)
    })

    it('should handle empty customer list', async () => {
      mockApiFetch.mockResolvedValue({ data: [], success: true })

      const { getAllCustomers } = useCustomer()
      const result = await getAllCustomers()

      expect(result).toEqual([])
      expect(result).toHaveLength(0)
    })
  })

  describe('getCustomerById', () => {
    it('should fetch a customer by id successfully', async () => {
      const mockCustomer: Customer = {
        id: '1',
        tenantId: 'tenant-1',
        name: 'Acme Corporation',
        email: 'contact@acme.com',
        phone: '+1-555-0100',
        taxId: 'TAX-12345',
        contactPerson: 'John Doe',
        billingStreet: '123 Business Ave',
        billingCity: 'New York',
        billingState: 'NY',
        billingPostalCode: '10001',
        billingCountry: 'USA',
        shippingStreet: '123 Business Ave',
        shippingCity: 'New York',
        shippingState: 'NY',
        shippingPostalCode: '10001',
        shippingCountry: 'USA',
        notes: 'VIP customer',
        website: 'https://acme.com',
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
      expect(result.name).toBe('Acme Corporation')
    })
  })

  describe('createCustomer', () => {
    it('should create a new customer successfully', async () => {
      const newCustomerData = {
        name: 'New Customer',
        email: 'new@customer.com',
        phone: '+1-555-0300',
        taxId: 'TAX-67890',
        contactPerson: 'Bob Johnson',
        billingStreet: '789 Commerce St',
        billingCity: 'Chicago',
        billingState: 'IL',
        billingPostalCode: '60601',
        billingCountry: 'USA',
        shippingStreet: '789 Commerce St',
        shippingCity: 'Chicago',
        shippingState: 'IL',
        shippingPostalCode: '60601',
        shippingCountry: 'USA',
        notes: 'New corporate client',
        website: 'https://newcustomer.com',
        isActive: true,
      }

      const mockCreatedCustomer: Customer = {
        id: '3',
        tenantId: 'tenant-1',
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
      expect(result.name).toBe('New Customer')
    })

    it('should create a customer with minimal required fields', async () => {
      const newCustomerData = {
        name: 'Minimal Customer',
        email: 'minimal@customer.com',
      }

      const mockCreatedCustomer: Customer = {
        id: '4',
        tenantId: 'tenant-1',
        ...newCustomerData,
        isActive: true,
        createdAt: '2024-01-04T00:00:00Z',
        updatedAt: '2024-01-04T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockCreatedCustomer, success: true })

      const { createCustomer } = useCustomer()
      const result = await createCustomer(newCustomerData)

      expect(mockApiFetch).toHaveBeenCalledWith('/customers', {
        method: 'POST',
        body: newCustomerData,
      })
      expect(result).toEqual(mockCreatedCustomer)
      expect(result.name).toBe('Minimal Customer')
    })
  })

  describe('updateCustomer', () => {
    it('should update an existing customer successfully', async () => {
      const updateData = {
        id: '1',
        name: 'Updated Acme Corporation',
        email: 'updated@acme.com',
        phone: '+1-555-0999',
        taxId: 'TAX-12345',
        contactPerson: 'John Doe Sr.',
        billingStreet: '123 Business Ave Suite 200',
        billingCity: 'New York',
        billingState: 'NY',
        billingPostalCode: '10001',
        billingCountry: 'USA',
        shippingStreet: '456 Shipping Lane',
        shippingCity: 'Brooklyn',
        shippingState: 'NY',
        shippingPostalCode: '11201',
        shippingCountry: 'USA',
        notes: 'VIP customer - updated terms',
        website: 'https://acme-new.com',
        isActive: true,
      }

      const mockUpdatedCustomer: Customer = {
        ...updateData,
        tenantId: 'tenant-1',
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
      expect(result.name).toBe('Updated Acme Corporation')
      expect(result.email).toBe('updated@acme.com')
    })

    it('should update customer status to inactive', async () => {
      const updateData = {
        id: '2',
        name: 'Tech Solutions Inc',
        email: 'info@techsolutions.com',
        isActive: false,
      }

      const mockUpdatedCustomer: Customer = {
        ...updateData,
        tenantId: 'tenant-1',
        createdAt: '2024-01-02T00:00:00Z',
        updatedAt: '2024-01-06T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockUpdatedCustomer, success: true })

      const { updateCustomer } = useCustomer()
      const result = await updateCustomer(updateData)

      expect(mockApiFetch).toHaveBeenCalledWith('/customers/2', {
        method: 'PUT',
        body: updateData,
      })
      expect(result.isActive).toBe(false)
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

    it('should delete a customer by different id', async () => {
      mockApiFetch.mockResolvedValue(undefined)

      const { deleteCustomer } = useCustomer()
      await deleteCustomer('999')

      expect(mockApiFetch).toHaveBeenCalledWith('/customers/999', {
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
      const mockError = new Error('Validation error: email already exists')
      mockApiFetch.mockRejectedValue(mockError)

      const newCustomerData = {
        name: 'Test Customer',
        email: 'duplicate@test.com',
      }

      const { createCustomer } = useCustomer()

      await expect(createCustomer(newCustomerData)).rejects.toThrow('Validation error: email already exists')
    })

    it('should handle API errors when updating customer', async () => {
      const mockError = new Error('Customer not found')
      mockApiFetch.mockRejectedValue(mockError)

      const updateData = {
        id: 'non-existent',
        name: 'Test',
        email: 'test@test.com',
      }

      const { updateCustomer } = useCustomer()

      await expect(updateCustomer(updateData)).rejects.toThrow('Customer not found')
    })

    it('should handle API errors when deleting customer', async () => {
      const mockError = new Error('Cannot delete customer with active invoices')
      mockApiFetch.mockRejectedValue(mockError)

      const { deleteCustomer } = useCustomer()

      await expect(deleteCustomer('1')).rejects.toThrow('Cannot delete customer with active invoices')
    })
  })
})
