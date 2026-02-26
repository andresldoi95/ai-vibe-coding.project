<script setup lang="ts">
import type { Establishment } from '~/types/establishment'

definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const route = useRoute()
const { getAllEmissionPoints, deleteEmissionPoint } = useEmissionPoint()
const { getAllEstablishments } = useEstablishment()
const { can } = usePermissions()

// Filters
const selectedEstablishment = ref<string | undefined>(
  route.query.establishment as string | undefined,
)
const selectedStatus = ref<boolean | undefined>(undefined)
const establishments = ref<Establishment[]>([])

// Using useCrudPage composable with custom load function
const {
  items: emissionPoints,
  loading,
  deleteDialog,
  selectedItem: selectedEmissionPoint,
  handleCreate,
  handleView,
  handleEdit,
  confirmDelete,
  handleDelete,
  loadData,
} = useCrudPage({
  resourceName: 'emissionPoints',
  parentRoute: 'billing',
  basePath: '/billing/emission-points',
  loadItems: async () => {
    return getAllEmissionPoints({
      establishmentId: selectedEstablishment.value,
      isActive: selectedStatus.value,
    })
  },
  deleteItem: deleteEmissionPoint,
})

// Using useStatus composable
const { getStatusLabel, getStatusSeverity } = useStatus()

// Load establishments for filter
async function loadEstablishments() {
  try {
    establishments.value = await getAllEstablishments()
  }
  catch (error) {
    console.error('Failed to load establishments:', error)
  }
}

// Watch filters and reload data
watch([selectedEstablishment, selectedStatus], () => {
  loadData()
})

// Establishment filter options
const establishmentOptions = computed(() => [
  { label: t('emissionPoints.all_emission_points'), value: undefined },
  ...establishments.value.map(e => ({
    label: `${e.establishmentCode} - ${e.name}`,
    value: e.id,
  })),
])

// Status filter options
const statusOptions = computed(() => [
  { label: t('emissionPoints.all_emission_points'), value: undefined },
  { label: t('emissionPoints.active_emission_points'), value: true },
  { label: t('emissionPoints.inactive_emission_points'), value: false },
])

onMounted(() => {
  loadEstablishments()
})
</script>

<template>
  <div>
    <!-- Page Header -->
    <PageHeader
      :title="t('emissionPoints.title')"
      :description="t('emissionPoints.description')"
    >
      <template #actions>
        <Button
          v-if="can.createEmissionPoint()"
          :label="t('emissionPoints.create')"
          icon="pi pi-plus"
          @click="handleCreate"
        />
      </template>
    </PageHeader>

    <!-- Filters Card -->
    <Card class="mb-6">
      <template #content>
        <div class="grid grid-cols-1 gap-4 md:grid-cols-2">
          <!-- Establishment Filter -->
          <div>
            <label for="establishment-filter" class="mb-2 block text-sm font-medium text-slate-700 dark:text-slate-300">
              {{ t('emissionPoints.filter_by_establishment') }}
            </label>
            <Dropdown
              id="establishment-filter"
              v-model="selectedEstablishment"
              :options="establishmentOptions"
              option-label="label"
              option-value="value"
              :placeholder="t('emissionPoints.all_emission_points')"
              class="w-full"
            />
          </div>

          <!-- Status Filter -->
          <div>
            <label for="status-filter" class="mb-2 block text-sm font-medium text-slate-700 dark:text-slate-300">
              {{ t('common.status') }}
            </label>
            <Dropdown
              id="status-filter"
              v-model="selectedStatus"
              :options="statusOptions"
              option-label="label"
              option-value="value"
              :placeholder="t('common.all_statuses')"
              class="w-full"
            />
          </div>
        </div>
      </template>
    </Card>

    <!-- Data Table Card -->
    <Card>
      <template #content>
        <LoadingState v-if="loading" :message="t('common.loading')" />
        <DataTable
          v-else
          :value="emissionPoints"
          :paginator="true"
          :rows="10"
          :rows-per-page-options="[10, 25, 50]"
          striped-rows
          responsive-layout="scroll"
        >
          <template #empty>
            <EmptyState
              icon="pi pi-sitemap"
              :title="t('common.no_data')"
              :description="can.createEmissionPoint() ? t('emissionPoints.empty_description') : undefined"
              :action-label="t('emissionPoints.create')"
              action-icon="pi pi-plus"
              @action="handleCreate"
            />
          </template>

          <Column field="emissionPointCode" :header="t('emissionPoints.code')" sortable>
            <template #body="{ data }">
              <span class="font-mono font-semibold">{{ data.emissionPointCode }}</span>
            </template>
          </Column>

          <Column field="name" :header="t('emissionPoints.name')" sortable />

          <Column field="establishmentCode" :header="t('emissionPoints.establishment')" sortable>
            <template #body="{ data }">
              <span v-if="data.establishmentCode">
                {{ data.establishmentCode }} - {{ data.establishmentName }}
              </span>
              <span v-else class="text-slate-400">
                {{ t('common.not_available') }}
              </span>
            </template>
          </Column>

          <Column :header="t('emissionPoints.sequential_numbers')">
            <template #body="{ data }">
              <div class="flex flex-wrap gap-2">
                <Tag severity="info" :value="`INV: ${data.invoiceSequence}`" />
                <Tag severity="success" :value="`CR: ${data.creditNoteSequence}`" />
                <Tag severity="warning" :value="`DB: ${data.debitNoteSequence}`" />
                <Tag severity="danger" :value="`RET: ${data.retentionSequence}`" />
              </div>
            </template>
          </Column>

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
                :show-view="can.viewEmissionPoints()"
                :show-edit="can.editEmissionPoint()"
                :show-delete="can.deleteEmissionPoint()"
                @view="handleView(data)"
                @edit="handleEdit(data)"
                @delete="confirmDelete(data)"
              />
            </template>
          </Column>
        </DataTable>
      </template>
    </Card>

    <!-- Delete Confirmation Dialog -->
    <DeleteConfirmDialog
      v-model:visible="deleteDialog"
      :item-name="selectedEmissionPoint?.name"
      @confirm="handleDelete"
    />
  </div>
</template>
