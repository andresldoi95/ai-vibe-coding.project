<script setup lang="ts">
import type { Warehouse } from '~/types/inventory'

definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const uiStore = useUiStore()
const toast = useNotification()
const { getAllWarehouses, deleteWarehouse, exportWarehouseStockSummary } = useWarehouse()
const { can } = usePermissions()

const warehouses = ref<Warehouse[]>([])
const loading = ref(false)
const deleteDialog = ref(false)
const exportDialog = ref(false)
const exporting = ref(false)
const selectedWarehouse = ref<Warehouse | null>(null)

// Export format
const exportFormat = ref<'csv' | 'excel'>('excel')

async function loadWarehouses() {
  loading.value = true
  try {
    warehouses.value = await getAllWarehouses()
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('messages.error_load'), errMessage)
  }
  finally {
    loading.value = false
  }
}

function createWarehouse() {
  navigateTo('/inventory/warehouses/new')
}

function confirmDelete(warehouse: Warehouse) {
  selectedWarehouse.value = warehouse
  deleteDialog.value = true
}

async function handleDelete() {
  if (!selectedWarehouse.value)
    return

  try {
    await deleteWarehouse(selectedWarehouse.value.id)
    toast.showSuccess(t('messages.success_delete'), t('warehouses.deleted_successfully'))
    await loadWarehouses()
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('messages.error_delete'), errMessage)
  }
  finally {
    deleteDialog.value = false
    selectedWarehouse.value = null
  }
}

function getStatusLabel(isActive: boolean): string {
  return isActive ? t('common.active') : t('common.inactive')
}

function getStatusSeverity(isActive: boolean): 'success' | 'danger' {
  return isActive ? 'success' : 'danger'
}

function openExportDialog() {
  exportDialog.value = true
}

async function handleExport() {
  exporting.value = true
  try {
    await exportWarehouseStockSummary({ format: exportFormat.value })
    toast.showSuccess(t('warehouses.export_success'))
    exportDialog.value = false
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('warehouses.export_error'), errMessage)
  }
  finally {
    exporting.value = false
  }
}

onMounted(() => {
  uiStore.setBreadcrumbs([
    { label: t('nav.inventory'), to: '/inventory' },
    { label: t('warehouses.title') },
  ])
  loadWarehouses()
})
</script>

<template>
  <div>
    <!-- Page Header Component - Following UX spacing guidelines -->
    <PageHeader
      :title="t('warehouses.title')"
      :description="t('warehouses.description')"
    >
      <template #actions>
        <Button
          :label="t('warehouses.export_stock_summary')"
          icon="pi pi-download"
          severity="secondary"
          outlined
          @click="openExportDialog"
        />
        <Button
          v-if="can.createWarehouse()"
          :label="t('warehouses.create')"
          icon="pi pi-plus"
          @click="createWarehouse"
        />
      </template>
    </PageHeader>

    <!-- Data Table Card - Using standard card padding (p-6) -->
    <Card>
      <template #content>
        <LoadingState v-if="loading" :message="t('common.loading')" />
        <DataTable
          v-else
          :value="warehouses"
          :paginator="true"
          :rows="10"
          :rows-per-page-options="[10, 25, 50]"
          striped-rows
          responsive-layout="scroll"
        >
          <template #empty>
            <EmptyState
              icon="pi pi-building"
              :title="t('common.no_data')"
              :description="can.createWarehouse() ? t('warehouses.get_started') : undefined"
              :action-label="t('warehouses.create')"
              action-icon="pi pi-plus"
              @action="createWarehouse"
            />
          </template>

          <Column field="name" :header="t('warehouses.name')" sortable />
          <Column field="code" :header="t('warehouses.code')" sortable />

          <Column field="location" :header="t('warehouses.location')" sortable>
            <template #body="{ data }">
              {{ data.city }}, {{ data.country }}
            </template>
          </Column>

          <Column field="email" :header="t('common.email')" sortable />
          <Column field="phone" :header="t('common.phone')" sortable />

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
                :show-view="can.viewWarehouses()"
                :show-edit="can.editWarehouse()"
                :show-delete="can.deleteWarehouse()"
                @view="navigateTo(`/inventory/warehouses/${data.id}`)"
                @edit="navigateTo(`/inventory/warehouses/${data.id}/edit`)"
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
        <span v-if="selectedWarehouse">
          {{ t('warehouses.confirm_delete', { name: selectedWarehouse.name }) }}
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

    <!-- Export Dialog -->
    <Dialog
      v-model:visible="exportDialog"
      :header="t('warehouses.export_dialog_title')"
      :modal="true"
      :style="{ width: '450px' }"
    >
      <div class="space-y-4">
        <p class="text-sm text-gray-600 dark:text-gray-400">
          Export current stock levels for all warehouses and products.
        </p>
        
        <!-- Export Format -->
        <div>
          <label class="block text-sm font-medium mb-2">{{ t('warehouses.export_format') }}</label>
          <div class="flex gap-4">
            <div class="flex items-center">
              <RadioButton v-model="exportFormat" input-id="wh-format-excel" value="excel" />
              <label for="wh-format-excel" class="ml-2">{{ t('warehouses.export_excel') }}</label>
            </div>
            <div class="flex items-center">
              <RadioButton v-model="exportFormat" input-id="wh-format-csv" value="csv" />
              <label for="wh-format-csv" class="ml-2">{{ t('warehouses.export_csv') }}</label>
            </div>
          </div>
        </div>
      </div>

      <template #footer>
        <Button
          :label="t('common.cancel')"
          severity="secondary"
          outlined
          :disabled="exporting"
          @click="exportDialog = false"
        />
        <Button
          :label="exporting ? t('warehouses.exporting') : t('warehouses.export')"
          icon="pi pi-download"
          :loading="exporting"
          @click="handleExport"
        />
      </template>
    </Dialog>
  </div>
</template>
