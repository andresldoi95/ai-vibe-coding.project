import { describe, expect, it } from 'vitest'
import {
  getInvoiceStatusSeverity,
  getPaymentStatusSeverity,
  getStatusLabel,
  getSubscriptionStatusSeverity,
} from '~/utils/status'
import type { InvoiceStatus, PaymentStatus, SubscriptionStatus } from '~/types/billing'

describe('status utility', () => {
  describe('getInvoiceStatusSeverity', () => {
    it('should return correct severity for draft status', () => {
      const severity = getInvoiceStatusSeverity('draft' as unknown as InvoiceStatus)
      expect(severity).toBe('secondary')
    })

    it('should return correct severity for sent status', () => {
      const severity = getInvoiceStatusSeverity('sent' as unknown as InvoiceStatus)
      expect(severity).toBe('info')
    })

    it('should return correct severity for paid status', () => {
      const severity = getInvoiceStatusSeverity('paid' as unknown as InvoiceStatus)
      expect(severity).toBe('success')
    })

    it('should return correct severity for overdue status', () => {
      const severity = getInvoiceStatusSeverity('overdue' as unknown as InvoiceStatus)
      expect(severity).toBe('danger')
    })

    it('should return correct severity for cancelled status', () => {
      const severity = getInvoiceStatusSeverity('cancelled' as unknown as InvoiceStatus)
      expect(severity).toBe('warning')
    })

    it('should return secondary as fallback for unknown status', () => {
      const severity = getInvoiceStatusSeverity('unknown' as unknown as InvoiceStatus)
      expect(severity).toBe('secondary')
    })
  })

  describe('getPaymentStatusSeverity', () => {
    it('should return correct severity for pending status', () => {
      const severity = getPaymentStatusSeverity('pending' as unknown as PaymentStatus)
      expect(severity).toBe('warning')
    })

    it('should return correct severity for completed status', () => {
      const severity = getPaymentStatusSeverity('completed' as unknown as PaymentStatus)
      expect(severity).toBe('success')
    })

    it('should return correct severity for failed status', () => {
      const severity = getPaymentStatusSeverity('failed' as unknown as PaymentStatus)
      expect(severity).toBe('danger')
    })

    it('should return correct severity for refunded status', () => {
      const severity = getPaymentStatusSeverity('refunded' as unknown as PaymentStatus)
      expect(severity).toBe('info')
    })

    it('should return secondary as fallback for unknown status', () => {
      const severity = getPaymentStatusSeverity('unknown' as unknown as PaymentStatus)
      expect(severity).toBe('secondary')
    })
  })

  describe('getSubscriptionStatusSeverity', () => {
    it('should return correct severity for active status', () => {
      const severity = getSubscriptionStatusSeverity('active' as unknown as SubscriptionStatus)
      expect(severity).toBe('success')
    })

    it('should return correct severity for cancelled status', () => {
      const severity = getSubscriptionStatusSeverity('cancelled' as unknown as SubscriptionStatus)
      expect(severity).toBe('warning')
    })

    it('should return correct severity for expired status', () => {
      const severity = getSubscriptionStatusSeverity('expired' as unknown as SubscriptionStatus)
      expect(severity).toBe('danger')
    })

    it('should return correct severity for trial status', () => {
      const severity = getSubscriptionStatusSeverity('trial' as unknown as SubscriptionStatus)
      expect(severity).toBe('info')
    })

    it('should return secondary as fallback for unknown status', () => {
      const severity = getSubscriptionStatusSeverity('unknown' as unknown as SubscriptionStatus)
      expect(severity).toBe('secondary')
    })
  })

  describe('getStatusLabel', () => {
    it('should capitalize first letter', () => {
      const label = getStatusLabel('active')
      expect(label).toBe('Active')
    })

    it('should replace underscores with spaces', () => {
      const label = getStatusLabel('in_progress')
      expect(label).toBe('In progress')
    })

    it('should handle multiple underscores', () => {
      const label = getStatusLabel('pending_approval_review')
      expect(label).toBe('Pending approval review')
    })

    it('should handle already capitalized strings', () => {
      const label = getStatusLabel('COMPLETED')
      expect(label).toBe('COMPLETED')
    })

    it('should handle single character', () => {
      const label = getStatusLabel('a')
      expect(label).toBe('A')
    })

    it('should handle empty string', () => {
      const label = getStatusLabel('')
      expect(label).toBe('')
    })

    it('should handle status with no underscores', () => {
      const label = getStatusLabel('draft')
      expect(label).toBe('Draft')
    })
  })
})
