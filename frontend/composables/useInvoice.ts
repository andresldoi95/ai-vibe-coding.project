import type { CreateInvoiceDto, Invoice, InvoiceFilters, InvoiceStatus } from '~/types/billing'

interface ApiResponse<T> {
  data: T
  success: boolean
}

export function useInvoice() {
  const { apiFetch } = useApi()

  async function getAllInvoices(filters?: InvoiceFilters): Promise<Invoice[]> {
    const params = new URLSearchParams()

    if (filters?.customerId) {
      params.append('customerId', filters.customerId)
    }
    if (filters?.status !== undefined && filters?.status !== null) {
      params.append('status', filters.status.toString())
    }
    if (filters?.dateFrom) {
      params.append('dateFrom', filters.dateFrom)
    }
    if (filters?.dateTo) {
      params.append('dateTo', filters.dateTo)
    }

    const queryString = params.toString()
    const url = `/invoices${queryString ? `?${queryString}` : ''}`

    const response = await apiFetch<ApiResponse<Invoice[]>>(url, {
      method: 'GET',
    })
    return response.data
  }

  async function getInvoiceById(id: string): Promise<Invoice> {
    const response = await apiFetch<ApiResponse<Invoice>>(`/invoices/${id}`, {
      method: 'GET',
    })
    return response.data
  }

  async function createInvoice(data: CreateInvoiceDto): Promise<Invoice> {
    const response = await apiFetch<ApiResponse<Invoice>>('/invoices', {
      method: 'POST',
      body: data,
    })
    return response.data
  }

  async function changeInvoiceStatus(id: string, newStatus: InvoiceStatus): Promise<Invoice> {
    const response = await apiFetch<ApiResponse<Invoice>>(`/invoices/${id}/status`, {
      method: 'PATCH',
      body: { id, newStatus },
    })
    return response.data
  }

  async function deleteInvoice(id: string): Promise<void> {
    await apiFetch(`/invoices/${id}`, {
      method: 'DELETE',
    })
  }

  return {
    getAllInvoices,
    getInvoiceById,
    createInvoice,
    changeInvoiceStatus,
    deleteInvoice,
  }
}
