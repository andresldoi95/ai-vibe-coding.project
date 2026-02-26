import type { Product, ProductFilters } from '~/types/inventory'

interface CreateProductData {
  name: string
  code: string
  sku: string
  description?: string
  category?: string
  brand?: string
  unitPrice: number
  costPrice: number
  minimumStockLevel: number
  currentStockLevel?: number
  weight?: number
  dimensions?: string
  isActive?: boolean
  initialQuantity?: number
  initialWarehouseId?: string
}

interface UpdateProductData extends CreateProductData {
  id: string
}

interface ApiResponse<T> {
  data: T
  success: boolean
}

export function useProduct() {
  const { apiFetch } = useApi()

  async function getAllProducts(filters?: ProductFilters): Promise<Product[]> {
    // Build query string from filters
    const params = new URLSearchParams()

    if (filters) {
      if (filters.searchTerm)
        params.append('searchTerm', filters.searchTerm)
      if (filters.category)
        params.append('category', filters.category)
      if (filters.brand)
        params.append('brand', filters.brand)
      if (filters.isActive !== undefined)
        params.append('isActive', filters.isActive.toString())
      if (filters.minPrice !== undefined)
        params.append('minPrice', filters.minPrice.toString())
      if (filters.maxPrice !== undefined)
        params.append('maxPrice', filters.maxPrice.toString())
      if (filters.lowStock !== undefined)
        params.append('lowStock', filters.lowStock.toString())
    }

    const queryString = params.toString()
    const endpoint = queryString ? `/products?${queryString}` : '/products'

    const response = await apiFetch<ApiResponse<Product[]>>(endpoint, {
      method: 'GET',
    })
    return response.data
  }

  async function getProductById(id: string): Promise<Product> {
    const response = await apiFetch<ApiResponse<Product>>(`/products/${id}`, {
      method: 'GET',
    })
    return response.data
  }

  async function createProduct(data: CreateProductData): Promise<Product> {
    const response = await apiFetch<ApiResponse<Product>>('/products', {
      method: 'POST',
      body: data,
    })
    return response.data
  }

  async function updateProduct(data: UpdateProductData): Promise<Product> {
    const response = await apiFetch<ApiResponse<Product>>(`/products/${data.id}`, {
      method: 'PUT',
      body: data,
    })
    return response.data
  }

  async function deleteProduct(id: string): Promise<void> {
    await apiFetch(`/products/${id}`, {
      method: 'DELETE',
    })
  }

  return {
    getAllProducts,
    getProductById,
    createProduct,
    updateProduct,
    deleteProduct,
  }
}
