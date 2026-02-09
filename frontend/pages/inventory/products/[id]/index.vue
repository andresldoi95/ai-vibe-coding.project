<script setup lang="ts">
import type { InventoryLevel, Product } from '~/types/inventory'

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
const { getProductById, deleteProduct } = useProduct()
const { getProductInventory, getTotalStock, getTotalAvailable } = useWarehouseInventory()

const product = ref<Product | null>(null)
const loading = ref(false)
const deleteDialog = ref(false)
const inventory = ref<InventoryLevel[]>([])
const inventoryLoading = ref(false)

async function loadProduct() {
  loading.value = true
  try {
    const id = route.params.id as string
    product.value = await getProductById(id)
    uiStore.setBreadcrumbs([
      { label: t('nav.inventory'), to: '/inventory' },
      { label: t('products.title'), to: '/inventory/products' },
      { label: product.value.name },
    ])

    // Load warehouse inventory
    await loadInventory()
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('messages.error_load'), errMessage)
    router.push('/inventory/products')
  }
  finally {
    loading.value = false
  }
}

async function loadInventory() {
  if (!product.value)
    return

  inventoryLoading.value = true
  try {
    inventory.value = await getProductInventory(product.value.id)
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('messages.error_load'), errMessage)
  }
  finally {
    inventoryLoading.value = false
  }
}

function editProduct() {
  router.push(`/inventory/products/${route.params.id}/edit`)
}

async function handleDelete() {
  if (!product.value)
    return

  try {
    await deleteProduct(product.value.id)
    toast.showSuccess(t('messages.success_delete'), t('products.deleted_successfully'))
    router.push('/inventory/products')
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('messages.error_delete'), errMessage)
  }
  finally {
    deleteDialog.value = false
  }
}

function formatCurrency(value: number): string {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
  }).format(value)
}

onMounted(() => {
  loadProduct()
})
</script>

