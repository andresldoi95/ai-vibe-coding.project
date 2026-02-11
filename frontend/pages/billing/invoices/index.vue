<script setup lang="ts">
import type { Invoice } from '~/types/billing'
import { InvoiceStatus } from '~/types/billing'

definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const { getAllInvoices, deleteInvoice } = useInvoice()
const { can } = usePermissions()

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
  loadItems: getAllInvoices,
  deleteItem: deleteInvoice,
})

function getStatusLabel(status: InvoiceStatus): string {
  const labels: Record<InvoiceStatus, string> = {
    [InvoiceStatus.Draft]: t('invoices.status_draft'),
    [InvoiceStatus.Sent]: t('invoices.status_sent'),
    [InvoiceStatus.Paid]: t('invoices.status_paid'),
    [InvoiceStatus.Overdue]: t('invoices.status_overdue'),
    [InvoiceStatus.Cancelled]: t('invoices.status_cancelled'),
  }
  return labels[status] || status.toString()
}

function getStatusSeverity(status: InvoiceStatus): string {
  const severities: Record<InvoiceStatus, string> = {
    [InvoiceStatus.Draft]: 'secondary',
    [InvoiceStatus.Sent]: 'info',
    [InvoiceStatus.Paid]: 'success',
    [InvoiceStatus.Overdue]: 'danger',
    [InvoiceStatus.Cancelled]: 'warn',
  }
  return severities[status] || 'secondary'
}

function formatCurrency(amount: number): string {
  return new Intl.NumberFormat('es-EC', {
    style: 'currency',
    currency: 'USD',
  }).format(amount)
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

    <Card>
      <template #content>
        <DataTable
          :value="invoices"
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
