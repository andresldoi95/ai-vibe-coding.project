<script setup lang="ts">
import type { Product, StockMovement, Warehouse } from '~/types/inventory'
import { MovementType, MovementTypeLabels } from '~/types/inventory'

definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const uiStore = useUiStore()
const toast = useNotification()
const route = useRoute()
const router = useRouter()
const { can } = usePermissions()
const { getStockMovementById, deleteStockMovement } = useStockMovement()
const { getProductById } = useProduct()
const { getWarehouseById } = useWarehouse()

const stockMovement = ref<StockMovement | null>(null)
const product = ref<Product | null>(null)
const warehouse = ref<Warehouse | null>(null)
const destinationWarehouse = ref<Warehouse | null>(null)
const loading = ref(false)
const deleteDialog = ref(false)

async function loadData() {
  loading.value = true
  try {
    const id = route.params.id as string
    stockMovement.value = await getStockMovementById(id)

    // Load related data
    if (stockMovement.value) {
      const promises = [
        getProductById(stockMovement.value.productId),
        getWarehouseById(stockMovement.value.warehouseId),
      ]

      if (stockMovement.value.destinationWarehouseId) {
        promises.push(getWarehouseById(stockMovement.value.destinationWarehouseId))
      }

      const results = await Promise.all(promises)
      product.value = results[0]
      warehouse.value = results[1]
      if (results[2]) {
        destinationWarehouse.value = results[2]
      }
    }

    // Set breadcrumbs
    uiStore.setBreadcrumbs([
      { label: t('nav.inventory'), to: '/inventory' },
      { label: t('stock_movements.title'), to: '/inventory/stock-movements' },
      { label: t('stock_movements.view_details') },
    ])
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('messages.error_load'), errMessage)
    router.push('/inventory/stock-movements')
  }
  finally {
    loading.value = false
  }
}

function editStockMovement() {
  router.push(`/inventory/stock-movements/${route.params.id}/edit`)
}

async function handleDelete() {
  if (!stockMovement.value)
    return

  try {
    await deleteStockMovement(stockMovement.value.id)
    toast.showSuccess(t('messages.success_delete'), t('stock_movements.deleted_successfully'))
    router.push('/inventory/stock-movements')
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('messages.error_delete'), errMessage)
  }
  finally {
    deleteDialog.value = false
  }
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
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  })
}

function formatCurrency(value?: number): string {
  if (!value)
    return '—'
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value)
}

onMounted(() => {
  loadData()
})
</script>

<template>
  <div class="space-y-6">
    <LoadingState v-if="loading" :message="t('common.loading')" />

    <div v-else-if="stockMovement">
      <!-- Page Header -->
      <PageHeader
        :title="t('stock_movements.view_details')"
      >
        <template #actions>
          <Button
            v-if="can.editStock()"
            :label="t('common.edit')"
            icon="pi pi-pencil"
            @click="editStockMovement"
          />
          <Button
            v-if="can.deleteStock()"
            :label="t('common.delete')"
            icon="pi pi-trash"
            severity="danger"
            outlined
            @click="deleteDialog = true"
          />
        </template>
      </PageHeader>

      <!-- Details Card -->
      <Card>
        <template #content>
          <div class="grid grid-cols-1 gap-8 lg:grid-cols-2">
            <!-- Movement Information -->
            <div>
              <h3 class="mb-4 text-lg font-semibold text-slate-900 dark:text-white">
                {{ t('stock_movements.movement_info') }}
              </h3>
              <div class="space-y-4">
                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('stock_movements.movement_type') }}
                  </label>
                  <div class="mt-1">
                    <Tag
                      :value="getMovementTypeLabel(stockMovement.movementType)"
                      :severity="getMovementTypeSeverity(stockMovement.movementType)"
                    />
                  </div>
                </div>

                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('stock_movements.movement_date') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ formatDate(stockMovement.movementDate) }}
                  </p>
                </div>

                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('stock_movements.product') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ product?.name || stockMovement.productId }}
                  </p>
                  <p v-if="product" class="text-sm text-slate-600 dark:text-slate-400">
                    {{ product.sku }}
                  </p>
                </div>

                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('stock_movements.warehouse') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ warehouse?.name || stockMovement.warehouseId }}
                  </p>
                  <p v-if="warehouse" class="text-sm text-slate-600 dark:text-slate-400">
                    {{ warehouse.code }}
                  </p>
                </div>

                <div v-if="stockMovement.destinationWarehouseId">
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('stock_movements.destination_warehouse') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ destinationWarehouse?.name || stockMovement.destinationWarehouseId }}
                  </p>
                  <p v-if="destinationWarehouse" class="text-sm text-slate-600 dark:text-slate-400">
                    {{ destinationWarehouse.code }}
                  </p>
                </div>

                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('stock_movements.quantity') }}
                  </label>
                  <p
                    class="text-lg font-semibold"
                    :class="stockMovement.quantity >= 0 ? 'text-green-600' : 'text-red-600'"
                  >
                    {{ stockMovement.quantity >= 0 ? '+' : '' }}{{ stockMovement.quantity }}
                  </p>
                </div>
              </div>
            </div>

            <!-- Cost & Additional Information -->
            <div>
              <h3 class="mb-4 text-lg font-semibold text-slate-900 dark:text-white">
                {{ t('stock_movements.cost_info') }}
              </h3>
              <div class="space-y-4">
                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('stock_movements.unit_cost') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ formatCurrency(stockMovement.unitCost) }}
                  </p>
                </div>

                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('stock_movements.total_cost') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ formatCurrency(stockMovement.totalCost) }}
                  </p>
                </div>

                <div v-if="stockMovement.reference">
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('stock_movements.reference') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ stockMovement.reference }}
                  </p>
                </div>

                <div v-if="stockMovement.notes">
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('stock_movements.notes') }}
                  </label>
                  <p class="text-slate-900 dark:text-white whitespace-pre-wrap">
                    {{ stockMovement.notes }}
                  </p>
                </div>
              </div>

              <div class="mt-8">
                <h3 class="mb-4 text-lg font-semibold text-slate-900 dark:text-white">
                  {{ t('common.audit_info') }}
                </h3>
                <div class="space-y-4">
                  <div>
                    <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                      {{ t('common.created_at') }}
                    </label>
                    <p class="text-slate-900 dark:text-white">
                      {{ formatDate(stockMovement.createdAt) }}
                    </p>
                  </div>

                  <div>
                    <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                      {{ t('common.updated_at') }}
                    </label>
                    <p class="text-slate-900 dark:text-white">
                      {{ formatDate(stockMovement.updatedAt) }}
                    </p>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </template>
      </Card>
    </div>

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
