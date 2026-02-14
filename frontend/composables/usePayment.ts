import type { Payment, SriPaymentMethod, PaymentStatus } from '~/types/billing'

interface CreatePaymentData {
  invoiceId: string
  amount: number
  paymentDate: string
  paymentMethod: SriPaymentMethod
  status: PaymentStatus
  transactionId?: string
  notes?: string
}

interface VoidPaymentData {
  reason?: string
}

interface ApiResponse<T> {
  data: T
  success: boolean
}

export function usePayment() {
  const { apiFetch } = useApi()

  async function getAllPayments(): Promise<Payment[]> {
    const response = await apiFetch<ApiResponse<Payment[]>>('/payments', {
      method: 'GET',
    })
    return response.data
  }

  async function getPaymentById(id: string): Promise<Payment> {
    const response = await apiFetch<ApiResponse<Payment>>(`/payments/${id}`, {
      method: 'GET',
    })
    return response.data
  }

  async function getPaymentsByInvoiceId(invoiceId: string): Promise<Payment[]> {
    const response = await apiFetch<ApiResponse<Payment[]>>(`/payments/invoice/${invoiceId}`, {
      method: 'GET',
    })
    return response.data
  }

  async function createPayment(data: CreatePaymentData): Promise<Payment> {
    const response = await apiFetch<ApiResponse<Payment>>('/payments', {
      method: 'POST',
      body: data,
    })
    return response.data
  }

  async function voidPayment(id: string, data: VoidPaymentData = {}): Promise<Payment> {
    const response = await apiFetch<ApiResponse<Payment>>(`/payments/${id}/void`, {
      method: 'PUT',
      body: data,
    })
    return response.data
  }

  return {
    getAllPayments,
    getPaymentById,
    getPaymentsByInvoiceId,
    createPayment,
    voidPayment,
  }
}
