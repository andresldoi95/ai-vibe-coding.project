<script setup lang="ts">
import type { Warehouse } from '~/types/inventory'

definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const uiStore = useUiStore()
const toast = useNotification()
const route = useRoute()
const router = useRouter()
const { can } = usePermissions()
const { getWarehouseById, deleteWarehouse } = useWarehouse()

const warehouse = ref<Warehouse | null>(null)
const loading = ref(false)
const deleteDialog = ref(false)

async function loadWarehouse() {
  loading.value = true
  try {
    const id = route.params.id as string
    warehouse.value = await getWarehouseById(id)
    uiStore.setBreadcrumbs([
      { label: t('nav.inventory'), to: '/inventory' },
      { label: t('warehouses.title'), to: '/inventory/warehouses' },
      { label: warehouse.value.name },
    ])
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('messages.error_load'), errMessage)
    router.push('/inventory/warehouses')
  }
  finally {
    loading.value = false
  }
}

function editWarehouse() {
  router.push(`/inventory/warehouses/${route.params.id}/edit`)
}

async function handleDelete() {
  if (!warehouse.value)
    return

  try {
    await deleteWarehouse(warehouse.value.id)
    toast.showSuccess(t('messages.success_delete'), t('warehouses.deleted_successfully'))
    router.push('/inventory/warehouses')
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('messages.error_delete'), errMessage)
  }
  finally {
    deleteDialog.value = false
  }
}

function getFullAddress(wh: Warehouse): string {
  const parts = [
    wh.streetAddress,
    wh.city,
    wh.state,
    wh.postalCode,
    wh.countryName,
  ].filter(Boolean)
  return parts.join(', ')
}

onMounted(() => {
  loadWarehouse()
})
</script>

<template>
  <div>
    <LoadingState v-if="loading" :message="t('common.loading')" />

    <div v-else-if="warehouse">
      <!-- Page Header -->
      <PageHeader
        :title="warehouse.name"
        :description="warehouse.description"
      >
        <template #actions>
          <Button
            v-if="can.editWarehouse()"
            :label="t('common.edit')"
            icon="pi pi-pencil"
            @click="editWarehouse"
          />
          <Button
            v-if="can.deleteWarehouse()"
            :label="t('common.delete')"
            icon="pi pi-trash"
            severity="danger"
            outlined
            @click="deleteDialog = true"
          />
        </template>
      </PageHeader>

      <!-- Details Card -->
      <Card>
        <template #content>
          <div class="grid grid-cols-1 gap-8 lg:grid-cols-2">
            <!-- Basic Information -->
            <div>
              <h3 class="mb-4 text-lg font-semibold text-slate-900 dark:text-white">
                {{ t('warehouses.basic_info') }}
              </h3>
              <div class="space-y-4">
                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('warehouses.name') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ warehouse.name }}
                  </p>
                </div>

                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('warehouses.code') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ warehouse.code }}
                  </p>
                </div>

                <div v-if="warehouse.description">
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('common.description') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ warehouse.description }}
                  </p>
                </div>

                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('common.status') }}
                  </label>
                  <div class="mt-1">
                    <Tag
                      :value="warehouse.isActive ? t('common.active') : t('common.inactive')"
                      :severity="warehouse.isActive ? 'success' : 'danger'"
                    />
                  </div>
                </div>
              </div>
            </div>

            <!-- Address Information -->
            <div>
              <h3 class="mb-4 text-lg font-semibold text-slate-900 dark:text-white">
                {{ t('warehouses.address_info') }}
              </h3>
              <div class="space-y-4">
                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('warehouses.full_address') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ getFullAddress(warehouse) }}
                  </p>
                </div>

                <div v-if="warehouse.phone">
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('common.phone') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ warehouse.phone }}
                  </p>
                </div>

                <div v-if="warehouse.email">
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('common.email') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ warehouse.email }}
                  </p>
                </div>
              </div>
            </div>

            <!-- Additional Information -->
            <div>
              <h3 class="mb-4 text-lg font-semibold text-slate-900 dark:text-white">
                {{ t('warehouses.additional_info') }}
              </h3>
              <div class="space-y-4">
                <div v-if="warehouse.squareFootage">
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('warehouses.square_footage') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ warehouse.squareFootage.toLocaleString() }} {{ t('warehouses.sq_ft') }}
                  </p>
                </div>

                <div v-if="warehouse.capacity">
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('warehouses.capacity') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ warehouse.capacity.toLocaleString() }} {{ t('warehouses.units') }}
                  </p>
                </div>
              </div>
            </div>

            <!-- Audit Information -->
            <div>
              <h3 class="mb-4 text-lg font-semibold text-slate-900 dark:text-white">
                {{ t('common.audit_info') }}
              </h3>
              <div class="space-y-4">
                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('common.created_at') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ new Date(warehouse.createdAt).toLocaleString() }}
                  </p>
                </div>

                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('common.updated_at') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ new Date(warehouse.updatedAt).toLocaleString() }}
                  </p>
                </div>
              </div>
            </div>
          </div>
        </template>
      </Card>
    </div>

    <!-- Delete Confirmation Dialog -->
    <Dialog
      v-model:visible="deleteDialog"
      :header="t('common.confirm')"
      :modal="true"
      :style="{ width: '450px' }"
    >
      <div class="flex items-center gap-4">
        <i class="pi pi-exclamation-triangle text-3xl text-orange-500" />
        <span v-if="warehouse">
          {{ t('warehouses.confirm_delete', { name: warehouse.name }) }}
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
