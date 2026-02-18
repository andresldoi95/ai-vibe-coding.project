import { beforeEach, describe, expect, it } from 'vitest'
import { mockApiFetch } from '../setup'
import { useCountry } from '~/composables/useCountry'
import type { Country } from '~/types/common'

describe('useCountry', () => {
  beforeEach(() => {
    mockApiFetch.mockReset()
  })

  describe('getAllCountries', () => {
    it('should fetch all countries successfully', async () => {
      const mockCountries: Country[] = [
        {
          id: '1',
          code: 'US',
          name: 'United States',
          alpha3Code: 'USA',
          numericCode: '840',
          isActive: true,
        },
        {
          id: '2',
          code: 'EC',
          name: 'Ecuador',
          alpha3Code: 'ECU',
          numericCode: '218',
          isActive: true,
        },
        {
          id: '3',
          code: 'CA',
          name: 'Canada',
          alpha3Code: 'CAN',
          numericCode: '124',
          isActive: true,
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockCountries, success: true })

      const { getAllCountries, countries, loading, error } = useCountry()

      expect(loading.value).toBe(false)
      expect(error.value).toBeNull()

      await getAllCountries()

      expect(mockApiFetch).toHaveBeenCalledWith('/countries')
      expect(countries.value).toEqual(mockCountries)
      expect(countries.value).toHaveLength(3)
      expect(loading.value).toBe(false)
      expect(error.value).toBeNull()
    })

    it('should set loading state during fetch', async () => {
      const mockCountries: Country[] = [
        {
          id: '1',
          code: 'US',
          name: 'United States',
          isActive: true,
        },
      ]

      let resolvePromise: (value: unknown) => void
      const promise = new Promise((resolve) => {
        resolvePromise = resolve
      })

      mockApiFetch.mockReturnValue(promise)

      const { getAllCountries, loading } = useCountry()

      const fetchPromise = getAllCountries()
      expect(loading.value).toBe(true)

      resolvePromise!({ data: mockCountries, success: true })
      await fetchPromise

      expect(loading.value).toBe(false)
    })

    it('should handle unsuccessful API response', async () => {
      mockApiFetch.mockResolvedValue({
        data: null,
        success: false,
        message: 'Failed to load countries',
      })

      const { getAllCountries, countries, error } = useCountry()

      await getAllCountries()

      expect(countries.value).toEqual([])
      expect(error.value).toBe('Failed to load countries')
    })

    it('should handle API errors', async () => {
      mockApiFetch.mockRejectedValue(new Error('Network error'))

      const { getAllCountries, countries, error } = useCountry()

      await getAllCountries()

      expect(countries.value).toEqual([])
      expect(error.value).toBe('Network error')
    })

    it('should clear previous error on successful fetch', async () => {
      const mockCountries: Country[] = [
        {
          id: '1',
          code: 'US',
          name: 'United States',
          isActive: true,
        },
      ]

      // First call fails
      mockApiFetch.mockRejectedValueOnce(new Error('First error'))

      const { getAllCountries, error } = useCountry()

      await getAllCountries()
      expect(error.value).toBe('First error')

      // Second call succeeds
      mockApiFetch.mockResolvedValue({ data: mockCountries, success: true })
      await getAllCountries()

      expect(error.value).toBeNull()
    })

    it('should handle empty countries list', async () => {
      mockApiFetch.mockResolvedValue({ data: [], success: true })

      const { getAllCountries, countries } = useCountry()

      await getAllCountries()

      expect(countries.value).toEqual([])
      expect(countries.value).toHaveLength(0)
    })
  })

  describe('getCountryById', () => {
    it('should find country by id', async () => {
      const mockCountries: Country[] = [
        {
          id: '1',
          code: 'US',
          name: 'United States',
          isActive: true,
        },
        {
          id: '2',
          code: 'EC',
          name: 'Ecuador',
          isActive: true,
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockCountries, success: true })

      const { getAllCountries, getCountryById } = useCountry()

      await getAllCountries()
      const result = getCountryById('2')

      expect(result).toEqual(mockCountries[1])
      expect(result?.code).toBe('EC')
      expect(result?.name).toBe('Ecuador')
    })

    it('should return undefined for non-existent id', async () => {
      const mockCountries: Country[] = [
        {
          id: '1',
          code: 'US',
          name: 'United States',
          isActive: true,
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockCountries, success: true })

      const { getAllCountries, getCountryById } = useCountry()

      await getAllCountries()
      const result = getCountryById('999')

      expect(result).toBeUndefined()
    })

    it('should return undefined when countries list is empty', () => {
      const { getCountryById } = useCountry()

      const result = getCountryById('1')

      expect(result).toBeUndefined()
    })
  })

  describe('getCountryByCode', () => {
    it('should find country by code', async () => {
      const mockCountries: Country[] = [
        {
          id: '1',
          code: 'US',
          name: 'United States',
          alpha3Code: 'USA',
          isActive: true,
        },
        {
          id: '2',
          code: 'EC',
          name: 'Ecuador',
          alpha3Code: 'ECU',
          isActive: true,
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockCountries, success: true })

      const { getAllCountries, getCountryByCode } = useCountry()

      await getAllCountries()
      const result = getCountryByCode('EC')

      expect(result).toEqual(mockCountries[1])
      expect(result?.id).toBe('2')
      expect(result?.name).toBe('Ecuador')
      expect(result?.alpha3Code).toBe('ECU')
    })

    it('should return undefined for non-existent code', async () => {
      const mockCountries: Country[] = [
        {
          id: '1',
          code: 'US',
          name: 'United States',
          isActive: true,
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockCountries, success: true })

      const { getAllCountries, getCountryByCode } = useCountry()

      await getAllCountries()
      const result = getCountryByCode('XX')

      expect(result).toBeUndefined()
    })

    it('should be case-sensitive for country codes', async () => {
      const mockCountries: Country[] = [
        {
          id: '1',
          code: 'US',
          name: 'United States',
          isActive: true,
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockCountries, success: true })

      const { getAllCountries, getCountryByCode } = useCountry()

      await getAllCountries()
      const result = getCountryByCode('us')

      expect(result).toBeUndefined()
    })
  })

  describe('getCountryOptions', () => {
    it('should format countries for PrimeVue dropdown', async () => {
      const mockCountries: Country[] = [
        {
          id: '1',
          code: 'US',
          name: 'United States',
          isActive: true,
        },
        {
          id: '2',
          code: 'EC',
          name: 'Ecuador',
          isActive: true,
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockCountries, success: true })

      const { getAllCountries, getCountryOptions } = useCountry()

      await getAllCountries()
      const options = getCountryOptions()

      expect(options).toEqual([
        { label: 'United States', value: '1', code: 'US' },
        { label: 'Ecuador', value: '2', code: 'EC' },
      ])
    })

    it('should return empty array when no countries loaded', () => {
      const { getCountryOptions } = useCountry()

      const options = getCountryOptions()

      expect(options).toEqual([])
    })

    it('should preserve country order in options', async () => {
      const mockCountries: Country[] = [
        {
          id: '3',
          code: 'CA',
          name: 'Canada',
          isActive: true,
        },
        {
          id: '1',
          code: 'US',
          name: 'United States',
          isActive: true,
        },
        {
          id: '2',
          code: 'EC',
          name: 'Ecuador',
          isActive: true,
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockCountries, success: true })

      const { getAllCountries, getCountryOptions } = useCountry()

      await getAllCountries()
      const options = getCountryOptions()

      expect(options[0].label).toBe('Canada')
      expect(options[1].label).toBe('United States')
      expect(options[2].label).toBe('Ecuador')
    })

    it('should include code in options for additional context', async () => {
      const mockCountries: Country[] = [
        {
          id: '1',
          code: 'US',
          name: 'United States',
          alpha3Code: 'USA',
          isActive: true,
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockCountries, success: true })

      const { getAllCountries, getCountryOptions } = useCountry()

      await getAllCountries()
      const options = getCountryOptions()

      expect(options[0].code).toBe('US')
    })
  })

  describe('reactive state', () => {
    it('should maintain reactive countries state', async () => {
      const mockCountries1: Country[] = [
        {
          id: '1',
          code: 'US',
          name: 'United States',
          isActive: true,
        },
      ]

      const mockCountries2: Country[] = [
        {
          id: '2',
          code: 'EC',
          name: 'Ecuador',
          isActive: true,
        },
      ]

      mockApiFetch.mockResolvedValueOnce({ data: mockCountries1, success: true })

      const { getAllCountries, countries } = useCountry()

      await getAllCountries()
      expect(countries.value).toEqual(mockCountries1)

      mockApiFetch.mockResolvedValueOnce({ data: mockCountries2, success: true })
      await getAllCountries()

      expect(countries.value).toEqual(mockCountries2)
    })

    it('should maintain reactive error state', async () => {
      mockApiFetch.mockRejectedValueOnce(new Error('First error'))

      const { getAllCountries, error } = useCountry()

      await getAllCountries()
      expect(error.value).toBe('First error')

      mockApiFetch.mockRejectedValueOnce(new Error('Second error'))
      await getAllCountries()

      expect(error.value).toBe('Second error')
    })
  })
})
