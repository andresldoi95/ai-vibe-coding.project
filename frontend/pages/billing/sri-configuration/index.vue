<script setup lang="ts">
import { useVuelidate } from '@vuelidate/core'
import { email, helpers, maxLength, required } from '@vuelidate/validators'
import type { SriConfiguration } from '~/types/sri-configuration'
import { SriEnvironment } from '~/types/sri-enums'

definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const uiStore = useUiStore()
const toast = useNotification()
const { can } = usePermissions()
const { getSriConfiguration, updateSriConfiguration, uploadCertificate } = useSriConfiguration()

const loading = ref(false)
const loadingData = ref(false)
const uploadingCertificate = ref(false)
const configuration = ref<SriConfiguration | null>(null)
const certificateFile = ref<File | null>(null)
const certificatePassword = ref('')
const showPassword = ref(false)

const formData = reactive({
  companyRuc: '',
  legalName: '',
  tradeName: '',
  mainAddress: '',
  phone: '',
  email: '',
  accountingRequired: false,
  specialTaxpayerNumber: '',
  isRiseRegime: false,
  environment: SriEnvironment.Test,
})

// Validation rules
const rucFormat = helpers.regex(/^\d{13}$/)

const rules = computed(() => ({
  companyRuc: {
    required,
    rucFormat: helpers.withMessage(
      t('sriConfiguration.company_ruc_helper'),
      rucFormat,
    ),
  },
  legalName: {
    required,
    maxLength: maxLength(300),
  },
  tradeName: {
    maxLength: maxLength(300),
  },
  mainAddress: {
    required,
    maxLength: maxLength(500),
  },
  phone: {
    required,
    maxLength: maxLength(20),
  },
  email: {
    required,
    email,
    maxLength: maxLength(256),
  },
  specialTaxpayerNumber: {
    maxLength: maxLength(50),
  },
  accountingRequired: {},
  isRiseRegime: {},
  environment: {
    required,
  },
}))

const v$ = useVuelidate(rules, formData)

// Environment options
const environmentOptions = [
  { label: t('sriConfiguration.environment_test'), value: SriEnvironment.Test },
  { label: t('sriConfiguration.environment_production'), value: SriEnvironment.Production },
]

async function loadConfiguration() {
  loadingData.value = true
  try {
    configuration.value = await getSriConfiguration()

    // Populate form if configuration exists
    if (configuration.value && configuration.value.id) {
      formData.companyRuc = configuration.value.companyRuc
      formData.legalName = configuration.value.legalName
      formData.tradeName = configuration.value.tradeName
      formData.mainAddress = configuration.value.mainAddress
      formData.phone = configuration.value.phone
      formData.email = configuration.value.email
      formData.accountingRequired = configuration.value.accountingRequired
      formData.specialTaxpayerNumber = configuration.value.specialTaxpayerNumber || ''
      formData.isRiseRegime = configuration.value.isRiseRegime
      formData.environment = configuration.value.environment
    }
  }
  catch (error) {
    // If no configuration exists yet, that's okay - form will be empty
    console.log('No SRI configuration found yet')
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
    const result = await updateSriConfiguration({
      companyRuc: formData.companyRuc,
      legalName: formData.legalName,
      tradeName: formData.tradeName,
      mainAddress: formData.mainAddress,
      phone: formData.phone,
      email: formData.email,
      accountingRequired: formData.accountingRequired,
      specialTaxpayerNumber: formData.specialTaxpayerNumber || undefined,
      isRiseRegime: formData.isRiseRegime,
      environment: formData.environment,
    })

    configuration.value = result
    toast.showSuccess(t('messages.success_update'), t('sriConfiguration.updated_successfully'))
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('messages.error_update'), errMessage)
  }
  finally {
    loading.value = false
  }
}

function onFileSelect(event: Event) {
  const target = event.target as HTMLInputElement
  if (target.files && target.files.length > 0) {
    certificateFile.value = target.files[0]
  }
}

async function handleCertificateUpload() {
  if (!certificateFile.value || !certificatePassword.value) {
    toast.showWarning(
      t('validation.invalid_form'),
      t('sriConfiguration.certificate_file_and_password_required'),
    )
    return
  }

  uploadingCertificate.value = true
  try {
    const result = await uploadCertificate({
      certificateFile: certificateFile.value,
      certificatePassword: certificatePassword.value,
    })

    configuration.value = result
    certificateFile.value = null
    certificatePassword.value = ''
    showPassword.value = false

    toast.showSuccess(
      t('messages.success_upload'),
      t('sriConfiguration.certificate_uploaded_successfully'),
    )
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('messages.error_upload'), errMessage)
  }
  finally {
    uploadingCertificate.value = false
  }
}

