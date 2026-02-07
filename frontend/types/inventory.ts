// Inventory module types
export interface Product {
  id: string
  tenantId: string
  name: string
  sku: string
  description?: string
  category: string
  unitPrice: number
  costPrice: number
  taxRate: number
  stockQuantity: number
  reorderPoint: number
  isActive: boolean
  createdAt: string
  updatedAt: string
}

export interface Warehouse {
  id: string
  tenantId: string
  name: string
  code: string
  description?: string
  streetAddress: string
  city: string
  state?: string
  postalCode: string
  country: string
  phone?: string
  email?: string
  isActive: boolean
  squareFootage?: number
  capacity?: number
  createdAt: string
  updatedAt: string
}

export interface StockMovement {
  id: string
  tenantId: string
  productId: string
  warehouseId: string
  movementType: MovementType
  quantity: number
  referenceNumber?: string
  notes?: string
  createdDate: string
  createdAt: string
  updatedAt: string
}

export type MovementType = 'in' | 'out' | 'transfer' | 'adjustment'

export interface InventoryLevel {
  productId: string
  warehouseId: string
  quantity: number
  reservedQuantity: number
  availableQuantity: number
  lastUpdated: string
}

export interface Supplier {
  id: string
  tenantId: string
  name: string
  email: string
  phone?: string
  address?: string
  isActive: boolean
  createdAt: string
  updatedAt: string
}

export interface PurchaseOrder {
  id: string
  tenantId: string
  orderNumber: string
  supplierId: string
  status: PurchaseOrderStatus
  totalAmount: number
  orderDate: string
  expectedDeliveryDate?: string
  receivedDate?: string
  createdAt: string
  updatedAt: string
  items: PurchaseOrderItem[]
}

export type PurchaseOrderStatus = 'draft' | 'sent' | 'received' | 'cancelled'

export interface PurchaseOrderItem {
  id: string
  purchaseOrderId: string
  productId: string
  quantity: number
  unitPrice: number
  totalPrice: number
  receivedQuantity: number
}
