<script setup lang="ts">
import { useVuelidate } from '@vuelidate/core'
import { email, helpers, maxLength, required } from '@vuelidate/validators'
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
const { getWarehouseById, updateWarehouse } = useWarehouse()
const { getAllCountries, getCountryOptions } = useCountry()

const loading = ref(false)
const loadingData = ref(false)
const warehouse = ref<Warehouse | null>(null)

const formData = reactive({
  name: '',
  code: '',
  description: '',
  streetAddress: '',
  city: '',
  state: '',
  postalCode: '',
  countryId: '',
  phone: '',
  email: '',
  isActive: true,
  squareFootage: null as number | null,
  capacity: null as number | null,
})

// Validation rules
const codeFormat = helpers.regex(/^[A-Z0-9-]+$/)

const rules = computed(() => ({
  name: {
    required,
    maxLength: maxLength(256),
  },
  code: {
    required,
    maxLength: maxLength(50),
    codeFormat: helpers.withMessage(t('validation.warehouse_code_format'), codeFormat),
  },
  description: {
    maxLength: maxLength(1000),
  },
  streetAddress: {
    required,
    maxLength: maxLength(500),
  },
  city: {
    required,
    maxLength: maxLength(100),
  },
  state: {
    maxLength: maxLength(100),
  },
  postalCode: {
    required,
    maxLength: maxLength(20),
  },
  countryId: {
    required,
  },
  phone: {
    maxLength: maxLength(50),
  },
  email: {
    email,
    maxLength: maxLength(256),
  },
  squareFootage: {},
  capacity: {},
}))

const v$ = useVuelidate(rules, formData)

