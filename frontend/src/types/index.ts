// User types
export interface IUser {
  id: string
  email: string
  fullName: string
  phone?: string
  status: string
}

// Company types
export interface ICompany {
  id: string
  ruc: string
  businessName: string
  tradeName?: string
  email: string
  phone?: string
  address?: string
}

// Invoice types
export interface IInvoice {
  id: string
  companyId: string
  customerId: string
  fullNumber: string
  issueDate: string
  total: number
  status: string
  sriAuthorizationNumber?: string
}

// Product types
export interface IProduct {
  id: string
  companyId: string
  sku?: string
  barcode?: string
  name: string
  description?: string
  unitOfMeasure: string
  status: string
}

// Customer types
export interface ICustomer {
  id: string
  companyId: string
  identificationType: string
  identification: string
  businessName: string
  tradeName?: string
  email?: string
  phone?: string
  address?: string
}

// API Response types
export interface ApiResponse<T> {
  success: boolean
  data: T
  message?: string
}

export interface PaginatedResponse<T> {
  success: boolean
  data: T[]
  pagination: {
    page: number
    limit: number
    total: number
    totalPages: number
  }
}
