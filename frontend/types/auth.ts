// Authentication types
export interface User {
  id: string
  name: string
  email: string
  isActive: boolean
  emailConfirmed: boolean
}

export interface Permission {
  id: string
  name: string
  description: string
  resource: string
  action: string
}

export interface Role {
  id: string
  name: string
  description: string
  priority: number
  isSystemRole?: boolean
  isActive?: boolean
  userCount?: number
  permissions?: Permission[]
}

export interface RoleFormData {
  name: string
  description: string
  priority: number
  permissionIds: string[]
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

export interface SelectTenantResponse {
  accessToken: string
  role: Role
  permissions: string[]
}

// API wrapper type
export interface ApiResponse<T> {
  data: T
  message: string
  success: boolean
}

export interface RegisterData {
  companyName: string
  slug: string
  name: string
  email: string
  password: string
  confirmPassword: string
}
