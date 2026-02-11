<script setup lang="ts">
import { useVuelidate } from '@vuelidate/core'
import { email, helpers, maxLength, required } from '@vuelidate/validators'
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
  email: '',
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
  email: {
    email,
    maxLength: maxLength(256),
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
    formData.email = establishment.value.email || ''
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
      email: formData.email || undefined,
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
              <div class="flex flex-col gap-2">
                <label for="establishmentCode" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('establishments.code') }} *
                </label>
                <InputText
                  id="establishmentCode"
                  v-model="formData.establishmentCode"
                  :invalid="v$.establishmentCode.$error"
                  :placeholder="t('establishments.code_placeholder')"
                  maxlength="3"
                  @blur="v$.establishmentCode.$touch()"
                />
                <small v-if="v$.establishmentCode.$error" class="text-red-600 dark:text-red-400">
                  {{ v$.establishmentCode.$errors[0].$message }}
                </small>
                <small v-else class="text-slate-500 dark:text-slate-400">
                  {{ t('establishments.code_helper') }}
                </small>
              </div>

              <!-- Establishment Name -->
              <div class="flex flex-col gap-2">
                <label for="name" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('establishments.name') }} *
                </label>
                <InputText
                  id="name"
                  v-model="formData.name"
                  :invalid="v$.name.$error"
                  :placeholder="t('establishments.name_placeholder')"
                  @blur="v$.name.$touch()"
                />
                <small v-if="v$.name.$error" class="text-red-600 dark:text-red-400">
                  {{ v$.name.$errors[0].$message }}
                </small>
              </div>
            </div>

            <!-- Address -->
            <div class="mt-6 flex flex-col gap-2">
              <label for="address" class="font-semibold text-slate-700 dark:text-slate-200">
                {{ t('establishments.address') }} *
              </label>
              <Textarea
                id="address"
                v-model="formData.address"
                :invalid="v$.address.$error"
                :placeholder="t('establishments.address_placeholder')"
                rows="3"
                @blur="v$.address.$touch()"
              />
              <small v-if="v$.address.$error" class="text-red-600 dark:text-red-400">
                {{ v$.address.$errors[0].$message }}
              </small>
            </div>

            <div class="mt-6 grid grid-cols-1 gap-6 md:grid-cols-2">
              <!-- Phone -->
              <div class="flex flex-col gap-2">
                <label for="phone" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('common.phone') }}
                </label>
                <InputText
                  id="phone"
                  v-model="formData.phone"
                  type="tel"
                  :invalid="v$.phone.$error"
                  :placeholder="t('establishments.phone_placeholder')"
                  @blur="v$.phone.$touch()"
                />
                <small v-if="v$.phone.$error" class="text-red-600 dark:text-red-400">
                  {{ v$.phone.$errors[0].$message }}
                </small>
              </div>

              <!-- Email -->
              <div class="flex flex-col gap-2">
                <label for="email" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('common.email') }}
                </label>
                <InputText
                  id="email"
                  v-model="formData.email"
                  type="email"
                  :invalid="v$.email.$error"
                  :placeholder="t('establishments.email_placeholder')"
                  @blur="v$.email.$touch()"
                />
                <small v-if="v$.email.$error" class="text-red-600 dark:text-red-400">
                  {{ v$.email.$errors[0].$message }}
                </small>
              </div>
            </div>

            <!-- Is Active -->
            <div class="mt-6 flex items-center gap-3">
              <InputSwitch
                id="isActive"
                v-model="formData.isActive"
              />
              <label for="isActive" class="cursor-pointer font-semibold text-slate-700 dark:text-slate-200">
                {{ t('establishments.is_active') }}
              </label>
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
