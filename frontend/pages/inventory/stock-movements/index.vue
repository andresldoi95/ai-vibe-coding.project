<script setup lang="ts">
import type { Product, StockMovement, Warehouse } from '~/types/inventory'
import { MovementType, MovementTypeLabels } from '~/types/inventory'

definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const toast = useNotification()
const { getAllStockMovements, deleteStockMovement, exportStockMovements } = useStockMovement()
const { getAllProducts } = useProduct()
const { getAllWarehouses } = useWarehouse()
const { can } = usePermissions()

const stockMovements = ref<StockMovement[]>([])
const loading = ref(false)
const deleteDialog = ref(false)
const exportDialog = ref(false)
const exporting = ref(false)
const selectedMovement = ref<StockMovement | null>(null)

// Export filters
const exportFormat = ref<'csv' | 'excel'>('excel')
const exportFilters = ref({
  brand: '',
  category: '',
  warehouseId: '',
  fromDate: '',
  toDate: '',
})

// For displaying product and warehouse names
const products = ref<Product[]>([])
const warehouses = ref<Warehouse[]>([])

async function loadData() {
  loading.value = true
  try {
    // Load all data in parallel
    const [movementsData, productsData, warehousesData] = await Promise.all([
      getAllStockMovements(),
      getAllProducts(),
      getAllWarehouses(),
    ])
    stockMovements.value = movementsData
    products.value = productsData
    warehouses.value = warehousesData
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('messages.error_load'), errMessage)
  }
  finally {
    loading.value = false
  }
}

function createStockMovement() {
  navigateTo('/inventory/stock-movements/new')
}

function viewMovement(movement: StockMovement) {
  navigateTo(`/inventory/stock-movements/${movement.id}`)
}

function editMovement(movement: StockMovement) {
  navigateTo(`/inventory/stock-movements/${movement.id}/edit`)
}

function confirmDelete(movement: StockMovement) {
  selectedMovement.value = movement
  deleteDialog.value = true
}

async function handleDelete() {
  if (!selectedMovement.value)
    return

  try {
    await deleteStockMovement(selectedMovement.value.id)
    toast.showSuccess(t('stock_movements.deleted_successfully'))
    deleteDialog.value = false
    selectedMovement.value = null
    await loadData()
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('messages.error_delete'), errMessage)
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

function openExportDialog() {
  exportDialog.value = true
}

async function handleExport() {
  exporting.value = true
  try {
    const filters: any = {
      format: exportFormat.value,
    }
    
    if (exportFilters.value.brand) filters.brand = exportFilters.value.brand
    if (exportFilters.value.category) filters.category = exportFilters.value.category
    if (exportFilters.value.warehouseId) filters.warehouseId = exportFilters.value.warehouseId
    if (exportFilters.value.fromDate) filters.fromDate = exportFilters.value.fromDate
    if (exportFilters.value.toDate) filters.toDate = exportFilters.value.toDate
    
    await exportStockMovements(filters)
    toast.showSuccess(t('stock_movements.export_success'))
    exportDialog.value = false
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('stock_movements.export_error'), errMessage)
  }
  finally {
    exporting.value = false
  }
}

onMounted(() => {
  loadData()
})
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
          @click="createStockMovement"
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
                  @click="viewMovement(data)"
                />
                <Button
                  v-if="can.editStock()"
                  icon="pi pi-pencil"
                  text
                  rounded
                  severity="warning"
                  :aria-label="t('common.edit')"
                  @click="editMovement(data)"
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

    <!-- Delete Confirmation Dialog -->
    <Dialog
      v-model:visible="deleteDialog"
      :header="t('stock_movements.delete_title')"
      :modal="true"
      :style="{ width: '450px' }"
    >
      <div class="flex items-center gap-3">
        <i class="pi pi-exclamation-triangle text-3xl text-orange-500" />
        <span>{{ t('stock_movements.delete_confirm') }}</span>
      </div>
      <template #footer>
        <Button
          :label="t('common.cancel')"
          severity="secondary"
          outlined
          @click="deleteDialog = false"
        />
        <Button
          :label="t('common.delete')"
          severity="danger"
          @click="handleDelete"
        />
      </template>
    </Dialog>

    <!-- Export Dialog -->
    <Dialog
      v-model:visible="exportDialog"
      :header="t('stock_movements.export_dialog_title')"
      :modal="true"
      :style="{ width: '600px' }"
    >
      <div class="space-y-4">
        <!-- Export Format -->
        <div>
          <label class="block text-sm font-medium mb-2">{{ t('stock_movements.export_format') }}</label>
          <div class="flex gap-4">
            <div class="flex items-center">
              <RadioButton v-model="exportFormat" input-id="format-excel" value="excel" />
              <label for="format-excel" class="ml-2">{{ t('stock_movements.export_excel') }}</label>
            </div>
            <div class="flex items-center">
              <RadioButton v-model="exportFormat" input-id="format-csv" value="csv" />
              <label for="format-csv" class="ml-2">{{ t('stock_movements.export_csv') }}</label>
            </div>
          </div>
        </div>

        <Divider />

        <!-- Export Filters -->
        <div>
          <h4 class="text-sm font-medium mb-3">{{ t('stock_movements.export_filters') }}</h4>
          
          <div class="space-y-3">
            <!-- Brand Filter -->
            <div>
              <label for="brand" class="block text-sm mb-1">{{ t('stock_movements.filter_by_brand') }}</label>
              <InputText
                id="brand"
                v-model="exportFilters.brand"
                class="w-full"
                placeholder="e.g., Nike, Adidas"
              />
            </div>

            <!-- Category Filter -->
            <div>
              <label for="category" class="block text-sm mb-1">{{ t('stock_movements.filter_by_category') }}</label>
              <InputText
                id="category"
                v-model="exportFilters.category"
                class="w-full"
                placeholder="e.g., Electronics, Clothing"
              />
            </div>

            <!-- Warehouse Filter -->
            <div>
              <label for="warehouse" class="block text-sm mb-1">{{ t('stock_movements.filter_by_warehouse') }}</label>
              <Dropdown
                id="warehouse"
                v-model="exportFilters.warehouseId"
                :options="warehouses"
                option-label="name"
                option-value="id"
                :placeholder="t('common.select')"
                class="w-full"
                show-clear
              />
            </div>

            <!-- Date Range -->
            <div class="grid grid-cols-2 gap-3">
              <div>
                <label for="fromDate" class="block text-sm mb-1">{{ t('stock_movements.from_date') }}</label>
                <Calendar
                  id="fromDate"
                  v-model="exportFilters.fromDate"
                  date-format="yy-mm-dd"
                  class="w-full"
                  show-icon
                />
              </div>
              <div>
                <label for="toDate" class="block text-sm mb-1">{{ t('stock_movements.to_date') }}</label>
                <Calendar
                  id="toDate"
                  v-model="exportFilters.toDate"
                  date-format="yy-mm-dd"
                  class="w-full"
                  show-icon
                />
              </div>
            </div>
          </div>
        </div>
      </div>

      <template #footer>
        <Button
          :label="t('common.cancel')"
          severity="secondary"
          outlined
          :disabled="exporting"
          @click="exportDialog = false"
        />
        <Button
          :label="exporting ? t('stock_movements.exporting') : t('stock_movements.export')"
          icon="pi pi-download"
          :loading="exporting"
          @click="handleExport"
        />
      </template>
    </Dialog>
  </div>
</template>
