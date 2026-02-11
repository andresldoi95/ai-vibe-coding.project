/**
 * Composable for checking user permissions
 */
export function usePermissions() {
  const authStore = useAuthStore()

  /**
   * Check if user has a specific permission
   */
  const hasPermission = (permission: string): boolean => {
    return authStore.hasPermission(permission)
  }

  /**
   * Check if user has any of the specified permissions
   */
  const hasAnyPermission = (permissions: string[]): boolean => {
    return authStore.hasAnyPermission(permissions)
  }

  /**
   * Check if user has all of the specified permissions
   */
  const hasAllPermissions = (permissions: string[]): boolean => {
    return authStore.hasAllPermissions(permissions)
  }

  /**
   * Permission helpers for common operations
   */
  const can = {
    // Warehouses
    viewWarehouses: () => hasPermission('warehouses.read'),
    createWarehouse: () => hasPermission('warehouses.create'),
    editWarehouse: () => hasPermission('warehouses.update'),
    deleteWarehouse: () => hasPermission('warehouses.delete'),

    // Products
    viewProducts: () => hasPermission('products.read'),
    createProduct: () => hasPermission('products.create'),
    editProduct: () => hasPermission('products.update'),
    deleteProduct: () => hasPermission('products.delete'),

    // Customers
    viewCustomers: () => hasPermission('customers.read'),
    createCustomer: () => hasPermission('customers.create'),
    editCustomer: () => hasPermission('customers.update'),
    deleteCustomer: () => hasPermission('customers.delete'),

    // Stock
    viewStock: () => hasPermission('stock.read'),
    createStock: () => hasPermission('stock.create'),
    editStock: () => hasPermission('stock.update'),
    deleteStock: () => hasPermission('stock.delete'),

    // Tenants
    viewTenants: () => hasPermission('tenants.read'),
    createTenant: () => hasPermission('tenants.create'),
    editTenant: () => hasPermission('tenants.update'),
    deleteTenant: () => hasPermission('tenants.delete'),

    // Users
    viewUsers: () => hasPermission('users.read'),
    createUser: () => hasPermission('users.create'),
    editUser: () => hasPermission('users.update'),
    deleteUser: () => hasPermission('users.delete'),
    inviteUser: () => hasPermission('users.invite'),
    removeUser: () => hasPermission('users.remove'),

    // Roles
    viewRoles: () => hasPermission('roles.read'),
    manageRoles: () => hasPermission('roles.manage'),

    // Tax Rates
    viewTaxRates: () => hasPermission('taxrates.read'),
    readTaxRate: () => hasPermission('taxrates.read'),
    createTaxRate: () => hasPermission('taxrates.create'),
    updateTaxRate: () => hasPermission('taxrates.update'),
    deleteTaxRate: () => hasPermission('taxrates.delete'),

    // Invoices
    viewInvoices: () => hasPermission('invoices.read'),
    readInvoice: () => hasPermission('invoices.read'),
    createInvoice: () => hasPermission('invoices.create'),
    updateInvoice: () => hasPermission('invoices.update'),
    deleteInvoice: () => hasPermission('invoices.delete'),

    // Invoice Configuration
    readInvoiceConfiguration: () => hasPermission('invoiceconfigurations.read'),
    updateInvoiceConfiguration: () => hasPermission('invoiceconfigurations.update'),
  }

  return {
    hasPermission,
    hasAnyPermission,
    hasAllPermissions,
    can,
  }
}
