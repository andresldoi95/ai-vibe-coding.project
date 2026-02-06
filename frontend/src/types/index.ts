// User types
export interface IUser {
  id: string
  email: string
  fullName: string
  phone?: string
  status: string
}

export interface IUserCompany {
  companyId: string
  companyName: string
  companyRuc: string
  roleName: string
  roleId: string
}

export interface ISelectedCompany {
  id: string
  name: string
  ruc: string
  role: string
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

export interface ICompanyRegistrationData {
  ruc: string
  businessName: string
  tradeName?: string
  email: string
  phone: string
  address: string
}

export interface IAdminUserData {
  email: string
  password: string
  fullName: string
  phone?: string
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
  code: string
  barcode?: string
  name: string
  description?: string
  category?: string
  unit: string
  price: number
  cost: number
  taxTypeId: string
  hasInventory: boolean
  minStock?: number
  maxStock?: number
  reorderPoint?: number
  image?: string
  status: 'active' | 'inactive'
  createdAt: Date
  updatedAt: Date
}

export interface ICreateProductDTO {
  code: string
  barcode?: string
  name: string
  description?: string
  category?: string
  unit: string
  price: number
  cost: number
  taxTypeId: string
  hasInventory: boolean
  minStock?: number
  maxStock?: number
  reorderPoint?: number
  image?: string
}

export interface IUpdateProductDTO {
  code?: string
  barcode?: string
  name?: string
  description?: string
  category?: string
  unit?: string
  price?: number
  cost?: number
  taxTypeId?: string
  hasInventory?: boolean
  minStock?: number
  maxStock?: number
  reorderPoint?: number
  image?: string
  status?: 'active' | 'inactive'
}

export interface IProductFilters {
  search?: string
  category?: string
  status?: 'active' | 'inactive'
  hasInventory?: boolean
  page?: number
  limit?: number
}

export interface IProductResponse {
  products: IProduct[]
  total: number
  page: number
  totalPages: number
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
