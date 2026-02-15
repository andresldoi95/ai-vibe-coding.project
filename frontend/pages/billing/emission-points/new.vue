<script setup lang="ts">
import { useVuelidate } from '@vuelidate/core'
import { helpers, maxLength, required } from '@vuelidate/validators'

definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const uiStore = useUiStore()
const toast = useNotification()
const router = useRouter()
const { createEmissionPoint } = useEmissionPoint()
const { getAllEstablishments } = useEstablishment()

const loading = ref(false)
const loadingEstablishments = ref(false)
const establishments = ref<{ id: string, code: string, name: string }[]>([])

const formData = reactive({
  emissionPointCode: '',
  name: '',
  establishmentId: '',
  isActive: true,
})

// Validation rules
const emissionPointCodeFormat = helpers.regex(/^\d{3}$/)

const rules = computed(() => ({
  emissionPointCode: {
    required,
    emissionPointCodeFormat: helpers.withMessage(
      t('emissionPoints.code_helper'),
      emissionPointCodeFormat,
    ),
  },
  name: {
    required,
    maxLength: maxLength(256),
  },
  establishmentId: {
    required: helpers.withMessage(
      t('emissionPoints.establishment_required'),
      required,
    ),
  },
}))

const v$ = useVuelidate(rules, formData)

// Load establishments for dropdown
async function loadEstablishments() {
  loadingEstablishments.value = true
  try {
    establishments.value = await getAllEstablishments()
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('messages.error_load'), errMessage)
  }
  finally {
    loadingEstablishments.value = false
  }
}

// Establishment dropdown options
const establishmentOptions = computed(() =>
  establishments.value.map(e => ({
    label: `${e.establishmentCode} - ${e.name}`,
    value: e.id,
  })),
)

async function handleSubmit() {
  const isValid = await v$.value.$validate()
  if (!isValid) {
    toast.showWarning(t('validation.invalid_form'), t('validation.fix_errors'))
    return
  }

  loading.value = true
  try {
    await createEmissionPoint({
      emissionPointCode: formData.emissionPointCode,
      name: formData.name,
      establishmentId: formData.establishmentId,
      isActive: formData.isActive,
    })

    toast.showSuccess(t('messages.success_create'), t('emissionPoints.created_successfully'))
    router.push('/billing/emission-points')
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('messages.error_create'), errMessage)
  }
  finally {
    loading.value = false
  }
}

function cancel() {
  router.push('/billing/emission-points')
}

onMounted(() => {
  uiStore.setBreadcrumbs([
    { label: t('nav.billing'), to: '/billing' },
    { label: t('emissionPoints.title'), to: '/billing/emission-points' },
    { label: t('emissionPoints.create') },
  ])
  loadEstablishments()
})
</script>

<template>
  <div>
    <!-- Page Header -->
    <PageHeader
      :title="t('emissionPoints.create')"
      :description="t('emissionPoints.create_description')"
    />

    <!-- Loading State -->
    <LoadingState v-if="loadingEstablishments" :message="t('common.loading')" />

    <!-- Form Card -->
    <Card v-else>
      <template #content>
        <form class="flex flex-col gap-6" @submit.prevent="handleSubmit">
          <!-- Basic Information Section -->
          <div class="mb-4">
            <h3 class="mb-4 text-lg font-semibold text-slate-900 dark:text-white">
              {{ t('emissionPoints.basic_info') }}
            </h3>

            <div class="grid grid-cols-1 gap-6 md:grid-cols-2">
              <!-- Establishment -->
              <div class="md:col-span-2">
                <label for="establishment" class="mb-2 block text-sm font-medium text-slate-700 dark:text-slate-300">
                  {{ t('emissionPoints.establishment') }} <span class="text-red-500">*</span>
                </label>
                <Dropdown
                  id="establishment"
                  v-model="formData.establishmentId"
                  :options="establishmentOptions"
                  option-label="label"
                  option-value="value"
                  :placeholder="t('emissionPoints.select_establishment')"
                  :class="{ 'p-invalid': v$.establishmentId.$errors.length > 0 }"
                  class="w-full"
                />
                <small v-if="v$.establishmentId.$errors.length > 0" class="p-error">
                  {{ v$.establishmentId.$errors[0].$message }}
                </small>
              </div>

              <!-- Emission Point Code -->
              <div class="flex flex-col gap-2">
                <label for="emissionPointCode" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('emissionPoints.code') }} *
                </label>
                <InputText
                  id="emissionPointCode"
                  v-model="formData.emissionPointCode"
                  :invalid="v$.emissionPointCode.$error"
                  :placeholder="t('emissionPoints.code_placeholder')"
                  maxlength="3"
                  @blur="v$.emissionPointCode.$touch()"
                />
                <small v-if="v$.emissionPointCode.$error" class="text-red-600 dark:text-red-400">
                  {{ v$.emissionPointCode.$errors[0].$message }}
                </small>
                <small v-else class="text-slate-500 dark:text-slate-400">
                  {{ t('emissionPoints.code_helper') }}
                </small>
              </div>

              <!-- Emission Point Name -->
              <div class="flex flex-col gap-2">
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
              <div class="flex items-center gap-3 pt-6 md:col-span-2">
                <InputSwitch
                  id="isActive"
                  v-model="formData.isActive"
                  :aria-label="t('emissionPoints.is_active')"
                />
                <label for="isActive" class="cursor-pointer text-sm font-medium text-slate-700 dark:text-slate-300">
                  {{ t('emissionPoints.is_active') }}
                </label>
              </div>
            </div>
          </div>

          <!-- Sequential Numbers Info -->
          <div class="rounded-lg border border-blue-200 bg-blue-50 p-4 dark:border-blue-800 dark:bg-blue-900/20">
            <div class="flex items-start gap-3">
              <i class="pi pi-info-circle text-lg text-blue-600 dark:text-blue-400" />
              <div>
                <p class="text-sm font-medium text-blue-900 dark:text-blue-200">
                  {{ t('emissionPoints.sequential_info') }}
                </p>
                <p class="mt-1 text-xs text-blue-700 dark:text-blue-300">
                  {{ t('emissionPoints.sequential_numbers') }}
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
              :label="t('common.create')"
              :loading="loading"
              icon="pi pi-check"
            />
          </div>
        </form>
      </template>
    </Card>
  </div>
</template>
