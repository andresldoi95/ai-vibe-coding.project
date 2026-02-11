<script setup lang="ts">
import { useVuelidate } from '@vuelidate/core'
import { helpers, maxLength, required } from '@vuelidate/validators'
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
const { getEmissionPointById, updateEmissionPoint } = useEmissionPoint()

const loading = ref(false)
const loadingData = ref(false)
const emissionPoint = ref<EmissionPoint | null>(null)

const formData = reactive({
  name: '',
  isActive: true,
})

// Validation rules
const rules = computed(() => ({
  name: {
    required,
    maxLength: maxLength(256),
  },
}))

const v$ = useVuelidate(rules, formData)

async function loadEmissionPoint() {
  loadingData.value = true
  try {
    const id = route.params.id as string
    emissionPoint.value = await getEmissionPointById(id)

    // Populate form data
    formData.name = emissionPoint.value.name
    formData.isActive = emissionPoint.value.isActive

    uiStore.setBreadcrumbs([
      { label: t('nav.billing'), to: '/billing' },
      { label: t('emissionPoints.title'), to: '/billing/emission-points' },
      { label: emissionPoint.value.name, to: `/billing/emission-points/${emissionPoint.value.id}` },
      { label: t('common.edit') },
    ])
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('messages.error_load'), errMessage)
    router.push('/billing/emission-points')
  }
  finally {
    loadingData.value = false
  }
}

async function handleSubmit() {
  const isValid = await v$.value.$validate()
  if (!isValid) {
    toast.showWarning(t('validation.invalid_form'), t('validation.fix_errors'))
    return
  }

  loading.value = true
  try {
    const id = route.params.id as string
    await updateEmissionPoint(id, {
      name: formData.name,
      isActive: formData.isActive,
    })

    toast.showSuccess(t('messages.success_update'), t('emissionPoints.updated_successfully'))
    router.push(`/billing/emission-points/${id}`)
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('messages.error_update'), errMessage)
  }
  finally {
    loading.value = false
  }
}

function cancel() {
  const id = route.params.id as string
  router.push(`/billing/emission-points/${id}`)
}

onMounted(() => {
  loadEmissionPoint()
})
</script>

<template>
  <div>
    <!-- Page Header -->
    <PageHeader
      :title="t('emissionPoints.edit')"
      :description="t('emissionPoints.edit_description')"
    />

    <!-- Loading State -->
    <LoadingState v-if="loadingData" :message="t('common.loading')" />

    <!-- Form Card -->
    <Card v-else-if="emissionPoint">
      <template #content>
        <form class="flex flex-col gap-6" @submit.prevent="handleSubmit">
          <!-- Read-only Information -->
          <div class="mb-4 rounded-lg border border-slate-200 bg-slate-50 p-4 dark:border-slate-700 dark:bg-slate-800/50">
            <h3 class="mb-3 text-sm font-semibold text-slate-700 dark:text-slate-300">
              {{ t('common.read_only_info') }}
            </h3>
            <div class="grid grid-cols-1 gap-3 md:grid-cols-2">
              <div>
                <label class="text-xs font-medium text-slate-600 dark:text-slate-400">
                  {{ t('emissionPoints.code') }}
                </label>
                <p class="font-mono font-semibold text-slate-900 dark:text-white">
                  {{ emissionPoint.emissionPointCode }}
                </p>
              </div>
              <div>
                <label class="text-xs font-medium text-slate-600 dark:text-slate-400">
                  {{ t('emissionPoints.establishment') }}
                </label>
                <p class="text-slate-900 dark:text-white">
                  {{ emissionPoint.establishmentCode }} - {{ emissionPoint.establishmentName }}
                </p>
              </div>
            </div>
          </div>

          <!-- Basic Information Section -->
          <div class="mb-4">
            <h3 class="mb-4 text-lg font-semibold text-slate-900 dark:text-white">
              {{ t('emissionPoints.basic_info') }}
            </h3>

            <div class="grid grid-cols-1 gap-6 md:grid-cols-2">
              <!-- Emission Point Name -->
              <div class="flex flex-col gap-2 md:col-span-2">
                <label for="name" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('emissionPoints.name') }} *
                </label>
                <InputText
                  id="name"
                  v-model="formData.name"
                  :invalid="v$.name.$error"
                  :placeholder="t('emissionPoints.name_placeholder')"
                  @blur="v$.name.$touch()"
                />
                <small v-if="v$.name.$error" class="text-red-600 dark:text-red-400">
                  {{ v$.name.$errors[0].$message }}
                </small>
              </div>

              <!-- Is Active -->
              <div class="flex items-center gap-3 pt-2 md:col-span-2">
                <InputSwitch
                  id="isActive"
                  v-model="formData.isActive"
                  :aria-label="t('emissionPoints.is_active')"
                />
                <label for="isActive" class="cursor-pointer font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('emissionPoints.is_active') }}
                </label>
              </div>
            </div>
          </div>

          <!-- Sequential Numbers Current State -->
          <div class="mb-4 rounded-lg border border-blue-200 bg-blue-50 p-4 dark:border-blue-800 dark:bg-blue-900/20">
            <h3 class="mb-3 text-sm font-semibold text-blue-900 dark:text-blue-200">
              {{ t('emissionPoints.sequential_numbers') }}
            </h3>
            <div class="grid grid-cols-2 gap-3 md:grid-cols-4">
              <div>
                <span class="text-xs text-blue-700 dark:text-blue-300">{{ t('emissionPoints.invoice_sequence') }}</span>
                <p class="font-mono font-semibold text-blue-900 dark:text-blue-100">
                  {{ emissionPoint.invoiceSequence }}
                </p>
              </div>
              <div>
                <span class="text-xs text-blue-700 dark:text-blue-300">{{ t('emissionPoints.credit_note_sequence') }}</span>
                <p class="font-mono font-semibold text-blue-900 dark:text-blue-100">
                  {{ emissionPoint.creditNoteSequence }}
                </p>
              </div>
              <div>
                <span class="text-xs text-blue-700 dark:text-blue-300">{{ t('emissionPoints.debit_note_sequence') }}</span>
                <p class="font-mono font-semibold text-blue-900 dark:text-blue-100">
                  {{ emissionPoint.debitNoteSequence }}
                </p>
              </div>
              <div>
                <span class="text-xs text-blue-700 dark:text-blue-300">{{ t('emissionPoints.retention_sequence') }}</span>
                <p class="font-mono font-semibold text-blue-900 dark:text-blue-100">
                  {{ emissionPoint.retentionSequence }}
                </p>
              </div>
            </div>
          </div>

          <!-- Action Buttons -->
          <div class="flex justify-end gap-3 pt-4">
            <Button
              :label="t('common.cancel')"
              severity="secondary"
              outlined
              @click="cancel"
            />
            <Button
              type="submit"
              :label="t('common.save_changes')"
              :loading="loading"
              icon="pi pi-check"
            />
          </div>
        </form>
      </template>
    </Card>
  </div>
</template>
