<script setup lang="ts">
import { InvoiceStatus } from '~/types/billing'
import type { CreditNoteFilters } from '~/types/billing'

definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const { getAllCreditNotes, deleteCreditNote } = useCreditNote()
const { can } = usePermissions()

// â”€â”€ Filter state â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
const showFilters = ref(true)
const searchTerm = ref('')
const statusFilter = ref<InvoiceStatus | undefined>(undefined)
const dateFromFilter = ref<Date | undefined>(undefined)
const dateToFilter = ref<Date | undefined>(undefined)

// Server-side filter loader
async function loadWithFilters() {
  const active: CreditNoteFilters = {}
  if (statusFilter.value !== undefined)
    active.status = statusFilter.value
  if (dateFromFilter.value)
    active.dateFrom = dateFromFilter.value.toISOString()
  if (dateToFilter.value)
    active.dateTo = dateToFilter.value.toISOString()
  return await getAllCreditNotes(active)
}

const {
  items: creditNotes,
  loading,
  loadData,
  deleteDialog,
  selectedItem: selectedCreditNote,
  handleCreate,
  handleView,
  handleEdit,
  confirmDelete,
  handleDelete,
} = useCrudPage({
  resourceName: 'creditNotes',
  parentRoute: 'billing',
  basePath: '/billing/credit-notes',
  loadItems: loadWithFilters,
  deleteItem: deleteCreditNote,
})

// Client-side text search on top of server results
const filteredCreditNotes = computed(() => {
  const q = searchTerm.value.trim().toLowerCase()
  if (!q)
    return creditNotes.value
  return creditNotes.value.filter(
    cn =>
      cn.creditNoteNumber?.toLowerCase().includes(q)
      || cn.customerName?.toLowerCase().includes(q)
      || cn.originalInvoiceNumber?.toLowerCase().includes(q),
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
      :title="t('creditNotes.title')"
      :description="t('creditNotes.description')"
    >
      <template #actions>
        <Button
          v-if="can.createCreditNote()"
          :label="t('creditNotes.create')"
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
              {{ t('creditNotes.filters') }}
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
            <label for="cn-search" class="font-semibold text-slate-700 dark:text-slate-200">
              {{ t('common.search') }}
            </label>
            <InputText
              id="cn-search"
              v-model="searchTerm"
              :placeholder="t('creditNotes.search_placeholder')"
            />
          </div>

          <div class="grid grid-cols-1 gap-4 md:grid-cols-3">
            <!-- Status -->
            <div class="flex flex-col gap-2">
              <label for="cn-status" class="font-semibold text-slate-700 dark:text-slate-200">
                {{ t('creditNotes.status') }}
              </label>
              <Select
                id="cn-status"
                v-model="statusFilter"
                :options="statusOptions"
                option-label="label"
                option-value="value"
                :placeholder="t('creditNotes.filter_status_all')"
                show-clear
              />
            </div>

            <!-- Date From -->
            <div class="flex flex-col gap-2">
              <label for="cn-date-from" class="font-semibold text-slate-700 dark:text-slate-200">
                {{ t('creditNotes.filter_date_from') }}
              </label>
              <DatePicker
                id="cn-date-from"
                v-model="dateFromFilter"
                date-format="dd/mm/yy"
                :placeholder="t('creditNotes.filter_date_from')"
                show-clear
              />
            </div>

            <!-- Date To -->
            <div class="flex flex-col gap-2">
              <label for="cn-date-to" class="font-semibold text-slate-700 dark:text-slate-200">
                {{ t('creditNotes.filter_date_to') }}
              </label>
              <DatePicker
                id="cn-date-to"
                v-model="dateToFilter"
                date-format="dd/mm/yy"
                :placeholder="t('creditNotes.filter_date_to')"
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
          :value="filteredCreditNotes"
          :loading="loading"
          striped-rows
          paginator
          :rows="10"
          :rows-per-page-options="[10, 25, 50]"
          responsive-layout="scroll"
        >
          <template #empty>
            <div class="text-center py-8 text-surface-500">
              {{ t('creditNotes.no_records') }}
            </div>
          </template>

          <Column field="creditNoteNumber" :header="t('creditNotes.number')" sortable />
          <Column field="customerName" :header="t('creditNotes.customer')" sortable />
          <Column field="originalInvoiceNumber" :header="t('creditNotes.original_invoice')" sortable />
          <Column field="issueDate" :header="t('creditNotes.issue_date')" sortable>
            <template #body="{ data }">
              {{ new Date(data.issueDate).toLocaleDateString('es-EC') }}
            </template>
          </Column>
          <Column field="valueModification" :header="t('creditNotes.value_modification')" sortable>
            <template #body="{ data }">
              {{ formatCurrency(data.valueModification) }}
            </template>
          </Column>
          <Column field="status" :header="t('creditNotes.status')" sortable>
            <template #body="{ data }">
              <Tag
                :value="getStatusLabel(data.status)"
                :severity="getStatusSeverity(data.status)"
              />
            </template>
          </Column>
          <Column :header="t('common.actions')" style="width: 12rem">
            <template #body="{ data }">
              <div class="flex gap-2">
                <Button
                  icon="pi pi-eye"
                  size="small"
                  severity="info"
                  text
                  :title="t('common.view')"
                  @click="handleView(data)"
                />
                <Button
                  v-if="can.updateCreditNote() && data.isEditable"
                  icon="pi pi-pencil"
                  size="small"
                  severity="secondary"
                  text
                  :title="t('common.edit')"
                  @click="handleEdit(data)"
                />
                <Button
                  v-if="can.deleteCreditNote() && data.isEditable"
                  icon="pi pi-trash"
                  size="small"
                  severity="danger"
                  text
                  :title="t('common.delete')"
                  @click="confirmDelete(data)"
                />
              </div>
            </template>
          </Column>
        </DataTable>
      </template>
    </Card>

    <!-- Delete Confirmation Dialog -->
    <DeleteConfirmDialog
      v-model:visible="deleteDialog"
      :item-name="selectedCreditNote?.creditNoteNumber"
      @confirm="handleDelete"
    />
  </div>
</template>
