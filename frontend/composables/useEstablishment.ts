import type { CreateEstablishmentDto, Establishment, UpdateEstablishmentDto } from '~/types/establishment'

interface ApiResponse<T> {
  data: T
  success: boolean
}

export function useEstablishment() {
  const { apiFetch } = useApi()

  async function getAllEstablishments(): Promise<Establishment[]> {
    const response = await apiFetch<ApiResponse<Establishment[]>>('/establishments', {
      method: 'GET',
    })
    return response.data
  }

  async function getEstablishmentById(id: string): Promise<Establishment> {
    const response = await apiFetch<ApiResponse<Establishment>>(`/establishments/${id}`, {
      method: 'GET',
    })
    return response.data
  }

  async function createEstablishment(data: CreateEstablishmentDto): Promise<Establishment> {
    const response = await apiFetch<ApiResponse<Establishment>>('/establishments', {
      method: 'POST',
      body: data,
    })
    return response.data
  }

  async function updateEstablishment(id: string, data: UpdateEstablishmentDto): Promise<Establishment> {
    const response = await apiFetch<ApiResponse<Establishment>>(`/establishments/${id}`, {
      method: 'PUT',
      body: data,
    })
    return response.data
  }

  async function deleteEstablishment(id: string): Promise<void> {
    await apiFetch(`/establishments/${id}`, {
      method: 'DELETE',
    })
  }

  return {
    getAllEstablishments,
    getEstablishmentById,
    createEstablishment,
    updateEstablishment,
    deleteEstablishment,
  }
}
