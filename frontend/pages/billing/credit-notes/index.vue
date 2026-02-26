<script setup lang="ts">
import { InvoiceStatus } from '~/types/billing'
import type { CreditNote } from '~/types/billing'

definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const { getAllCreditNotes, deleteCreditNote } = useCreditNote()
const { can } = usePermissions()

const {
  items: creditNotes,
  loading,
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
  loadItems: getAllCreditNotes,
  deleteItem: deleteCreditNote,
})

function getStatusLabel(status: InvoiceStatus): string {
  const labels: Record<InvoiceStatus, string> = {
    [InvoiceStatus.Draft]: t('invoices.status_draft'),
    [InvoiceStatus.PendingSignature]: t('invoices.status_pending_signature'),
    [InvoiceStatus.PendingAuthorization]: t('invoices.status_pending_authorization'),
    [InvoiceStatus.Authorized]: t('invoices.status_authorized'),
    [InvoiceStatus.Rejected]: t('invoices.status_rejected'),
    [InvoiceStatus.Sent]: t('invoices.status_sent'),
    [InvoiceStatus.Paid]: t('invoices.status_paid'),
    [InvoiceStatus.Overdue]: t('invoices.status_overdue'),
    [InvoiceStatus.Cancelled]: t('invoices.status_cancelled'),
    [InvoiceStatus.Voided]: t('invoices.status_voided'),
  }
  return labels[status] || status.toString()
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

    <Card>
      <template #content>
        <DataTable
          :value="creditNotes"
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
    <ConfirmDialog
      v-model:visible="deleteDialog"
      :header="t('creditNotes.delete_title')"
      :message="t('creditNotes.delete_confirm', { number: selectedCreditNote?.creditNoteNumber })"
      :loading="loading"
      @confirm="handleDelete"
    />
  </div>
</template>
