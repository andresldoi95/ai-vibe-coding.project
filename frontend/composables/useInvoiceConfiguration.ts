import type { InvoiceConfiguration, UpdateInvoiceConfigurationDto } from '~/types/billing'

interface ApiResponse<T> {
  data: T
  success: boolean
}

export function useInvoiceConfiguration() {
  const { apiFetch } = useApi()

  async function getInvoiceConfiguration(): Promise<InvoiceConfiguration> {
    const response = await apiFetch<ApiResponse<InvoiceConfiguration>>('/invoice-configurations', {
      method: 'GET',
    })
    return response.data
  }

  async function updateInvoiceConfiguration(data: UpdateInvoiceConfigurationDto): Promise<InvoiceConfiguration> {
    const response = await apiFetch<ApiResponse<InvoiceConfiguration>>('/invoice-configurations', {
      method: 'PUT',
      body: data,
    })
    return response.data
  }

  return {
    getInvoiceConfiguration,
    updateInvoiceConfiguration,
  }
}
