import { beforeEach, describe, expect, it } from 'vitest'
import { useStatus } from '~/composables/useStatus'

describe('useStatus', () => {
  beforeEach(() => {
    // Reset any state if needed
  })

  describe('getStatusLabel', () => {
    it('should return active label when status is true', () => {
      const { getStatusLabel } = useStatus()

      const result = getStatusLabel(true)

      expect(result).toBe('common.active')
    })

    it('should return inactive label when status is false', () => {
      const { getStatusLabel } = useStatus()

      const result = getStatusLabel(false)

      expect(result).toBe('common.inactive')
    })

    it('should use i18n translation keys', () => {
      const { getStatusLabel } = useStatus()

      const activeResult = getStatusLabel(true)
      const inactiveResult = getStatusLabel(false)

      expect(activeResult).toContain('common.')
      expect(inactiveResult).toContain('common.')
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

    it('should return only success or danger severities', () => {
      const { getStatusSeverity } = useStatus()

      const trueResult = getStatusSeverity(true)
      const falseResult = getStatusSeverity(false)

      expect(['success', 'danger']).toContain(trueResult)
      expect(['success', 'danger']).toContain(falseResult)
    })
  })

  describe('getStatusBadge', () => {
    it('should return badge configuration for active status', () => {
      const { getStatusBadge } = useStatus()

      const result = getStatusBadge(true)

      expect(result).toEqual({
        label: 'common.active',
        severity: 'success',
      })
    })

    it('should return badge configuration for inactive status', () => {
      const { getStatusBadge } = useStatus()

      const result = getStatusBadge(false)

      expect(result).toEqual({
        label: 'common.inactive',
        severity: 'danger',
      })
    })

    it('should have both label and severity properties', () => {
      const { getStatusBadge } = useStatus()

      const activeBadge = getStatusBadge(true)
      const inactiveBadge = getStatusBadge(false)

      expect(activeBadge).toHaveProperty('label')
      expect(activeBadge).toHaveProperty('severity')
      expect(inactiveBadge).toHaveProperty('label')
      expect(inactiveBadge).toHaveProperty('severity')
    })

    it('should combine label and severity correctly', () => {
      const { getStatusBadge, getStatusLabel, getStatusSeverity } = useStatus()

      const status = true
      const badge = getStatusBadge(status)
      const label = getStatusLabel(status)
      const severity = getStatusSeverity(status)

      expect(badge.label).toBe(label)
      expect(badge.severity).toBe(severity)
    })
  })

  describe('consistency', () => {
    it('should return consistent results across multiple calls', () => {
      const { getStatusLabel, getStatusSeverity, getStatusBadge } = useStatus()

      const label1 = getStatusLabel(true)
      const label2 = getStatusLabel(true)
      const severity1 = getStatusSeverity(false)
      const severity2 = getStatusSeverity(false)
      const badge1 = getStatusBadge(true)
      const badge2 = getStatusBadge(true)

      expect(label1).toBe(label2)
      expect(severity1).toBe(severity2)
      expect(badge1).toEqual(badge2)
    })

    it('should maintain opposite values for true and false', () => {
      const { getStatusLabel, getStatusSeverity } = useStatus()

      const activeLabel = getStatusLabel(true)
      const inactiveLabel = getStatusLabel(false)
      const activeSeverity = getStatusSeverity(true)
      const inactiveSeverity = getStatusSeverity(false)

      expect(activeLabel).not.toBe(inactiveLabel)
      expect(activeSeverity).not.toBe(inactiveSeverity)
    })
  })
})
