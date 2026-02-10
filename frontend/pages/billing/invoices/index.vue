<script setup lang="ts">
import type { Invoice } from '~/types/billing'
import { getInvoiceStatusSeverity, getStatusLabel } from '~/utils/status'

definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const toast = useNotification()
const { formatCurrency, formatDate } = useFormatters()

// Simple load function for invoices (backend not ready)
async function loadInvoices() {
  // This will work once backend is ready
  // return await apiFetch<Invoice[]>('/billing/invoices')

  // Mock data for now
  toast.showInfo(t('messages.error_load'), 'Backend not connected')
  return []
}

// Simple delete function (backend not ready)
async function deleteInvoice(_id: string) {
  toast.showSuccess(t('messages.success_delete'))
}

// ✅ Using useCrudPage composable
const {
  items: invoices,
  loading,
  deleteDialog,
  selectedItem: selectedInvoice,
  handleCreate,
  handleView,
  confirmDelete,
  handleDelete,
} = useCrudPage({
  resourceName: 'invoices',
  parentRoute: 'billing',
  basePath: '/billing/invoices',
  loadItems: loadInvoices,
  deleteItem: id => deleteInvoice(id),
})

const selectedInvoices = ref<Invoice[]>([])
</script>

<template>
  <div>
    <!-- Page Header Component -->
    <PageHeader
      :title="t('billing.invoices')"
      description="Manage and track all customer invoices"
    >
      <template #actions>
        <Button
          :label="t('billing.create_invoice')"
          icon="pi pi-plus"
          @click="handleCreate"
        />
      </template>
    </PageHeader>

    <!-- Data Table Card -->
    <Card>
      <template #content>
        <LoadingState v-if="loading" message="Loading invoices..." />
        <DataTable
          v-else
          v-model:selection="selectedInvoices"
          :value="invoices"
          :paginator="true"
          :rows="10"
          :rowsPerPageOptions="[10, 25, 50]"
          stripedRows
          responsiveLayout="scroll"
          filterDisplay="row"
        >
          <template #empty>
            <EmptyState
              icon="pi pi-file"
              :title="t('common.no_data')"
              description="Create your first invoice to get started with billing"
              :actionLabel="t('billing.create_invoice')"
              actionIcon="pi pi-plus"
              @action="handleCreate"
            />
          </template>

          <Column
            selectionMode="multiple"
            headerStyle="width: 3rem"
          />

          <Column
            field="number"
            header="Invoice #"
            sortable
          >
            <template #body="{ data }">
              <span class="font-mono text-sm">{{ data.number }}</span>
            </template>
          </Column>

          <Column
            field="customer"
            header="Customer"
            sortable
          />

          <Column
            field="issueDate"
            header="Issue Date"
            sortable
          >
            <template #body="{ data }">
              {{ formatDate(data.issueDate) }}
            </template>
          </Column>

          <Column
            field="dueDate"
            header="Due Date"
            sortable
          >
            <template #body="{ data }">
              {{ formatDate(data.dueDate) }}
            </template>
          </Column>

          <Column
            field="amount"
            header="Amount"
            sortable
          >
            <template #body="{ data }">
              {{ formatCurrency(data.amount) }}
            </template>
          </Column>

          <Column
            field="status"
            header="Status"
            sortable
          >
            <template #body="{ data }">
              <Tag
                :value="getStatusLabel(data.status)"
                :severity="getInvoiceStatusSeverity(data.status)"
              />
            </template>
          </Column>

          <Column header="Actions">
            <template #body="{ data }">
              <div class="flex gap-2">
                <Button
                  icon="pi pi-eye"
                  text
                  rounded
                  severity="info"
                  @click="handleView(data)"
                />
                <Button
                  icon="pi pi-trash"
                  text
                  rounded
                  severity="danger"
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
      :item-name="selectedInvoice?.number"
      @confirm="handleDelete"
    />
  </div>
</template>
