import { beforeEach, describe, expect, it } from 'vitest'
import { mockApiFetch } from '../setup'
import { useProduct } from '~/composables/useProduct'
import type { Product, ProductFilters } from '~/types/inventory'

describe('useProduct', () => {
  beforeEach(() => {
    // Reset all mocks before each test
    mockApiFetch.mockReset()
  })

  describe('getAllProducts', () => {
    it('should fetch all products successfully', async () => {
      const mockProducts: Product[] = [
        {
          id: '1',
          name: 'Laptop',
          code: 'PROD-001',
          sku: 'LAP-001',
          description: 'High-performance laptop',
          category: 'Electronics',
          brand: 'TechBrand',
          unitPrice: 999.99,
          costPrice: 750.00,
          minimumStockLevel: 5,
          currentStockLevel: 20,
          weight: 2.5,
          dimensions: '35x25x2 cm',
          isActive: true,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
        {
          id: '2',
          name: 'Wireless Mouse',
          code: 'PROD-002',
          sku: 'MOU-001',
          description: 'Ergonomic wireless mouse',
          category: 'Accessories',
          brand: 'TechBrand',
          unitPrice: 29.99,
          costPrice: 15.00,
          minimumStockLevel: 10,
          currentStockLevel: 50,
          isActive: true,
          createdAt: '2024-01-02T00:00:00Z',
          updatedAt: '2024-01-02T00:00:00Z',
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockProducts, success: true })

      const { getAllProducts } = useProduct()
      const result = await getAllProducts()

      expect(mockApiFetch).toHaveBeenCalledWith('/products', {
        method: 'GET',
      })
      expect(result).toEqual(mockProducts)
      expect(result).toHaveLength(2)
    })

    it('should fetch products with filters', async () => {
      const filters: ProductFilters = {
        searchTerm: 'laptop',
        category: 'Electronics',
        brand: 'TechBrand',
        isActive: true,
        minPrice: 500,
        maxPrice: 1500,
        lowStock: false,
      }

      const mockProducts: Product[] = [
        {
          id: '1',
          name: 'Laptop',
          code: 'PROD-001',
          sku: 'LAP-001',
          unitPrice: 999.99,
          costPrice: 750.00,
          minimumStockLevel: 5,
          isActive: true,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockProducts, success: true })

      const { getAllProducts } = useProduct()
      const result = await getAllProducts(filters)

      expect(mockApiFetch).toHaveBeenCalledWith(
        '/products?searchTerm=laptop&category=Electronics&brand=TechBrand&isActive=true&minPrice=500&maxPrice=1500&lowStock=false',
        {
          method: 'GET',
        },
      )
      expect(result).toEqual(mockProducts)
      expect(result).toHaveLength(1)
    })

    it('should fetch products with partial filters', async () => {
      const filters: ProductFilters = {
        category: 'Electronics',
        isActive: true,
      }

      const mockProducts: Product[] = []

      mockApiFetch.mockResolvedValue({ data: mockProducts, success: true })

      const { getAllProducts } = useProduct()
      const result = await getAllProducts(filters)

      expect(mockApiFetch).toHaveBeenCalledWith(
        '/products?category=Electronics&isActive=true',
        {
          method: 'GET',
        },
      )
      expect(result).toEqual([])
    })

    it('should handle empty product list', async () => {
      mockApiFetch.mockResolvedValue({ data: [], success: true })

      const { getAllProducts } = useProduct()
      const result = await getAllProducts()

      expect(result).toEqual([])
      expect(result).toHaveLength(0)
    })

    it('should handle API errors when fetching products', async () => {
      mockApiFetch.mockRejectedValue(new Error('Network error'))

      const { getAllProducts } = useProduct()

      await expect(getAllProducts()).rejects.toThrow('Network error')
    })
  })

  describe('getProductById', () => {
    it('should fetch a product by id successfully', async () => {
      const mockProduct: Product = {
        id: '1',
        name: 'Laptop',
        code: 'PROD-001',
        sku: 'LAP-001',
        description: 'High-performance laptop',
        category: 'Electronics',
        brand: 'TechBrand',
        unitPrice: 999.99,
        costPrice: 750.00,
        minimumStockLevel: 5,
        currentStockLevel: 20,
        weight: 2.5,
        dimensions: '35x25x2 cm',
        isActive: true,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockProduct, success: true })

      const { getProductById } = useProduct()
      const result = await getProductById('1')

      expect(mockApiFetch).toHaveBeenCalledWith('/products/1', {
        method: 'GET',
      })
      expect(result).toEqual(mockProduct)
      expect(result.id).toBe('1')
      expect(result.name).toBe('Laptop')
    })

    it('should handle API errors when fetching product by id', async () => {
      mockApiFetch.mockRejectedValue(new Error('Product not found'))

      const { getProductById } = useProduct()

      await expect(getProductById('999')).rejects.toThrow('Product not found')
    })
  })

  describe('createProduct', () => {
    it('should create a product successfully', async () => {
      const createData = {
        name: 'New Product',
        code: 'PROD-003',
        sku: 'NEW-001',
        description: 'A new product',
        category: 'Electronics',
        brand: 'NewBrand',
        unitPrice: 199.99,
        costPrice: 100.00,
        minimumStockLevel: 10,
        currentStockLevel: 25,
        weight: 1.5,
        dimensions: '20x15x5 cm',
        isActive: true,
      }

      const mockCreatedProduct: Product = {
        id: '3',
        ...createData,
        createdAt: '2024-01-03T00:00:00Z',
        updatedAt: '2024-01-03T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockCreatedProduct, success: true })

      const { createProduct } = useProduct()
      const result = await createProduct(createData)

      expect(mockApiFetch).toHaveBeenCalledWith('/products', {
        method: 'POST',
        body: createData,
      })
      expect(result).toEqual(mockCreatedProduct)
      expect(result.id).toBe('3')
      expect(result.name).toBe('New Product')
    })

    it('should create a product with minimal data', async () => {
      const createData = {
        name: 'Minimal Product',
        code: 'PROD-004',
        sku: 'MIN-001',
        unitPrice: 50.00,
        costPrice: 25.00,
        minimumStockLevel: 5,
      }

      const mockCreatedProduct: Product = {
        id: '4',
        name: 'Minimal Product',
        code: 'PROD-004',
        sku: 'MIN-001',
        unitPrice: 50.00,
        costPrice: 25.00,
        minimumStockLevel: 5,
        isActive: true,
        createdAt: '2024-01-04T00:00:00Z',
        updatedAt: '2024-01-04T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockCreatedProduct, success: true })

      const { createProduct } = useProduct()
      const result = await createProduct(createData)

      expect(mockApiFetch).toHaveBeenCalledWith('/products', {
        method: 'POST',
        body: createData,
      })
      expect(result).toEqual(mockCreatedProduct)
    })

    it('should handle API errors when creating product', async () => {
      const createData = {
        name: 'Invalid Product',
        code: 'PROD-999',
        sku: 'INV-001',
        unitPrice: 100.00,
        costPrice: 50.00,
        minimumStockLevel: 5,
      }

      mockApiFetch.mockRejectedValue(new Error('Validation error'))

      const { createProduct } = useProduct()

      await expect(createProduct(createData)).rejects.toThrow('Validation error')
    })
  })

  describe('updateProduct', () => {
    it('should update a product successfully', async () => {
      const updateData = {
        id: '1',
        name: 'Updated Laptop',
        code: 'PROD-001',
        sku: 'LAP-001',
        description: 'Updated high-performance laptop',
        category: 'Electronics',
        brand: 'TechBrand',
        unitPrice: 899.99,
        costPrice: 650.00,
        minimumStockLevel: 5,
        currentStockLevel: 15,
        weight: 2.3,
        dimensions: '35x25x2 cm',
        isActive: true,
      }

      const mockUpdatedProduct: Product = {
        ...updateData,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-05T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockUpdatedProduct, success: true })

      const { updateProduct } = useProduct()
      const result = await updateProduct(updateData)

      expect(mockApiFetch).toHaveBeenCalledWith('/products/1', {
        method: 'PUT',
        body: updateData,
      })
      expect(result).toEqual(mockUpdatedProduct)
      expect(result.name).toBe('Updated Laptop')
      expect(result.unitPrice).toBe(899.99)
    })

    it('should handle API errors when updating product', async () => {
      const updateData = {
        id: '999',
        name: 'Non-existent Product',
        code: 'PROD-999',
        sku: 'NON-001',
        unitPrice: 100.00,
        costPrice: 50.00,
        minimumStockLevel: 5,
      }

      mockApiFetch.mockRejectedValue(new Error('Product not found'))

      const { updateProduct } = useProduct()

      await expect(updateProduct(updateData)).rejects.toThrow('Product not found')
    })
  })

  describe('deleteProduct', () => {
    it('should delete a product successfully', async () => {
      mockApiFetch.mockResolvedValue({ success: true })

      const { deleteProduct } = useProduct()
      await deleteProduct('1')

      expect(mockApiFetch).toHaveBeenCalledWith('/products/1', {
        method: 'DELETE',
      })
    })

    it('should handle API errors when deleting product', async () => {
      mockApiFetch.mockRejectedValue(new Error('Product has dependencies'))

      const { deleteProduct } = useProduct()

      await expect(deleteProduct('1')).rejects.toThrow('Product has dependencies')
    })
  })
})
