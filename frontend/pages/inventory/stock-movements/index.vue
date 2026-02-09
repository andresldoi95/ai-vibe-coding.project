<script setup lang="ts">
import type { StockMovement } from '~/types/inventory'
import { MovementType, MovementTypeLabels } from '~/types/inventory'

definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const toast = useNotification()
const { getAllStockMovements, deleteStockMovement } = useStockMovement()
const { getAllProducts } = useProduct()
const { getAllWarehouses } = useWarehouse()
const { can } = usePermissions()

const stockMovements = ref<StockMovement[]>([])
const loading = ref(false)
const deleteDialog = ref(false)
const selectedMovement = ref<StockMovement | null>(null)

// For displaying product and warehouse names
const products = ref<any[]>([])
const warehouses = ref<any[]>([])

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
  if (!value) return '—'
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value)
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
  </div>
</template>
