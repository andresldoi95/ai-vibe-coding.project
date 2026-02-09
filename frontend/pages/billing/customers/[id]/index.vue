<script setup lang="ts">
import type { Customer } from '~/types/billing'

definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const uiStore = useUiStore()
const toast = useNotification()
const route = useRoute()
const router = useRouter()
const { can } = usePermissions()
const { getCustomerById, deleteCustomer } = useCustomer()

const customer = ref<Customer | null>(null)
const loading = ref(false)
const deleteDialog = ref(false)

async function loadCustomer() {
  loading.value = true
  try {
    const id = route.params.id as string
    customer.value = await getCustomerById(id)
    uiStore.setBreadcrumbs([
      { label: t('nav.billing'), to: '/billing' },
      { label: t('customers.title'), to: '/billing/customers' },
      { label: customer.value.name },
    ])
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('messages.error_load'), errMessage)
    router.push('/billing/customers')
  }
  finally {
    loading.value = false
  }
}

function editCustomer() {
  router.push(`/billing/customers/${route.params.id}/edit`)
}

async function handleDelete() {
  if (!customer.value)
    return

  try {
    await deleteCustomer(customer.value.id)
    toast.showSuccess(t('messages.success_delete'), t('customers.deleted_successfully'))
    router.push('/billing/customers')
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('messages.error_delete'), errMessage)
  }
  finally {
    deleteDialog.value = false
  }
}

onMounted(() => {
  loadCustomer()
})
</script>

<template>
  <div>
    <LoadingState v-if="loading" :message="t('common.loading')" />

    <div v-else-if="customer">
      <!-- Page Header -->
      <PageHeader
        :title="customer.name"
        :description="customer.email"
      >
        <template #actions>
          <Button
            v-if="can.editCustomer()"
            :label="t('common.edit')"
            icon="pi pi-pencil"
            @click="editCustomer"
          />
          <Button
            v-if="can.deleteCustomer()"
            :label="t('common.delete')"
            icon="pi pi-trash"
            severity="danger"
            outlined
            @click="deleteDialog = true"
          />
        </template>
      </PageHeader>

      <!-- Details Card -->
      <Card>
        <template #content>
          <div class="grid grid-cols-1 gap-8 lg:grid-cols-2">
            <!-- Basic Information -->
            <div>
              <h3 class="mb-4 text-lg font-semibold text-slate-900 dark:text-white">
                {{ t('common.basic_info') }}
              </h3>
              <div class="space-y-4">
                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('customers.name') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ customer.name }}
                  </p>
                </div>

                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('common.email') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ customer.email }}
                  </p>
                </div>

                <div v-if="customer.phone">
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('common.phone') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ customer.phone }}
                  </p>
                </div>

                <div v-if="customer.taxId">
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('customers.tax_id') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ customer.taxId }}
                  </p>
                </div>

                <div v-if="customer.contactPerson">
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('customers.contact_person') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ customer.contactPerson }}
                  </p>
                </div>

                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('common.status') }}
                  </label>
                  <div class="mt-1">
                    <Tag
                      :value="customer.isActive ? t('common.active') : t('common.inactive')"
                      :severity="customer.isActive ? 'success' : 'danger'"
                    />
                  </div>
                </div>
              </div>
            </div>

            <!-- Billing Address -->
            <div>
              <h3 class="mb-4 text-lg font-semibold text-slate-900 dark:text-white">
                {{ t('customers.billing_address') }}
              </h3>
              <div class="space-y-4">
                <div v-if="customer.billingStreet">
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('common.street_address') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ customer.billingStreet }}
                  </p>
                </div>

                <div v-if="customer.billingCity || customer.billingState">
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('customers.city_state') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ [customer.billingCity, customer.billingState].filter(Boolean).join(', ') || '—' }}
                  </p>
                </div>

                <div v-if="customer.billingPostalCode || customer.billingCountry">
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('customers.postal_country') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ [customer.billingPostalCode, customer.billingCountry].filter(Boolean).join(', ') || '—' }}
                  </p>
                </div>

                <div v-if="!customer.billingStreet && !customer.billingCity && !customer.billingState && !customer.billingPostalCode && !customer.billingCountry">
                  <p class="text-slate-500 dark:text-slate-400 italic">
                    {{ t('customers.no_billing_address') }}
                  </p>
                </div>
              </div>
            </div>

            <!-- Shipping Address -->
            <div>
              <h3 class="mb-4 text-lg font-semibold text-slate-900 dark:text-white">
                {{ t('customers.shipping_address') }}
              </h3>
              <div class="space-y-4">
                <div v-if="customer.shippingStreet">
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('common.street_address') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ customer.shippingStreet }}
                  </p>
                </div>

                <div v-if="customer.shippingCity || customer.shippingState">
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('customers.city_state') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ [customer.shippingCity, customer.shippingState].filter(Boolean).join(', ') || '—' }}
                  </p>
                </div>

                <div v-if="customer.shippingPostalCode || customer.shippingCountry">
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('customers.postal_country') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ [customer.shippingPostalCode, customer.shippingCountry].filter(Boolean).join(', ') || '—' }}
                  </p>
                </div>

                <div v-if="!customer.shippingStreet && !customer.shippingCity && !customer.shippingState && !customer.shippingPostalCode && !customer.shippingCountry">
                  <p class="text-slate-500 dark:text-slate-400 italic">
                    {{ t('customers.no_shipping_address') }}
                  </p>
                </div>
              </div>
            </div>

            <!-- Additional Information -->
            <div>
              <h3 class="mb-4 text-lg font-semibold text-slate-900 dark:text-white">
                {{ t('customers.additional_info') }}
              </h3>
              <div class="space-y-4">
                <div v-if="customer.website">
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('customers.website') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    <a :href="customer.website" target="_blank" rel="noopener noreferrer" class="text-teal-600 hover:underline">
                      {{ customer.website }}
                    </a>
                  </p>
                </div>

                <div v-if="customer.notes">
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('customers.notes') }}
                  </label>
                  <p class="text-slate-900 dark:text-white whitespace-pre-wrap">
                    {{ customer.notes }}
                  </p>
                </div>

                <div v-if="!customer.website && !customer.notes">
                  <p class="text-slate-500 dark:text-slate-400 italic">
                    {{ t('customers.no_additional_info') }}
                  </p>
                </div>
              </div>
            </div>

            <!-- Audit Information -->
            <div>
              <h3 class="mb-4 text-lg font-semibold text-slate-900 dark:text-white">
                {{ t('common.audit_info') }}
              </h3>
              <div class="space-y-4">
                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('common.created_at') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ new Date(customer.createdAt).toLocaleString() }}
                  </p>
                </div>

                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('common.updated_at') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ new Date(customer.updatedAt).toLocaleString() }}
                  </p>
                </div>
              </div>
            </div>
          </div>
        </template>
      </Card>
    </div>

    <!-- Delete Confirmation Dialog -->
    <Dialog
      v-model:visible="deleteDialog"
      :header="t('common.confirm')"
      :modal="true"
      :style="{ width: '450px' }"
    >
      <div class="flex items-center gap-4">
        <i class="pi pi-exclamation-triangle text-3xl text-orange-500" />
        <span v-if="customer">
          {{ t('customers.confirm_delete', { name: customer.name }) }}
        </span>
      </div>
      <template #footer>
        <Button
          :label="t('common.cancel')"
          icon="pi pi-times"
          text
          @click="deleteDialog = false"
        />
        <Button
          :label="t('common.delete')"
          icon="pi pi-trash"
          severity="danger"
          @click="handleDelete"
        />
      </template>
    </Dialog>
  </div>
</template>
