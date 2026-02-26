import { beforeEach, describe, expect, it } from 'vitest'
import { mockApiFetch } from '../setup'
import { useCreditNote } from '~/composables/useCreditNote'
import type { CreateCreditNoteDto, CreditNote, CreditNoteFilters, SriErrorLog, UpdateCreditNoteDto } from '~/types/billing'
import { InvoiceStatus } from '~/types/billing'

describe('useCreditNote', () => {
  beforeEach(() => {
    mockApiFetch.mockReset()
  })

  // ── getAllCreditNotes ────────────────────────────────────────────────────────

  describe('getAllCreditNotes', () => {
    it('should fetch all credit notes successfully', async () => {
      const mockCreditNotes: CreditNote[] = [
        {
          id: 'cn-1',
          tenantId: 'tenant-1',
          creditNoteNumber: 'NC-001-001-000001',
          customerId: 'customer-1',
          customerName: 'Acme Corp',
          originalInvoiceId: 'inv-1',
          originalInvoiceNumber: '001-001-000001',
          originalInvoiceDate: '2026-01-01',
          issueDate: '2026-01-01',
          reason: 'Product defect',
          valueModification: 115.00,
          subtotalAmount: 100.00,
          taxAmount: 15.00,
          totalAmount: 115.00,
          paymentMethod: 1,
          isPhysicalReturn: false,
          status: InvoiceStatus.Draft,
          documentType: 4,
          environment: 1,
          isEditable: true,
          items: [],
          createdAt: '2026-01-01T00:00:00Z',
          updatedAt: '2026-01-01T00:00:00Z',
        },
        {
          id: 'cn-2',
          tenantId: 'tenant-1',
          creditNoteNumber: 'NC-001-001-000002',
          customerId: 'customer-2',
          customerName: 'Tech Solutions',
          originalInvoiceId: 'inv-2',
          originalInvoiceNumber: '001-001-000002',
          originalInvoiceDate: '2026-01-02',
          issueDate: '2026-01-02',
          reason: 'Billing error',
          valueModification: 230.00,
          subtotalAmount: 200.00,
          taxAmount: 30.00,
          totalAmount: 230.00,
          paymentMethod: 1,
          isPhysicalReturn: false,
          status: InvoiceStatus.Authorized,
          documentType: 4,
          environment: 1,
          isEditable: false,
          items: [],
          createdAt: '2026-01-02T00:00:00Z',
          updatedAt: '2026-01-02T00:00:00Z',
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockCreditNotes, success: true })

      const { getAllCreditNotes } = useCreditNote()
      const result = await getAllCreditNotes()

      expect(mockApiFetch).toHaveBeenCalledWith('/credit-notes', { method: 'GET' })
      expect(result).toEqual(mockCreditNotes)
      expect(result).toHaveLength(2)
    })

    it('should fetch credit notes with filters', async () => {
      const mockCreditNotes: CreditNote[] = [
        {
          id: 'cn-1',
          tenantId: 'tenant-1',
          creditNoteNumber: 'NC-001-001-000001',
          customerId: 'customer-1',
          customerName: 'Acme Corp',
          originalInvoiceId: 'inv-1',
          originalInvoiceNumber: '001-001-000001',
          originalInvoiceDate: '2026-01-01',
          issueDate: '2026-01-01',
          reason: 'Return',
          valueModification: 115.00,
          subtotalAmount: 100.00,
          taxAmount: 15.00,
          totalAmount: 115.00,
          paymentMethod: 1,
          isPhysicalReturn: false,
          status: InvoiceStatus.Draft,
          documentType: 4,
          environment: 1,
          isEditable: true,
          items: [],
          createdAt: '2026-01-01T00:00:00Z',
          updatedAt: '2026-01-01T00:00:00Z',
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockCreditNotes, success: true })

      const filters: CreditNoteFilters = {
        customerId: 'customer-1',
        status: InvoiceStatus.Draft,
        dateFrom: '2026-01-01',
        dateTo: '2026-01-31',
      }

      const { getAllCreditNotes } = useCreditNote()
      const result = await getAllCreditNotes(filters)

      expect(mockApiFetch).toHaveBeenCalledWith(
        '/credit-notes?customerId=customer-1&status=0&dateFrom=2026-01-01&dateTo=2026-01-31',
        { method: 'GET' },
      )
      expect(result).toEqual(mockCreditNotes)
      expect(result).toHaveLength(1)
    })

    it('should handle empty credit note list', async () => {
      mockApiFetch.mockResolvedValue({ data: [], success: true })

      const { getAllCreditNotes } = useCreditNote()
      const result = await getAllCreditNotes()

      expect(result).toEqual([])
      expect(result).toHaveLength(0)
    })

    it('should handle API errors when fetching credit notes', async () => {
      mockApiFetch.mockRejectedValue(new Error('Network error'))

      const { getAllCreditNotes } = useCreditNote()

      await expect(getAllCreditNotes()).rejects.toThrow('Network error')
    })
  })

  // ── getCreditNoteById ────────────────────────────────────────────────────────

  describe('getCreditNoteById', () => {
    it('should fetch a single credit note by ID', async () => {
      const mockCreditNote: CreditNote = {
        id: 'cn-1',
        tenantId: 'tenant-1',
        creditNoteNumber: 'NC-001-001-000001',
        customerId: 'customer-1',
        customerName: 'Acme Corp',
        originalInvoiceId: 'inv-1',
        originalInvoiceNumber: '001-001-000001',
        originalInvoiceDate: '2026-01-01',
        issueDate: '2026-01-01',
        reason: 'Product defect',
        valueModification: 115.00,
        subtotalAmount: 100.00,
        taxAmount: 15.00,
        totalAmount: 115.00,
        paymentMethod: 1,
        isPhysicalReturn: false,
        status: InvoiceStatus.Draft,
        documentType: 4,
        environment: 1,
        isEditable: true,
        items: [
          {
            id: 'item-1',
            creditNoteId: 'cn-1',
            productId: 'product-1',
            productCode: 'WIDGET-A',
            productName: 'Widget A',
            description: 'Widget A description',
            quantity: 2,
            unitPrice: 50.00,
            taxRateId: 'tax-1',
            taxRate: 0.15,
            subtotalAmount: 100.00,
            taxAmount: 15.00,
            totalAmount: 115.00,
          },
        ],
        createdAt: '2026-01-01T00:00:00Z',
        updatedAt: '2026-01-01T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockCreditNote, success: true })

      const { getCreditNoteById } = useCreditNote()
      const result = await getCreditNoteById('cn-1')

      expect(mockApiFetch).toHaveBeenCalledWith('/credit-notes/cn-1', { method: 'GET' })
      expect(result).toEqual(mockCreditNote)
      expect(result.items).toHaveLength(1)
    })
  })

  // ── createCreditNote ─────────────────────────────────────────────────────────

  describe('createCreditNote', () => {
    it('should create a credit note and return the created record', async () => {
      const createData: CreateCreditNoteDto = {
        customerId: 'customer-1',
        originalInvoiceId: 'inv-1',
        emissionPointId: 'ep-1',
        issueDate: '2026-01-01',
        reason: 'Product defect',
        items: [
          {
            productId: 'product-1',
            description: 'Widget A',
            quantity: 2,
            unitPrice: 50.00,
            taxRateId: 'tax-1',
          },
        ],
      }

      const mockCreated: CreditNote = {
        id: 'cn-new',
        tenantId: 'tenant-1',
        creditNoteNumber: 'NC-001-001-000001',
        customerId: 'customer-1',
        customerName: 'Acme Corp',
        originalInvoiceId: 'inv-1',
        originalInvoiceNumber: '001-001-000001',
        originalInvoiceDate: '2026-01-01',
        issueDate: '2026-01-01',
        reason: 'Product defect',
        valueModification: 115.00,
        subtotalAmount: 100.00,
        taxAmount: 15.00,
        totalAmount: 115.00,
        paymentMethod: 1,
        isPhysicalReturn: false,
        status: InvoiceStatus.Draft,
        documentType: 4,
        environment: 1,
        isEditable: true,
        items: [],
        createdAt: '2026-01-01T00:00:00Z',
        updatedAt: '2026-01-01T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockCreated, success: true })

      const { createCreditNote } = useCreditNote()
      const result = await createCreditNote(createData)

      expect(mockApiFetch).toHaveBeenCalledWith('/credit-notes', {
        method: 'POST',
        body: createData,
      })
      expect(result).toEqual(mockCreated)
      expect(result.id).toBe('cn-new')
    })
  })

  // ── updateCreditNote ─────────────────────────────────────────────────────────

  describe('updateCreditNote', () => {
    it('should update a credit note and return the updated record', async () => {
      const updateData: UpdateCreditNoteDto = {
        customerId: 'customer-1',
        issueDate: '2026-01-01',
        reason: 'Updated reason',
        notes: 'Updated notes',
        items: [],
      }

      const mockUpdated: CreditNote = {
        id: 'cn-1',
        tenantId: 'tenant-1',
        creditNoteNumber: 'NC-001-001-000001',
        customerId: 'customer-1',
        customerName: 'Acme Corp',
        originalInvoiceId: 'inv-1',
        originalInvoiceNumber: '001-001-000001',
        originalInvoiceDate: '2026-01-01',
        issueDate: '2026-01-01',
        reason: 'Updated reason',
        notes: 'Updated notes',
        valueModification: 115.00,
        subtotalAmount: 100.00,
        taxAmount: 15.00,
        totalAmount: 115.00,
        paymentMethod: 1,
        isPhysicalReturn: false,
        status: InvoiceStatus.Draft,
        documentType: 4,
        environment: 1,
        isEditable: true,
        items: [],
        createdAt: '2026-01-01T00:00:00Z',
        updatedAt: '2026-01-05T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockUpdated, success: true })

      const { updateCreditNote } = useCreditNote()
      const result = await updateCreditNote('cn-1', updateData)

      expect(mockApiFetch).toHaveBeenCalledWith('/credit-notes/cn-1', {
        method: 'PUT',
        body: updateData,
      })
      expect(result.reason).toBe('Updated reason')
      expect(result.notes).toBe('Updated notes')
    })
  })

  // ── deleteCreditNote ─────────────────────────────────────────────────────────

  describe('deleteCreditNote', () => {
    it('should delete a credit note successfully', async () => {
      mockApiFetch.mockResolvedValue(undefined)

      const { deleteCreditNote } = useCreditNote()
      await deleteCreditNote('cn-1')

      expect(mockApiFetch).toHaveBeenCalledWith('/credit-notes/cn-1', { method: 'DELETE' })
    })
  })

  // ── SRI Workflow Methods ─────────────────────────────────────────────────────

  describe('sRI Workflow Methods', () => {
    const mockAuthorizedCreditNote: CreditNote = {
      id: 'cn-sri-1',
      tenantId: 'tenant-1',
      creditNoteNumber: 'NC-001-001-000001',
      customerId: 'customer-1',
      customerName: 'Acme Corp',
      customerTaxId: '1234567890001',
      originalInvoiceId: 'inv-1',
      originalInvoiceNumber: '001-001-000001',
      originalInvoiceDate: '2026-02-25',
      issueDate: '2026-02-25',
      reason: 'Product defect',
      valueModification: 115.00,
      subtotalAmount: 100.00,
      taxAmount: 15.00,
      totalAmount: 115.00,
      paymentMethod: 1,
      isPhysicalReturn: false,
      status: InvoiceStatus.Authorized,
      documentType: 4,
      environment: 1,
      isEditable: false,
      accessKey: '2502202504019990000000001100100100000000122345671',
      sriAuthorization: '2502202504019990000000001100100100000000122345671',
      authorizationDate: '2026-02-25T12:00:00Z',
      xmlFilePath: '/xml/nc-001.xml',
      signedXmlFilePath: '/xml/nc-001-signed.xml',
      rideFilePath: '/ride/nc-001.pdf',
      items: [],
      createdAt: '2026-02-25T00:00:00Z',
      updatedAt: '2026-02-25T00:00:00Z',
    }

    describe('generateXml', () => {
      it('should generate XML and return PendingSignature credit note', async () => {
        const mockPending: CreditNote = {
          ...mockAuthorizedCreditNote,
          status: InvoiceStatus.PendingSignature,
          xmlFilePath: '/xml/nc-001.xml',
          sriAuthorization: undefined,
          rideFilePath: undefined,
        }

        mockApiFetch.mockResolvedValue({ data: mockPending, success: true })

        const { generateXml } = useCreditNote()
        const result = await generateXml('cn-sri-1')

        expect(mockApiFetch).toHaveBeenCalledWith('/credit-notes/cn-sri-1/generate-xml', { method: 'POST' })
        expect(result.status).toBe(InvoiceStatus.PendingSignature)
        expect(result.xmlFilePath).toBe('/xml/nc-001.xml')
      })

      it('should handle XML generation errors', async () => {
        mockApiFetch.mockRejectedValue(new Error('XML generation failed'))

        const { generateXml } = useCreditNote()

        await expect(generateXml('cn-1')).rejects.toThrow('XML generation failed')
      })
    })

    describe('signDocument', () => {
      it('should sign the document and return PendingAuthorization credit note', async () => {
        const mockSigned: CreditNote = {
          ...mockAuthorizedCreditNote,
          status: InvoiceStatus.PendingAuthorization,
          signedXmlFilePath: '/xml/nc-001-signed.xml',
        }

        mockApiFetch.mockResolvedValue({ data: mockSigned, success: true })

        const { signDocument } = useCreditNote()
        const result = await signDocument('cn-sri-1')

        expect(mockApiFetch).toHaveBeenCalledWith('/credit-notes/cn-sri-1/sign', { method: 'POST' })
        expect(result.status).toBe(InvoiceStatus.PendingAuthorization)
        expect(result.signedXmlFilePath).toBe('/xml/nc-001-signed.xml')
      })

      it('should handle signing errors', async () => {
        mockApiFetch.mockRejectedValue(new Error('Certificate error'))

        const { signDocument } = useCreditNote()

        await expect(signDocument('cn-1')).rejects.toThrow('Certificate error')
      })
    })

    describe('submitToSri', () => {
      it('should submit to SRI and return the updated credit note', async () => {
        const mockSubmitted: CreditNote = {
          ...mockAuthorizedCreditNote,
          status: InvoiceStatus.PendingAuthorization,
        }

        mockApiFetch.mockResolvedValue({ data: mockSubmitted, success: true })

        const { submitToSri } = useCreditNote()
        const result = await submitToSri('cn-sri-1')

        expect(mockApiFetch).toHaveBeenCalledWith('/credit-notes/cn-sri-1/submit-sri', { method: 'POST' })
        expect(result).toEqual(mockSubmitted)
      })

      it('should handle SRI submission errors', async () => {
        mockApiFetch.mockRejectedValue(new Error('SRI service unavailable'))

        const { submitToSri } = useCreditNote()

        await expect(submitToSri('cn-1')).rejects.toThrow('SRI service unavailable')
      })
    })

    describe('checkAuthorization', () => {
      it('should check authorization status and return authorized credit note', async () => {
        mockApiFetch.mockResolvedValue({ data: mockAuthorizedCreditNote, success: true })

        const { checkAuthorization } = useCreditNote()
        const result = await checkAuthorization('cn-sri-1')

        expect(mockApiFetch).toHaveBeenCalledWith('/credit-notes/cn-sri-1/check-authorization', { method: 'POST' })
        expect(result.status).toBe(InvoiceStatus.Authorized)
        expect(result.sriAuthorization).toBe('2502202504019990000000001100100100000000122345671')
        expect(result.authorizationDate).toBe('2026-02-25T12:00:00Z')
      })

      it('should return pending credit note when still awaiting SRI authorization', async () => {
        const mockPending: CreditNote = {
          ...mockAuthorizedCreditNote,
          id: 'cn-2',
          status: InvoiceStatus.PendingAuthorization,
          sriAuthorization: undefined,
          authorizationDate: undefined,
        }

        mockApiFetch.mockResolvedValue({ data: mockPending, success: true })

        const { checkAuthorization } = useCreditNote()
        const result = await checkAuthorization('cn-2')

        expect(result.status).toBe(InvoiceStatus.PendingAuthorization)
        expect(result.sriAuthorization).toBeUndefined()
      })

      it('should handle authorization check errors', async () => {
        mockApiFetch.mockRejectedValue(new Error('Authorization check failed'))

        const { checkAuthorization } = useCreditNote()

        await expect(checkAuthorization('cn-1')).rejects.toThrow('Authorization check failed')
      })
    })

    describe('generateRide', () => {
      it('should generate RIDE PDF for an authorized credit note', async () => {
        const mockWithRide: CreditNote = {
          ...mockAuthorizedCreditNote,
          rideFilePath: '/ride/nc-001.pdf',
        }

        mockApiFetch.mockResolvedValue({ data: mockWithRide, success: true })

        const { generateRide } = useCreditNote()
        const result = await generateRide('cn-sri-1')

        expect(mockApiFetch).toHaveBeenCalledWith('/credit-notes/cn-sri-1/generate-ride', { method: 'POST' })
        expect(result.rideFilePath).toBe('/ride/nc-001.pdf')
      })

      it('should handle RIDE generation errors', async () => {
        mockApiFetch.mockRejectedValue(new Error('RIDE generation failed'))

        const { generateRide } = useCreditNote()

        await expect(generateRide('cn-1')).rejects.toThrow('RIDE generation failed')
      })
    })

    describe('downloadXml', () => {
      it('should download the signed XML file as a Blob', async () => {
        const mockBlob = new Blob(['<notaCredito>test</notaCredito>'], { type: 'application/xml' })
        mockApiFetch.mockResolvedValue(mockBlob)

        const { downloadXml } = useCreditNote()
        const result = await downloadXml('cn-sri-1')

        expect(mockApiFetch).toHaveBeenCalledWith('/credit-notes/cn-sri-1/download-xml', {
          method: 'GET',
          responseType: 'blob',
        })
        expect(result).toEqual(mockBlob)
      })

      it('should handle download XML errors', async () => {
        mockApiFetch.mockRejectedValue(new Error('File not found'))

        const { downloadXml } = useCreditNote()

        await expect(downloadXml('cn-1')).rejects.toThrow('File not found')
      })
    })

    describe('downloadRide', () => {
      it('should download the RIDE PDF file as a Blob', async () => {
        const mockBlob = new Blob(['%PDF-test'], { type: 'application/pdf' })
        mockApiFetch.mockResolvedValue(mockBlob)

        const { downloadRide } = useCreditNote()
        const result = await downloadRide('cn-sri-1')

        expect(mockApiFetch).toHaveBeenCalledWith('/credit-notes/cn-sri-1/download-ride', {
          method: 'GET',
          responseType: 'blob',
        })
        expect(result).toEqual(mockBlob)
      })

      it('should handle download RIDE errors', async () => {
        mockApiFetch.mockRejectedValue(new Error('RIDE file not found'))

        const { downloadRide } = useCreditNote()

        await expect(downloadRide('cn-1')).rejects.toThrow('RIDE file not found')
      })
    })

    describe('getSriErrors', () => {
      it('should fetch SRI error logs for a credit note', async () => {
        const mockErrors: SriErrorLog[] = [
          {
            id: 'err-1',
            operation: 'Submit',
            errorCode: 'SOAP_ERROR',
            errorMessage: 'Connection refused',
            additionalData: 'Endpoint unreachable',
            occurredAt: '2026-02-25T10:00:00Z',
          },
          {
            id: 'err-2',
            operation: 'CheckAuthorization',
            errorCode: undefined,
            errorMessage: 'Request timeout',
            occurredAt: '2026-02-25T11:00:00Z',
          },
        ]

        mockApiFetch.mockResolvedValue({ data: mockErrors, success: true })

        const { getSriErrors } = useCreditNote()
        const result = await getSriErrors('cn-1')

        expect(mockApiFetch).toHaveBeenCalledWith('/credit-notes/cn-1/sri-errors', { method: 'GET' })
        expect(result).toEqual(mockErrors)
        expect(result).toHaveLength(2)
        expect(result[0].operation).toBe('Submit')
        expect(result[0].errorCode).toBe('SOAP_ERROR')
        expect(result[1].operation).toBe('CheckAuthorization')
      })

      it('should return empty array when no SRI errors exist', async () => {
        mockApiFetch.mockResolvedValue({ data: [], success: true })

        const { getSriErrors } = useCreditNote()
        const result = await getSriErrors('cn-1')

        expect(result).toEqual([])
        expect(result).toHaveLength(0)
      })

      it('should handle API errors when fetching SRI errors', async () => {
        mockApiFetch.mockRejectedValue(new Error('Failed to fetch SRI errors'))

        const { getSriErrors } = useCreditNote()

        await expect(getSriErrors('cn-1')).rejects.toThrow('Failed to fetch SRI errors')
      })
    })
  })
})
