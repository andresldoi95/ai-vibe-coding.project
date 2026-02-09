// Billing module types
export interface Invoice {
  id: string
  tenantId: string
  invoiceNumber: string
  customerId: string
  customerName: string
  totalAmount: number
  taxAmount: number
  subtotalAmount: number
  status: InvoiceStatus
  dueDate: string
  issueDate: string
  paidDate?: string
  createdAt: string
  updatedAt: string
  items: InvoiceItem[]
}

export type InvoiceStatus = 'draft' | 'sent' | 'paid' | 'overdue' | 'cancelled'

export interface InvoiceItem {
  id: string
  invoiceId: string
  description: string
  quantity: number
  unitPrice: number
  totalPrice: number
  taxRate: number
}

export interface Customer {
  id: string
  tenantId: string
  name: string
  email: string
  phone?: string
  taxId?: string
  contactPerson?: string

  // Billing Address
  billingStreet?: string
  billingCity?: string
  billingState?: string
  billingPostalCode?: string
  billingCountry?: string

  // Shipping Address
  shippingStreet?: string
  shippingCity?: string
  shippingState?: string
  shippingPostalCode?: string
  shippingCountry?: string

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
