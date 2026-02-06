import apiClient from './client';
import type { 
  IProduct, 
  ICreateProductDTO, 
  IUpdateProductDTO, 
  IProductFilters, 
  IProductResponse 
} from '../types';

export const productsAPI = {
  /**
   * Get all products with filters
   */
  async getProducts(filters?: IProductFilters): Promise<IProductResponse> {
    const params = new URLSearchParams();
    
    if (filters?.search) params.append('search', filters.search);
    if (filters?.category) params.append('category', filters.category);
    if (filters?.status) params.append('status', filters.status);
    if (filters?.hasInventory !== undefined) params.append('hasInventory', String(filters.hasInventory));
    if (filters?.page) params.append('page', String(filters.page));
    if (filters?.limit) params.append('limit', String(filters.limit));

    const { data } = await apiClient.get<IProductResponse>(`/products?${params.toString()}`);
    return data;
  },

  /**
   * Get product by ID
   */
  async getProductById(id: string): Promise<IProduct> {
    const { data } = await apiClient.get<IProduct>(`/products/${id}`);
    return data;
  },

  /**
   * Create new product
   */
  async createProduct(product: ICreateProductDTO): Promise<IProduct> {
    const { data } = await apiClient.post<IProduct>('/products', product);
    return data;
  },

  /**
   * Update product
   */
  async updateProduct(id: string, product: IUpdateProductDTO): Promise<IProduct> {
    const { data } = await apiClient.put<IProduct>(`/products/${id}`, product);
    return data;
  },

  /**
   * Delete product
   */
  async deleteProduct(id: string): Promise<void> {
    await apiClient.delete(`/products/${id}`);
  },

  /**
   * Get product categories
   */
  async getCategories(): Promise<string[]> {
    const { data } = await apiClient.get<{ categories: string[] }>('/products/categories');
    return data.categories;
  }
};