<template>
  <div>
    <LoadingState v-if="loading" :message="t('common.loading')" />

    <div v-else-if="product">
      <!-- Page Header -->
      <PageHeader
        :title="product.name"
        :description="product.description"
      >
        <template #actions>
          <Button
            v-if="can.editProduct()"
            :label="t('common.edit')"
            icon="pi pi-pencil"
            @click="editProduct"
          />
          <Button
            v-if="can.deleteProduct()"
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
            <!-- Basic Information -->
            <div>
              <h3 class="mb-4 text-lg font-semibold text-slate-900 dark:text-white">
                {{ t('common.basic_info') }}
              </h3>
              <div class="space-y-4">
                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('products.name') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ product.name }}
                  </p>
                </div>

                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('products.code') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ product.code }}
                  </p>
                </div>

                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('products.sku') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ product.sku }}
                  </p>
                </div>

                <div v-if="product.description">
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('products.description') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ product.description }}
                  </p>
                </div>

                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('common.status') }}
                  </label>
                  <div class="mt-1">
                    <Tag
                      :value="product.isActive ? t('common.active') : t('common.inactive')"
                      :severity="product.isActive ? 'success' : 'danger'"
                    />
                  </div>
                </div>
              </div>
            </div>

            <!-- Classification -->
            <div>
              <h3 class="mb-4 text-lg font-semibold text-slate-900 dark:text-white">
                {{ t('products.classification') }}
              </h3>
              <div class="space-y-4">
                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('products.category') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ product.category || '—' }}
                  </p>
                </div>

                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('products.brand') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ product.brand || '—' }}
                  </p>
                </div>
              </div>
            </div>

            <!-- Pricing -->
            <div>
              <h3 class="mb-4 text-lg font-semibold text-slate-900 dark:text-white">
                {{ t('products.pricing') }}
              </h3>
              <div class="space-y-4">
                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('products.unit_price') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ formatCurrency(product.unitPrice) }}
                  </p>
                </div>

                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('products.cost_price') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ formatCurrency(product.costPrice) }}
                  </p>
                </div>

                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('products.profit_margin') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ formatCurrency(product.unitPrice - product.costPrice) }}
                    ({{ ((product.unitPrice - product.costPrice) / product.unitPrice * 100).toFixed(1) }}%)
                  </p>
                </div>
              </div>
            </div>

            <!-- Inventory -->
            <div>
              <h3 class="mb-4 text-lg font-semibold text-slate-900 dark:text-white">
                {{ t('nav.inventory') }}
              </h3>
              <div class="space-y-4">
                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('products.minimum_stock_level') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ product.minimumStockLevel }}
                  </p>
                </div>

                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('products.current_stock_level') }}
                  </label>
                  <p
                    class="text-slate-900 dark:text-white"
                    :class="{
                      'text-orange-600 dark:text-orange-400 font-semibold':
                        product.currentStockLevel !== undefined && product.currentStockLevel < product.minimumStockLevel,
                    }"
                  >
                    {{ product.currentStockLevel ?? '—' }}
                    <span v-if="product.currentStockLevel !== undefined && product.currentStockLevel < product.minimumStockLevel" class="text-sm">
                      ({{ t('products.low_stock_indicator') }})
                    </span>
                  </p>
                </div>
              </div>
            </div>

            <!-- Physical Properties -->
            <div>
              <h3 class="mb-4 text-lg font-semibold text-slate-900 dark:text-white">
                {{ t('products.physical_properties') }}
              </h3>
              <div class="space-y-4">
                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('products.weight') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ product.weight ? `${product.weight} kg` : '—' }}
                  </p>
                </div>

                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('products.dimensions') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ product.dimensions || '—' }}
                  </p>
                </div>
              </div>
            </div>

            <!-- Audit Information -->
            <div>
              <h3 class="mb-4 text-lg font-semibold text-slate-900 dark:text-white">
                {{ t('common.audit_info') }}
              </h3>
              <div class="space-y-4">
                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('common.created_at') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ new Date(product.createdAt).toLocaleString() }}
                  </p>
                </div>

                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('common.updated_at') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ new Date(product.updatedAt).toLocaleString() }}
                  </p>
                </div>
              </div>
            </div>
          </div>
        </template>
      </Card>

      <!-- Warehouse Inventory Card -->
      <Card class="mt-6">
        <template #title>
          <div class="flex items-center justify-between">
            <h2 class="text-xl font-semibold text-slate-900 dark:text-white">
              {{ t('inventory.warehouse_stock') }}
            </h2>
            <div class="text-right">
              <div class="text-sm text-slate-600 dark:text-slate-400">
                {{ t('inventory.total_stock') }}
              </div>
              <div class="text-2xl font-bold text-slate-900 dark:text-white">
                {{ getTotalStock(inventory) }}
              </div>
              <div class="text-sm text-slate-600 dark:text-slate-400">
                {{ t('inventory.available') }}: {{ getTotalAvailable(inventory) }}
              </div>
            </div>
          </div>
        </template>
        <template #content>
          <LoadingState v-if="inventoryLoading" :message="t('common.loading')" />

          <div v-else-if="inventory.length === 0" class="py-8 text-center text-slate-500 dark:text-slate-400">
            {{ t('inventory.no_stock_data') }}
          </div>

          <DataTable
            v-else
            :value="inventory"
            responsive-layout="scroll"
            class="text-sm"
          >
            <Column
              field="warehouseName"
              :header="t('warehouses.warehouse')"
              sortable
            >
              <template #body="{ data }">
                <div>
                  <div class="font-medium text-slate-900 dark:text-white">
                    {{ data.warehouseName }}
                  </div>
                  <div class="text-xs text-slate-500 dark:text-slate-400">
                    {{ data.warehouseCode }}
                  </div>
                </div>
              </template>
            </Column>

            <Column
              field="quantity"
              :header="t('inventory.quantity')"
              sortable
            >
              <template #body="{ data }">
                <span class="font-medium text-slate-900 dark:text-white">
                  {{ data.quantity }}
                </span>
              </template>
            </Column>

            <Column
              field="reservedQuantity"
              :header="t('inventory.reserved')"
              sortable
            >
              <template #body="{ data }">
                <span class="text-slate-700 dark:text-slate-300">
                  {{ data.reservedQuantity }}
                </span>
              </template>
            </Column>

            <Column
              field="availableQuantity"
              :header="t('inventory.available')"
              sortable
            >
              <template #body="{ data }">
                <Tag
                  :value="data.availableQuantity"
                  :severity="data.availableQuantity < product.minimumStockLevel ? 'warning' : 'success'"
                />
              </template>
            </Column>

            <Column
              field="lastMovementDate"
              :header="t('inventory.last_movement')"
              sortable
            >
              <template #body="{ data }">
                <span v-if="data.lastMovementDate" class="text-slate-600 dark:text-slate-400">
                  {{ new Date(data.lastMovementDate).toLocaleDateString() }}
                </span>
                <span v-else class="text-slate-400 dark:text-slate-500">—</span>
              </template>
            </Column>
          </DataTable>
        </template>
      </Card>
    </div>

    <!-- Delete Confirmation Dialog -->
    <Dialog
      v-model:visible="deleteDialog"
      :header="t('common.confirm')"
      :modal="true"
      :style="{ width: '450px' }"
    >
      <div class="flex items-center gap-4">
        <i class="pi pi-exclamation-triangle text-3xl text-orange-500" />
        <span v-if="product">
          {{ t('products.confirm_delete', { name: product.name }) }}
        </span>
      </div>
      <template #footer>
        <Button
          :label="t('common.cancel')"
          icon="pi pi-times"
          text
          @click="deleteDialog = false"
        />
        <Button
          :label="t('common.delete')"
          icon="pi pi-trash"
          severity="danger"
          @click="handleDelete"
        />
      </template>
    </Dialog>
  </div>
</template>
