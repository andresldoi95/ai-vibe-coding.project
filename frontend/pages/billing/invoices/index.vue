<script setup lang="ts">
import type { Invoice } from '~/types/billing'
import { getInvoiceStatusSeverity, getStatusLabel } from '~/utils/status'

definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const uiStore = useUiStore()
// const { apiFetch } = useApi() // Will be used when backend is ready
const toast = useNotification()
const { formatCurrency, formatDate } = useFormatters()

const invoices = ref<Invoice[]>([])
const loading = ref(false)
const selectedInvoices = ref<Invoice[]>([])

async function loadInvoices() {
  loading.value = true
  try {
    // This will work once backend is ready
    // invoices.value = await apiFetch<Invoice[]>('/billing/invoices')

    // Mock data for now
    invoices.value = []
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

function createInvoice() {
  navigateTo('/billing/invoices/new')
}

function viewInvoice(id: string) {
  navigateTo(`/billing/invoices/${id}`)
}

async function deleteInvoice(_id: string) {
  // Implement delete logic
  toast.showSuccess(t('messages.success_delete'))
}

onMounted(() => {
  uiStore.setBreadcrumbs([
    { label: t('nav.billing') },
    { label: t('nav.invoices') },
  ])
  loadInvoices()
})
</script>

<template>
  <div>
    <!-- Page Header Component - Following UX spacing guidelines -->
    <PageHeader
      :title="t('billing.invoices')"
      description="Manage and track all customer invoices"
    >
      <template #actions>
        <Button
          :label="t('billing.create_invoice')"
          icon="pi pi-plus"
          @click="createInvoice"
        />
      </template>
    </PageHeader>

    <!-- Data Table Card - Using standard card padding (p-6) -->
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
              @action="createInvoice"
            />
          </template>

          <Column selectionMode="multiple" headerStyle="width: 3rem" />

          <Column
            field="invoiceNumber"
            :header="t('billing.invoice_number')"
            sortable
          >
            <template #filter="{ filterModel, filterCallback }">
              <InputText
                v-model="filterModel.value"
                type="text"
                class="p-column-filter"
                :placeholder="t('common.search')"
                @input="filterCallback()"
              />
            </template>
          </Column>

          <Column
            field="customerName"
            :header="t('billing.customer_name')"
            sortable
          >
            <template #filter="{ filterModel, filterCallback }">
              <InputText
                v-model="filterModel.value"
                type="text"
                class="p-column-filter"
                :placeholder="t('common.search')"
                @input="filterCallback()"
              />
            </template>
          </Column>

          <Column
            field="totalAmount"
            :header="t('billing.total_amount')"
            sortable
          >
            <template #body="{ data }">
              {{ formatCurrency(data.totalAmount) }}
            </template>
          </Column>

          <Column field="status" :header="t('common.status')" sortable>
            <template #body="{ data }">
              <Tag
                :value="getStatusLabel(data.status)"
                :severity="getInvoiceStatusSeverity(data.status)"
              />
            </template>
            <template #filter="{ filterModel, filterCallback }">
              <Dropdown
                v-model="filterModel.value"
                :options="['draft', 'sent', 'paid', 'overdue', 'cancelled']"
                :placeholder="t('common.status')"
                class="p-column-filter"
                showClear
                @change="filterCallback()"
              >
                <template #option="slotProps">
                  <Tag
                    :value="getStatusLabel(slotProps.option)"
                    :severity="getInvoiceStatusSeverity(slotProps.option)"
                  />
                </template>
              </Dropdown>
            </template>
          </Column>

          <Column field="issueDate" :header="t('billing.issue_date')" sortable>
            <template #body="{ data }">
              {{ formatDate(data.issueDate) }}
            </template>
          </Column>

          <Column field="dueDate" :header="t('billing.due_date')" sortable>
            <template #body="{ data }">
              {{ formatDate(data.dueDate) }}
            </template>
          </Column>

          <Column :header="t('common.actions')">
            <template #body="{ data }">
              <DataTableActions
                :show-view="true"
                :show-edit="false"
                :show-delete="true"
                @view="viewInvoice(data.id)"
                @delete="deleteInvoice(data.id)"
              />
            </template>
          </Column>
        </DataTable>
      </template>
    </Card>
  </div>
</template>
