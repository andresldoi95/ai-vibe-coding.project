import type { CreateInvoiceDto, Invoice, InvoiceFilters, InvoiceStatus, UpdateInvoiceDto } from '~/types/billing'

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

  async function updateInvoice(id: string, data: UpdateInvoiceDto): Promise<Invoice> {
    const response = await apiFetch<ApiResponse<Invoice>>(`/invoices/${id}`, {
      method: 'PUT',
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

  // SRI Workflow Methods
  async function generateXml(id: string): Promise<Invoice> {
    const response = await apiFetch<ApiResponse<Invoice>>(`/invoices/${id}/generate-xml`, {
      method: 'POST',
    })
    return response.data
  }

  async function signDocument(id: string): Promise<Invoice> {
    const response = await apiFetch<ApiResponse<Invoice>>(`/invoices/${id}/sign`, {
      method: 'POST',
    })
    return response.data
  }

  async function submitToSri(id: string): Promise<Invoice> {
    const response = await apiFetch<ApiResponse<Invoice>>(`/invoices/${id}/submit-to-sri`, {
      method: 'POST',
    })
    return response.data
  }

  async function checkAuthorization(id: string): Promise<Invoice> {
    const response = await apiFetch<ApiResponse<Invoice>>(`/invoices/${id}/check-authorization`, {
      method: 'GET',
    })
    return response.data
  }

  async function generateRide(id: string): Promise<Invoice> {
    const response = await apiFetch<ApiResponse<Invoice>>(`/invoices/${id}/generate-ride`, {
      method: 'POST',
    })
    return response.data
  }

  async function downloadXml(id: string): Promise<Blob> {
    const response = await apiFetch(`/invoices/${id}/download-xml`, {
      method: 'GET',
      responseType: 'blob',
    })
    return response as unknown as Blob
  }

  async function downloadRide(id: string): Promise<Blob> {
    const response = await apiFetch(`/invoices/${id}/download-ride`, {
      method: 'GET',
      responseType: 'blob',
    })
    return response as unknown as Blob
  }

  return {
    getAllInvoices,
    getInvoiceById,
    createInvoice,
    updateInvoice,
    changeInvoiceStatus,
    deleteInvoice,
    // SRI Methods
    generateXml,
    signDocument,
    submitToSri,
    checkAuthorization,
    generateRide,
    downloadXml,
    downloadRide,
  }
}
