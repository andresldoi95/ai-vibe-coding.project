<script setup lang="ts">
import { useVuelidate } from '@vuelidate/core'
import { email, maxLength, required } from '@vuelidate/validators'

definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const uiStore = useUiStore()
const toast = useNotification()
const router = useRouter()
const { createCustomer } = useCustomer()

const loading = ref(false)

const formData = reactive({
  name: '',
  email: '',
  phone: '',
  taxId: '',
  contactPerson: '',

  // Billing Address
  billingStreet: '',
  billingCity: '',
  billingState: '',
  billingPostalCode: '',
  billingCountry: '',

  // Shipping Address
  shippingStreet: '',
  shippingCity: '',
  shippingState: '',
  shippingPostalCode: '',
  shippingCountry: '',

  // Additional Information
  notes: '',
  website: '',
  isActive: true,
})

// Validation rules
const rules = computed(() => ({
  name: {
    required,
    maxLength: maxLength(200),
  },
  email: {
    required,
    email,
    maxLength: maxLength(200),
  },
  phone: {
    maxLength: maxLength(50),
  },
  taxId: {
    maxLength: maxLength(50),
  },
  contactPerson: {
    maxLength: maxLength(200),
  },
  billingStreet: {
    maxLength: maxLength(512),
  },
  billingCity: {
    maxLength: maxLength(100),
  },
  billingState: {
    maxLength: maxLength(100),
  },
  billingPostalCode: {
    maxLength: maxLength(20),
  },
  billingCountry: {
    maxLength: maxLength(100),
  },
  shippingStreet: {
    maxLength: maxLength(512),
  },
  shippingCity: {
    maxLength: maxLength(100),
  },
  shippingState: {
    maxLength: maxLength(100),
  },
  shippingPostalCode: {
    maxLength: maxLength(20),
  },
  shippingCountry: {
    maxLength: maxLength(100),
  },
  notes: {
    maxLength: maxLength(1000),
  },
  website: {
    maxLength: maxLength(200),
  },
}))

const v$ = useVuelidate(rules, formData)

