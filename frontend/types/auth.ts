// Authentication types
export interface User {
  id: string
  name: string
  email: string
  isActive: boolean
  emailConfirmed: boolean
}

export interface LoginCredentials {
  email: string
  password: string
}

export interface LoginResponse {
  accessToken: string
  refreshToken: string
  user: User
  tenants: Array<{
    id: string
    name: string
    slug: string
    status: string
  }>
}

// API wrapper type
export interface ApiResponse<T> {
  data: T
  message: string
  success: boolean
}

export interface RegisterData {
  name: string
  email: string
  password: string
  confirmPassword: string
}
