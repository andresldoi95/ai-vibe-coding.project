// Authentication types
export interface User {
  id: string
  name: string
  email: string
  role: UserRole
  createdAt: string
  updatedAt: string
}

export type UserRole = 'admin' | 'user' | 'manager'

export interface LoginCredentials {
  email: string
  password: string
}

export interface LoginResponse {
  accessToken: string
  refreshToken: string
  user: User
}

export interface RegisterData {
  name: string
  email: string
  password: string
  confirmPassword: string
}
