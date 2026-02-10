import { beforeEach, describe, expect, it, vi } from 'vitest'
import { useI18n } from '../setup'
import { useStatus } from '~/composables/useStatus'

describe('useStatus', () => {
  const mockT = vi.fn()

  beforeEach(() => {
    // Reset all mocks before each test
    mockT.mockReset()
    // Configure useI18n to return our mockT function
    useI18n.mockReturnValue({
      locale: { value: 'en-US' },
      t: mockT,
    })
  })

  describe('getStatusLabel', () => {
    it('should return active label when status is true', () => {
      mockT.mockReturnValue('Active')
      const { getStatusLabel } = useStatus()

      const result = getStatusLabel(true)

      expect(mockT).toHaveBeenCalledWith('common.active')
      expect(result).toBe('Active')
    })

    it('should return inactive label when status is false', () => {
      mockT.mockReturnValue('Inactive')
      const { getStatusLabel } = useStatus()

      const result = getStatusLabel(false)

      expect(mockT).toHaveBeenCalledWith('common.inactive')
      expect(result).toBe('Inactive')
    })
  })

  describe('getStatusSeverity', () => {
    it('should return success severity when status is true', () => {
      const { getStatusSeverity } = useStatus()

      const result = getStatusSeverity(true)

      expect(result).toBe('success')
    })

    it('should return danger severity when status is false', () => {
      const { getStatusSeverity } = useStatus()

      const result = getStatusSeverity(false)

      expect(result).toBe('danger')
    })
  })

  describe('getStatusBadge', () => {
    it('should return badge object with active label and success severity when status is true', () => {
      mockT.mockReturnValue('Active')
      const { getStatusBadge } = useStatus()

      const result = getStatusBadge(true)

      expect(result).toEqual({
        label: 'Active',
        severity: 'success',
      })
      expect(mockT).toHaveBeenCalledWith('common.active')
    })

    it('should return badge object with inactive label and danger severity when status is false', () => {
      mockT.mockReturnValue('Inactive')
      const { getStatusBadge } = useStatus()

      const result = getStatusBadge(false)

      expect(result).toEqual({
        label: 'Inactive',
        severity: 'danger',
      })
      expect(mockT).toHaveBeenCalledWith('common.inactive')
    })
  })
})
