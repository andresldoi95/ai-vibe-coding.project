<script setup lang="ts">
import type { Customer, CustomerFilters } from '~/types/billing'

definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const uiStore = useUiStore()
const toast = useNotification()
const { getAllCustomers, deleteCustomer } = useCustomer()
const { can } = usePermissions()

const customers = ref<Customer[]>([])
const loading = ref(false)
const deleteDialog = ref(false)
const selectedCustomer = ref<Customer | null>(null)
const showFilters = ref(true)

// Filter state
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

// Debounce search term
const searchDebounce = ref<NodeJS.Timeout>()

watch(() => filters.searchTerm, () => {
  if (searchDebounce.value)
    clearTimeout(searchDebounce.value)
  searchDebounce.value = setTimeout(() => {
    loadCustomers()
  }, 300)
})

async function loadCustomers() {
  loading.value = true
  try {
    // Build filter object, excluding empty values
    const activeFilters: CustomerFilters = {}
    if (filters.searchTerm)
      activeFilters.searchTerm = filters.searchTerm
    if (filters.name)
      activeFilters.name = filters.name
    if (filters.email)
      activeFilters.email = filters.email
    if (filters.phone)
      activeFilters.phone = filters.phone
    if (filters.taxId)
      activeFilters.taxId = filters.taxId
    if (filters.city)
      activeFilters.city = filters.city
    if (filters.country)
      activeFilters.country = filters.country
    if (filters.isActive !== undefined)
      activeFilters.isActive = filters.isActive

    customers.value = await getAllCustomers(activeFilters)
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('messages.error_load'), errMessage)
  }
  finally {
    loading.value = false
  }
}

function applyFilters() {
  loadCustomers()
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
  loadCustomers()
}

function createCustomer() {
  navigateTo('/billing/customers/new')
}

function confirmDelete(customer: Customer) {
  selectedCustomer.value = customer
  deleteDialog.value = true
}

async function handleDelete() {
  if (!selectedCustomer.value)
    return

  try {
    await deleteCustomer(selectedCustomer.value.id)
    toast.showSuccess(t('messages.success_delete'), t('customers.deleted_successfully'))
    await loadCustomers()
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('messages.error_delete'), errMessage)
  }
  finally {
    deleteDialog.value = false
    selectedCustomer.value = null
  }
}

function getStatusLabel(isActive: boolean): string {
  return isActive ? t('common.active') : t('common.inactive')
}

function getStatusSeverity(isActive: boolean): 'success' | 'danger' {
  return isActive ? 'success' : 'danger'
}

function getActiveFilterCount(): number {
  let count = 0
  if (filters.searchTerm)
    count++
  if (filters.name)
    count++
  if (filters.email)
    count++
  if (filters.phone)
    count++
  if (filters.taxId)
    count++
  if (filters.city)
    count++
  if (filters.country)
    count++
  if (filters.isActive !== undefined)
    count++
  return count
}

onMounted(() => {
  uiStore.setBreadcrumbs([
    { label: t('nav.billing'), to: '/billing' },
    { label: t('customers.title') },
  ])
  loadCustomers()
})
</script>

<template>
  <div>
    <!-- Page Header Component - Following UX spacing guidelines -->
    <PageHeader
      :title="t('customers.title')"
      :description="t('customers.description')"
    >
      <template #actions>
        <Button
          v-if="can.createCustomer()"
          :label="t('customers.create')"
          icon="pi pi-plus"
          @click="createCustomer"
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

    <!-- Data Table Card - Using standard card padding (p-6) -->
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
              @action="createCustomer"
            />
          </template>

          <Column field="name" :header="t('customers.name')" sortable />
          <Column field="email" :header="t('common.email')" sortable />
          <Column field="phone" :header="t('common.phone')" sortable>
            <template #body="{ data }">
              {{ data.phone || '—' }}
            </template>
          </Column>
          <Column field="taxId" :header="t('customers.tax_id')" sortable>
            <template #body="{ data }">
              {{ data.taxId || '—' }}
            </template>
          </Column>
          <Column field="contactPerson" :header="t('customers.contact_person')" sortable>
            <template #body="{ data }">
              {{ data.contactPerson || '—' }}
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
                @view="navigateTo(`/billing/customers/${data.id}`)"
                @edit="navigateTo(`/billing/customers/${data.id}/edit`)"
                @delete="confirmDelete(data)"
              />
            </template>
          </Column>
        </DataTable>
      </template>
    </Card>

    <!-- Delete Confirmation Dialog -->
    <Dialog
      v-model:visible="deleteDialog"
      :header="t('common.confirm')"
      :modal="true"
      :style="{ width: '450px' }"
    >
      <div class="flex items-center gap-4">
        <i class="pi pi-exclamation-triangle text-3xl text-orange-500" />
        <span v-if="selectedCustomer">
          {{ t('customers.confirm_delete', { name: selectedCustomer.name }) }}
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