function getCertificateStatusColor() {
  if (!configuration.value?.isCertificateConfigured)
    return 'secondary'
  return configuration.value.isCertificateValid ? 'success' : 'danger'
}

function getCertificateStatusLabel() {
  if (!configuration.value?.isCertificateConfigured)
    return t('sriConfiguration.no_certificate')
  return configuration.value.isCertificateValid
    ? t('sriConfiguration.certificate_valid')
    : t('sriConfiguration.certificate_expired')
}

onMounted(() => {
  uiStore.setBreadcrumbs([
    { label: t('nav.billing'), to: '/billing' },
    { label: t('sriConfiguration.title') },
  ])
  loadConfiguration()
})
</script>

<template>
  <div>
    <!-- Page Header -->
    <PageHeader
      :title="t('sriConfiguration.title')"
      :description="t('sriConfiguration.description')"
    />

    <!-- Loading State -->
    <LoadingState v-if="loadingData" :message="t('common.loading')" />

    <div v-else class="space-y-6">
      <!-- Company Information Card -->
      <Card>
        <template #header>
          <div class="flex items-center justify-between p-6 pb-0">
            <h3 class="text-lg font-semibold text-slate-900 dark:text-white">
              {{ t('sriConfiguration.company_info') }}
            </h3>
          </div>
        </template>

        <template #content>
          <form class="flex flex-col gap-6" @submit.prevent="handleSubmit">
            <div class="grid grid-cols-1 gap-6 md:grid-cols-2">
              <!-- Company RUC -->
              <div class="flex flex-col gap-2">
                <label for="companyRuc" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('sriConfiguration.company_ruc') }} *
                </label>
                <InputText
                  id="companyRuc"
                  v-model="formData.companyRuc"
                  :invalid="v$.companyRuc.$error"
                  :placeholder="t('sriConfiguration.company_ruc_placeholder')"
                  maxlength="13"
                  @blur="v$.companyRuc.$touch()"
                />
                <small v-if="v$.companyRuc.$error" class="text-red-600 dark:text-red-400">
                  {{ v$.companyRuc.$errors[0].$message }}
                </small>
                <small v-else class="text-slate-500 dark:text-slate-400">
                  {{ t('sriConfiguration.company_ruc_helper') }}
                </small>
              </div>

              <!-- Legal Name -->
              <div class="flex flex-col gap-2">
                <label for="legalName" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('sriConfiguration.legal_name') }} *
                </label>
                <InputText
                  id="legalName"
                  v-model="formData.legalName"
                  :invalid="v$.legalName.$error"
                  :placeholder="t('sriConfiguration.legal_name_placeholder')"
                  @blur="v$.legalName.$touch()"
                />
                <small v-if="v$.legalName.$error" class="text-red-600 dark:text-red-400">
                  {{ v$.legalName.$errors[0].$message }}
                </small>
              </div>

              <!-- Trade Name -->
              <div class="flex flex-col gap-2">
                <label for="tradeName" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('sriConfiguration.trade_name') }}
                </label>
                <InputText
                  id="tradeName"
                  v-model="formData.tradeName"
                  :invalid="v$.tradeName.$error"
                  :placeholder="t('sriConfiguration.trade_name_placeholder')"
                  @blur="v$.tradeName.$touch()"
                />
                <small v-if="v$.tradeName.$error" class="text-red-600 dark:text-red-400">
                  {{ v$.tradeName.$errors[0].$message }}
                </small>
              </div>

              <!-- Main Address -->
              <div class="flex flex-col gap-2">
                <label for="mainAddress" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('sriConfiguration.main_address') }} *
                </label>
                <InputText
                  id="mainAddress"
                  v-model="formData.mainAddress"
                  :invalid="v$.mainAddress.$error"
                  :placeholder="t('sriConfiguration.main_address_placeholder')"
                  @blur="v$.mainAddress.$touch()"
                />
                <small v-if="v$.mainAddress.$error" class="text-red-600 dark:text-red-400">
                  {{ v$.mainAddress.$errors[0].$message }}
                </small>
              </div>

              <!-- Phone -->
              <div class="flex flex-col gap-2">
                <label for="phone" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('sriConfiguration.phone') }} *
                </label>
                <InputText
                  id="phone"
                  v-model="formData.phone"
                  :invalid="v$.phone.$error"
                  :placeholder="t('sriConfiguration.phone_placeholder')"
                  maxlength="20"
                  @blur="v$.phone.$touch()"
                />
                <small v-if="v$.phone.$error" class="text-red-600 dark:text-red-400">
                  {{ v$.phone.$errors[0].$message }}
                </small>
              </div>

              <!-- Email -->
              <div class="flex flex-col gap-2">
                <label for="email" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('sriConfiguration.email') }} *
                </label>
                <InputText
                  id="email"
                  v-model="formData.email"
                  type="email"
                  :invalid="v$.email.$error"
                  :placeholder="t('sriConfiguration.email_placeholder')"
                  maxlength="256"
                  @blur="v$.email.$touch()"
                />
                <small v-if="v$.email.$error" class="text-red-600 dark:text-red-400">
                  {{ v$.email.$errors[0].$message }}
                </small>
              </div>

              <!-- Special Taxpayer Number -->
              <div class="flex flex-col gap-2">
                <label for="specialTaxpayerNumber" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('sriConfiguration.special_taxpayer_number') }}
                </label>
                <InputText
                  id="specialTaxpayerNumber"
                  v-model="formData.specialTaxpayerNumber"
                  :invalid="v$.specialTaxpayerNumber.$error"
                  :placeholder="t('sriConfiguration.special_taxpayer_number_placeholder')"
                  maxlength="50"
                  @blur="v$.specialTaxpayerNumber.$touch()"
                />
                <small v-if="v$.specialTaxpayerNumber.$error" class="text-red-600 dark:text-red-400">
                  {{ v$.specialTaxpayerNumber.$errors[0].$message }}
                </small>
                <small v-else class="text-slate-500 dark:text-slate-400">
                  {{ t('sriConfiguration.special_taxpayer_number_helper') }}
                </small>
              </div>

              <!-- Environment -->
              <div>
                <label for="environment" class="mb-2 block text-sm font-medium text-slate-700 dark:text-slate-300">
                  {{ t('sriConfiguration.environment') }} <span class="text-red-500">*</span>
                </label>
                <Select
                  id="environment"
                  v-model="formData.environment"
                  :options="environmentOptions"
                  option-label="label"
                  option-value="value"
                  class="w-full"
                />
                <small class="text-slate-500 dark:text-slate-400">
                  {{ t('sriConfiguration.environment_helper') }}
                </small>
              </div>

              <!-- Accounting Required -->
              <div class="flex items-center gap-3 pt-6">
                <ToggleSwitch
                  id="accountingRequired"
                  v-model="formData.accountingRequired"
                  :aria-label="t('sriConfiguration.accounting_required')"
                />
                <label for="accountingRequired" class="cursor-pointer text-sm font-medium text-slate-700 dark:text-slate-300">
                  {{ t('sriConfiguration.accounting_required') }}
                </label>
              </div>

              <!-- RISE Regime -->
              <div class="flex items-center gap-3 pt-6">
                <ToggleSwitch
                  id="isRiseRegime"
                  v-model="formData.isRiseRegime"
                  :aria-label="t('sriConfiguration.is_rise_regime')"
                />
                <label for="isRiseRegime" class="cursor-pointer text-sm font-medium text-slate-700 dark:text-slate-300">
                  {{ t('sriConfiguration.is_rise_regime') }}
                </label>
                <small class="block w-full text-slate-500 dark:text-slate-400">
                  {{ t('sriConfiguration.is_rise_regime_helper') }}
                </small>
              </div>
            </div>

            <!-- Form Actions -->
            <div class="flex justify-end gap-3 border-t border-slate-200 pt-6 dark:border-slate-700">
              <Button
                type="submit"
                :label="configuration?.id ? t('common.save_changes') : t('common.create')"
                :loading="loading"
                :disabled="!can.updateSriConfiguration()"
              />
            </div>
          </form>
        </template>
      </Card>

      <!-- Certificate Card -->
      <Card v-if="configuration?.id">
        <template #header>
          <div class="flex items-center justify-between p-6 pb-0">
            <h3 class="text-lg font-semibold text-slate-900 dark:text-white">
              {{ t('sriConfiguration.digital_certificate') }}
            </h3>
            <Tag
              :value="getCertificateStatusLabel()"
              :severity="getCertificateStatusColor()"
            />
          </div>
        </template>

        <template #content>
          <!-- Certificate Info (if exists) -->
          <div v-if="configuration.isCertificateConfigured" class="mb-6 rounded-lg border border-slate-200 bg-slate-50 p-4 dark:border-slate-700 dark:bg-slate-800/50">
            <div class="grid grid-cols-1 gap-4 md:grid-cols-2">
              <div>
                <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                  {{ t('sriConfiguration.certificate_status') }}
                </label>
                <p class="text-slate-900 dark:text-white">
                  {{ configuration.isCertificateValid ? t('common.valid') : t('common.expired') }}
                </p>
              </div>

              <div>
                <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                  {{ t('sriConfiguration.certificate_expiry') }}
                </label>
                <p class="text-slate-900 dark:text-white">
                  <span v-if="configuration.certificateExpiryDate">
                    {{ new Date(configuration.certificateExpiryDate).toLocaleDateString() }}
                  </span>
                  <span v-else class="text-slate-500">
                    Not available
                  </span>
                </p>
                <small class="text-xs text-slate-400">
                  Raw: {{ configuration.certificateExpiryDate }}
                </small>
              </div>
            </div>
          </div>

          <!-- Upload Certificate Form -->
          <div class="space-y-4">
            <div>
              <label for="certificate-file" class="mb-2 block text-sm font-medium text-slate-700 dark:text-slate-300">
                {{ t('sriConfiguration.certificate_file') }}
                <span v-if="!configuration.isCertificateConfigured" class="text-red-500">*</span>
              </label>
              <input
                id="certificate-file"
                type="file"
                accept=".p12,.pfx"
                class="block w-full text-sm text-slate-500 file:mr-4 file:rounded file:border-0 file:bg-teal-50 file:px-4 file:py-2 file:text-sm file:font-semibold file:text-teal-700 hover:file:bg-teal-100 dark:text-slate-400 dark:file:bg-teal-900/30 dark:file:text-teal-300 dark:hover:file:bg-teal-900/50"
                @change="onFileSelect"
              >
              <small class="text-slate-500 dark:text-slate-400">
                {{ t('sriConfiguration.certificate_file_helper') }}
              </small>
            </div>

            <div class="relative">
              <label for="certificate-password" class="mb-2 block text-sm font-medium text-slate-700 dark:text-slate-300">
                {{ t('sriConfiguration.certificate_password') }}
                <span v-if="!configuration.isCertificateConfigured" class="text-red-500">*</span>
              </label>
              <div class="relative">
                <InputText
                  id="certificate-password"
                  v-model="certificatePassword"
                  :type="showPassword ? 'text' : 'password'"
                  :placeholder="t('sriConfiguration.certificate_password_placeholder')"
                  class="w-full pr-10"
                />
                <button
                  type="button"
                  class="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-600 dark:hover:text-slate-300"
                  @click="showPassword = !showPassword"
                >
                  <i :class="showPassword ? 'pi pi-eye-slash' : 'pi pi-eye'" />
                </button>
              </div>
            </div>

            <div class="flex justify-end">
              <Button
                :label="configuration.isCertificateConfigured ? t('sriConfiguration.update_certificate') : t('sriConfiguration.upload_certificate')"
                icon="pi pi-upload"
                :loading="uploadingCertificate"
                :disabled="!certificateFile || !certificatePassword || !can.updateSriConfiguration()"
                @click="handleCertificateUpload"
              />
            </div>
          </div>
        </template>
      </Card>

      <!-- Info Card -->
      <Card v-if="!configuration?.id">
        <template #content>
          <div class="flex items-start gap-4 rounded-lg border border-blue-200 bg-blue-50 p-4 dark:border-blue-800 dark:bg-blue-900/20">
            <i class="pi pi-info-circle text-2xl text-blue-600 dark:text-blue-400" />
            <div>
              <h4 class="mb-1 font-semibold text-blue-900 dark:text-blue-200">
                {{ t('sriConfiguration.first_time_setup') }}
              </h4>
              <p class="text-sm text-blue-700 dark:text-blue-300">
                {{ t('sriConfiguration.first_time_setup_description') }}
              </p>
            </div>
          </div>
        </template>
      </Card>
    </div>
  </div>
</template>
