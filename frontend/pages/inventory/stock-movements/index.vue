<script setup lang="ts">
import type { Product, StockMovement, Warehouse } from '~/types/inventory'
import { MovementType, MovementTypeLabels } from '~/types/inventory'

definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const { getAllStockMovements, deleteStockMovement, exportStockMovements } = useStockMovement()
const { getAllProducts } = useProduct()
const { getAllWarehouses } = useWarehouse()
const { can } = usePermissions()

// For displaying product and warehouse names
const products = ref<Product[]>([])
const warehouses = ref<Warehouse[]>([])

// Custom load function that loads movements, products, and warehouses
async function loadAllData() {
  const [movementsData, productsData, warehousesData] = await Promise.all([
    getAllStockMovements(),
    getAllProducts(),
    getAllWarehouses(),
  ])
  products.value = productsData
  warehouses.value = warehousesData
  return movementsData
}

// ✅ Using useCrudPage composable with custom load function
const {
  items: stockMovements,
  loading,
  deleteDialog,
  selectedItem: selectedMovement,
  loadData,
  handleCreate,
  handleView,
  handleEdit,
  confirmDelete,
  handleDelete,
} = useCrudPage({
  resourceName: 'stock_movements',
  parentRoute: 'inventory',
  basePath: '/inventory/stock-movements',
  loadItems: loadAllData,
  deleteItem: deleteStockMovement,
})

// Export functionality
const exportDialog = ref(false)
const exporting = ref(false)

const exportFilters = [
  { name: 'brand', label: t('stock_movements.filter_by_brand'), type: 'text' as const, placeholder: 'e.g., Nike, Adidas' },
  { name: 'category', label: t('stock_movements.filter_by_category'), type: 'text' as const, placeholder: 'e.g., Electronics, Clothing' },
  { 
    name: 'warehouseId', 
    label: t('stock_movements.filter_by_warehouse'), 
    type: 'select' as const,
    options: computed(() => warehouses.value.map(w => ({ label: w.name, value: w.id }))),
    placeholder: t('common.select')
  },
  { name: 'fromDate', label: t('stock_movements.from_date'), type: 'date' as const },
  { name: 'toDate', label: t('stock_movements.to_date'), type: 'date' as const },
]

function openExportDialog() {
  exportDialog.value = true
}

async function handleExport({ format, filters }: { format: string; filters: Record<string, any> }) {
  exporting.value = true
  try {
    const exportParams: Record<string, any> = { format }
    if (filters.brand) exportParams.brand = filters.brand
    if (filters.category) exportParams.category = filters.category
    if (filters.warehouseId) exportParams.warehouseId = filters.warehouseId
    if (filters.fromDate) exportParams.fromDate = filters.fromDate
    if (filters.toDate) exportParams.toDate = filters.toDate
    
    await exportStockMovements(exportParams)
    const toast = useNotification()
    toast.showSuccess(t('stock_movements.export_success'))
    exportDialog.value = false
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    const toast = useNotification()
    toast.showError(t('stock_movements.export_error'), errMessage)
  }
  finally {
    exporting.value = false
  }
}

function getProductName(productId: string): string {
  const product = products.value.find(p => p.id === productId)
  return product?.name || productId
}

function getWarehouseName(warehouseId: string): string {
  const warehouse = warehouses.value.find(w => w.id === warehouseId)
  return warehouse?.name || warehouseId
}

function getMovementTypeLabel(type: MovementType): string {
  return MovementTypeLabels[type] || type.toString()
}

function getMovementTypeSeverity(type: MovementType): string {
  switch (type) {
    case MovementType.InitialInventory:
      return 'info'
    case MovementType.Purchase:
    case MovementType.Return:
      return 'success'
    case MovementType.Sale:
      return 'warning'
    case MovementType.Transfer:
      return 'info'
    case MovementType.Adjustment:
      return 'contrast'
    default:
      return 'secondary'
  }
}

function formatDate(dateString: string): string {
  return new Date(dateString).toLocaleDateString()
}

function formatCurrency(value?: number): string {
  if (!value)
    return '—'
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value)
}
</script>

