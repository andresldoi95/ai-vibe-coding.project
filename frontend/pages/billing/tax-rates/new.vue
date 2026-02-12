<script setup lang="ts">
import { useVuelidate } from '@vuelidate/core'
import { between, helpers, maxLength, required } from '@vuelidate/validators'

definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const toast = useNotification()
const router = useRouter()
const { createTaxRate } = useTaxRate()
const { countries, getAllCountries, getCountryOptions } = useCountry()

const loading = ref(false)

const formData = reactive({
  code: '',
  name: '',
  rate: 0,
  isDefault: false,
  isActive: true,
  countryId: undefined as string | undefined,
})

// Load countries on mount
onMounted(async () => {
  await getAllCountries()
})

const codeFormat = helpers.regex(/^[A-Z0-9_]+$/)

const rules = computed(() => ({
  code: {
    required,
    maxLength: maxLength(50),
    codeFormat: helpers.withMessage(t('validation.tax_code_format'), codeFormat),
  },
  name: {
    required,
    maxLength: maxLength(256),
  },
  rate: {
    required,
    between: between(0, 1),
  },
}))

const v$ = useVuelidate(rules, formData)

async function handleSubmit() {
  const isValid = await v$.value.$validate()
  if (!isValid) {
    toast.showError(t('common.validation_error'), t('common.fix_errors'))
    return
  }

  loading.value = true
  try {
    await createTaxRate(formData)
    toast.showSuccess(t('taxRates.create_success'))
    router.push('/billing/tax-rates')
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('taxRates.create_error'), errMessage)
  }
  finally {
    loading.value = false
  }
}

function handleCancel() {
  router.push('/billing/tax-rates')
}
</script>

<template>
  <div>
    <PageHeader
      :title="t('taxRates.create')"
      :description="t('taxRates.create_description')"
    />

    <Card>
      <template #content>
        <form @submit.prevent="handleSubmit">
          <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <!-- Code -->
            <div class="field">
              <label for="code" class="font-semibold">
                {{ t('taxRates.code') }} <span class="text-red-500">*</span>
              </label>
              <InputText
                id="code"
                v-model="formData.code"
                :placeholder="t('taxRates.code_placeholder')"
                :class="{ 'p-invalid': v$.code.$error }"
                class="w-full"
              />
              <small v-if="v$.code.$error" class="p-error">
                {{ v$.code.$errors[0].$message }}
              </small>
              <small class="text-surface-500">{{ t('taxRates.code_hint') }}</small>
            </div>

            <!-- Name -->
            <div class="field">
              <label for="name" class="font-semibold">
                {{ t('taxRates.name') }} <span class="text-red-500">*</span>
              </label>
              <InputText
                id="name"
                v-model="formData.name"
                :placeholder="t('taxRates.name_placeholder')"
                :class="{ 'p-invalid': v$.name.$error }"
                class="w-full"
              />
              <small v-if="v$.name.$error" class="p-error">
                {{ v$.name.$errors[0].$message }}
              </small>
            </div>

            <!-- Rate -->
            <div class="field">
              <label for="rate" class="font-semibold">
                {{ t('taxRates.rate') }} <span class="text-red-500">*</span>
              </label>
              <InputNumber
                id="rate"
                v-model="formData.rate"
                :min="0"
                :max="1"
                :min-fraction-digits="2"
                :max-fraction-digits="4"
                :step="0.01"
                :placeholder="t('taxRates.rate_placeholder')"
                :class="{ 'p-invalid': v$.rate.$error }"
                class="w-full"
              />
              <small v-if="v$.rate.$error" class="p-error">
                {{ v$.rate.$errors[0].$message }}
              </small>
              <small class="text-surface-500">{{ t('taxRates.rate_hint') }}</small>
            </div>

            <!-- Country -->
            <div class="field">
              <label for="country" class="font-semibold">
                {{ t('taxRates.country') }}
              </label>
              <Dropdown
                id="country"
                v-model="formData.countryId"
                :options="getCountryOptions()"
                option-label="label"
                option-value="value"
                :placeholder="t('taxRates.select_country')"
                show-clear
                filter
                class="w-full"
              />
            </div>

            <!-- Is Default -->
            <div class="field flex items-center gap-2">
              <Checkbox
                id="isDefault"
                v-model="formData.isDefault"
                binary
              />
              <label for="isDefault" class="font-semibold cursor-pointer">
                {{ t('taxRates.is_default') }}
              </label>
            </div>

            <!-- Is Active -->
            <div class="field flex items-center gap-2">
              <Checkbox
                id="isActive"
                v-model="formData.isActive"
                binary
              />
              <label for="isActive" class="font-semibold cursor-pointer">
                {{ t('taxRates.is_active') }}
              </label>
            </div>
          </div>

          <!-- Form Actions -->
          <div class="flex justify-end gap-3 mt-6 pt-6 border-t">
            <Button
              :label="t('common.cancel')"
              severity="secondary"
              outlined
              @click="handleCancel"
            />
            <Button
              type="submit"
              :label="t('common.save')"
              :loading="loading"
            />
          </div>
        </form>
      </template>
    </Card>
  </div>
</template>
