// Billing module types
import type { SriPaymentMethod } from './sri-enums'

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

  // SRI Ecuador fields - Emission Point
  emissionPointId?: string
  emissionPointCode?: string
  emissionPointName?: string
  establishmentCode?: string

  // SRI Ecuador fields - Document
  documentType: number
  accessKey?: string
  paymentMethod: number
  xmlFilePath?: string
  signedXmlFilePath?: string
  rideFilePath?: string
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
  emissionPointId: string
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
  invoiceNumber: string
  customerName: string
  amount: number
  paymentDate: string
  paymentMethod: SriPaymentMethod
  status: PaymentStatus
  transactionId?: string
  notes?: string
  createdAt: string
  updatedAt: string
  createdBy?: string
}

// Payment Status
export enum PaymentStatus {
  Pending = 1,
  Completed = 2,
  Voided = 3,
}

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

export interface SriErrorLog {
  id: string
  operation: string
  errorCode?: string
  errorMessage: string
  additionalData?: string
  occurredAt: string
}

// ── Credit Notes ─────────────────────────────────────────────────────────────

export interface CreditNote {
  id: string
  tenantId: string
  creditNoteNumber: string
  customerId: string
  customerName: string
  customerTaxId?: string
  issueDate: string
  subtotalAmount: number
  taxAmount: number
  totalAmount: number
  valueModification: number
  reason: string
  status: InvoiceStatus
  notes?: string

  // Original document reference
  originalInvoiceId?: string
  originalInvoiceNumber: string
  originalInvoiceDate: string

  // SRI Ecuador fields - Emission Point
  emissionPointId?: string
  emissionPointCode?: string
  emissionPointName?: string
  establishmentCode?: string

  // SRI Ecuador fields - Document
  documentType: number
  accessKey?: string
  paymentMethod: number
  xmlFilePath?: string
  signedXmlFilePath?: string
  rideFilePath?: string
  environment: number
  sriAuthorization?: string
  authorizationDate?: string

  isPhysicalReturn: boolean
  items: CreditNoteItem[]
  isEditable: boolean
  createdAt: string
  updatedAt: string
}

export interface CreditNoteItem {
  id: string
  creditNoteId: string
  productId: string
  productCode: string
  productName: string
  description?: string
  quantity: number
  unitPrice: number
  taxRateId: string
  taxRate: number
  subtotalAmount: number
  taxAmount: number
  totalAmount: number
}

export interface CreateCreditNoteItemDto {
  productId: string
  quantity: number
  unitPrice: number
  taxRateId: string
  description?: string
}

export interface UpdateCreditNoteItemDto {
  id?: string
  productId: string
  quantity: number
  unitPrice: number
  taxRateId: string
  description?: string
}

export interface CreateCreditNoteDto {
  customerId: string
  originalInvoiceId: string
  emissionPointId: string
  issueDate: string
  reason: string
  notes?: string
  isPhysicalReturn?: boolean
  items: CreateCreditNoteItemDto[]
}

export interface UpdateCreditNoteDto {
  customerId: string
  issueDate: string
  reason: string
  notes?: string
  items: UpdateCreditNoteItemDto[]
}

export interface CreditNoteFilters {
  customerId?: string
  status?: InvoiceStatus
  dateFrom?: string
  dateTo?: string
}
