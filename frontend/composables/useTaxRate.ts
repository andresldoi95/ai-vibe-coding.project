import type { CreateTaxRateDto, TaxRate, UpdateTaxRateDto } from '~/types/billing'

interface ApiResponse<T> {
  data: T
  success: boolean
}

export function useTaxRate() {
  const { apiFetch } = useApi()

  async function getAllTaxRates(): Promise<TaxRate[]> {
    const response = await apiFetch<ApiResponse<TaxRate[]>>('/taxrates', {
      method: 'GET',
    })
    return response.data
  }

  async function getTaxRateById(id: string): Promise<TaxRate> {
    const response = await apiFetch<ApiResponse<TaxRate>>(`/taxrates/${id}`, {
      method: 'GET',
    })
    return response.data
  }

  async function createTaxRate(data: CreateTaxRateDto): Promise<TaxRate> {
    const response = await apiFetch<ApiResponse<TaxRate>>('/taxrates', {
      method: 'POST',
      body: data,
    })
    return response.data
  }

  async function updateTaxRate(data: UpdateTaxRateDto): Promise<TaxRate> {
    const response = await apiFetch<ApiResponse<TaxRate>>(`/taxrates/${data.id}`, {
      method: 'PUT',
      body: data,
    })
    return response.data
  }

  async function deleteTaxRate(id: string): Promise<void> {
    await apiFetch(`/taxrates/${id}`, {
      method: 'DELETE',
    })
  }

  return {
    getAllTaxRates,
    getTaxRateById,
    createTaxRate,
    updateTaxRate,
    deleteTaxRate,
  }
}
