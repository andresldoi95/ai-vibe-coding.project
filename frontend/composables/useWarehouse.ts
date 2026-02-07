import type { Warehouse } from '~/types/inventory'

interface CreateWarehouseData {
  name: string
  code: string
  description?: string
  streetAddress: string
  city: string
  state?: string
  postalCode: string
  country: string
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

export function useWarehouse() {
  const { apiFetch } = useApi()

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

  return {
    getAllWarehouses,
    getWarehouseById,
    createWarehouse,
    updateWarehouse,
    deleteWarehouse,
  }
}
