import type { CreateCreditNoteDto, CreditNote, CreditNoteFilters, SriErrorLog, UpdateCreditNoteDto } from '~/types/billing'

interface ApiResponse<T> {
  data: T
  success: boolean
}

export function useCreditNote() {
  const { apiFetch } = useApi()

  async function getAllCreditNotes(filters?: CreditNoteFilters): Promise<CreditNote[]> {
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
    const url = `/credit-notes${queryString ? `?${queryString}` : ''}`

    const response = await apiFetch<ApiResponse<CreditNote[]>>(url, { method: 'GET' })
    return response.data
  }

  async function getCreditNoteById(id: string): Promise<CreditNote> {
    const response = await apiFetch<ApiResponse<CreditNote>>(`/credit-notes/${id}`, { method: 'GET' })
    return response.data
  }

  async function createCreditNote(data: CreateCreditNoteDto): Promise<CreditNote> {
    const response = await apiFetch<ApiResponse<CreditNote>>('/credit-notes', {
      method: 'POST',
      body: data,
    })
    return response.data
  }

  async function updateCreditNote(id: string, data: UpdateCreditNoteDto): Promise<CreditNote> {
    const response = await apiFetch<ApiResponse<CreditNote>>(`/credit-notes/${id}`, {
      method: 'PUT',
      body: data,
    })
    return response.data
  }

  async function deleteCreditNote(id: string): Promise<void> {
    await apiFetch(`/credit-notes/${id}`, { method: 'DELETE' })
  }

  // ── SRI Workflow Methods ────────────────────────────────────────────────────

  async function generateXml(id: string): Promise<CreditNote> {
    const response = await apiFetch<ApiResponse<CreditNote>>(`/credit-notes/${id}/generate-xml`, { method: 'POST' })
    return response.data
  }

  async function signDocument(id: string): Promise<CreditNote> {
    const response = await apiFetch<ApiResponse<CreditNote>>(`/credit-notes/${id}/sign`, { method: 'POST' })
    return response.data
  }

  async function submitToSri(id: string): Promise<CreditNote> {
    const response = await apiFetch<ApiResponse<CreditNote>>(`/credit-notes/${id}/submit-sri`, { method: 'POST' })
    return response.data
  }

  async function checkAuthorization(id: string): Promise<CreditNote> {
    const response = await apiFetch<ApiResponse<CreditNote>>(`/credit-notes/${id}/check-authorization`, { method: 'POST' })
    return response.data
  }

  async function generateRide(id: string): Promise<CreditNote> {
    const response = await apiFetch<ApiResponse<CreditNote>>(`/credit-notes/${id}/generate-ride`, { method: 'POST' })
    return response.data
  }

  async function downloadXml(id: string): Promise<Blob> {
    const response = await apiFetch(`/credit-notes/${id}/download-xml`, {
      method: 'GET',
      responseType: 'blob',
    })
    return response as unknown as Blob
  }

  async function downloadRide(id: string): Promise<Blob> {
    const response = await apiFetch(`/credit-notes/${id}/download-ride`, {
      method: 'GET',
      responseType: 'blob',
    })
    return response as unknown as Blob
  }

  async function getSriErrors(id: string): Promise<SriErrorLog[]> {
    const response = await apiFetch<ApiResponse<SriErrorLog[]>>(`/credit-notes/${id}/sri-errors`, { method: 'GET' })
    return response.data
  }

  return {
    getAllCreditNotes,
    getCreditNoteById,
    createCreditNote,
    updateCreditNote,
    deleteCreditNote,
    // SRI Methods
    generateXml,
    signDocument,
    submitToSri,
    checkAuthorization,
    generateRide,
    downloadXml,
    downloadRide,
    getSriErrors,
  }
}
