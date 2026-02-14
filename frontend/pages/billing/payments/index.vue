<script setup lang="ts">
import { PaymentStatus } from '~/types/billing'
import type { Payment } from '~/types/billing'

definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const { getAllPayments } = usePayment()
const { can } = usePermissions()
const router = useRouter()

const payments = ref<Payment[]>([])
const loading = ref(false)

onMounted(async () => {
  loading.value = true
  try {
    payments.value = await getAllPayments()
  }
  catch (error) {
    const toast = useNotification()
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('common.error'), errMessage)
  }
  finally {
    loading.value = false
  }
})

function handleCreate() {
  router.push('/billing/payments/new')
}

function handleView(payment: Payment) {
  router.push(`/billing/payments/${payment.id}`)
}

function getStatusSeverity(status: PaymentStatus): 'success' | 'warning' | 'secondary' {
  switch (status) {
    case PaymentStatus.Completed:
      return 'success'
    case PaymentStatus.Pending:
      return 'warning'
    case PaymentStatus.Voided:
      return 'secondary'
    default:
      return 'secondary'
  }
}

function getPaymentMethodLabel(method: number): string {
  const methodMap: Record<number, string> = {
    1: t('payments.payment_methods.cash'),
    2: t('payments.payment_methods.check'),
    3: t('payments.payment_methods.bank_transfer'),
    4: t('payments.payment_methods.account_deposit'),
    16: t('payments.payment_methods.debit_card'),
    17: t('payments.payment_methods.electronic_money'),
    18: t('payments.payment_methods.prepaid_card'),
    19: t('payments.payment_methods.credit_card'),
    20: t('payments.payment_methods.other'),
  }
  return methodMap[method] || t('payments.payment_methods.other')
}

function formatCurrency(amount: number): string {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
  }).format(amount)
}

function formatDate(date: string): string {
  return new Date(date).toLocaleDateString()
}
</script>

<template>
  <div>
    <PageHeader
      :title="t('payments.title')"
      :description="t('payments.description')"
    >
      <template #actions>
        <Button
          v-if="can.createPayment()"
          :label="t('payments.create')"
          icon="pi pi-plus"
          @click="handleCreate"
        />
      </template>
    </PageHeader>

    <Card>
      <template #content>
        <LoadingState v-if="loading" :message="t('common.loading')" />
        <DataTable
          v-else
          :value="payments"
          :paginator="true"
          :rows="10"
          :rows-per-page-options="[10, 25, 50]"
          striped-rows
          responsive-layout="scroll"
        >
          <template #empty>
            <EmptyState
              icon="pi pi-money-bill"
              :title="t('common.no_data')"
              :description="can.createPayment() ? t('payments.get_started') : undefined"
              :action-label="t('payments.create')"
              action-icon="pi pi-plus"
              @action="handleCreate"
            />
          </template>

          <Column field="paymentDate" :header="t('payments.payment_date')" sortable>
            <template #body="{ data }">
              {{ formatDate(data.paymentDate) }}
            </template>
          </Column>

          <Column field="invoiceNumber" :header="t('payments.invoice_number')" sortable />

          <Column field="customerName" :header="t('payments.customer_name')" sortable />

          <Column field="amount" :header="t('payments.amount')" sortable>
            <template #body="{ data }">
              {{ formatCurrency(data.amount) }}
            </template>
          </Column>

          <Column field="paymentMethod" :header="t('payments.payment_method')" sortable>
            <template #body="{ data }">
              {{ getPaymentMethodLabel(data.paymentMethod) }}
            </template>
          </Column>

          <Column field="status" :header="t('common.status')" sortable>
            <template #body="{ data }">
              <Tag
                :value="t(`payments.status.${data.status}`)"
                :severity="getStatusSeverity(data.status)"
              />
            </template>
          </Column>

          <Column :header="t('common.actions')">
            <template #body="{ data }">
              <Button
                icon="pi pi-eye"
                severity="secondary"
                text
                rounded
                @click="handleView(data)"
              />
            </template>
          </Column>
        </DataTable>
      </template>
    </Card>
  </div>
</template>
