import type { MovementType, StockMovement } from '~/types/inventory'

interface CreateStockMovementData {
  productId: string
  warehouseId: string
  destinationWarehouseId?: string
  movementType: MovementType
  quantity: number
  unitCost?: number
  totalCost?: number
  reference?: string
  notes?: string
  movementDate?: string
}

interface UpdateStockMovementData extends CreateStockMovementData {
  id: string
}

interface ApiResponse<T> {
  data: T
  success: boolean
}

interface ExportStockMovementsFilters {
  format?: 'csv' | 'excel'
  brand?: string
  category?: string
  warehouseId?: string
  fromDate?: string
  toDate?: string
}

export function useStockMovement() {
  const { apiFetch } = useApi()
  const config = useRuntimeConfig()
  const authStore = useAuthStore()
  const tenantStore = useTenantStore()

  async function getAllStockMovements(): Promise<StockMovement[]> {
    const response = await apiFetch<ApiResponse<StockMovement[]>>('/stock-movements', {
      method: 'GET',
    })
    return response.data
  }

  async function getStockMovementById(id: string): Promise<StockMovement> {
    const response = await apiFetch<ApiResponse<StockMovement>>(`/stock-movements/${id}`, {
      method: 'GET',
    })
    return response.data
  }

  async function createStockMovement(data: CreateStockMovementData): Promise<StockMovement> {
    const response = await apiFetch<ApiResponse<StockMovement>>('/stock-movements', {
      method: 'POST',
      body: data,
    })
    return response.data
  }

  async function updateStockMovement(data: UpdateStockMovementData): Promise<StockMovement> {
    const response = await apiFetch<ApiResponse<StockMovement>>(`/stock-movements/${data.id}`, {
      method: 'PUT',
      body: data,
    })
    return response.data
  }

  async function deleteStockMovement(id: string): Promise<void> {
    await apiFetch<ApiResponse<void>>(`/stock-movements/${id}`, {
      method: 'DELETE',
    })
  }

  async function exportStockMovements(filters: ExportStockMovementsFilters = {}): Promise<void> {
    if (!tenantStore.currentTenantId) {
      throw new Error('No tenant selected')
    }

    if (!authStore.token) {
      throw new Error('Not authenticated')
    }

    const params = new URLSearchParams()
    if (filters.format)
      params.append('format', filters.format)
    if (filters.brand)
      params.append('brand', filters.brand)
    if (filters.category)
      params.append('category', filters.category)
    if (filters.warehouseId)
      params.append('warehouseId', filters.warehouseId)
    if (filters.fromDate)
      params.append('fromDate', filters.fromDate)
    if (filters.toDate)
      params.append('toDate', filters.toDate)

    const queryString = params.toString()
    const url = `${config.public.apiBase}/stock-movements/export${queryString ? `?${queryString}` : ''}`

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
    const filename = filenameMatch?.[1]?.replace(/['"]/g, '') || `stock-movements-${new Date().toISOString().split('T')[0]}.${fileExtension}`

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
    getAllStockMovements,
    getStockMovementById,
    createStockMovement,
    updateStockMovement,
    deleteStockMovement,
    exportStockMovements,
  }
}
