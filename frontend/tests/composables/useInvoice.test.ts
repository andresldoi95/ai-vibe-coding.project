import { beforeEach, describe, expect, it } from 'vitest'
import { mockApiFetch } from '../setup'
import { useInvoice } from '~/composables/useInvoice'
import type { CreateInvoiceDto, Invoice, InvoiceFilters, SriErrorLog } from '~/types/billing'
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

  describe('sRI Workflow Methods', () => {
    const mockAuthorizedInvoice: Invoice = {
      id: 'inv-sri-1',
      tenantId: 'tenant-1',
      invoiceNumber: 'INV-001-001-000001',
      customerId: 'customer-1',
      customerName: 'Acme Corp',
      customerTaxId: '1234567890001',
      issueDate: '2024-01-01T00:00:00Z',
      dueDate: '2024-01-31T00:00:00Z',
      subtotalAmount: 1000.00,
      taxAmount: 120.00,
      totalAmount: 1120.00,
      status: InvoiceStatus.Authorized,
      documentType: 1,
      paymentMethod: 1,
      environment: 1,
      accessKey: '2401202301179000001800120010010000000011234567813',
      sriAuthorization: '2401202301179000001800120010010000000011234567813',
      authorizationDate: '2024-01-01T12:00:00Z',
      xmlFilePath: '/xml/inv-001.xml',
      signedXmlFilePath: '/xml/inv-001-signed.xml',
      rideFilePath: '/ride/inv-001.pdf',
      items: [],
      createdAt: '2024-01-01T00:00:00Z',
      updatedAt: '2024-01-01T00:00:00Z',
    }

    describe('generateXml', () => {
      it('should generate XML for a draft invoice', async () => {
        const mockInvoice: Invoice = {
          ...mockAuthorizedInvoice,
          id: 'inv-1',
          status: InvoiceStatus.PendingSignature,
          xmlFilePath: '/xml/inv-001.xml',
          signedXmlFilePath: undefined,
        }

        mockApiFetch.mockResolvedValue({ data: mockInvoice, success: true })

        const { generateXml } = useInvoice()
        const result = await generateXml('inv-1')

        expect(mockApiFetch).toHaveBeenCalledWith('/invoices/inv-1/generate-xml', {
          method: 'POST',
        })
        expect(result).toEqual(mockInvoice)
        expect(result.status).toBe(InvoiceStatus.PendingSignature)
        expect(result.xmlFilePath).toBe('/xml/inv-001.xml')
      })

      it('should handle API errors when generating XML', async () => {
        mockApiFetch.mockRejectedValue(new Error('XML generation failed'))

        const { generateXml } = useInvoice()

        await expect(generateXml('inv-1')).rejects.toThrow('XML generation failed')
      })
    })

    describe('signDocument', () => {
      it('should sign the XML document with digital certificate', async () => {
        const mockInvoice: Invoice = {
          ...mockAuthorizedInvoice,
          id: 'inv-1',
          status: InvoiceStatus.PendingAuthorization,
          signedXmlFilePath: '/xml/inv-001-signed.xml',
        }

        mockApiFetch.mockResolvedValue({ data: mockInvoice, success: true })

        const { signDocument } = useInvoice()
        const result = await signDocument('inv-1')

        expect(mockApiFetch).toHaveBeenCalledWith('/invoices/inv-1/sign', {
          method: 'POST',
        })
        expect(result).toEqual(mockInvoice)
        expect(result.status).toBe(InvoiceStatus.PendingAuthorization)
        expect(result.signedXmlFilePath).toBe('/xml/inv-001-signed.xml')
      })

      it('should handle certificate errors when signing', async () => {
        mockApiFetch.mockRejectedValue(new Error('Certificate not configured'))

        const { signDocument } = useInvoice()

        await expect(signDocument('inv-1')).rejects.toThrow('Certificate not configured')
      })
    })

    describe('submitToSri', () => {
      it('should submit signed document to SRI', async () => {
        const mockInvoice: Invoice = {
          ...mockAuthorizedInvoice,
          id: 'inv-1',
          status: InvoiceStatus.PendingAuthorization,
          accessKey: '2401202301179000001800120010010000000011234567813',
        }

        mockApiFetch.mockResolvedValue({ data: mockInvoice, success: true })

        const { submitToSri } = useInvoice()
        const result = await submitToSri('inv-1')

        expect(mockApiFetch).toHaveBeenCalledWith('/invoices/inv-1/submit-to-sri', {
          method: 'POST',
        })
        expect(result).toEqual(mockInvoice)
        expect(result.accessKey).toBeDefined()
      })

      it('should handle SRI submission errors', async () => {
        mockApiFetch.mockRejectedValue(new Error('SRI service unavailable'))

        const { submitToSri } = useInvoice()

        await expect(submitToSri('inv-1')).rejects.toThrow('SRI service unavailable')
      })
    })

    describe('checkAuthorization', () => {
      it('should check authorization status and return authorized invoice', async () => {
        mockApiFetch.mockResolvedValue({ data: mockAuthorizedInvoice, success: true })

        const { checkAuthorization } = useInvoice()
        const result = await checkAuthorization('inv-sri-1')

        expect(mockApiFetch).toHaveBeenCalledWith('/invoices/inv-sri-1/check-authorization', {
          method: 'GET',
        })
        expect(result).toEqual(mockAuthorizedInvoice)
        expect(result.status).toBe(InvoiceStatus.Authorized)
        expect(result.sriAuthorization).toBe('2401202301179000001800120010010000000011234567813')
        expect(result.authorizationDate).toBe('2024-01-01T12:00:00Z')
      })

      it('should return pending invoice when still awaiting SRI authorization', async () => {
        const mockPendingInvoice: Invoice = {
          ...mockAuthorizedInvoice,
          id: 'inv-2',
          status: InvoiceStatus.PendingAuthorization,
          sriAuthorization: undefined,
          authorizationDate: undefined,
        }

        mockApiFetch.mockResolvedValue({ data: mockPendingInvoice, success: true })

        const { checkAuthorization } = useInvoice()
        const result = await checkAuthorization('inv-2')

        expect(mockApiFetch).toHaveBeenCalledWith('/invoices/inv-2/check-authorization', {
          method: 'GET',
        })
        expect(result.status).toBe(InvoiceStatus.PendingAuthorization)
        expect(result.sriAuthorization).toBeUndefined()
      })

      it('should handle authorization check errors', async () => {
        mockApiFetch.mockRejectedValue(new Error('Authorization check failed'))

        const { checkAuthorization } = useInvoice()

        await expect(checkAuthorization('inv-1')).rejects.toThrow('Authorization check failed')
      })
    })

    describe('generateRide', () => {
      it('should generate RIDE PDF for an authorized invoice', async () => {
        const mockInvoiceWithRide: Invoice = {
          ...mockAuthorizedInvoice,
          rideFilePath: '/ride/inv-001.pdf',
        }

        mockApiFetch.mockResolvedValue({ data: mockInvoiceWithRide, success: true })

        const { generateRide } = useInvoice()
        const result = await generateRide('inv-sri-1')

        expect(mockApiFetch).toHaveBeenCalledWith('/invoices/inv-sri-1/generate-ride', {
          method: 'POST',
        })
        expect(result).toEqual(mockInvoiceWithRide)
        expect(result.rideFilePath).toBe('/ride/inv-001.pdf')
      })

      it('should handle RIDE generation errors', async () => {
        mockApiFetch.mockRejectedValue(new Error('RIDE generation failed'))

        const { generateRide } = useInvoice()

        await expect(generateRide('inv-1')).rejects.toThrow('RIDE generation failed')
      })
    })

    describe('downloadXml', () => {
      it('should download the signed XML file as a Blob', async () => {
        const mockBlob = new Blob(['<xml>test</xml>'], { type: 'application/xml' })
        mockApiFetch.mockResolvedValue(mockBlob)

        const { downloadXml } = useInvoice()
        const result = await downloadXml('inv-sri-1')

        expect(mockApiFetch).toHaveBeenCalledWith('/invoices/inv-sri-1/download-xml', {
          method: 'GET',
          responseType: 'blob',
        })
        expect(result).toEqual(mockBlob)
      })

      it('should handle download XML errors', async () => {
        mockApiFetch.mockRejectedValue(new Error('File not found'))

        const { downloadXml } = useInvoice()

        await expect(downloadXml('inv-1')).rejects.toThrow('File not found')
      })
    })

    describe('downloadRide', () => {
      it('should download the RIDE PDF file as a Blob', async () => {
        const mockBlob = new Blob(['%PDF-test'], { type: 'application/pdf' })
        mockApiFetch.mockResolvedValue(mockBlob)

        const { downloadRide } = useInvoice()
        const result = await downloadRide('inv-sri-1')

        expect(mockApiFetch).toHaveBeenCalledWith('/invoices/inv-sri-1/download-ride', {
          method: 'GET',
          responseType: 'blob',
        })
        expect(result).toEqual(mockBlob)
      })

      it('should handle download RIDE errors', async () => {
        mockApiFetch.mockRejectedValue(new Error('RIDE file not found'))

        const { downloadRide } = useInvoice()

        await expect(downloadRide('inv-1')).rejects.toThrow('RIDE file not found')
      })
    })

    describe('getSriErrors', () => {
      it('should fetch SRI error logs for an invoice', async () => {
        const mockErrors: SriErrorLog[] = [
          {
            id: 'err-1',
            operation: 'Submit',
            errorCode: 'CERT-001',
            errorMessage: 'Certificate expired',
            additionalData: 'Certificate expired on 2024-12-31',
            occurredAt: '2024-01-01T10:00:00Z',
          },
          {
            id: 'err-2',
            operation: 'CheckAuthorization',
            errorCode: undefined,
            errorMessage: 'Connection timeout',
            occurredAt: '2024-01-01T11:00:00Z',
          },
        ]

        mockApiFetch.mockResolvedValue({ data: mockErrors, success: true })

        const { getSriErrors } = useInvoice()
        const result = await getSriErrors('inv-1')

        expect(mockApiFetch).toHaveBeenCalledWith('/invoices/inv-1/sri-errors', {
          method: 'GET',
        })
        expect(result).toEqual(mockErrors)
        expect(result).toHaveLength(2)
        expect(result[0].operation).toBe('Submit')
        expect(result[0].errorCode).toBe('CERT-001')
        expect(result[1].operation).toBe('CheckAuthorization')
      })

      it('should return empty array when no SRI errors exist', async () => {
        mockApiFetch.mockResolvedValue({ data: [], success: true })

        const { getSriErrors } = useInvoice()
        const result = await getSriErrors('inv-1')

        expect(result).toEqual([])
        expect(result).toHaveLength(0)
      })

      it('should handle API errors when fetching SRI errors', async () => {
        mockApiFetch.mockRejectedValue(new Error('Failed to fetch SRI errors'))

        const { getSriErrors } = useInvoice()

        await expect(getSriErrors('inv-1')).rejects.toThrow('Failed to fetch SRI errors')
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
