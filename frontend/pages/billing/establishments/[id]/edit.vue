<script setup lang="ts">
import { useVuelidate } from '@vuelidate/core'
import { helpers, maxLength, required } from '@vuelidate/validators'
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
const { getEstablishmentById, updateEstablishment } = useEstablishment()

const loading = ref(false)
const loadingData = ref(false)
const establishment = ref<Establishment | null>(null)

const formData = reactive({
  establishmentCode: '',
  name: '',
  address: '',
  phone: '',
  isActive: true,
})

// Validation rules
const establishmentCodeFormat = helpers.regex(/^\d{3}$/)

const rules = computed(() => ({
  establishmentCode: {
    required,
    establishmentCodeFormat: helpers.withMessage(
      t('establishments.code_helper'),
      establishmentCodeFormat,
    ),
  },
  name: {
    required,
    maxLength: maxLength(256),
  },
  address: {
    required,
    maxLength: maxLength(500),
  },
  phone: {
    maxLength: maxLength(50),
  },
}))

const v$ = useVuelidate(rules, formData)

async function loadEstablishment() {
  loadingData.value = true
  try {
    const id = route.params.id as string
    establishment.value = await getEstablishmentById(id)

    // Populate form data
    formData.establishmentCode = establishment.value.establishmentCode
    formData.name = establishment.value.name
    formData.address = establishment.value.address
    formData.phone = establishment.value.phone || ''
    formData.isActive = establishment.value.isActive

    uiStore.setBreadcrumbs([
      { label: t('nav.billing'), to: '/billing' },
      { label: t('establishments.title'), to: '/billing/establishments' },
      { label: establishment.value.name, to: `/billing/establishments/${establishment.value.id}` },
      { label: t('common.edit') },
    ])
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('messages.error_load'), errMessage)
    router.push('/billing/establishments')
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
    await updateEstablishment(id, {
      establishmentCode: formData.establishmentCode,
      name: formData.name,
      address: formData.address,
      phone: formData.phone || undefined,
      isActive: formData.isActive,
    })

    toast.showSuccess(t('messages.success_update'), t('establishments.updated_successfully'))
    router.push(`/billing/establishments/${id}`)
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
  router.push(`/billing/establishments/${id}`)
}

onMounted(() => {
  loadEstablishment()
})
</script>

<template>
  <div>
    <!-- Page Header -->
    <PageHeader
      :title="t('establishments.edit')"
      :description="t('establishments.edit_description')"
    />

    <!-- Loading State -->
    <LoadingState v-if="loadingData" :message="t('common.loading')" />

    <!-- Form Card -->
    <Card v-else-if="establishment">
      <template #content>
        <form class="flex flex-col gap-6" @submit.prevent="handleSubmit">
          <!-- Basic Information Section -->
          <div class="mb-4">
            <h3 class="mb-4 text-lg font-semibold text-slate-900 dark:text-white">
              {{ t('establishments.basic_info') }}
            </h3>

            <div class="grid grid-cols-1 gap-6 md:grid-cols-2">
              <!-- Establishment Code -->
              <FormField
                v-model="formData.establishmentCode"
                name="establishmentCode"
                :label="t('establishments.code')"
                :placeholder="t('establishments.code_placeholder')"
                :error="v$.establishmentCode.$errors[0]?.$message"
                :help-text="t('establishments.code_helper')"
                required
                maxlength="3"
              />

              <!-- Establishment Name -->
              <FormField
                v-model="formData.name"
                name="name"
                :label="t('establishments.name')"
                :placeholder="t('establishments.name_placeholder')"
                :error="v$.name.$errors[0]?.$message"
                required
              />

              <!-- Address -->
              <div class="md:col-span-2">
                <FormField
                  v-model="formData.address"
                  name="address"
                  :label="t('establishments.address')"
                  :placeholder="t('establishments.address_placeholder')"
                  :error="v$.address.$errors[0]?.$message"
                  required
                />
              </div>

              <!-- Phone -->
              <FormField
                v-model="formData.phone"
                name="phone"
                type="tel"
                :label="t('common.phone')"
                :placeholder="t('establishments.phone_placeholder')"
                :error="v$.phone.$errors[0]?.$message"
              />

              <!-- Is Active -->
              <div class="flex items-center gap-3 pt-6">
                <InputSwitch
                  id="isActive"
                  v-model="formData.isActive"
                  :aria-label="t('establishments.is_active')"
                />
                <label for="isActive" class="cursor-pointer text-sm font-medium text-slate-700 dark:text-slate-300">
                  {{ t('establishments.is_active') }}
                </label>
              </div>
            </div>
          </div>

          <!-- Form Actions -->
          <FormActions
            :loading="loading"
            :submit-label="t('common.save_changes')"
            :cancel-label="t('common.cancel')"
            @cancel="cancel"
          />
        </form>
      </template>
    </Card>
  </div>
</template>