async function loadWarehouse() {
  loadingData.value = true
  try {
    const id = route.params.id as string
    // Load countries and warehouse in parallel
    await getAllCountries()
    warehouse.value = await getWarehouseById(id)

    // Populate form data
    formData.name = warehouse.value.name
    formData.code = warehouse.value.code
    formData.description = warehouse.value.description || ''
    formData.streetAddress = warehouse.value.streetAddress
    formData.city = warehouse.value.city
    formData.state = warehouse.value.state || ''
    formData.postalCode = warehouse.value.postalCode
    formData.countryId = warehouse.value.countryId
    formData.phone = warehouse.value.phone || ''
    formData.email = warehouse.value.email || ''
    formData.isActive = warehouse.value.isActive
    formData.squareFootage = warehouse.value.squareFootage || null
    formData.capacity = warehouse.value.capacity || null

    uiStore.setBreadcrumbs([
      { label: t('nav.inventory'), to: '/inventory' },
      { label: t('warehouses.title'), to: '/inventory/warehouses' },
      { label: warehouse.value.name, to: `/inventory/warehouses/${warehouse.value.id}` },
      { label: t('common.edit') },
    ])
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('messages.error_load'), errMessage)
    router.push('/inventory/warehouses')
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

  if (!warehouse.value)
    return

  loading.value = true
  try {
    await updateWarehouse({
      id: warehouse.value.id,
      name: formData.name,
      code: formData.code,
      description: formData.description || undefined,
      streetAddress: formData.streetAddress,
      city: formData.city,
      state: formData.state || undefined,
      postalCode: formData.postalCode,
      countryId: formData.countryId,
      phone: formData.phone || undefined,
      email: formData.email || undefined,
      isActive: formData.isActive,
      squareFootage: formData.squareFootage || undefined,
      capacity: formData.capacity || undefined,
    })

    toast.showSuccess(t('messages.success_update'), t('warehouses.updated_successfully'))
    router.push(`/inventory/warehouses/${warehouse.value.id}`)
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
  router.push(`/inventory/warehouses/${route.params.id}`)
}

onMounted(() => {
  loadWarehouse()
})
</script>

<template>
  <div>
    <LoadingState v-if="loadingData" :message="t('common.loading')" />

    <div v-else-if="warehouse">
      <!-- Page Header -->
      <PageHeader
        :title="t('warehouses.edit')"
        :description="t('warehouses.edit_description')"
      />

      <!-- Form Card -->
      <Card>
        <template #content>
          <form class="flex flex-col gap-6" @submit.prevent="handleSubmit">
            <!-- Basic Information Section -->
            <div class="mb-4">
              <h3 class="mb-4 text-lg font-semibold text-slate-900 dark:text-white">
                {{ t('warehouses.basic_info') }}
              </h3>

              <div class="grid grid-cols-1 gap-6 md:grid-cols-2">
                <!-- Name -->
                <div class="flex flex-col gap-2">
                  <label for="name" class="font-semibold text-slate-700 dark:text-slate-200">
                    {{ t('warehouses.name') }} *
                  </label>
                  <InputText
                    id="name"
                    v-model="formData.name"
                    :invalid="v$.name.$error"
                    :placeholder="t('warehouses.name_placeholder')"
                    @blur="v$.name.$touch()"
                  />
                  <small v-if="v$.name.$error" class="text-red-600 dark:text-red-400">
                    {{ v$.name.$errors[0].$message }}
                  </small>
                </div>

                <!-- Code -->
                <div class="flex flex-col gap-2">
                  <label for="code" class="font-semibold text-slate-700 dark:text-slate-200">
                    {{ t('warehouses.code') }} *
                  </label>
                  <InputText
                    id="code"
                    v-model="formData.code"
                    :invalid="v$.code.$error"
                    :placeholder="t('warehouses.code_placeholder')"
                    @blur="v$.code.$touch()"
                  />
                  <small v-if="v$.code.$error" class="text-red-600 dark:text-red-400">
                    {{ v$.code.$errors[0].$message }}
                  </small>
                  <small v-else class="text-slate-500 dark:text-slate-400">
                    {{ t('warehouses.code_helper') }}
                  </small>
                </div>
              </div>

              <!-- Description -->
              <div class="mt-6 flex flex-col gap-2">
                <label for="description" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('common.description') }}
                </label>
                <Textarea
                  id="description"
                  v-model="formData.description"
                  :invalid="v$.description.$error"
                  :placeholder="t('warehouses.description_placeholder')"
                  rows="3"
                  @blur="v$.description.$touch()"
                />
              </div>
            </div>

            <!-- Address Information Section -->
            <div class="mb-4">
              <h3 class="mb-4 text-lg font-semibold text-slate-900 dark:text-white">
                {{ t('warehouses.address_info') }}
              </h3>

              <!-- Street Address -->
              <div class="mb-6 flex flex-col gap-2">
                <label for="streetAddress" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('common.street_address') }} *
                </label>
                <InputText
                  id="streetAddress"
                  v-model="formData.streetAddress"
                  :invalid="v$.streetAddress.$error"
                  :placeholder="t('warehouses.street_address_placeholder')"
                  @blur="v$.streetAddress.$touch()"
                />
                <small v-if="v$.streetAddress.$error" class="text-red-600 dark:text-red-400">
                  {{ v$.streetAddress.$errors[0].$message }}
                </small>
              </div>

              <div class="grid grid-cols-1 gap-6 md:grid-cols-2">
                <!-- City -->
                <div class="flex flex-col gap-2">
                  <label for="city" class="font-semibold text-slate-700 dark:text-slate-200">
                    {{ t('common.city') }} *
                  </label>
                  <InputText
                    id="city"
                    v-model="formData.city"
                    :invalid="v$.city.$error"
                    :placeholder="t('warehouses.city_placeholder')"
                    @blur="v$.city.$touch()"
                  />
                  <small v-if="v$.city.$error" class="text-red-600 dark:text-red-400">
                    {{ v$.city.$errors[0].$message }}
                  </small>
                </div>

                <!-- State -->
                <div class="flex flex-col gap-2">
                  <label for="state" class="font-semibold text-slate-700 dark:text-slate-200">
                    {{ t('common.state') }}
                  </label>
                  <InputText
                    id="state"
                    v-model="formData.state"
                    :placeholder="t('warehouses.state_placeholder')"
                  />
                </div>

                <!-- Postal Code -->
                <div class="flex flex-col gap-2">
                  <label for="postalCode" class="font-semibold text-slate-700 dark:text-slate-200">
                    {{ t('common.postal_code') }} *
                  </label>
                  <InputText
                    id="postalCode"
                    v-model="formData.postalCode"
                    :invalid="v$.postalCode.$error"
                    :placeholder="t('warehouses.postal_code_placeholder')"
                    @blur="v$.postalCode.$touch()"
                  />
                  <small v-if="v$.postalCode.$error" class="text-red-600 dark:text-red-400">
                    {{ v$.postalCode.$errors[0].$message }}
                  </small>
                </div>

                <!-- Country -->
                <div class="flex flex-col gap-2">
                  <label for="country" class="font-semibold text-slate-700 dark:text-slate-200">
                    {{ t('common.country') }} *
                  </label>
                  <Dropdown
                    id="country"
                    v-model="formData.countryId"
                    :options="getCountryOptions()"
                    option-label="label"
                    option-value="value"
                    :invalid="v$.countryId.$error"
                    :placeholder="t('warehouses.select_country')"
                    filter
                    class="w-full"
                    @blur="v$.countryId.$touch()"
                  />
                  <small v-if="v$.countryId.$error" class="text-red-600 dark:text-red-400">
                    {{ v$.countryId.$errors[0].$message }}
                  </small>
                </div>
              </div>
            </div>

            <!-- Contact Information Section -->
            <div class="mb-4">
              <h3 class="mb-4 text-lg font-semibold text-slate-900 dark:text-white">
                {{ t('warehouses.contact_info') }}
              </h3>

              <div class="grid grid-cols-1 gap-6 md:grid-cols-2">
                <!-- Phone -->
                <div class="flex flex-col gap-2">
                  <label for="phone" class="font-semibold text-slate-700 dark:text-slate-200">
                    {{ t('common.phone') }}
                  </label>
                  <InputText
                    id="phone"
                    v-model="formData.phone"
                    :placeholder="t('warehouses.phone_placeholder')"
                  />
                </div>

                <!-- Email -->
                <div class="flex flex-col gap-2">
                  <label for="email" class="font-semibold text-slate-700 dark:text-slate-200">
                    {{ t('common.email') }}
                  </label>
                  <InputText
                    id="email"
                    v-model="formData.email"
                    :invalid="v$.email.$error"
                    :placeholder="t('warehouses.email_placeholder')"
                    type="email"
                    @blur="v$.email.$touch()"
                  />
                  <small v-if="v$.email.$error" class="text-red-600 dark:text-red-400">
                    {{ v$.email.$errors[0].$message }}
                  </small>
                </div>
              </div>
            </div>

            <!-- Additional Information Section -->
            <div class="mb-4">
              <h3 class="mb-4 text-lg font-semibold text-slate-900 dark:text-white">
                {{ t('warehouses.additional_info') }}
              </h3>

              <div class="grid grid-cols-1 gap-6 md:grid-cols-2">
                <!-- Square Footage -->
                <div class="flex flex-col gap-2">
                  <label for="squareFootage" class="font-semibold text-slate-700 dark:text-slate-200">
                    {{ t('warehouses.square_footage') }}
                  </label>
                  <InputNumber
                    id="squareFootage"
                    v-model="formData.squareFootage"
                    :placeholder="t('warehouses.square_footage_placeholder')"
                    :min="0"
                    mode="decimal"
                    :min-fraction-digits="0"
                    :max-fraction-digits="2"
                  />
                </div>

                <!-- Capacity -->
                <div class="flex flex-col gap-2">
                  <label for="capacity" class="font-semibold text-slate-700 dark:text-slate-200">
                    {{ t('warehouses.capacity') }}
                  </label>
                  <InputNumber
                    id="capacity"
                    v-model="formData.capacity"
                    :placeholder="t('warehouses.capacity_placeholder')"
                    :min="0"
                  />
                </div>
              </div>

              <!-- Is Active -->
              <div class="mt-6 flex items-center gap-2">
                <Checkbox
                  id="isActive"
                  v-model="formData.isActive"
                  :binary="true"
                />
                <label for="isActive" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('warehouses.is_active') }}
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
                :label="t('common.save')"
                :loading="loading"
                icon="pi pi-check"
              />
            </div>
          </form>
        </template>
      </Card>
    </div>
  </div>
</template>
