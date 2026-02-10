<script setup lang="ts">
import type { Customer, CustomerFilters } from '~/types/billing'

definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const { getAllCustomers, deleteCustomer } = useCustomer()
const { can } = usePermissions()

// Filter state - customers page has custom filters
const showFilters = ref(true)
const filters = reactive<CustomerFilters>({
  searchTerm: '',
  name: '',
  email: '',
  phone: '',
  taxId: '',
  city: '',
  country: '',
  isActive: undefined,
})

// Custom load function that handles filters
async function loadCustomersWithFilters() {
  const activeFilters: CustomerFilters = {}
  if (filters.searchTerm) activeFilters.searchTerm = filters.searchTerm
  if (filters.name) activeFilters.name = filters.name
  if (filters.email) activeFilters.email = filters.email
  if (filters.phone) activeFilters.phone = filters.phone
  if (filters.taxId) activeFilters.taxId = filters.taxId
  if (filters.city) activeFilters.city = filters.city
  if (filters.country) activeFilters.country = filters.country
  if (filters.isActive !== undefined) activeFilters.isActive = filters.isActive

  return await getAllCustomers(activeFilters)
}

// ✅ Using useCrudPage composable
const {
  items: customers,
  loading,
  deleteDialog,
  selectedItem: selectedCustomer,
  loadData,
  handleCreate,
  handleView,
  handleEdit,
  confirmDelete,
  handleDelete,
} = useCrudPage({
  resourceName: 'customers',
  parentRoute: 'billing',
  basePath: '/billing/customers',
  loadItems: loadCustomersWithFilters,
  deleteItem: deleteCustomer,
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
  filters.name = ''
  filters.email = ''
  filters.phone = ''
  filters.taxId = ''
  filters.city = ''
  filters.country = ''
  filters.isActive = undefined
  loadData()
}

function getActiveFilterCount(): number {
  let count = 0
  if (filters.searchTerm) count++
  if (filters.name) count++
  if (filters.email) count++
  if (filters.phone) count++
  if (filters.taxId) count++
  if (filters.city) count++
  if (filters.country) count++
  if (filters.isActive !== undefined) count++
  return count
}
</script>

<template>
  <div>
    <!-- Page Header Component -->
    <PageHeader
      :title="t('customers.title')"
      :description="t('customers.description')"
    >
      <template #actions>
        <Button
          v-if="can.createCustomer()"
          :label="t('customers.create')"
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
              {{ t('customers.filters') }}
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
              {{ t('customers.search') }}
            </label>
            <InputText
              id="search"
              v-model="filters.searchTerm"
              :placeholder="t('customers.search_placeholder')"
              icon="pi pi-search"
            />
          </div>

          <div class="grid grid-cols-1 gap-4 md:grid-cols-3">
            <!-- Name -->
            <div class="flex flex-col gap-2">
              <label for="name" class="font-semibold text-slate-700 dark:text-slate-200">
                {{ t('customers.name') }}
              </label>
              <InputText
                id="name"
                v-model="filters.name"
                :placeholder="t('customers.name')"
              />
            </div>

            <!-- Email -->
            <div class="flex flex-col gap-2">
              <label for="email" class="font-semibold text-slate-700 dark:text-slate-200">
                {{ t('common.email') }}
              </label>
              <InputText
                id="email"
                v-model="filters.email"
                :placeholder="t('common.email')"
              />
            </div>

            <!-- Phone -->
            <div class="flex flex-col gap-2">
              <label for="phone" class="font-semibold text-slate-700 dark:text-slate-200">
                {{ t('common.phone') }}
              </label>
              <InputText
                id="phone"
                v-model="filters.phone"
                :placeholder="t('common.phone')"
              />
            </div>
          </div>

          <div class="grid grid-cols-1 gap-4 md:grid-cols-3">
            <!-- Tax ID -->
            <div class="flex flex-col gap-2">
              <label for="taxId" class="font-semibold text-slate-700 dark:text-slate-200">
                {{ t('customers.tax_id') }}
              </label>
              <InputText
                id="taxId"
                v-model="filters.taxId"
                :placeholder="t('customers.tax_id')"
              />
            </div>

            <!-- City -->
            <div class="flex flex-col gap-2">
              <label for="city" class="font-semibold text-slate-700 dark:text-slate-200">
                {{ t('common.city') }}
              </label>
              <InputText
                id="city"
                v-model="filters.city"
                :placeholder="t('common.city')"
              />
            </div>

            <!-- Country -->
            <div class="flex flex-col gap-2">
              <label for="country" class="font-semibold text-slate-700 dark:text-slate-200">
                {{ t('common.country') }}
              </label>
              <InputText
                id="country"
                v-model="filters.country"
                :placeholder="t('common.country')"
              />
            </div>
          </div>

          <div class="grid grid-cols-1 gap-4 md:grid-cols-3">
            <!-- Status -->
            <div class="flex flex-col gap-2">
              <label for="status" class="font-semibold text-slate-700 dark:text-slate-200">
                {{ t('common.status') }}
              </label>
              <Dropdown
                id="status"
                v-model="filters.isActive"
                :options="[
                  { label: t('customers.all_customers'), value: undefined },
                  { label: t('customers.active_customers'), value: true },
                  { label: t('customers.inactive_customers'), value: false },
                ]"
                option-label="label"
                option-value="value"
                :placeholder="t('common.status')"
              />
            </div>
          </div>

          <!-- Filter Actions -->
          <div class="flex justify-end gap-3 pt-2">
            <Button
              :label="t('customers.reset_filters')"
              icon="pi pi-refresh"
              outlined
              @click="resetFilters"
            />
            <Button
              :label="t('customers.apply_filters')"
              icon="pi pi-filter"
              @click="applyFilters"
            />
          </div>
        </div>
      </template>
    </Card>

    <!-- Data Table Card -->
    <Card>
      <template #content>
        <LoadingState v-if="loading" :message="t('common.loading')" />
        <DataTable
          v-else
          :value="customers"
          :paginator="true"
          :rows="10"
          :rows-per-page-options="[10, 25, 50]"
          striped-rows
          responsive-layout="scroll"
        >
          <template #empty>
            <EmptyState
              icon="pi pi-users"
              :title="t('common.no_data')"
              :description="can.createCustomer() ? t('customers.empty_description') : undefined"
              :action-label="t('customers.create')"
              action-icon="pi pi-plus"
              @action="handleCreate"
            />
          </template>

          <Column field="name" :header="t('customers.name')" sortable />
          <Column field="email" :header="t('common.email')" sortable />
          <Column field="phone" :header="t('common.phone')" sortable />
          <Column field="taxId" :header="t('customers.tax_id')" sortable />

          <Column field="location" :header="t('customers.location')" sortable>
            <template #body="{ data }">
              {{ data.city }}, {{ data.country }}
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
                :show-view="can.viewCustomers()"
                :show-edit="can.editCustomer()"
                :show-delete="can.deleteCustomer()"
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
      :item-name="selectedCustomer?.name"
      @confirm="handleDelete"
    />
  </div>
</template>
