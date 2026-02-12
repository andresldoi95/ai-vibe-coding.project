<script setup lang="ts">
import { useVuelidate } from '@vuelidate/core'
import { helpers, maxLength, minValue, required } from '@vuelidate/validators'
import type { TaxRate } from '~/types/billing'
import type { Warehouse } from '~/types/inventory'

definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const toast = useNotification()
const { getInvoiceConfiguration, updateInvoiceConfiguration } = useInvoiceConfiguration()
const { getAllTaxRates } = useTaxRate()
const { getAllWarehouses } = useWarehouse()

const loading = ref(false)
const initialLoading = ref(true)
const taxRates = ref<TaxRate[]>([])
const warehouses = ref<Warehouse[]>([])

const formData = reactive({
  establishmentCode: '',
  emissionPointCode: '',
  defaultTaxRateId: null as string | null,
  defaultWarehouseId: null as string | null,
  dueDays: 30,
  requireCustomerTaxId: true,
})

const establishmentFormat = helpers.regex(/^\d{3}$/)
const emissionPointFormat = helpers.regex(/^\d{3}$/)

const rules = computed(() => ({
  establishmentCode: {
    required,
    maxLength: maxLength(3),
    establishmentFormat: helpers.withMessage(
      t('validation.establishment_code_format'),
      establishmentFormat,
    ),
  },
  emissionPointCode: {
    required,
    maxLength: maxLength(3),
    emissionPointFormat: helpers.withMessage(
      t('validation.emission_point_format'),
      emissionPointFormat,
    ),
  },
  dueDays: {
    required,
    minValue: minValue(0),
  },
}))

const v$ = useVuelidate(rules, formData)

onMounted(async () => {
  try {
    const [config, rates, warehouseList] = await Promise.all([
      getInvoiceConfiguration(),
      getAllTaxRates(),
      getAllWarehouses(),
    ])

    taxRates.value = rates
    warehouses.value = warehouseList

    formData.establishmentCode = config.establishmentCode
    formData.emissionPointCode = config.emissionPointCode
    formData.defaultTaxRateId = config.defaultTaxRateId || null
    formData.defaultWarehouseId = config.defaultWarehouseId || null
    formData.dueDays = config.dueDays
    formData.requireCustomerTaxId = config.requireCustomerTaxId
  }
  catch {
    toast.showError(t('invoiceConfig.load_error'))
  }
  finally {
    initialLoading.value = false
  }
})

async function handleSubmit() {
  const isValid = await v$.value.$validate()
  if (!isValid) {
    toast.showError(t('common.validation_error'), t('common.fix_errors'))
    return
  }

  loading.value = true
  try {
    await updateInvoiceConfiguration({
      establishmentCode: formData.establishmentCode,
      emissionPointCode: formData.emissionPointCode,
      defaultTaxRateId: formData.defaultTaxRateId || undefined,
      defaultWarehouseId: formData.defaultWarehouseId || undefined,
      dueDays: formData.dueDays,
      requireCustomerTaxId: formData.requireCustomerTaxId,
    })
    toast.showSuccess(t('invoiceConfig.update_success'))
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('invoiceConfig.update_error'), errMessage)
  }
  finally {
    loading.value = false
  }
}
</script>

