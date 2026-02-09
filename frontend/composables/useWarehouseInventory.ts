import type { InventoryLevel } from '~/types/inventory'

export function useWarehouseInventory() {
  const { $apiClient } = useNuxtApp()

  /**
   * Get inventory for a specific product across all warehouses
   */
  const getProductInventory = async (productId: string): Promise<InventoryLevel[]> => {
    const response = await $apiClient<{ data: InventoryLevel[] }>(`/products/${productId}/inventory`)
    return response.data
  }

  /**
   * Calculate total stock for a product across all warehouses
   */
  const getTotalStock = (inventory: InventoryLevel[]): number => {
    return inventory.reduce((total, inv) => total + inv.quantity, 0)
  }

  /**
   * Calculate total available stock for a product across all warehouses
   */
  const getTotalAvailable = (inventory: InventoryLevel[]): number => {
    return inventory.reduce((total, inv) => total + inv.availableQuantity, 0)
  }

  /**
   * Get inventory for a specific warehouse
   */
  const getWarehouseStock = (inventory: InventoryLevel[], warehouseId: string): InventoryLevel | undefined => {
    return inventory.find(inv => inv.warehouseId === warehouseId)
  }

  /**
   * Check if product is low stock (below minimum) in any warehouse
   */
  const isLowStock = (inventory: InventoryLevel[], minimumLevel: number): boolean => {
    const totalStock = getTotalStock(inventory)
    return totalStock < minimumLevel
  }

  return {
    getProductInventory,
    getTotalStock,
    getTotalAvailable,
    getWarehouseStock,
    isLowStock,
  }
}
