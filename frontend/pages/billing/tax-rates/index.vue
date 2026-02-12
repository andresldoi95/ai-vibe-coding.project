<script setup lang="ts">
definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const { getAllTaxRates, deleteTaxRate } = useTaxRate()
const { can } = usePermissions()

const {
  items: taxRates,
  loading,
  deleteDialog,
  selectedItem: selectedTaxRate,
  handleCreate,
  handleView,
  handleEdit,
  confirmDelete,
  handleDelete,
} = useCrudPage({
  resourceName: 'taxrates',
  parentRoute: 'billing',
  basePath: '/billing/tax-rates',
  loadItems: getAllTaxRates,
  deleteItem: deleteTaxRate,
})

const { getStatusLabel, getStatusSeverity } = useStatus()

function formatRate(rate: number): string {
  return `${(rate * 100).toFixed(2)}%`
}
</script>

<template>
  <div>
    <PageHeader
      :title="t('taxRates.title')"
      :description="t('taxRates.description')"
    >
      <template #actions>
        <Button
          v-if="can.createTaxRate()"
          :label="t('taxRates.create')"
          icon="pi pi-plus"
          @click="handleCreate"
        />
      </template>
    </PageHeader>

    <Card>
      <template #content>
        <DataTable
          :value="taxRates"
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
              {{ t('taxRates.no_records') }}
            </div>
          </template>

          <Column field="code" :header="t('taxRates.code')" sortable />
          <Column field="name" :header="t('taxRates.name')" sortable />

          <Column field="rate" :header="t('taxRates.rate')" sortable>
            <template #body="{ data }">
              <span class="font-semibold">{{ formatRate(data.rate) }}</span>
            </template>
          </Column>

          <Column field="countryName" :header="t('taxRates.country')" sortable />

          <Column field="isDefault" :header="t('taxRates.default')" sortable>
            <template #body="{ data }">
              <Tag
                v-if="data.isDefault"
                :value="t('common.yes')"
                severity="info"
              />
              <span v-else class="text-surface-500">{{ t('common.no') }}</span>
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

          <Column :header="t('common.actions')" style="width: 12rem">
            <template #body="{ data }">
              <div class="flex gap-2">
                <Button
                  v-if="can.readTaxRate()"
                  icon="pi pi-eye"
                  severity="secondary"
                  text
                  rounded
                  @click="handleView(data.id)"
                />
                <Button
                  v-if="can.updateTaxRate()"
                  icon="pi pi-pencil"
                  severity="secondary"
                  text
                  rounded
                  @click="handleEdit(data.id)"
                />
                <Button
                  v-if="can.deleteTaxRate()"
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

    <!-- Delete Confirmation Dialog -->
    <Dialog
      v-model:visible="deleteDialog"
      :header="t('taxRates.delete_confirm_title')"
      :modal="true"
      class="w-[450px]"
    >
      <div class="flex items-start gap-4">
        <i class="pi pi-exclamation-triangle text-4xl text-yellow-500" />
        <div class="flex-1">
          <p class="mb-2">
            {{ t('taxRates.delete_confirm_message') }}
          </p>
          <p class="font-semibold">
            {{ selectedTaxRate?.name }} ({{ selectedTaxRate?.code }})
          </p>
        </div>
      </div>
      <template #footer>
        <Button
          :label="t('common.cancel')"
          severity="secondary"
          text
          @click="deleteDialog = false"
        />
        <Button
          :label="t('common.delete')"
          severity="danger"
          @click="handleDelete"
        />
      </template>
    </Dialog>
  </div>
</template>
