import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { nextTick } from '../setup'
import { useFilters } from '~/composables/useFilters'
import type { FilterOptions } from '~/composables/useFilters'

describe('useFilters', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    vi.useFakeTimers()
  })

  afterEach(() => {
    vi.useRealTimers()
  })

  describe('initialization', () => {
    it('should initialize with default filters', () => {
      const initialFilters = { search: '', status: '', category: '' }
      const options: FilterOptions<typeof initialFilters> = {
        initialFilters,
      }

      const { filters } = useFilters(options)

      expect(filters.search).toBe('')
      expect(filters.status).toBe('')
      expect(filters.category).toBe('')
    })

    it('should initialize with provided filter values', () => {
      const initialFilters = { name: 'John', age: 25, active: true }
      const options: FilterOptions<typeof initialFilters> = {
        initialFilters,
      }

      const { filters } = useFilters(options)

      expect(filters.name).toBe('John')
      expect(filters.age).toBe(25)
      expect(filters.active).toBe(true)
    })
  })

  describe('activeFilterCount', () => {
    it('should count active filters', () => {
      const initialFilters = { search: 'test', status: '', category: 'electronics' }
      const options: FilterOptions<typeof initialFilters> = {
        initialFilters,
      }

      const { activeFilterCount } = useFilters(options)

      expect(activeFilterCount.value).toBe(2) // search and category
    })

    it('should not count empty strings', () => {
      const initialFilters = { search: '', status: '', category: '' }
      const options: FilterOptions<typeof initialFilters> = {
        initialFilters,
      }

      const { activeFilterCount } = useFilters(options)

      expect(activeFilterCount.value).toBe(0)
    })

    it('should not count undefined values', () => {
      const initialFilters = { search: undefined, status: undefined }
      const options: FilterOptions<typeof initialFilters> = {
        initialFilters,
      }

      const { activeFilterCount } = useFilters(options)

      expect(activeFilterCount.value).toBe(0)
    })

    it('should not count null values', () => {
      const initialFilters = { search: null, status: null }
      const options: FilterOptions<typeof initialFilters> = {
        initialFilters,
      }

      const { activeFilterCount } = useFilters(options)

      expect(activeFilterCount.value).toBe(0)
    })

    it('should count true boolean values', () => {
      const initialFilters = { active: true, archived: false }
      const options: FilterOptions<typeof initialFilters> = {
        initialFilters,
      }

      const { activeFilterCount } = useFilters(options)

      expect(activeFilterCount.value).toBe(1) // Only active (true)
    })

    it('should count numeric values including zero', () => {
      const initialFilters = { minPrice: 0, maxPrice: 100 }
      const options: FilterOptions<typeof initialFilters> = {
        initialFilters,
      }

      const { activeFilterCount } = useFilters(options)

      expect(activeFilterCount.value).toBe(2) // Both 0 and 100
    })
  })

  describe('hasActiveFilters', () => {
    it('should be true when filters are active', () => {
      const initialFilters = { search: 'test' }
      const options: FilterOptions<typeof initialFilters> = {
        initialFilters,
      }

      const { hasActiveFilters } = useFilters(options)

      expect(hasActiveFilters.value).toBe(true)
    })

    it('should be false when no filters are active', () => {
      const initialFilters = { search: '', status: '' }
      const options: FilterOptions<typeof initialFilters> = {
        initialFilters,
      }

      const { hasActiveFilters } = useFilters(options)

      expect(hasActiveFilters.value).toBe(false)
    })
  })

  describe('setFilter', () => {
    it('should update filter value', () => {
      const initialFilters = { search: '' }
      const options: FilterOptions<typeof initialFilters> = {
        initialFilters,
      }

      const { filters, setFilter } = useFilters(options)

      setFilter('search', 'test query')

      expect(filters.search).toBe('test query')
    })

    it('should trigger onChange without debounce', async () => {
      const onChange = vi.fn()
      const initialFilters = { search: '' }
      const options: FilterOptions<typeof initialFilters> = {
        initialFilters,
        onChange,
        debounceMs: 300,
      }

      const { setFilter } = useFilters(options)

      setFilter('search', 'test')

      expect(onChange).not.toHaveBeenCalled()

      vi.advanceTimersByTime(300)

      expect(onChange).toHaveBeenCalledWith({ search: 'test' })
    })

    it('should debounce multiple rapid changes', () => {
      const onChange = vi.fn()
      const initialFilters = { search: '' }
      const options: FilterOptions<typeof initialFilters> = {
        initialFilters,
        onChange,
        debounceMs: 300,
      }

      const { setFilter } = useFilters(options)

      setFilter('search', 't')
      setFilter('search', 'te')
      setFilter('search', 'tes')
      setFilter('search', 'test')

      vi.advanceTimersByTime(299)
      expect(onChange).not.toHaveBeenCalled()

      vi.advanceTimersByTime(1)
      expect(onChange).toHaveBeenCalledTimes(1)
      expect(onChange).toHaveBeenCalledWith({ search: 'test' })
    })

    it('should update multiple filter fields', () => {
      const initialFilters = { search: '', category: '', status: '' }
      const options: FilterOptions<typeof initialFilters> = {
        initialFilters,
      }

      const { filters, setFilter } = useFilters(options)

      setFilter('search', 'laptop')
      setFilter('category', 'electronics')
      setFilter('status', 'active')

      expect(filters.search).toBe('laptop')
      expect(filters.category).toBe('electronics')
      expect(filters.status).toBe('active')
    })
  })

  describe('applyFilters', () => {
    it('should call onChange callback', () => {
      const onChange = vi.fn()
      const initialFilters = { search: 'test' }
      const options: FilterOptions<typeof initialFilters> = {
        initialFilters,
        onChange,
      }

      const { applyFilters } = useFilters(options)

      applyFilters()

      expect(onChange).toHaveBeenCalledWith({ search: 'test' })
    })

    it('should work without onChange callback', () => {
      const initialFilters = { search: 'test' }
      const options: FilterOptions<typeof initialFilters> = {
        initialFilters,
      }

      const { applyFilters } = useFilters(options)

      expect(() => applyFilters()).not.toThrow()
    })
  })

  describe('resetFilters', () => {
    it('should reset filters to initial values', () => {
      const initialFilters = { search: '', status: '' }
      const options: FilterOptions<typeof initialFilters> = {
        initialFilters,
      }

      const { filters, setFilter, resetFilters } = useFilters(options)

      setFilter('search', 'test')
      setFilter('status', 'active')

      expect(filters.search).toBe('test')
      expect(filters.status).toBe('active')

      resetFilters()

      expect(filters.search).toBe('')
      expect(filters.status).toBe('')
    })

    it('should call onChange after reset', () => {
      const onChange = vi.fn()
      const initialFilters = { search: 'test' }
      const options: FilterOptions<typeof initialFilters> = {
        initialFilters,
        onChange,
      }

      const { resetFilters } = useFilters(options)

      resetFilters()

      expect(onChange).toHaveBeenCalledWith({ search: 'test' })
    })

    it('should reset activeFilterCount', () => {
      const initialFilters = { search: '', status: '' }
      const options: FilterOptions<typeof initialFilters> = {
        initialFilters,
      }

      const { setFilter, resetFilters, activeFilterCount } = useFilters(options)

      setFilter('search', 'test')
      expect(activeFilterCount.value).toBe(1)

      resetFilters()
      expect(activeFilterCount.value).toBe(0)
    })
  })

  describe('automatic change detection', () => {
    it('should automatically call onChange when filters change without debounce', async () => {
      const onChange = vi.fn()
      const initialFilters = { search: '' }
      const options: FilterOptions<typeof initialFilters> = {
        initialFilters,
        onChange,
      }

      const { filters } = useFilters(options)

      filters.search = 'test'
      await nextTick()

      expect(onChange).toHaveBeenCalledWith({ search: 'test' })
    })

    it('should not auto-trigger onChange when debounce is set', async () => {
      const onChange = vi.fn()
      const initialFilters = { search: '' }
      const options: FilterOptions<typeof initialFilters> = {
        initialFilters,
        onChange,
        debounceMs: 300,
      }

      const { filters } = useFilters(options)

      filters.search = 'test'
      await nextTick()

      expect(onChange).not.toHaveBeenCalled()
    })
  })

  describe('reactivity', () => {
    it('should maintain reactivity after setFilter', () => {
      const initialFilters = { search: '', status: '' }
      const options: FilterOptions<typeof initialFilters> = {
        initialFilters,
      }

      const { filters, setFilter, activeFilterCount } = useFilters(options)

      expect(activeFilterCount.value).toBe(0)

      setFilter('search', 'test')

      expect(activeFilterCount.value).toBe(1)
      expect(filters.search).toBe('test')
    })

    it('should maintain reactivity after resetFilters', () => {
      const initialFilters = { search: 'initial', status: 'active' }
      const options: FilterOptions<typeof initialFilters> = {
        initialFilters,
      }

      const { filters, setFilter, resetFilters, activeFilterCount } = useFilters(options)

      expect(activeFilterCount.value).toBe(2)

      setFilter('search', '')
      expect(activeFilterCount.value).toBe(1)

      resetFilters()
      expect(activeFilterCount.value).toBe(2)
      expect(filters.search).toBe('initial')
      expect(filters.status).toBe('active')
    })
  })
})
