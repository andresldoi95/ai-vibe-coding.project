<script setup lang="ts">
import { InvoiceStatus } from '~/types/billing'
import type { InvoiceFilters } from '~/types/billing'

definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const { getAllInvoices, deleteInvoice } = useInvoice()
const { can } = usePermissions()

// ── Filter state ──────────────────────────────────────────────────────────────
const showFilters = ref(true)
const searchTerm = ref('')
const statusFilter = ref<InvoiceStatus | undefined>(undefined)
const dateFromFilter = ref<Date | undefined>(undefined)
const dateToFilter = ref<Date | undefined>(undefined)

// Server-side filter loader
async function loadWithFilters() {
  const active: InvoiceFilters = {}
  if (statusFilter.value !== undefined)
    active.status = statusFilter.value
  if (dateFromFilter.value)
    active.dateFrom = dateFromFilter.value.toISOString()
  if (dateToFilter.value)
    active.dateTo = dateToFilter.value.toISOString()
  return await getAllInvoices(active)
}

const {
  items: invoices,
  loading,
  loadData,
  deleteDialog,
  selectedItem: selectedInvoice,
  handleCreate,
  handleView,
  handleEdit,
  confirmDelete,
  handleDelete,
} = useCrudPage({
  resourceName: 'invoices',
  parentRoute: 'billing',
  basePath: '/billing/invoices',
  loadItems: loadWithFilters,
  deleteItem: deleteInvoice,
})

// Client-side text search on top of server results
const filteredInvoices = computed(() => {
  const q = searchTerm.value.trim().toLowerCase()
  if (!q)
    return invoices.value
  return invoices.value.filter(
    inv =>
      inv.invoiceNumber?.toLowerCase().includes(q)
      || inv.customerName?.toLowerCase().includes(q),
  )
})

// Debounce text search
const searchDebounce = ref<ReturnType<typeof setTimeout>>()
watch(searchTerm, () => {
  clearTimeout(searchDebounce.value)
  searchDebounce.value = setTimeout(() => loadData(), 300)
})

// Re-fetch when server-side filters change
watch(statusFilter, () => loadData())
watch(dateFromFilter, () => loadData())
watch(dateToFilter, () => loadData())

function applyFilters() {
  loadData()
}

function resetFilters() {
  searchTerm.value = ''
  statusFilter.value = undefined
  dateFromFilter.value = undefined
  dateToFilter.value = undefined
  loadData()
}

function getActiveFilterCount(): number {
  let count = 0
  if (searchTerm.value)
    count++
  if (statusFilter.value !== undefined)
    count++
  if (dateFromFilter.value)
    count++
  if (dateToFilter.value)
    count++
  return count
}

// Status options for the Select
const statusOptions = [
  { label: t('invoices.status_draft'), value: InvoiceStatus.Draft },
  { label: t('invoices.status_pending_signature'), value: InvoiceStatus.PendingSignature },
  { label: t('invoices.status_pending_authorization'), value: InvoiceStatus.PendingAuthorization },
  { label: t('invoices.status_authorized'), value: InvoiceStatus.Authorized },
  { label: t('invoices.status_rejected'), value: InvoiceStatus.Rejected },
  { label: t('invoices.status_sent'), value: InvoiceStatus.Sent },
  { label: t('invoices.status_paid'), value: InvoiceStatus.Paid },
  { label: t('invoices.status_overdue'), value: InvoiceStatus.Overdue },
  { label: t('invoices.status_cancelled'), value: InvoiceStatus.Cancelled },
  { label: t('invoices.status_voided'), value: InvoiceStatus.Voided },
]

function getStatusLabel(status: InvoiceStatus): string {
  const opt = statusOptions.find(o => o.value === status)
  return opt ? opt.label : status.toString()
}

function getStatusSeverity(status: InvoiceStatus): string {
  const severities: Record<InvoiceStatus, string> = {
    [InvoiceStatus.Draft]: 'secondary',
    [InvoiceStatus.PendingSignature]: 'info',
    [InvoiceStatus.PendingAuthorization]: 'info',
    [InvoiceStatus.Authorized]: 'success',
    [InvoiceStatus.Rejected]: 'danger',
    [InvoiceStatus.Sent]: 'info',
    [InvoiceStatus.Paid]: 'success',
    [InvoiceStatus.Overdue]: 'danger',
    [InvoiceStatus.Cancelled]: 'warn',
    [InvoiceStatus.Voided]: 'warn',
  }
  return severities[status] || 'secondary'
}

function formatCurrency(amount: number): string {
  return new Intl.NumberFormat('es-EC', { style: 'currency', currency: 'USD' }).format(amount)
}
</script>

