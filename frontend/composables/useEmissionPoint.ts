import type { CreateEmissionPointDto, EmissionPoint, UpdateEmissionPointDto } from '~/types/emission-point'
import type { DocumentType } from '~/types/sri-enums'

interface ApiResponse<T> {
  data: T
  success: boolean
}

interface GetEmissionPointsFilters {
  establishmentId?: string
  isActive?: boolean
}

interface NextSequentialResponse {
  documentType: DocumentType
  currentSequence: number
  nextSequential: number
  formattedSequential: string
}

export function useEmissionPoint() {
  const { apiFetch } = useApi()

  async function getAllEmissionPoints(filters?: GetEmissionPointsFilters): Promise<EmissionPoint[]> {
    const params = new URLSearchParams()
    if (filters?.establishmentId)
      params.append('establishmentId', filters.establishmentId)
    if (filters?.isActive !== undefined)
      params.append('isActive', filters.isActive.toString())

    const queryString = params.toString()
    const url = `/emission-points${queryString ? `?${queryString}` : ''}`

    const response = await apiFetch<ApiResponse<EmissionPoint[]>>(url, {
      method: 'GET',
    })
    return response.data
  }

  async function getEmissionPointById(id: string): Promise<EmissionPoint> {
    const response = await apiFetch<ApiResponse<EmissionPoint>>(`/emission-points/${id}`, {
      method: 'GET',
    })
    return response.data
  }

  async function getEmissionPointsByEstablishment(establishmentId: string): Promise<EmissionPoint[]> {
    return getAllEmissionPoints({ establishmentId })
  }

  async function getNextSequential(emissionPointId: string, documentType: DocumentType): Promise<NextSequentialResponse> {
    const response = await apiFetch<ApiResponse<NextSequentialResponse>>(
      `/emission-points/${emissionPointId}/next-sequential/${documentType}`,
      {
        method: 'GET',
      },
    )
    return response.data
  }

  async function createEmissionPoint(data: CreateEmissionPointDto): Promise<EmissionPoint> {
    const response = await apiFetch<ApiResponse<EmissionPoint>>('/emission-points', {
      method: 'POST',
      body: data,
    })
    return response.data
  }

  async function updateEmissionPoint(id: string, data: UpdateEmissionPointDto): Promise<EmissionPoint> {
    const response = await apiFetch<ApiResponse<EmissionPoint>>(`/emission-points/${id}`, {
      method: 'PUT',
      body: data,
    })
    return response.data
  }

  async function deleteEmissionPoint(id: string): Promise<void> {
    await apiFetch(`/emission-points/${id}`, {
      method: 'DELETE',
    })
  }

  return {
    getAllEmissionPoints,
    getEmissionPointById,
    getEmissionPointsByEstablishment,
    getNextSequential,
    createEmissionPoint,
    updateEmissionPoint,
    deleteEmissionPoint,
  }
}
