<script setup lang="ts">
import type { ProductFilters } from '~/types/inventory'

definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const { getAllProducts, deleteProduct } = useProduct()
const { can } = usePermissions()

// Filter state - products page has custom filters, so we keep this custom
const showFilters = ref(true)
const filters = reactive<ProductFilters>({
  searchTerm: '',
  category: '',
  brand: '',
  isActive: undefined,
  minPrice: undefined,
  maxPrice: undefined,
  lowStock: false,
})

// Custom load function that handles filters
async function loadProductsWithFilters() {
  // Build filter object, excluding empty values
  const activeFilters: ProductFilters = {}
  if (filters.searchTerm)
    activeFilters.searchTerm = filters.searchTerm
  if (filters.category)
    activeFilters.category = filters.category
  if (filters.brand)
    activeFilters.brand = filters.brand
  if (filters.isActive !== undefined)
    activeFilters.isActive = filters.isActive
  if (filters.minPrice !== undefined)
    activeFilters.minPrice = filters.minPrice
  if (filters.maxPrice !== undefined)
    activeFilters.maxPrice = filters.maxPrice
  if (filters.lowStock)
    activeFilters.lowStock = filters.lowStock

  return await getAllProducts(activeFilters)
}

// ✅ Using useCrudPage composable with custom load function
const {
  items: products,
  loading,
  deleteDialog,
  selectedItem: selectedProduct,
  loadData,
  handleCreate,
  handleView,
  handleEdit,
  confirmDelete,
  handleDelete,
} = useCrudPage({
  resourceName: 'products',
  parentRoute: 'inventory',
  basePath: '/inventory/products',
  loadItems: loadProductsWithFilters,
  deleteItem: deleteProduct,
})

// ✅ Using useStatus composable
const { getStatusLabel, getStatusSeverity } = useStatus()

// Debounce search term
const searchDebounce = ref<NodeJS.Timeout>()

watch(() => filters.searchTerm, () => {
  if (searchDebounce.value)
    clearTimeout(searchDebounce.value)
  searchDebounce.value = setTimeout(() => {
    loadData()
  }, 300)
})

function applyFilters() {
  loadData()
}

function resetFilters() {
  filters.searchTerm = ''
  filters.category = ''
  filters.brand = ''
  filters.isActive = undefined
  filters.minPrice = undefined
  filters.maxPrice = undefined
  filters.lowStock = false
  loadData()
}

function formatCurrency(value: number): string {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
  }).format(value)
}

function getActiveFilterCount(): number {
  let count = 0
  if (filters.searchTerm)
    count++
  if (filters.category)
    count++
  if (filters.brand)
    count++
  if (filters.isActive !== undefined)
    count++
  if (filters.minPrice !== undefined)
    count++
  if (filters.maxPrice !== undefined)
    count++
  if (filters.lowStock)
    count++
  return count
}
</script>

