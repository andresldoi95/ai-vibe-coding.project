// Common API response types
export interface ApiResponse<T> {
  data: T
  message?: string
  success: boolean
}

export interface PaginatedResponse<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
  totalPages: number
}

export interface ApiError {
  message: string
  errors?: Record<string, string[]>
  statusCode: number
}

export interface SelectOption {
  label: string
  value: string | number
}
