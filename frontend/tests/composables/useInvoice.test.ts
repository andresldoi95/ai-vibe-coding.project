import { beforeEach, describe, expect, it } from 'vitest'
import { mockApiFetch } from '../setup'
import { useInvoice } from '~/composables/useInvoice'
import type { CreateInvoiceDto, Invoice, InvoiceFilters } from '~/types/billing'
import { InvoiceStatus } from '~/types/billing'

describe('useInvoice', () => {
  beforeEach(() => {
    // Reset all mocks before each test
    mockApiFetch.mockReset()
  })

  describe('getAllInvoices', () => {
    it('should fetch all invoices successfully', async () => {
      const mockInvoices: Invoice[] = [
        {
          id: '1',
          tenantId: 'tenant-1',
          invoiceNumber: 'INV-001-001-000001',
          customerId: 'customer-1',
          customerName: 'Acme Corp',
          customerTaxId: '1234567890',
          warehouseId: 'warehouse-1',
          warehouseName: 'Main Warehouse',
          issueDate: '2024-01-01T00:00:00Z',
          dueDate: '2024-01-31T00:00:00Z',
          subtotalAmount: 1000.00,
          taxAmount: 120.00,
          totalAmount: 1120.00,
          status: InvoiceStatus.Draft,
          documentType: 1,
          paymentMethod: 1,
          environment: 1,
          items: [],
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
        {
          id: '2',
          tenantId: 'tenant-1',
          invoiceNumber: 'INV-001-001-000002',
          customerId: 'customer-2',
          customerName: 'Tech Solutions Inc',
          issueDate: '2024-01-02T00:00:00Z',
          dueDate: '2024-02-01T00:00:00Z',
          subtotalAmount: 2000.00,
          taxAmount: 240.00,
          totalAmount: 2240.00,
          status: InvoiceStatus.Authorized,
          documentType: 1,
          paymentMethod: 1,
          environment: 1,
          items: [],
          createdAt: '2024-01-02T00:00:00Z',
          updatedAt: '2024-01-02T00:00:00Z',
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockInvoices, success: true })

      const { getAllInvoices } = useInvoice()
      const result = await getAllInvoices()

      expect(mockApiFetch).toHaveBeenCalledWith('/invoices', {
        method: 'GET',
      })
      expect(result).toEqual(mockInvoices)
      expect(result).toHaveLength(2)
    })

    it('should fetch invoices with filters', async () => {
      const mockInvoices: Invoice[] = [
        {
          id: '1',
          tenantId: 'tenant-1',
          invoiceNumber: 'INV-001-001-000001',
          customerId: 'customer-1',
          customerName: 'Acme Corp',
          issueDate: '2024-01-01T00:00:00Z',
          dueDate: '2024-01-31T00:00:00Z',
          subtotalAmount: 1000.00,
          taxAmount: 120.00,
          totalAmount: 1120.00,
          status: InvoiceStatus.Draft,
          documentType: 1,
          paymentMethod: 1,
          environment: 1,
          items: [],
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockInvoices, success: true })

      const filters: InvoiceFilters = {
        customerId: 'customer-1',
        status: InvoiceStatus.Draft,
        dateFrom: '2024-01-01',
        dateTo: '2024-01-31',
      }

      const { getAllInvoices } = useInvoice()
      const result = await getAllInvoices(filters)

      expect(mockApiFetch).toHaveBeenCalledWith(
        '/invoices?customerId=customer-1&status=0&dateFrom=2024-01-01&dateTo=2024-01-31',
        {
          method: 'GET',
        },
      )
      expect(result).toEqual(mockInvoices)
      expect(result).toHaveLength(1)
    })

    it('should handle empty invoice list', async () => {
      mockApiFetch.mockResolvedValue({ data: [], success: true })

      const { getAllInvoices } = useInvoice()
      const result = await getAllInvoices()

      expect(result).toEqual([])
      expect(result).toHaveLength(0)
    })
  })

  describe('getInvoiceById', () => {
    it('should fetch an invoice by id successfully', async () => {
      const mockInvoice: Invoice = {
        id: '1',
        tenantId: 'tenant-1',
        invoiceNumber: 'INV-001-001-000001',
        customerId: 'customer-1',
        customerName: 'Acme Corp',
        customerTaxId: '1234567890',
        warehouseId: 'warehouse-1',
        warehouseName: 'Main Warehouse',
        issueDate: '2024-01-01T00:00:00Z',
        dueDate: '2024-01-31T00:00:00Z',
        subtotalAmount: 1000.00,
        taxAmount: 120.00,
        totalAmount: 1120.00,
        status: InvoiceStatus.Draft,
        notes: 'Test invoice',
        documentType: 1,
        paymentMethod: 1,
        environment: 1,
        items: [
          {
            id: 'item-1',
            invoiceId: '1',
            productId: 'product-1',
            productCode: 'PROD-001',
            productName: 'Product 1',
            description: 'Product description',
            quantity: 10,
            unitPrice: 100.00,
            taxRateId: 'tax-1',
            taxRate: 0.12,
            subtotalAmount: 1000.00,
            taxAmount: 120.00,
            totalAmount: 1120.00,
          },
        ],
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockInvoice, success: true })

      const { getInvoiceById } = useInvoice()
      const result = await getInvoiceById('1')

      expect(mockApiFetch).toHaveBeenCalledWith('/invoices/1', {
        method: 'GET',
      })
      expect(result).toEqual(mockInvoice)
      expect(result.id).toBe('1')
      expect(result.invoiceNumber).toBe('INV-001-001-000001')
      expect(result.items).toHaveLength(1)
    })
  })

  describe('createInvoice', () => {
    it('should create a new invoice successfully', async () => {
      const newInvoiceData: CreateInvoiceDto = {
        customerId: 'customer-1',
        warehouseId: 'warehouse-1',
        issueDate: '2024-01-01T00:00:00Z',
        notes: 'New invoice',
        items: [
          {
            productId: 'product-1',
            quantity: 5,
            unitPrice: 100.00,
            taxRateId: 'tax-1',
            description: 'Product description',
          },
        ],
      }

      const mockCreatedInvoice: Invoice = {
        id: '3',
        tenantId: 'tenant-1',
        invoiceNumber: 'INV-001-001-000003',
        customerId: 'customer-1',
        customerName: 'Acme Corp',
        warehouseId: 'warehouse-1',
        warehouseName: 'Main Warehouse',
        issueDate: '2024-01-01T00:00:00Z',
        dueDate: '2024-01-31T00:00:00Z',
        subtotalAmount: 500.00,
        taxAmount: 60.00,
        totalAmount: 560.00,
        status: InvoiceStatus.Draft,
        notes: 'New invoice',
        documentType: 1,
        paymentMethod: 1,
        environment: 1,
        items: [
          {
            id: 'item-1',
            invoiceId: '3',
            productId: 'product-1',
            productCode: 'PROD-001',
            productName: 'Product 1',
            description: 'Product description',
            quantity: 5,
            unitPrice: 100.00,
            taxRateId: 'tax-1',
            taxRate: 0.12,
            subtotalAmount: 500.00,
            taxAmount: 60.00,
            totalAmount: 560.00,
          },
        ],
        createdAt: '2024-01-03T00:00:00Z',
        updatedAt: '2024-01-03T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockCreatedInvoice, success: true })

      const { createInvoice } = useInvoice()
      const result = await createInvoice(newInvoiceData)

      expect(mockApiFetch).toHaveBeenCalledWith('/invoices', {
        method: 'POST',
        body: newInvoiceData,
      })
      expect(result).toEqual(mockCreatedInvoice)
      expect(result.id).toBe('3')
      expect(result.items).toHaveLength(1)
    })
  })

  describe('updateInvoice', () => {
    it('should update an existing invoice successfully', async () => {
      const updateData = {
        customerId: 'customer-1',
        warehouseId: 'warehouse-1',
        issueDate: '2024-01-01T00:00:00Z',
        notes: 'Updated notes',
        items: [
          {
            id: 'item-1',
            productId: 'product-1',
            quantity: 10,
            unitPrice: 100.00,
            taxRateId: 'tax-1',
          },
        ],
      }

      const mockUpdatedInvoice: Invoice = {
        id: '1',
        tenantId: 'tenant-1',
        invoiceNumber: 'INV-001-001-000001',
        customerId: 'customer-1',
        customerName: 'Acme Corp',
        warehouseId: 'warehouse-1',
        warehouseName: 'Main Warehouse',
        issueDate: '2024-01-01T00:00:00Z',
        dueDate: '2024-01-31T00:00:00Z',
        subtotalAmount: 1000.00,
        taxAmount: 120.00,
        totalAmount: 1120.00,
        status: InvoiceStatus.Draft,
        notes: 'Updated notes',
        documentType: 1,
        paymentMethod: 1,
        environment: 1,
        items: [
          {
            id: 'item-1',
            invoiceId: '1',
            productId: 'product-1',
            productCode: 'PROD-001',
            productName: 'Product 1',
            quantity: 10,
            unitPrice: 100.00,
            taxRateId: 'tax-1',
            taxRate: 0.12,
            subtotalAmount: 1000.00,
            taxAmount: 120.00,
            totalAmount: 1120.00,
          },
        ],
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-05T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockUpdatedInvoice, success: true })

      const { updateInvoice } = useInvoice()
      const result = await updateInvoice('1', updateData)

      expect(mockApiFetch).toHaveBeenCalledWith('/invoices/1', {
        method: 'PUT',
        body: updateData,
      })
      expect(result).toEqual(mockUpdatedInvoice)
      expect(result.notes).toBe('Updated notes')
    })
  })

  describe('changeInvoiceStatus', () => {
    it('should change invoice status successfully', async () => {
      const mockUpdatedInvoice: Invoice = {
        id: '1',
        tenantId: 'tenant-1',
        invoiceNumber: 'INV-001-001-000001',
        customerId: 'customer-1',
        customerName: 'Acme Corp',
        issueDate: '2024-01-01T00:00:00Z',
        dueDate: '2024-01-31T00:00:00Z',
        subtotalAmount: 1000.00,
        taxAmount: 120.00,
        totalAmount: 1120.00,
        status: InvoiceStatus.Authorized,
        documentType: 1,
        paymentMethod: 1,
        environment: 1,
        items: [],
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-05T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockUpdatedInvoice, success: true })

      const { changeInvoiceStatus } = useInvoice()
      const result = await changeInvoiceStatus('1', InvoiceStatus.Authorized)

      expect(mockApiFetch).toHaveBeenCalledWith('/invoices/1/status', {
        method: 'PATCH',
        body: { id: '1', newStatus: InvoiceStatus.Authorized },
      })
      expect(result).toEqual(mockUpdatedInvoice)
      expect(result.status).toBe(InvoiceStatus.Authorized)
    })
  })

  describe('deleteInvoice', () => {
    it('should delete an invoice successfully', async () => {
      mockApiFetch.mockResolvedValue(undefined)

      const { deleteInvoice } = useInvoice()
      await deleteInvoice('1')

      expect(mockApiFetch).toHaveBeenCalledWith('/invoices/1', {
        method: 'DELETE',
      })
    })
  })

  describe('error handling', () => {
    it('should handle API errors when fetching invoices', async () => {
      const mockError = new Error('Network error')
      mockApiFetch.mockRejectedValue(mockError)

      const { getAllInvoices } = useInvoice()

      await expect(getAllInvoices()).rejects.toThrow('Network error')
    })

    it('should handle API errors when creating invoice', async () => {
      const mockError = new Error('Validation error')
      mockApiFetch.mockRejectedValue(mockError)

      const newInvoiceData: CreateInvoiceDto = {
        customerId: 'customer-1',
        issueDate: '2024-01-01T00:00:00Z',
        items: [],
      }

      const { createInvoice } = useInvoice()

      await expect(createInvoice(newInvoiceData)).rejects.toThrow('Validation error')
    })

    it('should handle API errors when changing status', async () => {
      const mockError = new Error('Status change error')
      mockApiFetch.mockRejectedValue(mockError)

      const { changeInvoiceStatus } = useInvoice()

      await expect(changeInvoiceStatus('1', InvoiceStatus.Paid)).rejects.toThrow('Status change error')
    })
  })
})
