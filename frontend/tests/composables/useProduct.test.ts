import { beforeEach, describe, expect, it } from 'vitest'
import { mockApiFetch } from '../setup'
import { useProduct } from '~/composables/useProduct'
import type { Product } from '~/types/inventory'

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
          unitPrice: 1200.00,
          costPrice: 800.00,
          minimumStockLevel: 10,
          currentStockLevel: 50,
          weight: 2.5,
          dimensions: '35x25x2 cm',
          isActive: true,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
        {
          id: '2',
          name: 'Mouse',
          code: 'PROD-002',
          sku: 'MOU-001',
          description: 'Wireless mouse',
          category: 'Electronics',
          brand: 'TechBrand',
          unitPrice: 25.00,
          costPrice: 15.00,
          minimumStockLevel: 50,
          currentStockLevel: 200,
          weight: 0.1,
          dimensions: '10x5x3 cm',
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

    it('should handle empty product list', async () => {
      mockApiFetch.mockResolvedValue({ data: [], success: true })

      const { getAllProducts } = useProduct()
      const result = await getAllProducts()

      expect(result).toEqual([])
      expect(result).toHaveLength(0)
    })

    it('should apply searchTerm filter', async () => {
      const mockProducts: Product[] = [
        {
          id: '1',
          name: 'Laptop',
          code: 'PROD-001',
          sku: 'LAP-001',
          category: 'Electronics',
          unitPrice: 1200.00,
          costPrice: 800.00,
          minimumStockLevel: 10,
          isActive: true,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockProducts, success: true })

      const { getAllProducts } = useProduct()
      const result = await getAllProducts({ searchTerm: 'laptop' })

      expect(mockApiFetch).toHaveBeenCalledWith('/products?searchTerm=laptop', {
        method: 'GET',
      })
      expect(result).toEqual(mockProducts)
    })

    it('should apply category and brand filters', async () => {
      const mockProducts: Product[] = [
        {
          id: '1',
          name: 'Laptop',
          code: 'PROD-001',
          sku: 'LAP-001',
          category: 'Electronics',
          brand: 'TechBrand',
          unitPrice: 1200.00,
          costPrice: 800.00,
          minimumStockLevel: 10,
          isActive: true,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockProducts, success: true })

      const { getAllProducts } = useProduct()
      const result = await getAllProducts({
        category: 'Electronics',
        brand: 'TechBrand',
      })

      expect(mockApiFetch).toHaveBeenCalledWith('/products?category=Electronics&brand=TechBrand', {
        method: 'GET',
      })
      expect(result).toEqual(mockProducts)
    })

    it('should apply price range filters', async () => {
      const mockProducts: Product[] = [
        {
          id: '1',
          name: 'Laptop',
          code: 'PROD-001',
          sku: 'LAP-001',
          category: 'Electronics',
          unitPrice: 1200.00,
          costPrice: 800.00,
          minimumStockLevel: 10,
          isActive: true,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockProducts, success: true })

      const { getAllProducts } = useProduct()
      const result = await getAllProducts({
        minPrice: 100,
        maxPrice: 2000,
      })

      expect(mockApiFetch).toHaveBeenCalledWith('/products?minPrice=100&maxPrice=2000', {
        method: 'GET',
      })
      expect(result).toEqual(mockProducts)
    })

    it('should apply isActive and lowStock filters', async () => {
      const mockProducts: Product[] = [
        {
          id: '1',
          name: 'Laptop',
          code: 'PROD-001',
          sku: 'LAP-001',
          category: 'Electronics',
          unitPrice: 1200.00,
          costPrice: 800.00,
          minimumStockLevel: 10,
          currentStockLevel: 5,
          isActive: true,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockProducts, success: true })

      const { getAllProducts } = useProduct()
      const result = await getAllProducts({
        isActive: true,
        lowStock: true,
      })

      expect(mockApiFetch).toHaveBeenCalledWith('/products?isActive=true&lowStock=true', {
        method: 'GET',
      })
      expect(result).toEqual(mockProducts)
    })

    it('should apply all filters combined', async () => {
      const mockProducts: Product[] = [
        {
          id: '1',
          name: 'Laptop',
          code: 'PROD-001',
          sku: 'LAP-001',
          category: 'Electronics',
          brand: 'TechBrand',
          unitPrice: 1200.00,
          costPrice: 800.00,
          minimumStockLevel: 10,
          currentStockLevel: 5,
          isActive: true,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockProducts, success: true })

      const { getAllProducts } = useProduct()
      const result = await getAllProducts({
        searchTerm: 'laptop',
        category: 'Electronics',
        brand: 'TechBrand',
        isActive: true,
        minPrice: 100,
        maxPrice: 2000,
        lowStock: true,
      })

      expect(mockApiFetch).toHaveBeenCalledWith(
        '/products?searchTerm=laptop&category=Electronics&brand=TechBrand&isActive=true&minPrice=100&maxPrice=2000&lowStock=true',
        {
          method: 'GET',
        },
      )
      expect(result).toEqual(mockProducts)
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
        unitPrice: 1200.00,
        costPrice: 800.00,
        minimumStockLevel: 10,
        currentStockLevel: 50,
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
  })

  describe('createProduct', () => {
    it('should create a new product successfully', async () => {
      const newProductData = {
        name: 'Keyboard',
        code: 'PROD-003',
        sku: 'KEY-001',
        description: 'Mechanical keyboard',
        category: 'Electronics',
        brand: 'TechBrand',
        unitPrice: 150.00,
        costPrice: 100.00,
        minimumStockLevel: 20,
        currentStockLevel: 100,
        weight: 1.2,
        dimensions: '45x15x3 cm',
        isActive: true,
      }

      const mockCreatedProduct: Product = {
        id: '3',
        ...newProductData,
        createdAt: '2024-01-03T00:00:00Z',
        updatedAt: '2024-01-03T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockCreatedProduct, success: true })

      const { createProduct } = useProduct()
      const result = await createProduct(newProductData)

      expect(mockApiFetch).toHaveBeenCalledWith('/products', {
        method: 'POST',
        body: newProductData,
      })
      expect(result).toEqual(mockCreatedProduct)
      expect(result.id).toBe('3')
      expect(result.name).toBe('Keyboard')
    })

    it('should create a product with minimal required fields', async () => {
      const newProductData = {
        name: 'Monitor',
        code: 'PROD-004',
        sku: 'MON-001',
        unitPrice: 300.00,
        costPrice: 200.00,
        minimumStockLevel: 15,
      }

      const mockCreatedProduct: Product = {
        id: '4',
        ...newProductData,
        isActive: true,
        createdAt: '2024-01-04T00:00:00Z',
        updatedAt: '2024-01-04T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockCreatedProduct, success: true })

      const { createProduct } = useProduct()
      const result = await createProduct(newProductData)

      expect(mockApiFetch).toHaveBeenCalledWith('/products', {
        method: 'POST',
        body: newProductData,
      })
      expect(result).toEqual(mockCreatedProduct)
      expect(result.id).toBe('4')
      expect(result.name).toBe('Monitor')
    })
  })

  describe('updateProduct', () => {
    it('should update an existing product successfully', async () => {
      const updateData = {
        id: '1',
        name: 'Updated Laptop',
        code: 'PROD-001',
        sku: 'LAP-001',
        description: 'Updated high-performance laptop',
        category: 'Electronics',
        brand: 'TechBrand',
        unitPrice: 1400.00,
        costPrice: 900.00,
        minimumStockLevel: 15,
        currentStockLevel: 60,
        weight: 2.5,
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
      expect(result.unitPrice).toBe(1400.00)
    })

    it('should update product status to inactive', async () => {
      const updateData = {
        id: '1',
        name: 'Laptop',
        code: 'PROD-001',
        sku: 'LAP-001',
        unitPrice: 1200.00,
        costPrice: 800.00,
        minimumStockLevel: 10,
        isActive: false,
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
      expect(result.isActive).toBe(false)
    })
  })

  describe('deleteProduct', () => {
    it('should delete a product successfully', async () => {
      mockApiFetch.mockResolvedValue(undefined)

      const { deleteProduct } = useProduct()
      await deleteProduct('1')

      expect(mockApiFetch).toHaveBeenCalledWith('/products/1', {
        method: 'DELETE',
      })
    })
  })

  describe('error handling', () => {
    it('should handle API errors when fetching products', async () => {
      const mockError = new Error('Network error')
      mockApiFetch.mockRejectedValue(mockError)

      const { getAllProducts } = useProduct()

      await expect(getAllProducts()).rejects.toThrow('Network error')
    })

    it('should handle API errors when creating product', async () => {
      const mockError = new Error('Validation error')
      mockApiFetch.mockRejectedValue(mockError)

      const newProductData = {
        name: 'Test Product',
        code: 'PROD-999',
        sku: 'TST-999',
        unitPrice: 100.00,
        costPrice: 50.00,
        minimumStockLevel: 10,
      }

      const { createProduct } = useProduct()

      await expect(createProduct(newProductData)).rejects.toThrow('Validation error')
    })

    it('should handle API errors when updating product', async () => {
      const mockError = new Error('Product not found')
      mockApiFetch.mockRejectedValue(mockError)

      const updateData = {
        id: '999',
        name: 'Non-existent Product',
        code: 'PROD-999',
        sku: 'TST-999',
        unitPrice: 100.00,
        costPrice: 50.00,
        minimumStockLevel: 10,
      }

      const { updateProduct } = useProduct()

      await expect(updateProduct(updateData)).rejects.toThrow('Product not found')
    })

    it('should handle API errors when deleting product', async () => {
      const mockError = new Error('Cannot delete product with existing stock')
      mockApiFetch.mockRejectedValue(mockError)

      const { deleteProduct } = useProduct()

      await expect(deleteProduct('1')).rejects.toThrow('Cannot delete product with existing stock')
    })
  })
})
