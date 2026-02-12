import type { Country } from '~/types/common'

interface ApiResponse<T> {
  data: T
  success: boolean
  message?: string
}

export function useCountry() {
  const { apiFetch } = useApi()

  const countries = ref<Country[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  /**
   * Get all active countries
   */
  const getAllCountries = async (): Promise<void> => {
    loading.value = true
    error.value = null

    try {
      const response = await apiFetch<ApiResponse<Country[]>>('/countries')

      if (response.success && response.data) {
        countries.value = response.data
      }
      else {
        error.value = response.message || 'Failed to load countries'
      }
    }
    catch (err: any) {
      console.error('Error fetching countries:', err)
      error.value = err.message || 'An error occurred while fetching countries'
    }
    finally {
      loading.value = false
    }
  }

  /**
   * Get country by ID
   */
  const getCountryById = (id: string): Country | undefined => {
    return countries.value.find(c => c.id === id)
  }

  /**
   * Get country by code
   */
  const getCountryByCode = (code: string): Country | undefined => {
    return countries.value.find(c => c.code === code)
  }

  /**
   * Get countries formatted for PrimeVue Dropdown
   */
  const getCountryOptions = () => {
    return countries.value.map(country => ({
      label: country.name,
      value: country.id,
      code: country.code,
    }))
  }

  return {
    countries,
    loading,
    error,
    getAllCountries,
    getCountryById,
    getCountryByCode,
    getCountryOptions,
  }
}
