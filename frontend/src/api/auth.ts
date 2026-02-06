import apiClient from './client'
import type { IUser, IUserCompany, ISelectedCompany } from '@/types'

export interface LoginRequest {
  email: string
  password: string
}

export interface LoginResponse {
  success: boolean
  requiresCompanySelection: boolean
  user: IUser
  companies: IUserCompany[]
  token?: string
  company?: ISelectedCompany
}

export interface SelectCompanyRequest {
  email: string
  companyId: string
}

export interface SelectCompanyResponse {
  success: boolean
  token: string
  user: IUser
  company: ISelectedCompany
}

export interface RegisterRequest {
  email: string
  password: string
  fullName: string
  phone?: string
  companyId: string
  roleId: string
}

export interface RegisterResponse {
  success: boolean
  message: string
  user: IUser
}

export interface CompanyRegistrationRequest {
  company: {
    ruc: string
    businessName: string
    tradeName?: string
    email: string
    phone: string
    address: string
  }
  admin: {
    email: string
    password: string
    fullName: string
    phone?: string
  }
}

export interface CompanyRegistrationResponse {
  success: boolean
  message: string
  data: {
    token: string
    user: IUser & { companies: IUserCompany[] }
    currentCompany: ISelectedCompany
    currentRole: string
  }
}

export interface CurrentUserResponse {
  success: boolean
  user: IUser
}

/**
 * Auth API
 * Handles authentication endpoints with multi-company support
 */

export const authAPI = {
  /**
   * Login with email and password
   * Returns user with available companies
   */
  async login(email: string, password: string): Promise<LoginResponse> {
    const response = await apiClient.post<LoginResponse>('/auth/login', {
      email,
      password
    })
    return response.data
  },

  /**
   * Select company and get JWT token
   */
  async selectCompany(email: string, companyId: string): Promise<SelectCompanyResponse> {
    const response = await apiClient.post<SelectCompanyResponse>('/auth/select-company', {
      email,
      companyId
    })
    return response.data
  },

  /**
   * Register new user
   */
  async register(data: RegisterRequest): Promise<RegisterResponse> {
    const response = await apiClient.post<RegisterResponse>('/auth/register', data)
    return response.data
  },

  /**
   * Get current authenticated user
   */
  async getCurrentUser(): Promise<CurrentUserResponse> {
    const response = await apiClient.get<CurrentUserResponse>('/auth/me')
    return response.data
  },

  /**
   * Logout (clears server-side session if any)
   */
  async logout(): Promise<void> {
    await apiClient.post('/auth/logout')
  },

  /**
   * Register new company with admin user
   */
  async registerCompany(data: CompanyRegistrationRequest): Promise<CompanyRegistrationResponse> {
    const response = await apiClient.post<CompanyRegistrationResponse>('/companies/register', data)
    return response.data
  },

  /**
   * Check if RUC exists
   */
  async checkRucExists(ruc: string): Promise<{ exists: boolean; available: boolean }> {
    const response = await apiClient.get<{ success: boolean; data: { exists: boolean; available: boolean } }>(`/companies/check-ruc/${ruc}`)
    return response.data.data
  }
}

export default authAPI
