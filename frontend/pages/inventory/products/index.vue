<script setup lang="ts">
import type { Product } from '~/types/inventory'

definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const uiStore = useUiStore()
const toast = useNotification()
const { formatCurrency, formatNumber } = useFormatters()

const products = ref<Product[]>([])
const loading = ref(false)

async function loadProducts() {
  loading.value = true
  try {
    // Mock data for now
    products.value = []
    toast.showInfo(t('messages.error_load'), 'Backend not connected')
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('messages.error_load'), errMessage)
  }
  finally {
    loading.value = false
  }
}

function createProduct() {
  navigateTo('/inventory/products/new')
}

function deleteProduct(_id: string) {
  // Implement delete logic
  toast.showSuccess(t('messages.success_delete'))
}

onMounted(() => {
  uiStore.setBreadcrumbs([
    { label: t('nav.inventory') },
    { label: t('nav.products') },
  ])
  loadProducts()
})
</script>

<template>
  <div>
    <!-- Page Header Component - Following UX spacing guidelines -->
    <PageHeader
      :title="t('inventory.products')"
      description="Manage your product catalog and inventory levels"
    >
      <template #actions>
        <Button
          :label="t('inventory.create_product')"
          icon="pi pi-plus"
          @click="createProduct"
        />
      </template>
    </PageHeader>

    <!-- Data Table Card - Using standard card padding (p-6) -->
    <Card>
      <template #content>
        <LoadingState v-if="loading" message="Loading products..." />
        <DataTable
          v-else
          :value="products"
          :paginator="true"
          :rows="10"
          :rowsPerPageOptions="[10, 25, 50]"
          stripedRows
          responsiveLayout="scroll"
        >
          <template #empty>
            <EmptyState
              icon="pi pi-box"
              :title="t('common.no_data')"
              description="Add your first product to start managing inventory"
              :actionLabel="t('inventory.create_product')"
              actionIcon="pi pi-plus"
              @action="createProduct"
            />
          </template>

          <Column field="name" :header="t('inventory.product')" sortable />
          <Column field="sku" :header="t('inventory.sku')" sortable />
          <Column field="category" :header="t('common.status')" sortable />

          <Column field="stockQuantity" :header="t('inventory.stock')" sortable>
            <template #body="{ data }">
              <Tag
                :value="formatNumber(data.stockQuantity)"
                :severity="
                  data.stockQuantity <= data.reorderPoint ? 'danger' : 'success'
                "
              />
            </template>
          </Column>

          <Column
            field="unitPrice"
            :header="t('inventory.unit_price')"
            sortable
          >
            <template #body="{ data }">
              {{ formatCurrency(data.unitPrice) }}
            </template>
          </Column>

          <Column :header="t('common.actions')">
            <template #body="{ data }">
              <DataTableActions
                :show-view="true"
                :show-edit="true"
                :show-delete="true"
                @view="navigateTo(`/inventory/products/${data.id}`)"
                @edit="navigateTo(`/inventory/products/${data.id}/edit`)"
                @delete="deleteProduct(data.id)"
              />
            </template>
          </Column>
        </DataTable>
      </template>
    </Card>
  </div>
</template>