<template>
  <div>
    <PageHeader
      :title="t('invoiceConfig.title')"
      :description="t('invoiceConfig.description')"
    />

    <Card v-if="initialLoading">
      <template #content>
        <div class="flex justify-center py-8">
          <ProgressSpinner />
        </div>
      </template>
    </Card>

    <Card v-else>
      <template #content>
        <form @submit.prevent="handleSubmit">
          <!-- Ecuador Invoice Numbering -->
          <div class="mb-8">
            <h3 class="text-lg font-semibold mb-4">
              {{ t('invoiceConfig.numbering_section') }}
            </h3>
            <p class="text-surface-600 mb-4">
              {{ t('invoiceConfig.numbering_hint') }}
            </p>

            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
              <!-- Establishment Code -->
              <div class="field">
                <label for="establishmentCode" class="font-semibold">
                  {{ t('invoiceConfig.establishment_code') }} <span class="text-red-500">*</span>
                </label>
                <InputText
                  id="establishmentCode"
                  v-model="formData.establishmentCode"
                  placeholder="001"
                  :class="{ 'p-invalid': v$.establishmentCode.$error }"
                  class="w-full"
                  maxlength="3"
                />
                <small v-if="v$.establishmentCode.$error" class="p-error">
                  {{ v$.establishmentCode.$errors[0].$message }}
                </small>
                <small class="text-surface-500">{{ t('invoiceConfig.establishment_code_hint') }}</small>
              </div>

              <!-- Emission Point Code -->
              <div class="field">
                <label for="emissionPointCode" class="font-semibold">
                  {{ t('invoiceConfig.emission_point_code') }} <span class="text-red-500">*</span>
                </label>
                <InputText
                  id="emissionPointCode"
                  v-model="formData.emissionPointCode"
                  placeholder="001"
                  :class="{ 'p-invalid': v$.emissionPointCode.$error }"
                  class="w-full"
                  maxlength="3"
                />
                <small v-if="v$.emissionPointCode.$error" class="p-error">
                  {{ v$.emissionPointCode.$errors[0].$message }}
                </small>
                <small class="text-surface-500">{{ t('invoiceConfig.emission_point_code_hint') }}</small>
              </div>
            </div>

            <div class="mt-4 p-4 bg-teal-50 dark:bg-teal-900/20 border border-teal-200 dark:border-teal-800 rounded">
              <p class="text-sm text-teal-800 dark:text-teal-200">
                <i class="pi pi-info-circle mr-2" />
                {{ t('invoiceConfig.numbering_preview') }}:
                <strong class="font-mono">{{ formData.establishmentCode || '001' }}-{{ formData.emissionPointCode || '001' }}-000000001</strong>
              </p>
            </div>
          </div>

          <!-- Default Settings -->
          <div class="mb-8">
            <h3 class="text-lg font-semibold mb-4">
              {{ t('invoiceConfig.defaults_section') }}
            </h3>

            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
              <!-- Default Tax Rate -->
              <div class="field">
                <label for="defaultTaxRateId" class="font-semibold">
                  {{ t('invoiceConfig.default_tax_rate') }}
                </label>
                <Select
                  id="defaultTaxRateId"
                  v-model="formData.defaultTaxRateId"
                  :options="taxRates"
                  option-label="name"
                  option-value="id"
                  :placeholder="t('invoiceConfig.select_tax_rate')"
                  class="w-full"
                  show-clear
                />
                <small class="text-surface-500">{{ t('invoiceConfig.default_tax_rate_hint') }}</small>
              </div>

              <!-- Default Warehouse -->
              <div class="field">
                <label for="defaultWarehouseId" class="font-semibold">
                  {{ t('invoiceConfig.default_warehouse') }}
                </label>
                <Select
                  id="defaultWarehouseId"
                  v-model="formData.defaultWarehouseId"
                  :options="warehouses"
                  option-label="name"
                  option-value="id"
                  :placeholder="t('invoiceConfig.select_warehouse')"
                  class="w-full"
                  show-clear
                />
                <small class="text-surface-500">{{ t('invoiceConfig.default_warehouse_hint') }}</small>
              </div>

              <!-- Due Days -->
              <div class="field">
                <label for="dueDays" class="font-semibold">
                  {{ t('invoiceConfig.due_days') }} <span class="text-red-500">*</span>
                </label>
                <InputNumber
                  id="dueDays"
                  v-model="formData.dueDays"
                  :min="0"
                  :step="1"
                  :placeholder="t('invoiceConfig.due_days_placeholder')"
                  :class="{ 'p-invalid': v$.dueDays.$error }"
                  class="w-full"
                />
                <small v-if="v$.dueDays.$error" class="p-error">
                  {{ v$.dueDays.$errors[0].$message }}
                </small>
                <small class="text-surface-500">{{ t('invoiceConfig.due_days_hint') }}</small>
              </div>

              <!-- Require Customer Tax ID -->
              <div class="field flex items-center gap-2">
                <Checkbox
                  id="requireCustomerTaxId"
                  v-model="formData.requireCustomerTaxId"
                  binary
                />
                <label for="requireCustomerTaxId" class="font-semibold cursor-pointer">
                  {{ t('invoiceConfig.require_customer_tax_id') }}
                </label>
              </div>
            </div>
          </div>

          <!-- Form Actions -->
          <div class="flex justify-end gap-3 pt-6 border-t">
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