<template>
  <div>
    <!-- Page Header Component - Following UX spacing guidelines -->
    <PageHeader
      :title="t('products.title')"
      :description="t('products.description')"
    >
      <template #actions>
        <Button
          v-if="can.createProduct()"
          :label="t('products.create')"
          icon="pi pi-plus"
          @click="handleCreate"
        />
      </template>
    </PageHeader>

    <!-- Filters Panel -->
    <Card class="mb-6">
      <template #content>
        <div class="flex items-center justify-between mb-4">
          <div class="flex items-center gap-2">
            <h3 class="text-lg font-semibold text-slate-900 dark:text-white">
              {{ t('products.filters') }}
            </h3>
            <Badge
              v-if="getActiveFilterCount() > 0"
              :value="getActiveFilterCount()"
              severity="info"
            />
          </div>
          <Button
            :icon="showFilters ? 'pi pi-chevron-up' : 'pi pi-chevron-down'"
            text
            rounded
            @click="showFilters = !showFilters"
          />
        </div>

        <div v-if="showFilters" class="space-y-4">
          <!-- Search -->
          <div class="flex flex-col gap-2">
            <label for="search" class="font-semibold text-slate-700 dark:text-slate-200">
              {{ t('products.search') }}
            </label>
            <InputText
              id="search"
              v-model="filters.searchTerm"
              :placeholder="t('products.search_placeholder')"
              icon="pi pi-search"
            />
          </div>

          <div class="grid grid-cols-1 gap-4 md:grid-cols-3">
            <!-- Category -->
            <div class="flex flex-col gap-2">
              <label for="category" class="font-semibold text-slate-700 dark:text-slate-200">
                {{ t('products.category') }}
              </label>
              <InputText
                id="category"
                v-model="filters.category"
                :placeholder="t('products.category')"
              />
            </div>

            <!-- Brand -->
            <div class="flex flex-col gap-2">
              <label for="brand" class="font-semibold text-slate-700 dark:text-slate-200">
                {{ t('products.brand') }}
              </label>
              <InputText
                id="brand"
                v-model="filters.brand"
                :placeholder="t('products.brand')"
              />
            </div>

            <!-- Status -->
            <div class="flex flex-col gap-2">
              <label for="status" class="font-semibold text-slate-700 dark:text-slate-200">
                {{ t('common.status') }}
              </label>
              <Select
                id="status"
                v-model="filters.isActive"
                :options="[
                  { label: t('products.all_products'), value: undefined },
                  { label: t('products.active_products'), value: true },
                  { label: t('products.inactive_products'), value: false },
                ]"
                option-label="label"
                option-value="value"
                :placeholder="t('common.status')"
              />
            </div>
          </div>

          <div class="grid grid-cols-1 gap-4 md:grid-cols-3">
            <!-- Min Price -->
            <div class="flex flex-col gap-2">
              <label for="minPrice" class="font-semibold text-slate-700 dark:text-slate-200">
                {{ t('products.min_price') }}
              </label>
              <InputNumber
                id="minPrice"
                v-model="filters.minPrice"
                :placeholder="t('products.min_price')"
                mode="currency"
                currency="USD"
                :min="0"
              />
            </div>

            <!-- Max Price -->
            <div class="flex flex-col gap-2">
              <label for="maxPrice" class="font-semibold text-slate-700 dark:text-slate-200">
                {{ t('products.max_price') }}
              </label>
              <InputNumber
                id="maxPrice"
                v-model="filters.maxPrice"
                :placeholder="t('products.max_price')"
                mode="currency"
                currency="USD"
                :min="0"
              />
            </div>

            <!-- Low Stock -->
            <div class="flex items-end gap-2 pb-2">
              <Checkbox
                id="lowStock"
                v-model="filters.lowStock"
                :binary="true"
              />
              <label for="lowStock" class="font-semibold text-slate-700 dark:text-slate-200">
                {{ t('products.low_stock') }}
              </label>
            </div>
          </div>

          <!-- Filter Actions -->
          <div class="flex justify-end gap-3 pt-2">
            <Button
              :label="t('products.reset_filters')"
              icon="pi pi-refresh"
              outlined
              @click="resetFilters"
            />
            <Button
              :label="t('products.apply_filters')"
              icon="pi pi-filter"
              @click="applyFilters"
            />
          </div>
        </div>
      </template>
    </Card>

    <!-- Data Table Card - Using standard card padding (p-6) -->
    <Card>
      <template #content>
        <LoadingState v-if="loading" :message="t('common.loading')" />
        <DataTable
          v-else
          :value="products"
          :paginator="true"
          :rows="10"
          :rows-per-page-options="[10, 25, 50]"
          striped-rows
          responsive-layout="scroll"
        >
          <template #empty>
            <EmptyState
              icon="pi pi-box"
              :title="t('common.no_data')"
              :description="can.createProduct() ? t('products.empty_description') : undefined"
              :action-label="t('products.create')"
              action-icon="pi pi-plus"
              @action="handleCreate"
            />
          </template>

          <Column field="code" :header="t('products.code')" sortable />
          <Column field="sku" :header="t('products.sku')" sortable />
          <Column field="name" :header="t('products.name')" sortable />
          <Column field="category" :header="t('products.category')" sortable>
            <template #body="{ data }">
              {{ data.category || '—' }}
            </template>
          </Column>
          <Column field="brand" :header="t('products.brand')" sortable>
            <template #body="{ data }">
              {{ data.brand || '—' }}
            </template>
          </Column>
          <Column field="unitPrice" :header="t('products.unit_price')" sortable>
            <template #body="{ data }">
              {{ formatCurrency(data.unitPrice) }}
            </template>
          </Column>
          <Column field="currentStockLevel" :header="t('products.stock_level')" sortable>
            <template #body="{ data }">
              <span :class="{ 'text-orange-600 dark:text-orange-400 font-semibold': data.currentStockLevel !== undefined && data.currentStockLevel < data.minimumStockLevel }">
                {{ data.currentStockLevel ?? '—' }}
              </span>
            </template>
          </Column>

          <Column field="isActive" :header="t('common.status')" sortable>
            <template #body="{ data }">
              <Tag
                :value="getStatusLabel(data.isActive)"
                :severity="getStatusSeverity(data.isActive)"
              />
            </template>
          </Column>

          <Column :header="t('common.actions')">
            <template #body="{ data }">
              <DataTableActions
                :show-view="can.viewProducts()"
                :show-edit="can.editProduct()"
                :show-delete="can.deleteProduct()"
                @view="handleView(data)"
                @edit="handleEdit(data)"
                @delete="confirmDelete(data)"
              />
            </template>
          </Column>
        </DataTable>
      </template>
    </Card>

    <!-- ✅ Using DeleteConfirmDialog component -->
    <DeleteConfirmDialog
      v-model:visible="deleteDialog"
      :item-name="selectedProduct?.name"
      @confirm="handleDelete"
    />
  </div>
</template>
