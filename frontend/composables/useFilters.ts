/**
 * Composable for managing filter state and operations
 * Eliminates repetitive filter management code
 */

import type { Ref } from 'vue'

export interface FilterOptions<T> {
  /**
   * Initial filter values
   */
  initialFilters: T

  /**
   * Callback when filters change
   */
  onChange?: (filters: T) => void

  /**
   * Debounce delay in milliseconds
   */
  debounceMs?: number
}

export interface FilterState<T> {
  filters: T
  activeFilterCount: Ref<number>
  hasActiveFilters: Ref<boolean>
  applyFilters: () => void
  resetFilters: () => void
  setFilter: <K extends keyof T>(key: K, value: T[K]) => void
}

export function useFilters<T extends Record<string, unknown>>(
  options: FilterOptions<T>,
): FilterState<T> {
  const filters = reactive<T>({ ...options.initialFilters })

  const debounceTimeout = ref<NodeJS.Timeout>()

  // Count active filters (non-empty, non-undefined, non-false values)
  const activeFilterCount = computed(() => {
    return Object.entries(filters).filter(([_, value]) => {
      if (value === undefined || value === null || value === '')
        return false
      if (typeof value === 'boolean')
        return value === true
      return true
    }).length
  })

  const hasActiveFilters = computed(() => activeFilterCount.value > 0)

  function applyFilters() {
    if (options.onChange) {
      options.onChange(filters as T)
    }
  }

  function resetFilters() {
    Object.assign(filters, options.initialFilters)
    applyFilters()
  }

  function setFilter<K extends keyof T>(key: K, value: T[K]) {
    filters[key] = value

    if (options.debounceMs) {
      if (debounceTimeout.value) {
        clearTimeout(debounceTimeout.value)
      }
      debounceTimeout.value = setTimeout(() => {
        applyFilters()
      }, options.debounceMs)
    }
  }

  // Watch for changes if debounce is not set
  if (!options.debounceMs) {
    watch(
      () => ({ ...filters }),
      () => applyFilters(),
      { deep: true },
    )
  }

  return {
    filters,
    activeFilterCount,
    hasActiveFilters,
    applyFilters,
    resetFilters,
    setFilter,
  }
}