async function handleSubmit() {
  const isValid = await v$.value.$validate()
  if (!isValid) {
    toast.showWarning(t('validation.invalid_form'), t('validation.fix_errors'))
    return
  }

  loading.value = true
  try {
    await createCustomer({
      name: formData.name,
      email: formData.email,
      phone: formData.phone || undefined,
      taxId: formData.taxId || undefined,
      contactPerson: formData.contactPerson || undefined,

      billingStreet: formData.billingStreet || undefined,
      billingCity: formData.billingCity || undefined,
      billingState: formData.billingState || undefined,
      billingPostalCode: formData.billingPostalCode || undefined,
      billingCountry: formData.billingCountry || undefined,

      shippingStreet: formData.shippingStreet || undefined,
      shippingCity: formData.shippingCity || undefined,
      shippingState: formData.shippingState || undefined,
      shippingPostalCode: formData.shippingPostalCode || undefined,
      shippingCountry: formData.shippingCountry || undefined,

      notes: formData.notes || undefined,
      website: formData.website || undefined,
      isActive: formData.isActive,
    })

    toast.showSuccess(t('messages.success_create'), t('customers.created_successfully'))
    router.push('/billing/customers')
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
  router.push('/billing/customers')
}

onMounted(() => {
  uiStore.setBreadcrumbs([
    { label: t('nav.billing'), to: '/billing' },
    { label: t('customers.title'), to: '/billing/customers' },
    { label: t('customers.create') },
  ])
})
</script>

<template>
  <div>
    <!-- Page Header -->
    <PageHeader
      :title="t('customers.create')"
      :description="t('customers.create_description')"
    />

    <!-- Form Card -->
    <Card>
      <template #content>
        <form class="flex flex-col gap-6" @submit.prevent="handleSubmit">
          <!-- Basic Information Section -->
          <div class="mb-4">
            <h3 class="mb-4 text-lg font-semibold text-slate-900 dark:text-white">
              {{ t('common.basic_info') }}
            </h3>

            <div class="grid grid-cols-1 gap-6 md:grid-cols-2">
              <!-- Name -->
              <div class="flex flex-col gap-2">
                <label for="name" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('customers.name') }} *
                </label>
                <InputText
                  id="name"
                  v-model="formData.name"
                  :invalid="v$.name.$error"
                  :placeholder="t('customers.name_placeholder')"
                  @blur="v$.name.$touch()"
                />
                <small v-if="v$.name.$error" class="text-red-600 dark:text-red-400">
                  {{ v$.name.$errors[0].$message }}
                </small>
              </div>

              <!-- Email -->
              <div class="flex flex-col gap-2">
                <label for="email" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('common.email') }} *
                </label>
                <InputText
                  id="email"
                  v-model="formData.email"
                  :invalid="v$.email.$error"
                  :placeholder="t('customers.email_placeholder')"
                  @blur="v$.email.$touch()"
                />
                <small v-if="v$.email.$error" class="text-red-600 dark:text-red-400">
                  {{ v$.email.$errors[0].$message }}
                </small>
              </div>

              <!-- Phone -->
              <div class="flex flex-col gap-2">
                <label for="phone" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('common.phone') }}
                </label>
                <InputText
                  id="phone"
                  v-model="formData.phone"
                  :placeholder="t('customers.phone_placeholder')"
                />
              </div>

              <!-- Tax ID -->
              <div class="flex flex-col gap-2">
                <label for="taxId" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('customers.tax_id') }}
                </label>
                <InputText
                  id="taxId"
                  v-model="formData.taxId"
                  :placeholder="t('customers.tax_id_placeholder')"
                />
              </div>

              <!-- Contact Person -->
              <div class="flex flex-col gap-2">
                <label for="contactPerson" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('customers.contact_person') }}
                </label>
                <InputText
                  id="contactPerson"
                  v-model="formData.contactPerson"
                  :placeholder="t('customers.contact_person_placeholder')"
                />
              </div>
            </div>
          </div>

          <!-- Billing Address Section -->
          <div class="mb-4">
            <h3 class="mb-4 text-lg font-semibold text-slate-900 dark:text-white">
              {{ t('customers.billing_address') }}
            </h3>

            <div class="grid grid-cols-1 gap-6">
              <!-- Street -->
              <div class="flex flex-col gap-2">
                <label for="billingStreet" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('common.street_address') }}
                </label>
                <InputText
                  id="billingStreet"
                  v-model="formData.billingStreet"
                  :placeholder="t('customers.street_placeholder')"
                />
              </div>

              <div class="grid grid-cols-1 gap-6 md:grid-cols-2">
                <!-- City -->
                <div class="flex flex-col gap-2">
                  <label for="billingCity" class="font-semibold text-slate-700 dark:text-slate-200">
                    {{ t('common.city') }}
                  </label>
                  <InputText
                    id="billingCity"
                    v-model="formData.billingCity"
                    :placeholder="t('customers.city_placeholder')"
                  />
                </div>

                <!-- State -->
                <div class="flex flex-col gap-2">
                  <label for="billingState" class="font-semibold text-slate-700 dark:text-slate-200">
                    {{ t('common.state') }}
                  </label>
                  <InputText
                    id="billingState"
                    v-model="formData.billingState"
                    :placeholder="t('customers.state_placeholder')"
                  />
                </div>

                <!-- Postal Code -->
                <div class="flex flex-col gap-2">
                  <label for="billingPostalCode" class="font-semibold text-slate-700 dark:text-slate-200">
                    {{ t('common.postal_code') }}
                  </label>
                  <InputText
                    id="billingPostalCode"
                    v-model="formData.billingPostalCode"
                    :placeholder="t('customers.postal_code_placeholder')"
                  />
                </div>

                <!-- Country -->
                <div class="flex flex-col gap-2">
                  <label for="billingCountry" class="font-semibold text-slate-700 dark:text-slate-200">
                    {{ t('common.country') }}
                  </label>
                  <InputText
                    id="billingCountry"
                    v-model="formData.billingCountry"
                    :placeholder="t('customers.country_placeholder')"
                  />
                </div>
              </div>
            </div>
          </div>

          <!-- Shipping Address Section -->
          <div class="mb-4">
            <h3 class="mb-4 text-lg font-semibold text-slate-900 dark:text-white">
              {{ t('customers.shipping_address') }}
            </h3>

            <div class="grid grid-cols-1 gap-6">
              <!-- Street -->
              <div class="flex flex-col gap-2">
                <label for="shippingStreet" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('common.street_address') }}
                </label>
                <InputText
                  id="shippingStreet"
                  v-model="formData.shippingStreet"
                  :placeholder="t('customers.street_placeholder')"
                />
              </div>

              <div class="grid grid-cols-1 gap-6 md:grid-cols-2">
                <!-- City -->
                <div class="flex flex-col gap-2">
                  <label for="shippingCity" class="font-semibold text-slate-700 dark:text-slate-200">
                    {{ t('common.city') }}
                  </label>
                  <InputText
                    id="shippingCity"
                    v-model="formData.shippingCity"
                    :placeholder="t('customers.city_placeholder')"
                  />
                </div>

                <!-- State -->
                <div class="flex flex-col gap-2">
                  <label for="shippingState" class="font-semibold text-slate-700 dark:text-slate-200">
                    {{ t('common.state') }}
                  </label>
                  <InputText
                    id="shippingState"
                    v-model="formData.shippingState"
                    :placeholder="t('customers.state_placeholder')"
                  />
                </div>

                <!-- Postal Code -->
                <div class="flex flex-col gap-2">
                  <label for="shippingPostalCode" class="font-semibold text-slate-700 dark:text-slate-200">
                    {{ t('common.postal_code') }}
                  </label>
                  <InputText
                    id="shippingPostalCode"
                    v-model="formData.shippingPostalCode"
                    :placeholder="t('customers.postal_code_placeholder')"
                  />
                </div>

                <!-- Country -->
                <div class="flex flex-col gap-2">
                  <label for="shippingCountry" class="font-semibold text-slate-700 dark:text-slate-200">
                    {{ t('common.country') }}
                  </label>
                  <InputText
                    id="shippingCountry"
                    v-model="formData.shippingCountry"
                    :placeholder="t('customers.country_placeholder')"
                  />
                </div>
              </div>
            </div>
          </div>

          <!-- Additional Information Section -->
          <div class="mb-4">
            <h3 class="mb-4 text-lg font-semibold text-slate-900 dark:text-white">
              {{ t('customers.additional_info') }}
            </h3>

            <div class="grid grid-cols-1 gap-6">
              <!-- Website -->
              <div class="flex flex-col gap-2">
                <label for="website" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('customers.website') }}
                </label>
                <InputText
                  id="website"
                  v-model="formData.website"
                  :placeholder="t('customers.website_placeholder')"
                />
              </div>

              <!-- Notes -->
              <div class="flex flex-col gap-2">
                <label for="notes" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('customers.notes') }}
                </label>
                <Textarea
                  id="notes"
                  v-model="formData.notes"
                  :placeholder="t('customers.notes_placeholder')"
                  rows="3"
                />
              </div>
            </div>
          </div>

          <!-- Status Section -->
          <div class="mb-4">
            <h3 class="mb-4 text-lg font-semibold text-slate-900 dark:text-white">
              {{ t('common.status') }}
            </h3>

            <!-- Is Active -->
            <div class="flex items-center gap-2">
              <Checkbox
                id="isActive"
                v-model="formData.isActive"
                :binary="true"
              />
              <label for="isActive" class="font-semibold text-slate-700 dark:text-slate-200">
                {{ t('customers.is_active') }}
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
