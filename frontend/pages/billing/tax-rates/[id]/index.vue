<script setup lang="ts">
import type { TaxRate } from '~/types/billing'

definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const route = useRoute()
const router = useRouter()
const { getTaxRateById } = useTaxRate()
const { can } = usePermissions()

const taxRate = ref<TaxRate | null>(null)
const loading = ref(true)

onMounted(async () => {
  try {
    const id = route.params.id as string
    taxRate.value = await getTaxRateById(id)
  }
  catch (error) {
    const toast = useNotification()
    toast.showError(t('taxRates.load_error'))
    router.push('/billing/tax-rates')
  }
  finally {
    loading.value = false
  }
})

function handleEdit() {
  router.push(`/billing/tax-rates/${route.params.id}/edit`)
}

function handleBack() {
  router.push('/billing/tax-rates')
}

function formatRate(rate: number): string {
  return `${(rate * 100).toFixed(2)}%`
}
</script>

<template>
  <div>
    <PageHeader
      :title="t('taxRates.view_title')"
      :description="t('taxRates.view_description')"
    >
      <template #actions>
        <Button
          v-if="can.updateTaxRate()"
          :label="t('common.edit')"
          icon="pi pi-pencil"
          @click="handleEdit"
        />
        <Button
          :label="t('common.back')"
          severity="secondary"
          outlined
          @click="handleBack"
        />
      </template>
    </PageHeader>

    <Card v-if="loading">
      <template #content>
        <div class="flex justify-center py-8">
          <ProgressSpinner />
        </div>
      </template>
    </Card>

    <Card v-else-if="taxRate">
      <template #content>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div class="field">
            <label class="font-semibold text-surface-700">{{ t('taxRates.code') }}</label>
            <p class="mt-1">
              {{ taxRate.code }}
            </p>
          </div>

          <div class="field">
            <label class="font-semibold text-surface-700">{{ t('taxRates.name') }}</label>
            <p class="mt-1">
              {{ taxRate.name }}
            </p>
          </div>

          <div class="field">
            <label class="font-semibold text-surface-700">{{ t('taxRates.rate') }}</label>
            <p class="mt-1 text-xl font-bold">
              {{ formatRate(taxRate.rate) }}
            </p>
          </div>

          <div class="field">
            <label class="font-semibold text-surface-700">{{ t('taxRates.country') }}</label>
            <p class="mt-1">
              {{ taxRate.countryName || t('common.not_specified') }}
            </p>
          </div>

          <div class="field">
            <label class="font-semibold text-surface-700">{{ t('taxRates.default') }}</label>
            <p class="mt-1">
              <Tag
                v-if="taxRate.isDefault"
                :value="t('common.yes')"
                severity="info"
              />
              <span v-else class="text-surface-500">{{ t('common.no') }}</span>
            </p>
          </div>

          <div class="field">
            <label class="font-semibold text-surface-700">{{ t('common.status') }}</label>
            <p class="mt-1">
              <Tag
                :value="taxRate.isActive ? t('common.active') : t('common.inactive')"
                :severity="taxRate.isActive ? 'success' : 'danger'"
              />
            </p>
          </div>

          <div class="field">
            <label class="font-semibold text-surface-700">{{ t('common.created_at') }}</label>
            <p class="mt-1">
              {{ new Date(taxRate.createdAt).toLocaleString() }}
            </p>
          </div>

          <div class="field">
            <label class="font-semibold text-surface-700">{{ t('common.updated_at') }}</label>
            <p class="mt-1">
              {{ new Date(taxRate.updatedAt).toLocaleString() }}
            </p>
          </div>
        </div>
      </template>
    </Card>
  </div>
</template>
