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

export function useStockMovement() {
  const { apiFetch } = useApi()

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

  return {
    getAllStockMovements,
    getStockMovementById,
    createStockMovement,
    updateStockMovement,
    deleteStockMovement,
  }
}
