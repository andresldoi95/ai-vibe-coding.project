/**
 * Composable for status label and severity formatting
 * Eliminates duplicate status helper functions
 */

export function useStatus() {
  const { t } = useI18n()
  
  /**
   * Get human-readable status label
   */
  function getStatusLabel(isActive: boolean): string {
    return isActive ? t('common.active') : t('common.inactive')
  }
  
  /**
   * Get PrimeVue severity for status tag
   */
  function getStatusSeverity(isActive: boolean): 'success' | 'danger' {
    return isActive ? 'success' : 'danger'
  }
  
  /**
   * Get status badge configuration
   */
  function getStatusBadge(isActive: boolean) {
    return {
      label: getStatusLabel(isActive),
      severity: getStatusSeverity(isActive),
    }
  }
  
  return {
    getStatusLabel,
    getStatusSeverity,
    getStatusBadge,
  }
}
