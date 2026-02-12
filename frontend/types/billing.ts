// Billing module types

// Tax Rates
export interface TaxRate {
  id: string
  tenantId: string
  code: string
  name: string
  rate: number // Decimal value (e.g., 0.12 for 12%)
  isDefault: boolean
  isActive: boolean
  countryId?: string
  countryCode?: string
  countryName?: string
  createdAt: string
  updatedAt: string
}

export interface CreateTaxRateDto {
  code: string
  name: string
  rate: number
  isDefault: boolean
  isActive: boolean
  countryId?: string
}

export interface UpdateTaxRateDto {
  id: string
  code: string
  name: string
  rate: number
  isDefault: boolean
  isActive: boolean
  countryId?: string
}

// Invoice Configuration
export interface InvoiceConfiguration {
  id: string
  tenantId: string
  establishmentCode: string // Ecuador: 001
  emissionPointCode: string // Ecuador: 001
  nextSequentialNumber: number
  defaultTaxRateId?: string
  defaultTaxRateName?: string // Included from backend DTO
  defaultWarehouseId?: string
  defaultWarehouseName?: string // Included from backend DTO
  dueDays: number
  requireCustomerTaxId: boolean
  createdAt: string
  updatedAt: string
}

export interface UpdateInvoiceConfigurationDto {
  establishmentCode: string
  emissionPointCode: string
  defaultTaxRateId?: string
  defaultWarehouseId?: string
  dueDays: number
  requireCustomerTaxId: boolean
}

// Invoices
export interface Invoice {
  id: string
  tenantId: string
  invoiceNumber: string
  customerId: string
  customerName: string
  customerTaxId?: string
  warehouseId?: string
  warehouseName?: string
  issueDate: string
  dueDate: string
  paidDate?: string
  subtotalAmount: number
  taxAmount: number
  totalAmount: number
  status: InvoiceStatus
  notes?: string

  // SRI Ecuador fields
  emissionPointId?: string
  documentType: number
  accessKey?: string
  paymentMethod: number
  xmlFilePath?: string
  signedXmlFilePath?: string
  environment: number
  sriAuthorization?: string // Ecuador SRI authorization number
  authorizationDate?: string // Ecuador SRI authorization date
  items: InvoiceItem[]
  createdAt: string
  updatedAt: string
}

export enum InvoiceStatus {
  Draft = 0,
  PendingSignature = 1,
  PendingAuthorization = 2,
  Authorized = 3,
  Rejected = 4,
  Sent = 5,
  Paid = 6,
  Overdue = 7,
  Cancelled = 8,
  Voided = 9,
}

export interface InvoiceItem {
  id: string
  invoiceId: string
  productId: string
  productCode: string // Denormalized for history
  productName: string // Denormalized for history
  description?: string
  quantity: number
  unitPrice: number
  taxRateId: string
  taxRate: number // Denormalized tax rate value for history
  subtotalAmount: number // Calculated: quantity * unitPrice
  taxAmount: number // Calculated: subtotalAmount * taxRate
  totalAmount: number // Calculated: subtotalAmount + taxAmount
}

export interface CreateInvoiceItemDto {
  productId: string
  quantity: number
  unitPrice: number
  taxRateId: string
  description?: string
}

export interface UpdateInvoiceItemDto {
  id?: string // Present if updating existing item
  productId: string
  quantity: number
  unitPrice: number
  taxRateId: string
  description?: string
}

export interface CreateInvoiceDto {
  customerId: string
  warehouseId?: string
  issueDate: string
  notes?: string
  items: CreateInvoiceItemDto[]
}

export interface UpdateInvoiceDto {
  customerId: string
  warehouseId?: string
  issueDate: string
  notes?: string
  items: UpdateInvoiceItemDto[]
}

export interface InvoiceFilters {
  customerId?: string
  status?: InvoiceStatus
  dateFrom?: string
  dateTo?: string
}

// Customer (existing, no changes needed)

export interface Customer {
  id: string
  tenantId: string
  name: string
  email: string
  phone?: string
  identificationType: number // IdentificationType enum
  taxId?: string
  contactPerson?: string

  // Billing Address
  billingStreet?: string
  billingCity?: string
  billingState?: string
  billingPostalCode?: string
  billingCountryId?: string
  billingCountryName?: string

  // Shipping Address
  shippingStreet?: string
  shippingCity?: string
  shippingState?: string
  shippingPostalCode?: string
  shippingCountryId?: string
  shippingCountryName?: string

  // Additional Information
  notes?: string
  website?: string

  isActive: boolean
  createdAt: string
  updatedAt: string
}

export interface CustomerFilters {
  searchTerm?: string
  name?: string
  email?: string
  phone?: string
  taxId?: string
  city?: string
  country?: string
  isActive?: boolean
}

export interface Payment {
  id: string
  tenantId: string
  invoiceId: string
  amount: number
  paymentDate: string
  paymentMethod: PaymentMethod
  status: PaymentStatus
  transactionId?: string
  notes?: string
  createdAt: string
  updatedAt: string
}

export type PaymentMethod = 'credit_card' | 'bank_transfer' | 'cash' | 'paypal' | 'stripe'
export type PaymentStatus = 'pending' | 'completed' | 'failed' | 'refunded'

export interface Subscription {
  id: string
  tenantId: string
  customerId: string
  planId: string
  status: SubscriptionStatus
  currentPeriodStart: string
  currentPeriodEnd: string
  cancelAtPeriodEnd: boolean
  createdAt: string
  updatedAt: string
}

export type SubscriptionStatus = 'active' | 'cancelled' | 'expired' | 'trial'
