<script setup lang="ts">
import type { Warehouse } from '~/types/inventory'

definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const uiStore = useUiStore()
const toast = useNotification()
const { getAllWarehouses, deleteWarehouse } = useWarehouse()

const warehouses = ref<Warehouse[]>([])
const loading = ref(false)
const deleteDialog = ref(false)
const selectedWarehouse = ref<Warehouse | null>(null)

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

function getFullAddress(warehouse: Warehouse): string {
  const parts = [
    warehouse.streetAddress,
    warehouse.city,
    warehouse.state,
    warehouse.postalCode,
    warehouse.country,
  ].filter(Boolean)
  return parts.join(', ')
}

onMounted(() => {
  uiStore.setBreadcrumbs([
    { label: t('nav.inventory') },
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
              :description="t('warehouses.empty_description')"
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
                :value="data.isActive ? t('common.active') : t('common.inactive')"
                :severity="data.isActive ? 'success' : 'danger'"
              />
            </template>
          </Column>

          <Column :header="t('common.actions')">
            <template #body="{ data }">
              <DataTableActions
                :show-view="true"
                :show-edit="true"
                :show-delete="true"
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
  </div>
</template>
