<script setup lang="ts">
import type { Establishment } from '~/types/establishment'

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
const { getEstablishmentById, deleteEstablishment } = useEstablishment()
const { getStatusLabel, getStatusSeverity } = useStatus()

const establishment = ref<Establishment | null>(null)
const loading = ref(false)
const deleteDialog = ref(false)

async function loadEstablishment() {
  loading.value = true
  try {
    const id = route.params.id as string
    establishment.value = await getEstablishmentById(id)
    uiStore.setBreadcrumbs([
      { label: t('nav.billing'), to: '/billing' },
      { label: t('establishments.title'), to: '/billing/establishments' },
      { label: establishment.value.name },
    ])
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('messages.error_load'), errMessage)
    router.push('/billing/establishments')
  }
  finally {
    loading.value = false
  }
}

function editEstablishment() {
  router.push(`/billing/establishments/${route.params.id}/edit`)
}

async function handleDelete() {
  if (!establishment.value)
    return

  try {
    await deleteEstablishment(establishment.value.id)
    toast.showSuccess(t('messages.success_delete'), t('establishments.deleted_successfully'))
    router.push('/billing/establishments')
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('messages.error_delete'), errMessage)
  }
  finally {
    deleteDialog.value = false
  }
}

function viewEmissionPoints() {
  router.push(`/billing/emission-points?establishment=${route.params.id}`)
}

onMounted(() => {
  loadEstablishment()
})
</script>

<template>
  <div>
    <LoadingState v-if="loading" :message="t('common.loading')" />

    <div v-else-if="establishment">
      <!-- Page Header -->
      <PageHeader
        :title="establishment.name"
      >
        <template #actions>
          <Button
            v-if="can.viewEmissionPoints()"
            :label="t('establishments.view_emission_points')"
            icon="pi pi-sitemap"
            severity="secondary"
            outlined
            @click="viewEmissionPoints"
          />
          <Button
            v-if="can.editEstablishment()"
            :label="t('common.edit')"
            icon="pi pi-pencil"
            @click="editEstablishment"
          />
          <Button
            v-if="can.deleteEstablishment()"
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
                {{ t('establishments.basic_info') }}
              </h3>
              <div class="space-y-4">
                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('establishments.code') }}
                  </label>
                  <p class="font-mono text-lg font-semibold text-slate-900 dark:text-white">
                    {{ establishment.establishmentCode }}
                  </p>
                </div>

                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('establishments.name') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ establishment.name }}
                  </p>
                </div>

                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('establishments.address') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ establishment.address }}
                  </p>
                </div>

                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('common.status') }}
                  </label>
                  <div class="mt-1">
                    <Tag
                      :value="getStatusLabel(establishment.isActive)"
                      :severity="getStatusSeverity(establishment.isActive)"
                    />
                  </div>
                </div>
              </div>
            </div>

            <!-- Contact Information -->
            <div>
              <h3 class="mb-4 text-lg font-semibold text-slate-900 dark:text-white">
                {{ t('establishments.contact_info') }}
              </h3>
              <div class="space-y-4">
                <div v-if="establishment.phone">
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('common.phone') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ establishment.phone }}
                  </p>
                </div>

                <div v-if="establishment.email">
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('common.email') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ establishment.email }}
                  </p>
                </div>

                <div v-if="!establishment.phone && !establishment.email" class="text-slate-500 dark:text-slate-400">
                  {{ t('common.no_contact_info') }}
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
                  {{ $d(new Date(establishment.createdAt), 'long') }}
                </p>
              </div>

              <div v-if="establishment.updatedAt">
                <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                  {{ t('common.updated_at') }}
                </label>
                <p class="text-slate-900 dark:text-white">
                  {{ $d(new Date(establishment.updatedAt), 'long') }}
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
      :item-name="establishment?.name"
      @confirm="handleDelete"
    />
  </div>
</template>
