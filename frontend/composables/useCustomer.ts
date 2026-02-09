import type { Customer, CustomerFilters } from '~/types/billing'

interface CreateCustomerData {
  name: string
  email: string
  phone?: string
  taxId?: string
  contactPerson?: string
  billingStreet?: string
  billingCity?: string
  billingState?: string
  billingPostalCode?: string
  billingCountry?: string
  shippingStreet?: string
  shippingCity?: string
  shippingState?: string
  shippingPostalCode?: string
  shippingCountry?: string
  notes?: string
  website?: string
  isActive?: boolean
}

interface UpdateCustomerData extends CreateCustomerData {
  id: string
}

interface ApiResponse<T> {
  data: T
  success: boolean
}

export function useCustomer() {
  const { apiFetch } = useApi()

  async function getAllCustomers(filters?: CustomerFilters): Promise<Customer[]> {
    // Build query string from filters
    const params = new URLSearchParams()

    if (filters) {
      if (filters.searchTerm)
        params.append('searchTerm', filters.searchTerm)
      if (filters.name)
        params.append('name', filters.name)
      if (filters.email)
        params.append('email', filters.email)
      if (filters.phone)
        params.append('phone', filters.phone)
      if (filters.taxId)
        params.append('taxId', filters.taxId)
      if (filters.city)
        params.append('city', filters.city)
      if (filters.country)
        params.append('country', filters.country)
      if (filters.isActive !== undefined)
        params.append('isActive', filters.isActive.toString())
    }

    const queryString = params.toString()
    const endpoint = queryString ? `/customers?${queryString}` : '/customers'

    const response = await apiFetch<ApiResponse<Customer[]>>(endpoint, {
      method: 'GET',
    })
    return response.data
  }

  async function getCustomerById(id: string): Promise<Customer> {
    const response = await apiFetch<ApiResponse<Customer>>(`/customers/${id}`, {
      method: 'GET',
    })
    return response.data
  }

  async function createCustomer(data: CreateCustomerData): Promise<Customer> {
    const response = await apiFetch<ApiResponse<Customer>>('/customers', {
      method: 'POST',
      body: data,
    })
    return response.data
  }

  async function updateCustomer(data: UpdateCustomerData): Promise<Customer> {
    const response = await apiFetch<ApiResponse<Customer>>(`/customers/${data.id}`, {
      method: 'PUT',
      body: data,
    })
    return response.data
  }

  async function deleteCustomer(id: string): Promise<void> {
    await apiFetch(`/customers/${id}`, {
      method: 'DELETE',
    })
  }

  return {
    getAllCustomers,
    getCustomerById,
    createCustomer,
    updateCustomer,
    deleteCustomer,
  }
}
