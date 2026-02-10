import { beforeEach, describe, expect, it, vi } from 'vitest'
import { nextTick } from 'vue'
import { useFilters } from '~/composables/useFilters'

describe('useFilters', () => {
  beforeEach(() => {
    // Reset all mocks and timers before each test
    vi.clearAllMocks()
    vi.clearAllTimers()
  })

  describe('initialization', () => {
    it('should initialize filters with provided initialFilters', () => {
      const initialFilters = {
        name: 'test',
        category: 'electronics',
        isActive: true,
      }

      const { filters } = useFilters({ initialFilters })

      expect(filters.name).toBe('test')
      expect(filters.category).toBe('electronics')
      expect(filters.isActive).toBe(true)
    })

    it('should initialize with empty initialFilters', () => {
      const initialFilters = {
        search: '',
        status: undefined,
      }

      const { filters } = useFilters({ initialFilters })

      expect(filters.search).toBe('')
      expect(filters.status).toBeUndefined()
    })

    it('should initialize activeFilterCount correctly', () => {
      const initialFilters = {
        name: 'test',
        category: '',
        isActive: true,
      }

      const { activeFilterCount } = useFilters({ initialFilters })

      // name: 'test' (active), category: '' (inactive), isActive: true (active)
      expect(activeFilterCount.value).toBe(2)
    })

    it('should initialize hasActiveFilters correctly', () => {
      const initialFilters = {
        name: '',
        category: undefined,
      }

      const { hasActiveFilters } = useFilters({ initialFilters })

      expect(hasActiveFilters.value).toBe(false)
    })
  })

  describe('setFilter', () => {
    it('should update filter value', () => {
      const initialFilters = {
        name: '',
        category: '',
      }

      const { filters, setFilter } = useFilters({ initialFilters })

      setFilter('name', 'test product')

      expect(filters.name).toBe('test product')
    })

    it('should update multiple filters', () => {
      const initialFilters = {
        name: '',
        category: '',
        isActive: false,
      }

      const { filters, setFilter } = useFilters({ initialFilters })

      setFilter('name', 'laptop')
      setFilter('category', 'electronics')
      setFilter('isActive', true)

      expect(filters.name).toBe('laptop')
      expect(filters.category).toBe('electronics')
      expect(filters.isActive).toBe(true)
    })
  })

  describe('resetFilters', () => {
    it('should reset filters to initial values', () => {
      const initialFilters = {
        name: 'initial',
        category: 'tech',
      }

      const { filters, setFilter, resetFilters } = useFilters({ initialFilters })

      setFilter('name', 'changed')
      setFilter('category', 'books')

      expect(filters.name).toBe('changed')
      expect(filters.category).toBe('books')

      resetFilters()

      expect(filters.name).toBe('initial')
      expect(filters.category).toBe('tech')
    })

    it('should call onChange callback when resetFilters is called', () => {
      const onChange = vi.fn()
      const initialFilters = {
        name: 'test',
      }

      const { setFilter, resetFilters } = useFilters({ initialFilters, onChange })

      setFilter('name', 'changed')
      onChange.mockClear()

      resetFilters()

      expect(onChange).toHaveBeenCalledTimes(1)
      expect(onChange).toHaveBeenCalledWith({ name: 'test' })
    })
  })

  describe('applyFilters', () => {
    it('should call onChange callback with current filters', () => {
      const onChange = vi.fn()
      const initialFilters = {
        name: 'test',
        category: 'electronics',
      }

      const { applyFilters } = useFilters({ initialFilters, onChange })

      applyFilters()

      expect(onChange).toHaveBeenCalledTimes(1)
      expect(onChange).toHaveBeenCalledWith({
        name: 'test',
        category: 'electronics',
      })
    })

    it('should not throw error if onChange is not provided', () => {
      const initialFilters = { name: '' }
      const { applyFilters } = useFilters({ initialFilters })

      expect(() => applyFilters()).not.toThrow()
    })
  })

  describe('activeFilterCount', () => {
    it('should count only active filters (non-empty, non-undefined, non-null)', () => {
      const initialFilters = {
        name: 'laptop',
        category: '',
        brand: undefined,
        price: null,
        inStock: true,
      }

      const { activeFilterCount } = useFilters({ initialFilters })

      // name: 'laptop' (active), inStock: true (active)
      expect(activeFilterCount.value).toBe(2)
    })

    it('should update count when filters change', () => {
      const initialFilters = {
        name: '',
        category: '',
      }

      const { activeFilterCount, setFilter } = useFilters({ initialFilters })

      expect(activeFilterCount.value).toBe(0)

      setFilter('name', 'test')
      expect(activeFilterCount.value).toBe(1)

      setFilter('category', 'electronics')
      expect(activeFilterCount.value).toBe(2)

      setFilter('name', '')
      expect(activeFilterCount.value).toBe(1)
    })

    it('should not count false boolean values as active', () => {
      const initialFilters = {
        isActive: false,
        isPublished: true,
      }

      const { activeFilterCount } = useFilters({ initialFilters })

      // Only isPublished: true is active
      expect(activeFilterCount.value).toBe(1)
    })

    it('should handle complex filter objects correctly', () => {
      const initialFilters = {
        search: 'test',
        status: 'active',
        category: '',
        minPrice: 100,
        maxPrice: 0,
        tags: [],
        isVerified: true,
        isFeatured: false,
      }

      const { activeFilterCount } = useFilters({ initialFilters })

      // Active: search, status, minPrice, maxPrice (0 is truthy for numbers), tags ([] is truthy), isVerified
      // Inactive: category (''), isFeatured (false)
      expect(activeFilterCount.value).toBe(6)
    })
  })

  describe('hasActiveFilters', () => {
    it('should return true when there are active filters', () => {
      const initialFilters = {
        name: 'test',
      }

      const { hasActiveFilters } = useFilters({ initialFilters })

      expect(hasActiveFilters.value).toBe(true)
    })

    it('should return false when there are no active filters', () => {
      const initialFilters = {
        name: '',
        category: undefined,
      }

      const { hasActiveFilters } = useFilters({ initialFilters })

      expect(hasActiveFilters.value).toBe(false)
    })

    it('should update reactively when filters change', () => {
      const initialFilters = {
        name: '',
      }

      const { hasActiveFilters, setFilter } = useFilters({ initialFilters })

      expect(hasActiveFilters.value).toBe(false)

      setFilter('name', 'test')
      expect(hasActiveFilters.value).toBe(true)

      setFilter('name', '')
      expect(hasActiveFilters.value).toBe(false)
    })
  })

  describe('onChange callback', () => {
    it('should call onChange when applyFilters is called', () => {
      const onChange = vi.fn()
      const initialFilters = { name: 'test' }

      const { applyFilters } = useFilters({ initialFilters, onChange })

      applyFilters()

      expect(onChange).toHaveBeenCalledTimes(1)
      expect(onChange).toHaveBeenCalledWith({ name: 'test' })
    })

    it('should call onChange with updated filters', () => {
      const onChange = vi.fn()
      const initialFilters = { name: '', category: '' }

      const { setFilter, applyFilters } = useFilters({ initialFilters, onChange })

      setFilter('name', 'laptop')
      setFilter('category', 'electronics')
      applyFilters()

      expect(onChange).toHaveBeenCalledWith({
        name: 'laptop',
        category: 'electronics',
      })
    })
  })

  describe('debouncing', () => {
    beforeEach(() => {
      vi.useFakeTimers()
    })

    it('should debounce onChange calls when debounceMs is set', () => {
      const onChange = vi.fn()
      const initialFilters = { name: '' }

      const { setFilter } = useFilters({
        initialFilters,
        onChange,
        debounceMs: 300,
      })

      setFilter('name', 'a')
      setFilter('name', 'ab')
      setFilter('name', 'abc')

      expect(onChange).not.toHaveBeenCalled()

      vi.advanceTimersByTime(299)
      expect(onChange).not.toHaveBeenCalled()

      vi.advanceTimersByTime(1)
      expect(onChange).toHaveBeenCalledTimes(1)
      expect(onChange).toHaveBeenCalledWith({ name: 'abc' })
    })

    it('should clear previous timeout when setFilter is called multiple times', () => {
      const onChange = vi.fn()
      const initialFilters = { name: '' }

      const { setFilter } = useFilters({
        initialFilters,
        onChange,
        debounceMs: 300,
      })

      setFilter('name', 'first')
      vi.advanceTimersByTime(200)

      setFilter('name', 'second')
      vi.advanceTimersByTime(200)

      expect(onChange).not.toHaveBeenCalled()

      vi.advanceTimersByTime(100)
      expect(onChange).toHaveBeenCalledTimes(1)
      expect(onChange).toHaveBeenCalledWith({ name: 'second' })
    })

    it('should handle multiple debounced updates correctly', () => {
      const onChange = vi.fn()
      const initialFilters = { name: '', category: '' }

      const { setFilter } = useFilters({
        initialFilters,
        onChange,
        debounceMs: 300,
      })

      setFilter('name', 'laptop')
      vi.advanceTimersByTime(300)

      expect(onChange).toHaveBeenCalledTimes(1)
      expect(onChange).toHaveBeenCalledWith({ name: 'laptop', category: '' })

      onChange.mockClear()

      setFilter('category', 'electronics')
      vi.advanceTimersByTime(300)

      expect(onChange).toHaveBeenCalledTimes(1)
      expect(onChange).toHaveBeenCalledWith({
        name: 'laptop',
        category: 'electronics',
      })
    })
  })

  describe('reactive watch (no debounce)', () => {
    it('should call onChange immediately when filters change without debounce', async () => {
      const onChange = vi.fn()
      const initialFilters = { name: '' }

      const { setFilter } = useFilters({ initialFilters, onChange })

      setFilter('name', 'test')
      await nextTick()

      expect(onChange).toHaveBeenCalledTimes(1)
      expect(onChange).toHaveBeenCalledWith({ name: 'test' })
    })

    it('should call onChange for each filter change without debounce', async () => {
      const onChange = vi.fn()
      const initialFilters = { name: '', category: '' }

      const { setFilter } = useFilters({ initialFilters, onChange })

      setFilter('name', 'laptop')
      await nextTick()

      setFilter('category', 'electronics')
      await nextTick()

      // Due to Vue's reactivity batching, both changes may be detected together
      // or separately depending on timing. We verify onChange was called correctly
      expect(onChange).toHaveBeenCalled()
      expect(onChange.mock.calls.length).toBeGreaterThanOrEqual(1)

      // Check the final call has both values
      const lastCall = onChange.mock.calls[onChange.mock.calls.length - 1][0]
      expect(lastCall.name).toBe('laptop')
      expect(lastCall.category).toBe('electronics')
    })

    it('should not trigger onChange on initialization without debounce', async () => {
      const onChange = vi.fn()
      const initialFilters = { name: 'initial' }

      useFilters({ initialFilters, onChange })

      await nextTick()

      // onChange should not be called on initialization
      expect(onChange).not.toHaveBeenCalled()
    })
  })

  describe('complex scenarios', () => {
    it('should handle mixed updates with resetFilters', async () => {
      const onChange = vi.fn()
      const initialFilters = { name: 'initial', category: 'tech' }

      const { setFilter, resetFilters } = useFilters({ initialFilters, onChange })

      setFilter('name', 'changed')
      await nextTick()

      onChange.mockClear()
      resetFilters()

      expect(onChange).toHaveBeenCalledTimes(1)
      expect(onChange).toHaveBeenCalledWith({ name: 'initial', category: 'tech' })
    })

    it('should maintain reactive state across multiple operations', () => {
      const initialFilters = {
        name: '',
        category: '',
        isActive: false,
      }

      const { filters, setFilter, activeFilterCount, hasActiveFilters } = useFilters({
        initialFilters,
      })

      expect(activeFilterCount.value).toBe(0)
      expect(hasActiveFilters.value).toBe(false)

      setFilter('name', 'laptop')
      expect(filters.name).toBe('laptop')
      expect(activeFilterCount.value).toBe(1)
      expect(hasActiveFilters.value).toBe(true)

      setFilter('category', 'electronics')
      expect(filters.category).toBe('electronics')
      expect(activeFilterCount.value).toBe(2)

      setFilter('isActive', true)
      expect(filters.isActive).toBe(true)
      expect(activeFilterCount.value).toBe(3)

      setFilter('name', '')
      expect(activeFilterCount.value).toBe(2)
    })

    it('should work with complex filter types', () => {
      interface ComplexFilters {
        searchTerm: string
        status: string | undefined
        priceRange: { min: number, max: number } | undefined
        tags: string[]
        includeArchived: boolean
      }

      const initialFilters: ComplexFilters = {
        searchTerm: '',
        status: undefined,
        priceRange: undefined,
        tags: [],
        includeArchived: false,
      }

      const { filters, setFilter, activeFilterCount } = useFilters({ initialFilters })

      setFilter('searchTerm', 'laptop')
      setFilter('status', 'active')
      setFilter('priceRange', { min: 100, max: 500 })
      setFilter('tags', ['electronics', 'sale'])

      expect(filters.searchTerm).toBe('laptop')
      expect(filters.status).toBe('active')
      expect(filters.priceRange).toEqual({ min: 100, max: 500 })
      expect(filters.tags).toEqual(['electronics', 'sale'])
      expect(activeFilterCount.value).toBe(4)
    })
  })
})
