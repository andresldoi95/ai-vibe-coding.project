export interface IProduct {
  id: string;
  companyId: string;
  code: string;
  barcode?: string;
  name: string;
  description?: string;
  category?: string;
  unit: string; // Unidad: UND, KG, LT, etc.
  price: number;
  cost: number;
  taxTypeId: string;
  hasInventory: boolean;
  minStock?: number;
  maxStock?: number;
  reorderPoint?: number;
  image?: string;
  status: 'active' | 'inactive';
  createdAt: Date;
  updatedAt: Date;
}

export interface ICreateProductDTO {
  code: string;
  barcode?: string;
  name: string;
  description?: string;
  category?: string;
  unit: string;
  price: number;
  cost: number;
  taxTypeId: string;
  hasInventory: boolean;
  minStock?: number;
  maxStock?: number;
  reorderPoint?: number;
  image?: string;
}

export interface IUpdateProductDTO {
  code?: string;
  barcode?: string;
  name?: string;
  description?: string;
  category?: string;
  unit?: string;
  price?: number;
  cost?: number;
  taxTypeId?: string;
  hasInventory?: boolean;
  minStock?: number;
  maxStock?: number;
  reorderPoint?: number;
  image?: string;
  status?: 'active' |'inactive';
}

export interface IProductFilters {
  search?: string;
  category?: string;
  status?: 'active' | 'inactive';
  hasInventory?: boolean;
  page?: number;
  limit?: number;
}

export interface IProductResponse {
  products: IProduct[];
  total: number;
  page: number;
  totalPages: number;
}
