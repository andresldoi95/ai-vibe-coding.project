<script setup lang="ts">
import type { EmissionPoint } from '~/types/emission-point'

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
const { getEmissionPointById, deleteEmissionPoint } = useEmissionPoint()
const { getStatusLabel, getStatusSeverity } = useStatus()

const emissionPoint = ref<EmissionPoint | null>(null)
const loading = ref(false)
const deleteDialog = ref(false)

async function loadEmissionPoint() {
  loading.value = true
  try {
    const id = route.params.id as string
    emissionPoint.value = await getEmissionPointById(id)

    uiStore.setBreadcrumbs([
      { label: t('nav.billing'), to: '/billing' },
      { label: t('emissionPoints.title'), to: '/billing/emission-points' },
      { label: emissionPoint.value.name },
    ])
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('messages.error_load'), errMessage)
    router.push('/billing/emission-points')
  }
  finally {
    loading.value = false
  }
}

function editEmissionPoint() {
  router.push(`/billing/emission-points/${route.params.id}/edit`)
}

async function handleDelete() {
  if (!emissionPoint.value)
    return

  try {
    await deleteEmissionPoint(emissionPoint.value.id)
    toast.showSuccess(t('messages.success_delete'), t('emissionPoints.deleted_successfully'))
    router.push('/billing/emission-points')
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('messages.error_delete'), errMessage)
  }
  finally {
    deleteDialog.value = false
  }
}

function viewEstablishment() {
  if (emissionPoint.value?.establishmentId) {
    router.push(`/billing/establishments/${emissionPoint.value.establishmentId}`)
  }
}

onMounted(() => {
  loadEmissionPoint()
})
</script>

<template>
  <div>
    <LoadingState v-if="loading" :message="t('common.loading')" />

    <div v-else-if="emissionPoint">
      <!-- Page Header -->
      <PageHeader
        :title="emissionPoint.name"
      >
        <template #actions>
          <Button
            v-if="emissionPoint.establishmentCode && can.viewEstablishments()"
            :label="t('common.view_establishment')"
            icon="pi pi-building"
            severity="secondary"
            outlined
            @click="viewEstablishment"
          />
          <Button
            v-if="can.editEmissionPoint()"
            :label="t('common.edit')"
            icon="pi pi-pencil"
            @click="editEmissionPoint"
          />
          <Button
            v-if="can.deleteEmissionPoint()"
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
                {{ t('emissionPoints.basic_info') }}
              </h3>
              <div class="space-y-4">
                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('emissionPoints.code') }}
                  </label>
                  <p class="font-mono text-lg font-semibold text-slate-900 dark:text-white">
                    {{ emissionPoint.emissionPointCode }}
                  </p>
                </div>

                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('emissionPoints.name') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ emissionPoint.name }}
                  </p>
                </div>

                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('emissionPoints.establishment') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    <span v-if="emissionPoint.establishmentCode">
                      {{ emissionPoint.establishmentCode }} - {{ emissionPoint.establishmentName }}
                    </span>
                    <span v-else class="text-slate-500">
                      {{ emissionPoint.establishmentId }}
                    </span>
                  </p>
                </div>

                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('common.status') }}
                  </label>
                  <div class="mt-1">
                    <Tag
                      :value="getStatusLabel(emissionPoint.isActive)"
                      :severity="getStatusSeverity(emissionPoint.isActive)"
                    />
                  </div>
                </div>
              </div>
            </div>

            <!-- Sequential Numbers -->
            <div>
              <h3 class="mb-4 text-lg font-semibold text-slate-900 dark:text-white">
                {{ t('emissionPoints.sequential_numbers') }}
              </h3>
              <div class="space-y-4">
                <div class="rounded-lg border border-slate-200 p-4 dark:border-slate-700">
                  <div class="mb-2 flex items-center justify-between">
                    <span class="text-sm font-medium text-slate-600 dark:text-slate-400">
                      {{ t('emissionPoints.invoice_sequence') }}
                    </span>
                    <Tag severity="info" :value="`${emissionPoint.invoiceSequence}`" />
                  </div>
                  <p class="text-xs text-slate-500 dark:text-slate-400">
                    {{ t('emissionPoints.current_sequence', { sequence: emissionPoint.invoiceSequence }) }}
                  </p>
                </div>

                <div class="rounded-lg border border-slate-200 p-4 dark:border-slate-700">
                  <div class="mb-2 flex items-center justify-between">
                    <span class="text-sm font-medium text-slate-600 dark:text-slate-400">
                      {{ t('emissionPoints.credit_note_sequence') }}
                    </span>
                    <Tag severity="success" :value="`${emissionPoint.creditNoteSequence}`" />
                  </div>
                  <p class="text-xs text-slate-500 dark:text-slate-400">
                    {{ t('emissionPoints.current_sequence', { sequence: emissionPoint.creditNoteSequence }) }}
                  </p>
                </div>

                <div class="rounded-lg border border-slate-200 p-4 dark:border-slate-700">
                  <div class="mb-2 flex items-center justify-between">
                    <span class="text-sm font-medium text-slate-600 dark:text-slate-400">
                      {{ t('emissionPoints.debit_note_sequence') }}
                    </span>
                    <Tag severity="warning" :value="`${emissionPoint.debitNoteSequence}`" />
                  </div>
                  <p class="text-xs text-slate-500 dark:text-slate-400">
                    {{ t('emissionPoints.current_sequence', { sequence: emissionPoint.debitNoteSequence }) }}
                  </p>
                </div>

                <div class="rounded-lg border border-slate-200 p-4 dark:border-slate-700">
                  <div class="mb-2 flex items-center justify-between">
                    <span class="text-sm font-medium text-slate-600 dark:text-slate-400">
                      {{ t('emissionPoints.retention_sequence') }}
                    </span>
                    <Tag severity="danger" :value="`${emissionPoint.retentionSequence}`" />
                  </div>
                  <p class="text-xs text-slate-500 dark:text-slate-400">
                    {{ t('emissionPoints.current_sequence', { sequence: emissionPoint.retentionSequence }) }}
                  </p>
                </div>
              </div>
            </div>
          </div>

          <!-- Metadata -->
          <div class="mt-8 border-t border-slate-200 pt-6 dark:border-slate-700">
            <div class="grid grid-cols-1 gap-4 md:grid-cols-2">
              <div>
                <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                  {{ t('common.created_at') }}
                </label>
                <p class="text-slate-900 dark:text-white">
                  {{ $d(new Date(emissionPoint.createdAt), 'long') }}
                </p>
              </div>

              <div v-if="emissionPoint.updatedAt">
                <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                  {{ t('common.updated_at') }}
                </label>
                <p class="text-slate-900 dark:text-white">
                  {{ $d(new Date(emissionPoint.updatedAt), 'long') }}
                </p>
              </div>
            </div>
          </div>
        </template>
      </Card>
    </div>

    <!-- Delete Confirmation Dialog -->
    <DeleteConfirmDialog
      v-model:visible="deleteDialog"
      :item-name="emissionPoint?.name"
      @confirm="handleDelete"
    />
  </div>
</template>
