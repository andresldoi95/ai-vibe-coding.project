<script setup lang="ts">
definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const { getAllEstablishments, deleteEstablishment } = useEstablishment()
const { can } = usePermissions()

// Using useCrudPage composable - eliminates boilerplate
const {
  items: establishments,
  loading,
  deleteDialog,
  selectedItem: selectedEstablishment,
  handleCreate,
  handleView,
  handleEdit,
  confirmDelete,
  handleDelete,
} = useCrudPage({
  resourceName: 'establishments',
  parentRoute: 'billing',
  basePath: '/billing/establishments',
  loadItems: getAllEstablishments,
  deleteItem: deleteEstablishment,
})

// Using useStatus composable
const { getStatusLabel, getStatusSeverity } = useStatus()
</script>

<template>
  <div>
    <!-- Page Header -->
    <PageHeader
      :title="t('establishments.title')"
      :description="t('establishments.description')"
    >
      <template #actions>
        <Button
          v-if="can.createEstablishment()"
          :label="t('establishments.create')"
          icon="pi pi-plus"
          @click="handleCreate"
        />
      </template>
    </PageHeader>

    <!-- Data Table Card -->
    <Card>
      <template #content>
        <LoadingState v-if="loading" :message="t('common.loading')" />
        <DataTable
          v-else
          :value="establishments"
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
              :description="can.createEstablishment() ? t('establishments.empty_description') : undefined"
              :action-label="t('establishments.create')"
              action-icon="pi pi-plus"
              @action="handleCreate"
            />
          </template>

          <Column field="establishmentCode" :header="t('establishments.code')" sortable>
            <template #body="{ data }">
              <span class="font-mono font-semibold">{{ data.establishmentCode }}</span>
            </template>
          </Column>

          <Column field="name" :header="t('establishments.name')" sortable />

          <Column field="address" :header="t('establishments.address')" sortable />

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
                :show-view="can.viewEstablishments()"
                :show-edit="can.editEstablishment()"
                :show-delete="can.deleteEstablishment()"
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
      :item-name="selectedEstablishment?.name"
      @confirm="handleDelete"
    />
  </div>
</template>
