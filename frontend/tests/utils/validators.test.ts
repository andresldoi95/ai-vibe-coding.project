import { describe, expect, it } from 'vitest'
import {
  isValidEmail,
  isValidPhone,
  isValidSku,
  isValidUrl,
} from '~/utils/validators'

describe('validators utility', () => {
  describe('isValidEmail', () => {
    it('should validate correct email format', () => {
      expect(isValidEmail('user@example.com')).toBe(true)
      expect(isValidEmail('test.user@example.com')).toBe(true)
      expect(isValidEmail('user+tag@example.com')).toBe(true)
      expect(isValidEmail('user123@test-domain.co.uk')).toBe(true)
    })

    it('should reject email without @', () => {
      expect(isValidEmail('userexample.com')).toBe(false)
    })

    it('should reject email without domain', () => {
      expect(isValidEmail('user@')).toBe(false)
    })

    it('should reject email without local part', () => {
      expect(isValidEmail('@example.com')).toBe(false)
    })

    it('should reject email with spaces', () => {
      expect(isValidEmail('user @example.com')).toBe(false)
      expect(isValidEmail('user@ example.com')).toBe(false)
    })

    it('should reject email without TLD', () => {
      expect(isValidEmail('user@example')).toBe(false)
    })

    it('should reject email with multiple @', () => {
      expect(isValidEmail('user@@example.com')).toBe(false)
      expect(isValidEmail('user@test@example.com')).toBe(false)
    })

    it('should reject empty string', () => {
      expect(isValidEmail('')).toBe(false)
    })

    it('should accept email starting with dot after @ (regex allows it)', () => {
      expect(isValidEmail('user@.example.com')).toBe(true)
    })

    it('should accept email with numbers in domain', () => {
      expect(isValidEmail('user@example123.com')).toBe(true)
    })

    it('should accept email with subdomain', () => {
      expect(isValidEmail('user@mail.example.com')).toBe(true)
    })
  })

  describe('isValidUrl', () => {
    it('should validate correct HTTP URL', () => {
      expect(isValidUrl('http://example.com')).toBe(true)
      expect(isValidUrl('http://www.example.com')).toBe(true)
    })

    it('should validate correct HTTPS URL', () => {
      expect(isValidUrl('https://example.com')).toBe(true)
      expect(isValidUrl('https://www.example.com')).toBe(true)
    })

    it('should validate URL with path', () => {
      expect(isValidUrl('https://example.com/path/to/page')).toBe(true)
    })

    it('should validate URL with query parameters', () => {
      expect(isValidUrl('https://example.com?param=value')).toBe(true)
      expect(isValidUrl('https://example.com?param1=value1&param2=value2')).toBe(true)
    })

    it('should validate URL with port', () => {
      expect(isValidUrl('http://localhost:3000')).toBe(true)
      expect(isValidUrl('https://example.com:8080/api')).toBe(true)
    })

    it('should validate URL with fragment', () => {
      expect(isValidUrl('https://example.com/page#section')).toBe(true)
    })

    it('should reject URL without protocol', () => {
      expect(isValidUrl('example.com')).toBe(false)
      expect(isValidUrl('www.example.com')).toBe(false)
    })

    it('should reject invalid URL format', () => {
      expect(isValidUrl('not a url')).toBe(false)
      expect(isValidUrl('http://')).toBe(false)
    })

    it('should reject empty string', () => {
      expect(isValidUrl('')).toBe(false)
    })

    it('should validate FTP URL', () => {
      expect(isValidUrl('ftp://files.example.com')).toBe(true)
    })

    it('should validate localhost URL', () => {
      expect(isValidUrl('http://localhost')).toBe(true)
      expect(isValidUrl('http://127.0.0.1')).toBe(true)
    })

    it('should accept URL constructor valid formats', () => {
      expect(isValidUrl('https:/example.com')).toBe(true)
    })
  })

  describe('isValidPhone', () => {
    it('should validate phone with 10 digits', () => {
      expect(isValidPhone('1234567890')).toBe(true)
    })

    it('should validate phone with country code and spaces', () => {
      expect(isValidPhone('+1 234 567 8900')).toBe(true)
    })

    it('should validate phone with dashes', () => {
      expect(isValidPhone('123-456-7890')).toBe(true)
      expect(isValidPhone('+1-234-567-8900')).toBe(true)
    })

    it('should validate phone with parentheses', () => {
      expect(isValidPhone('(123) 456-7890')).toBe(true)
      expect(isValidPhone('+1 (234) 567-8900')).toBe(true)
    })

    it('should validate phone with 11+ digits', () => {
      expect(isValidPhone('12345678901')).toBe(true)
      expect(isValidPhone('+1 234 567 890012')).toBe(true)
    })

    it('should reject phone with letters', () => {
      expect(isValidPhone('123-456-ABCD')).toBe(false)
      expect(isValidPhone('phone number')).toBe(false)
    })

    it('should reject phone with less than 10 digits', () => {
      expect(isValidPhone('123456789')).toBe(false)
      expect(isValidPhone('12-34-567')).toBe(false)
    })

    it('should reject empty string', () => {
      expect(isValidPhone('')).toBe(false)
    })

    it('should reject phone with special characters', () => {
      expect(isValidPhone('123@456#7890')).toBe(false)
    })

    it('should accept phone with only formatting characters', () => {
      expect(isValidPhone('+1 (234) 567-8900')).toBe(true)
    })

    it('should reject phone with exactly 9 digits', () => {
      expect(isValidPhone('123 456 789')).toBe(false)
    })

    it('should validate international format', () => {
      expect(isValidPhone('+44 20 7946 0958')).toBe(true)
      expect(isValidPhone('+593 2 234 5678')).toBe(true)
    })
  })

  describe('isValidSku', () => {
    it('should validate alphanumeric SKU', () => {
      expect(isValidSku('ABC123')).toBe(true)
      expect(isValidSku('PROD001')).toBe(true)
    })

    it('should validate SKU with dashes', () => {
      expect(isValidSku('ABC-123')).toBe(true)
      expect(isValidSku('PROD-001-XL')).toBe(true)
    })

    it('should validate SKU with underscores', () => {
      expect(isValidSku('ABC_123')).toBe(true)
      expect(isValidSku('PROD_001_XL')).toBe(true)
    })

    it('should validate SKU with mixed separators', () => {
      expect(isValidSku('ABC-123_XL')).toBe(true)
    })

    it('should validate SKU with minimum 3 characters', () => {
      expect(isValidSku('ABC')).toBe(true)
      expect(isValidSku('A12')).toBe(true)
    })

    it('should reject SKU with less than 3 characters', () => {
      expect(isValidSku('AB')).toBe(false)
      expect(isValidSku('A')).toBe(false)
      expect(isValidSku('12')).toBe(false)
    })

    it('should reject empty string', () => {
      expect(isValidSku('')).toBe(false)
    })

    it('should reject SKU with spaces', () => {
      expect(isValidSku('ABC 123')).toBe(false)
      expect(isValidSku('PROD 001')).toBe(false)
    })

    it('should reject SKU with special characters', () => {
      expect(isValidSku('ABC@123')).toBe(false)
      expect(isValidSku('PROD#001')).toBe(false)
      expect(isValidSku('ABC.123')).toBe(false)
    })

    it('should validate long SKUs', () => {
      expect(isValidSku('PRODUCT-CATEGORY-VARIANT-001')).toBe(true)
    })

    it('should validate numeric-only SKU with 3+ digits', () => {
      expect(isValidSku('123')).toBe(true)
      expect(isValidSku('12345')).toBe(true)
    })

    it('should validate letter-only SKU with 3+ characters', () => {
      expect(isValidSku('ABC')).toBe(true)
      expect(isValidSku('PRODUCT')).toBe(true)
    })
  })
})