<template>
  <div>
    <PageHeader
      :title="t('invoices.title')"
      :description="t('invoices.description')"
    >
      <template #actions>
        <Button
          v-if="can.createInvoice()"
          :label="t('invoices.create')"
          icon="pi pi-plus"
          @click="handleCreate"
        />
      </template>
    </PageHeader>

    <!-- Filter Panel -->
    <Card class="mb-6">
      <template #content>
        <div class="flex items-center justify-between mb-4">
          <div class="flex items-center gap-2">
            <h3 class="text-lg font-semibold text-slate-900 dark:text-white">
              {{ t('invoices.filters') }}
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
          <!-- Text search -->
          <div class="flex flex-col gap-2">
            <label for="inv-search" class="font-semibold text-slate-700 dark:text-slate-200">
              {{ t('common.search') }}
            </label>
            <InputText
              id="inv-search"
              v-model="searchTerm"
              :placeholder="t('invoices.search_placeholder')"
            />
          </div>

          <div class="grid grid-cols-1 gap-4 md:grid-cols-3">
            <!-- Status -->
            <div class="flex flex-col gap-2">
              <label for="inv-status" class="font-semibold text-slate-700 dark:text-slate-200">
                {{ t('invoices.status') }}
              </label>
              <Select
                id="inv-status"
                v-model="statusFilter"
                :options="statusOptions"
                option-label="label"
                option-value="value"
                :placeholder="t('invoices.filter_status_all')"
                show-clear
              />
            </div>

            <!-- Date From -->
            <div class="flex flex-col gap-2">
              <label for="inv-date-from" class="font-semibold text-slate-700 dark:text-slate-200">
                {{ t('invoices.filter_date_from') }}
              </label>
              <DatePicker
                id="inv-date-from"
                v-model="dateFromFilter"
                date-format="dd/mm/yy"
                :placeholder="t('invoices.filter_date_from')"
                show-clear
              />
            </div>

            <!-- Date To -->
            <div class="flex flex-col gap-2">
              <label for="inv-date-to" class="font-semibold text-slate-700 dark:text-slate-200">
                {{ t('invoices.filter_date_to') }}
              </label>
              <DatePicker
                id="inv-date-to"
                v-model="dateToFilter"
                date-format="dd/mm/yy"
                :placeholder="t('invoices.filter_date_to')"
                show-clear
              />
            </div>
          </div>

          <!-- Actions -->
          <div class="flex gap-2 justify-end">
            <Button
              :label="t('common.reset')"
              severity="secondary"
              @click="resetFilters"
            />
            <Button
              :label="t('common.apply')"
              icon="pi pi-filter"
              @click="applyFilters"
            />
          </div>
        </div>
      </template>
    </Card>

    <Card>
      <template #content>
        <DataTable
          :value="filteredInvoices"
          :loading="loading"
          striped-rows
          paginator
          :rows="10"
          :rows-per-page-options="[5, 10, 20, 50]"
          current-page-report-template="{first} to {last} of {totalRecords}"
          paginator-template="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown CurrentPageReport"
        >
          <template #empty>
            <div class="text-center py-8 text-surface-500">
              {{ t('invoices.no_records') }}
            </div>
          </template>

          <Column field="invoiceNumber" :header="t('invoices.invoice_number')" sortable>
            <template #body="{ data }">
              <span class="font-mono font-semibold">{{ data.invoiceNumber }}</span>
            </template>
          </Column>

          <Column field="customerName" :header="t('invoices.customer')" sortable />

          <Column field="emissionPointCode" :header="t('invoices.emission_point_code')" sortable>
            <template #body="{ data }">
              <span v-if="data.emissionPointCode && data.establishmentCode" class="font-mono text-sm">
                {{ data.establishmentCode }}-{{ data.emissionPointCode }}
              </span>
              <span v-else class="text-surface-400">-</span>
            </template>
          </Column>

          <Column field="issueDate" :header="t('invoices.issue_date')" sortable>
            <template #body="{ data }">
              {{ new Date(data.issueDate).toLocaleDateString() }}
            </template>
          </Column>

          <Column field="dueDate" :header="t('invoices.due_date')" sortable>
            <template #body="{ data }">
              {{ new Date(data.dueDate).toLocaleDateString() }}
            </template>
          </Column>

          <Column field="totalAmount" :header="t('invoices.total')" sortable>
            <template #body="{ data }">
              <span class="font-semibold">{{ formatCurrency(data.totalAmount) }}</span>
            </template>
          </Column>

          <Column field="status" :header="t('common.status')" sortable>
            <template #body="{ data }">
              <Tag
                :value="getStatusLabel(data.status)"
                :severity="getStatusSeverity(data.status)"
              />
            </template>
          </Column>

          <Column :header="t('common.actions')" style="width: 10rem">
            <template #body="{ data }">
              <div class="flex gap-2">
                <Button
                  v-if="can.readInvoice()"
                  icon="pi pi-eye"
                  severity="secondary"
                  text
                  rounded
                  @click="handleView(data)"
                />
                <Button
                  v-if="can.updateInvoice() && data.status === InvoiceStatus.Draft"
                  icon="pi pi-pencil"
                  severity="info"
                  text
                  rounded
                  @click="handleEdit(data)"
                />
                <Button
                  v-if="can.deleteInvoice() && data.status === InvoiceStatus.Draft"
                  icon="pi pi-trash"
                  severity="danger"
                  text
                  rounded
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
      :item-name="selectedInvoice?.invoiceNumber"
      @confirm="handleDelete"
    />
  </div>
</template>