<template>
  <div class="space-y-6">
    <PageHeader
      :title="t('stock_movements.title')"
      :description="t('stock_movements.description')"
    >
      <template #actions>
        <Button
          :label="t('stock_movements.export')"
          icon="pi pi-download"
          severity="secondary"
          outlined
          @click="openExportDialog"
        />
        <Button
          v-if="can.createStock()"
          :label="t('stock_movements.create')"
          icon="pi pi-plus"
          @click="handleCreate"
        />
      </template>
    </PageHeader>

    <!-- Data Table -->
    <Card>
      <template #content>
        <DataTable
          :value="stockMovements"
          :loading="loading"
          paginator
          :rows="10"
          :rows-per-page-options="[10, 25, 50]"
          striped-rows
          responsive-layout="scroll"
          :empty-message="t('stock_movements.no_movements')"
        >
          <Column field="movementDate" :header="t('stock_movements.date')" sortable>
            <template #body="{ data }">
              {{ formatDate(data.movementDate) }}
            </template>
          </Column>

          <Column field="movementType" :header="t('stock_movements.type')" sortable>
            <template #body="{ data }">
              <Tag
                :value="getMovementTypeLabel(data.movementType)"
                :severity="getMovementTypeSeverity(data.movementType)"
              />
            </template>
          </Column>

          <Column field="productId" :header="t('stock_movements.product')" sortable>
            <template #body="{ data }">
              {{ getProductName(data.productId) }}
            </template>
          </Column>

          <Column field="warehouseId" :header="t('stock_movements.warehouse')" sortable>
            <template #body="{ data }">
              {{ getWarehouseName(data.warehouseId) }}
            </template>
          </Column>

          <Column field="destinationWarehouseId" :header="t('stock_movements.destination')" sortable>
            <template #body="{ data }">
              {{ data.destinationWarehouseId ? getWarehouseName(data.destinationWarehouseId) : '—' }}
            </template>
          </Column>

          <Column field="quantity" :header="t('stock_movements.quantity')" sortable>
            <template #body="{ data }">
              <span :class="data.quantity >= 0 ? 'text-green-600' : 'text-red-600'">
                {{ data.quantity >= 0 ? '+' : '' }}{{ data.quantity }}
              </span>
            </template>
          </Column>

          <Column field="totalCost" :header="t('stock_movements.total_cost')" sortable>
            <template #body="{ data }">
              {{ formatCurrency(data.totalCost) }}
            </template>
          </Column>

          <Column field="reference" :header="t('stock_movements.reference')">
            <template #body="{ data }">
              {{ data.reference || '—' }}
            </template>
          </Column>

          <Column :header="t('common.actions')" style="width: 150px">
            <template #body="{ data }">
              <div class="flex gap-2">
                <Button
                  icon="pi pi-eye"
                  text
                  rounded
                  severity="info"
                  :aria-label="t('common.view')"
                  @click="handleView(data)"
                />
                <Button
                  v-if="can.editStock()"
                  icon="pi pi-pencil"
                  text
                  rounded
                  severity="warning"
                  :aria-label="t('common.edit')"
                  @click="handleEdit(data)"
                />
                <Button
                  v-if="can.deleteStock()"
                  icon="pi pi-trash"
                  text
                  rounded
                  severity="danger"
                  :aria-label="t('common.delete')"
                  @click="confirmDelete(data)"
                />
              </div>
            </template>
          </Column>
        </DataTable>
      </template>
    </Card>

    <!-- ✅ Using DeleteConfirmDialog component -->
    <DeleteConfirmDialog
      v-model:visible="deleteDialog"
      :title="t('stock_movements.delete_title')"
      :message="t('stock_movements.delete_confirm')"
      @confirm="handleDelete"
    />

    <!-- ✅ Using ExportDialog component -->
    <ExportDialog
      v-model:visible="exportDialog"
      :title="t('stock_movements.export_dialog_title')"
      :description="t('stock_movements.export_data')"
      :filters="exportFilters"
      :loading="exporting"
      @export="handleExport"
    />
  </div>
</template>
