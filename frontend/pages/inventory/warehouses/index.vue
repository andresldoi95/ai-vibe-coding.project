<script setup lang="ts">
definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const { getAllWarehouses, deleteWarehouse, exportWarehouseStockSummary } = useWarehouse()
const { can } = usePermissions()

// ✅ Using useCrudPage composable - eliminates ~40 lines of boilerplate
const {
  items: warehouses,
  loading,
  deleteDialog,
  selectedItem: selectedWarehouse,
  handleCreate,
  handleView,
  handleEdit,
  confirmDelete,
  handleDelete,
} = useCrudPage({
  resourceName: 'warehouses',
  parentRoute: 'inventory',
  basePath: '/inventory/warehouses',
  loadItems: getAllWarehouses,
  deleteItem: deleteWarehouse,
})

// ✅ Using useStatus composable - eliminates ~9 lines
const { getStatusLabel, getStatusSeverity } = useStatus()

// Export functionality
const exportDialog = ref(false)
const exporting = ref(false)

function openExportDialog() {
  exportDialog.value = true
}

async function handleExport({ format }: { format: string, filters?: Record<string, unknown> }) {
  exporting.value = true
  try {
    await exportWarehouseStockSummary({ format: format as 'csv' | 'excel' })
    const toast = useNotification()
    toast.showSuccess(t('warehouses.export_success'))
    exportDialog.value = false
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    const toast = useNotification()
    toast.showError(t('warehouses.export_error'), errMessage)
  }
  finally {
    exporting.value = false
  }
}
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
          @click="handleCreate"
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
              @action="handleCreate"
            />
          </template>

          <Column field="name" :header="t('warehouses.name')" sortable />
          <Column field="code" :header="t('warehouses.code')" sortable />

          <Column field="location" :header="t('warehouses.location')" sortable>
            <template #body="{ data }">
              {{ data.city }}, {{ data.countryName }}
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
                @view="handleView(data)"
                @edit="handleEdit(data)"
                @delete="confirmDelete(data)"
              />
            </template>
          </Column>
        </DataTable>
      </template>
    </Card>

    <!-- ✅ Using DeleteConfirmDialog component - eliminates ~25 lines -->
    <DeleteConfirmDialog
      v-model:visible="deleteDialog"
      :item-name="selectedWarehouse?.name"
      @confirm="handleDelete"
    />

    <!-- ✅ Using ExportDialog component - eliminates ~35 lines -->
    <ExportDialog
      v-model:visible="exportDialog"
      :title="t('warehouses.export_dialog_title')"
      :description="t('warehouses.export_description')"
      :loading="exporting"
      @export="handleExport"
    />
  </div>
</template>
