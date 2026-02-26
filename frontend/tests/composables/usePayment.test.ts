import { beforeEach, describe, expect, it } from 'vitest'
import { mockApiFetch } from '../setup'
import { usePayment } from '~/composables/usePayment'
import { type Payment, PaymentStatus } from '~/types/billing'
import { SriPaymentMethod } from '~/types/sri-enums'

describe('usePayment', () => {
  beforeEach(() => {
    // Reset all mocks before each test
    mockApiFetch.mockReset()
  })

  describe('getAllPayments', () => {
    it('should fetch all payments successfully', async () => {
      const mockPayments: Payment[] = [
        {
          id: '1',
          tenantId: 'tenant-1',
          invoiceId: 'invoice-1',
          invoiceNumber: 'INV-001',
          customerName: 'John Doe',
          amount: 150.00,
          paymentDate: '2024-01-01T00:00:00Z',
          paymentMethod: SriPaymentMethod.Cash,
          status: PaymentStatus.Completed,
          transactionId: 'TXN-001',
          notes: 'Cash payment received',
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
          createdBy: 'user-1',
        },
        {
          id: '2',
          tenantId: 'tenant-1',
          invoiceId: 'invoice-2',
          invoiceNumber: 'INV-002',
          customerName: 'Jane Smith',
          amount: 300.00,
          paymentDate: '2024-01-02T00:00:00Z',
          paymentMethod: SriPaymentMethod.BankTransfer,
          status: PaymentStatus.Pending,
          notes: 'Bank transfer pending confirmation',
          createdAt: '2024-01-02T00:00:00Z',
          updatedAt: '2024-01-02T00:00:00Z',
          createdBy: 'user-1',
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockPayments, success: true })

      const { getAllPayments } = usePayment()
      const result = await getAllPayments()

      expect(mockApiFetch).toHaveBeenCalledWith('/payments', {
        method: 'GET',
      })
      expect(result).toEqual(mockPayments)
      expect(result).toHaveLength(2)
    })

    it('should handle empty payments list', async () => {
      mockApiFetch.mockResolvedValue({ data: [], success: true })

      const { getAllPayments } = usePayment()
      const result = await getAllPayments()

      expect(result).toEqual([])
      expect(result).toHaveLength(0)
    })

    it('should handle API errors when fetching payments', async () => {
      mockApiFetch.mockRejectedValue(new Error('Network error'))

      const { getAllPayments } = usePayment()

      await expect(getAllPayments()).rejects.toThrow('Network error')
    })
  })

  describe('getPaymentById', () => {
    it('should fetch a payment by id successfully', async () => {
      const mockPayment: Payment = {
        id: '1',
        tenantId: 'tenant-1',
        invoiceId: 'invoice-1',
        invoiceNumber: 'INV-001',
        customerName: 'John Doe',
        amount: 150.00,
        paymentDate: '2024-01-01T00:00:00Z',
        paymentMethod: SriPaymentMethod.Cash,
        status: PaymentStatus.Completed,
        transactionId: 'TXN-001',
        notes: 'Cash payment received',
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
        createdBy: 'user-1',
      }

      mockApiFetch.mockResolvedValue({ data: mockPayment, success: true })

      const { getPaymentById } = usePayment()
      const result = await getPaymentById('1')

      expect(mockApiFetch).toHaveBeenCalledWith('/payments/1', {
        method: 'GET',
      })
      expect(result).toEqual(mockPayment)
      expect(result.id).toBe('1')
      expect(result.status).toBe(PaymentStatus.Completed)
    })

    it('should handle API errors when fetching payment by id', async () => {
      mockApiFetch.mockRejectedValue(new Error('Payment not found'))

      const { getPaymentById } = usePayment()

      await expect(getPaymentById('999')).rejects.toThrow('Payment not found')
    })
  })

  describe('getPaymentsByInvoiceId', () => {
    it('should fetch payments by invoice id successfully', async () => {
      const mockPayments: Payment[] = [
        {
          id: '1',
          tenantId: 'tenant-1',
          invoiceId: 'invoice-1',
          invoiceNumber: 'INV-001',
          customerName: 'John Doe',
          amount: 100.00,
          paymentDate: '2024-01-01T00:00:00Z',
          paymentMethod: SriPaymentMethod.Cash,
          status: PaymentStatus.Completed,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
        {
          id: '2',
          tenantId: 'tenant-1',
          invoiceId: 'invoice-1',
          invoiceNumber: 'INV-001',
          customerName: 'John Doe',
          amount: 50.00,
          paymentDate: '2024-01-05T00:00:00Z',
          paymentMethod: SriPaymentMethod.CreditCard,
          status: PaymentStatus.Completed,
          createdAt: '2024-01-05T00:00:00Z',
          updatedAt: '2024-01-05T00:00:00Z',
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockPayments, success: true })

      const { getPaymentsByInvoiceId } = usePayment()
      const result = await getPaymentsByInvoiceId('invoice-1')

      expect(mockApiFetch).toHaveBeenCalledWith('/payments/invoice/invoice-1', {
        method: 'GET',
      })
      expect(result).toEqual(mockPayments)
      expect(result).toHaveLength(2)
      expect(result.every(p => p.invoiceId === 'invoice-1')).toBe(true)
    })

    it('should handle empty payments list for invoice', async () => {
      mockApiFetch.mockResolvedValue({ data: [], success: true })

      const { getPaymentsByInvoiceId } = usePayment()
      const result = await getPaymentsByInvoiceId('invoice-999')

      expect(result).toEqual([])
      expect(result).toHaveLength(0)
    })

    it('should handle API errors when fetching payments by invoice id', async () => {
      mockApiFetch.mockRejectedValue(new Error('Invoice not found'))

      const { getPaymentsByInvoiceId } = usePayment()

      await expect(getPaymentsByInvoiceId('invalid-invoice')).rejects.toThrow('Invoice not found')
    })
  })

  describe('createPayment', () => {
    it('should create a payment successfully', async () => {
      const createData = {
        invoiceId: 'invoice-1',
        amount: 200.00,
        paymentDate: '2024-01-03T00:00:00Z',
        paymentMethod: SriPaymentMethod.DebitCard,
        status: PaymentStatus.Completed,
        transactionId: 'TXN-003',
        notes: 'Debit card payment',
      }

      const mockCreatedPayment: Payment = {
        id: '3',
        tenantId: 'tenant-1',
        invoiceNumber: 'INV-001',
        customerName: 'John Doe',
        ...createData,
        createdAt: '2024-01-03T00:00:00Z',
        updatedAt: '2024-01-03T00:00:00Z',
        createdBy: 'user-1',
      }

      mockApiFetch.mockResolvedValue({ data: mockCreatedPayment, success: true })

      const { createPayment } = usePayment()
      const result = await createPayment(createData)

      expect(mockApiFetch).toHaveBeenCalledWith('/payments', {
        method: 'POST',
        body: createData,
      })
      expect(result).toEqual(mockCreatedPayment)
      expect(result.id).toBe('3')
      expect(result.amount).toBe(200.00)
      expect(result.status).toBe(PaymentStatus.Completed)
    })

    it('should create a payment without optional fields', async () => {
      const createData = {
        invoiceId: 'invoice-2',
        amount: 500.00,
        paymentDate: '2024-01-04T00:00:00Z',
        paymentMethod: SriPaymentMethod.Cash,
        status: PaymentStatus.Pending,
      }

      const mockCreatedPayment: Payment = {
        id: '4',
        tenantId: 'tenant-1',
        invoiceNumber: 'INV-002',
        customerName: 'Jane Smith',
        ...createData,
        createdAt: '2024-01-04T00:00:00Z',
        updatedAt: '2024-01-04T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockCreatedPayment, success: true })

      const { createPayment } = usePayment()
      const result = await createPayment(createData)

      expect(mockApiFetch).toHaveBeenCalledWith('/payments', {
        method: 'POST',
        body: createData,
      })
      expect(result).toEqual(mockCreatedPayment)
    })

    it('should handle API errors when creating payment', async () => {
      const createData = {
        invoiceId: 'invalid-invoice',
        amount: 100.00,
        paymentDate: '2024-01-05T00:00:00Z',
        paymentMethod: SriPaymentMethod.Cash,
        status: PaymentStatus.Pending,
      }

      mockApiFetch.mockRejectedValue(new Error('Invoice not found'))

      const { createPayment } = usePayment()

      await expect(createPayment(createData)).rejects.toThrow('Invoice not found')
    })
  })

  describe('voidPayment', () => {
    it('should void a payment successfully', async () => {
      const voidData = {
        reason: 'Duplicate payment',
      }

      const mockVoidedPayment: Payment = {
        id: '1',
        tenantId: 'tenant-1',
        invoiceId: 'invoice-1',
        invoiceNumber: 'INV-001',
        customerName: 'John Doe',
        amount: 150.00,
        paymentDate: '2024-01-01T00:00:00Z',
        paymentMethod: SriPaymentMethod.Cash,
        status: PaymentStatus.Voided,
        notes: 'Duplicate payment',
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-05T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockVoidedPayment, success: true })

      const { voidPayment } = usePayment()
      const result = await voidPayment('1', voidData)

      expect(mockApiFetch).toHaveBeenCalledWith('/payments/1/void', {
        method: 'PUT',
        body: voidData,
      })
      expect(result).toEqual(mockVoidedPayment)
      expect(result.status).toBe(PaymentStatus.Voided)
    })

    it('should void a payment without reason', async () => {
      const mockVoidedPayment: Payment = {
        id: '2',
        tenantId: 'tenant-1',
        invoiceId: 'invoice-2',
        invoiceNumber: 'INV-002',
        customerName: 'Jane Smith',
        amount: 300.00,
        paymentDate: '2024-01-02T00:00:00Z',
        paymentMethod: SriPaymentMethod.BankTransfer,
        status: PaymentStatus.Voided,
        createdAt: '2024-01-02T00:00:00Z',
        updatedAt: '2024-01-05T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockVoidedPayment, success: true })

      const { voidPayment } = usePayment()
      const result = await voidPayment('2')

      expect(mockApiFetch).toHaveBeenCalledWith('/payments/2/void', {
        method: 'PUT',
        body: {},
      })
      expect(result.status).toBe(PaymentStatus.Voided)
    })

    it('should handle API errors when voiding payment', async () => {
      mockApiFetch.mockRejectedValue(new Error('Payment already voided'))

      const { voidPayment } = usePayment()

      await expect(voidPayment('1')).rejects.toThrow('Payment already voided')
    })
  })

  describe('completePayment', () => {
    it('should complete a payment successfully', async () => {
      const completeData = {
        notes: 'Payment confirmed by bank',
      }

      const mockCompletedPayment: Payment = {
        id: '2',
        tenantId: 'tenant-1',
        invoiceId: 'invoice-2',
        invoiceNumber: 'INV-002',
        customerName: 'Jane Smith',
        amount: 300.00,
        paymentDate: '2024-01-02T00:00:00Z',
        paymentMethod: SriPaymentMethod.BankTransfer,
        status: PaymentStatus.Completed,
        notes: 'Payment confirmed by bank',
        createdAt: '2024-01-02T00:00:00Z',
        updatedAt: '2024-01-05T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockCompletedPayment, success: true })

      const { completePayment } = usePayment()
      const result = await completePayment('2', completeData)

      expect(mockApiFetch).toHaveBeenCalledWith('/payments/2/complete', {
        method: 'PUT',
        body: completeData,
      })
      expect(result).toEqual(mockCompletedPayment)
      expect(result.status).toBe(PaymentStatus.Completed)
    })

    it('should complete a payment without notes', async () => {
      const mockCompletedPayment: Payment = {
        id: '3',
        tenantId: 'tenant-1',
        invoiceId: 'invoice-3',
        invoiceNumber: 'INV-003',
        customerName: 'Alice Johnson',
        amount: 400.00,
        paymentDate: '2024-01-03T00:00:00Z',
        paymentMethod: SriPaymentMethod.CreditCard,
        status: PaymentStatus.Completed,
        createdAt: '2024-01-03T00:00:00Z',
        updatedAt: '2024-01-05T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockCompletedPayment, success: true })

      const { completePayment } = usePayment()
      const result = await completePayment('3')

      expect(mockApiFetch).toHaveBeenCalledWith('/payments/3/complete', {
        method: 'PUT',
        body: {},
      })
      expect(result.status).toBe(PaymentStatus.Completed)
    })

    it('should handle API errors when completing payment', async () => {
      mockApiFetch.mockRejectedValue(new Error('Payment already completed'))

      const { completePayment } = usePayment()

      await expect(completePayment('1')).rejects.toThrow('Payment already completed')
    })
  })

  describe('payment status transitions', () => {
    it('should transition payment from Pending to Completed', async () => {
      const pendingPayment: Payment = {
        id: '1',
        tenantId: 'tenant-1',
        invoiceId: 'invoice-1',
        invoiceNumber: 'INV-001',
        customerName: 'John Doe',
        amount: 150.00,
        paymentDate: '2024-01-01T00:00:00Z',
        paymentMethod: SriPaymentMethod.BankTransfer,
        status: PaymentStatus.Pending,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
      }

      const completedPayment: Payment = {
        ...pendingPayment,
        status: PaymentStatus.Completed,
        updatedAt: '2024-01-02T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: completedPayment, success: true })

      const { completePayment } = usePayment()
      const result = await completePayment('1')

      expect(result.status).toBe(PaymentStatus.Completed)
      expect(result.updatedAt).not.toBe(pendingPayment.updatedAt)
    })

    it('should transition payment from Completed to Voided', async () => {
      const completedPayment: Payment = {
        id: '1',
        tenantId: 'tenant-1',
        invoiceId: 'invoice-1',
        invoiceNumber: 'INV-001',
        customerName: 'John Doe',
        amount: 150.00,
        paymentDate: '2024-01-01T00:00:00Z',
        paymentMethod: SriPaymentMethod.Cash,
        status: PaymentStatus.Completed,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
      }

      const voidedPayment: Payment = {
        ...completedPayment,
        status: PaymentStatus.Voided,
        notes: 'Payment error',
        updatedAt: '2024-01-03T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: voidedPayment, success: true })

      const { voidPayment } = usePayment()
      const result = await voidPayment('1', { reason: 'Payment error' })

      expect(result.status).toBe(PaymentStatus.Voided)
      expect(result.notes).toBe('Payment error')
    })
  })
})
