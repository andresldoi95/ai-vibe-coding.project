import {
  INVOICE_STATUS_SEVERITY,
  PAYMENT_STATUS_SEVERITY,
  SUBSCRIPTION_STATUS_SEVERITY,
} from './constants'
import type { InvoiceStatus, PaymentStatus, SubscriptionStatus } from '~/types/billing'

/**
 * Get PrimeVue severity for invoice status
 */
export function getInvoiceStatusSeverity(status: InvoiceStatus): string {
  return INVOICE_STATUS_SEVERITY[status] || 'secondary'
}

/**
 * Get PrimeVue severity for payment status
 */
export function getPaymentStatusSeverity(status: PaymentStatus): string {
  return PAYMENT_STATUS_SEVERITY[status] || 'secondary'
}

/**
 * Get PrimeVue severity for subscription status
 */
export function getSubscriptionStatusSeverity(status: SubscriptionStatus): string {
  return SUBSCRIPTION_STATUS_SEVERITY[status] || 'secondary'
}

/**
 * Get status label with proper formatting
 */
export function getStatusLabel(status: string): string {
  return status.charAt(0).toUpperCase() + status.slice(1).replace(/_/g, ' ')
}
