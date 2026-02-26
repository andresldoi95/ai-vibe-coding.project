import type { Warehouse } from '~/types/inventory'

interface CreateWarehouseData {
  name: string
  code: string
  description?: string
  streetAddress: string
  city: string
  state?: string
  postalCode: string
  countryId: string
  phone?: string
  email?: string
  isActive?: boolean
  squareFootage?: number
  capacity?: number
}

interface UpdateWarehouseData extends CreateWarehouseData {
  id: string
}

interface ApiResponse<T> {
  data: T
  success: boolean
}

interface ExportWarehouseStockSummaryFilters {
  format?: 'csv' | 'excel'
}

export function useWarehouse() {
  const { apiFetch } = useApi()
  const config = useRuntimeConfig()
  const authStore = useAuthStore()
  const tenantStore = useTenantStore()

  async function getAllWarehouses(): Promise<Warehouse[]> {
    const response = await apiFetch<ApiResponse<Warehouse[]>>('/warehouses', {
      method: 'GET',
    })
    return response.data
  }

  async function getWarehouseById(id: string): Promise<Warehouse> {
    const response = await apiFetch<ApiResponse<Warehouse>>(`/warehouses/${id}`, {
      method: 'GET',
    })
    return response.data
  }

  async function createWarehouse(data: CreateWarehouseData): Promise<Warehouse> {
    const response = await apiFetch<ApiResponse<Warehouse>>('/warehouses', {
      method: 'POST',
      body: data,
    })
    return response.data
  }

  async function updateWarehouse(data: UpdateWarehouseData): Promise<Warehouse> {
    const response = await apiFetch<ApiResponse<Warehouse>>(`/warehouses/${data.id}`, {
      method: 'PUT',
      body: data,
    })
    return response.data
  }

  async function deleteWarehouse(id: string): Promise<void> {
    await apiFetch(`/warehouses/${id}`, {
      method: 'DELETE',
    })
  }

  async function exportWarehouseStockSummary(filters: ExportWarehouseStockSummaryFilters = {}): Promise<void> {
    if (!tenantStore.currentTenantId) {
      throw new Error('No tenant selected')
    }

    if (!authStore.token) {
      throw new Error('Not authenticated')
    }

    const params = new URLSearchParams()
    if (filters.format)
      params.append('format', filters.format)

    const queryString = params.toString()
    const url = `${config.public.apiBase}/warehouses/export/stock-summary${queryString ? `?${queryString}` : ''}`

    const response = await fetch(url, {
      headers: {
        'Authorization': `Bearer ${authStore.token}`,
        'X-Tenant-Id': tenantStore.currentTenantId,
      },
    })

    if (!response.ok) {
      throw new Error('Export failed')
    }

    // Get filename from Content-Disposition header or use default
    const contentDisposition = response.headers.get('Content-Disposition')
    const filenameMatch = contentDisposition?.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/)
    const fileExtension = filters.format === 'excel' ? 'xlsx' : filters.format || 'xlsx'
    const filename = filenameMatch?.[1]?.replace(/['"]/g, '') || `warehouse-stock-summary-${new Date().toISOString().split('T')[0]}.${fileExtension}`

    // Download file
    const blob = await response.blob()
    const downloadUrl = window.URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = downloadUrl
    a.download = filename
    document.body.appendChild(a)
    a.click()
    document.body.removeChild(a)
    window.URL.revokeObjectURL(downloadUrl)
  }

  return {
    getAllWarehouses,
    getWarehouseById,
    createWarehouse,
    updateWarehouse,
    deleteWarehouse,
    exportWarehouseStockSummary,
  }
}
