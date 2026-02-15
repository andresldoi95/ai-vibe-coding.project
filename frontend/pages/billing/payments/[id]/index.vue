<script setup lang="ts">
import { PaymentStatus } from '~/types/billing'
import type { Payment } from '~/types/billing'

definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const uiStore = useUiStore()
const toast = useNotification()
const router = useRouter()
const route = useRoute()
const { getPaymentById, voidPayment, completePayment } = usePayment()
const { can } = usePermissions()

const payment = ref<Payment | null>(null)
const loading = ref(false)
const voidDialog = ref(false)
const voidReason = ref('')
const voiding = ref(false)
const completeDialog = ref(false)
const completeNotes = ref('')
const completing = ref(false)

const paymentId = computed(() => route.params.id as string)

onMounted(async () => {
  loading.value = true
  try {
    payment.value = await getPaymentById(paymentId.value)
    updateBreadcrumbs()
  }
  catch (error) {
    toast.showError(t('common.error'), t('payments.not_found'))
    router.push('/billing/payments')
  }
  finally {
    loading.value = false
  }
})

function updateBreadcrumbs() {
  uiStore.setBreadcrumbs([
    { label: t('nav.billing'), to: '/billing' },
    { label: t('payments.title'), to: '/billing/payments' },
    { label: payment.value?.invoiceNumber || paymentId.value },
  ])
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

function formatDateTime(date: string): string {
  return new Date(date).toLocaleString()
}

function openVoidDialog() {
  voidDialog.value = true
}

async function handleVoid() {
  if (!payment.value)
    return

  voiding.value = true
  try {
    await voidPayment(payment.value.id, { reason: voidReason.value || undefined })
    toast.showSuccess(t('messages.success'), t('payments.voided_successfully'))
    voidDialog.value = false
    voidReason.value = ''
    // Reload payment
    payment.value = await getPaymentById(paymentId.value)
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('messages.error'), errMessage)
  }
  finally {
    voiding.value = false
  }
}

function openCompleteDialog() {
  completeDialog.value = true
}

async function handleComplete() {
  if (!payment.value)
    return

  completing.value = true
  try {
    await completePayment(payment.value.id, { notes: completeNotes.value || undefined })
    toast.showSuccess(t('messages.success'), t('payments.completed_successfully'))
    completeDialog.value = false
    completeNotes.value = ''
    // Reload payment
    payment.value = await getPaymentById(paymentId.value)
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('messages.error'), errMessage)
  }
  finally {
    completing.value = false
  }
}

function viewInvoice() {
  if (payment.value?.invoiceId) {
    router.push(`/billing/invoices/${payment.value.invoiceId}`)
  }
}
</script>

<template>
  <div>
    <PageHeader
      :title="t('payments.view_title')"
      :description="payment ? `${t('payments.payment_for_invoice')} ${payment.invoiceNumber}` : ''"
    >
      <template #actions>
        <Button
          v-if="payment && payment.status === PaymentStatus.Pending && can.completePayment()"
          :label="t('payments.mark_as_paid')"
          icon="pi pi-check"
          severity="success"
          @click="openCompleteDialog"
        />
        <Button
          v-if="payment && payment.status !== PaymentStatus.Voided && can.voidPayment()"
          :label="t('payments.void_payment')"
          icon="pi pi-times"
          severity="danger"
          outlined
          @click="openVoidDialog"
        />
      </template>
    </PageHeader>

    <LoadingState v-if="loading" :message="t('common.loading')" />

    <div v-else-if="payment" class="grid grid-cols-1 gap-6 lg:grid-cols-3">
      <!-- Main Information -->
      <div class="lg:col-span-2">
        <Card>
          <template #title>
            <div class="flex items-center justify-between">
              <span>{{ t('payments.payment_details') }}</span>
              <Tag
                :value="t(`payments.status.${payment.status}`)"
                :severity="getStatusSeverity(payment.status)"
              />
            </div>
          </template>

          <template #content>
            <div class="grid grid-cols-1 gap-6 md:grid-cols-2">
              <!-- Amount -->
              <div>
                <div class="mb-1 text-sm font-semibold text-slate-500 dark:text-slate-400">
                  {{ t('payments.amount') }}
                </div>
                <div class="text-2xl font-bold text-slate-900 dark:text-white">
                  {{ formatCurrency(payment.amount) }}
                </div>
              </div>

              <!-- Payment Date -->
              <div>
                <div class="mb-1 text-sm font-semibold text-slate-500 dark:text-slate-400">
                  {{ t('payments.payment_date') }}
                </div>
                <div class="text-lg font-medium text-slate-900 dark:text-white">
                  {{ formatDate(payment.paymentDate) }}
                </div>
              </div>

              <!-- Invoice -->
              <div>
                <div class="mb-1 text-sm font-semibold text-slate-500 dark:text-slate-400">
                  {{ t('payments.invoice_number') }}
                </div>
                <div class="text-lg font-medium text-slate-900 dark:text-white">
                  <Button
                    :label="payment.invoiceNumber"
                    link
                    @click="viewInvoice"
                  />
                </div>
              </div>

              <!-- Customer -->
              <div>
                <div class="mb-1 text-sm font-semibold text-slate-500 dark:text-slate-400">
                  {{ t('payments.customer_name') }}
                </div>
                <div class="text-lg font-medium text-slate-900 dark:text-white">
                  {{ payment.customerName }}
                </div>
              </div>

              <!-- Payment Method -->
              <div>
                <div class="mb-1 text-sm font-semibold text-slate-500 dark:text-slate-400">
                  {{ t('payments.payment_method') }}
                </div>
                <div class="text-lg font-medium text-slate-900 dark:text-white">
                  {{ getPaymentMethodLabel(payment.paymentMethod) }}
                </div>
              </div>

              <!-- Transaction ID -->
              <div v-if="payment.transactionId">
                <div class="mb-1 text-sm font-semibold text-slate-500 dark:text-slate-400">
                  {{ t('payments.transaction_id') }}
                </div>
                <div class="text-lg font-medium text-slate-900 dark:text-white">
                  {{ payment.transactionId }}
                </div>
              </div>

              <!-- Notes -->
              <div v-if="payment.notes" class="md:col-span-2">
                <div class="mb-1 text-sm font-semibold text-slate-500 dark:text-slate-400">
                  {{ t('common.notes') }}
                </div>
                <div class="text-base text-slate-700 dark:text-slate-300">
                  {{ payment.notes }}
                </div>
              </div>
            </div>
          </template>
        </Card>
      </div>

      <!-- Metadata Sidebar -->
      <div class="lg:col-span-1">
        <Card>
          <template #title>
            {{ t('common.metadata') }}
          </template>

          <template #content>
            <div class="flex flex-col gap-4">
              <!-- Created At -->
              <div>
                <div class="mb-1 text-sm font-semibold text-slate-500 dark:text-slate-400">
                  {{ t('common.created_at') }}
                </div>
                <div class="text-sm text-slate-700 dark:text-slate-300">
                  {{ formatDateTime(payment.createdAt) }}
                </div>
              </div>

              <!-- Updated At -->
              <div>
                <div class="mb-1 text-sm font-semibold text-slate-500 dark:text-slate-400">
                  {{ t('common.updated_at') }}
                </div>
                <div class="text-sm text-slate-700 dark:text-slate-300">
                  {{ formatDateTime(payment.updatedAt) }}
                </div>
              </div>

              <!-- Created By -->
              <div v-if="payment.createdBy">
                <div class="mb-1 text-sm font-semibold text-slate-500 dark:text-slate-400">
                  {{ t('common.created_by') }}
                </div>
                <div class="text-sm text-slate-700 dark:text-slate-300">
                  {{ payment.createdBy }}
                </div>
              </div>
            </div>
          </template>
        </Card>
      </div>
    </div>

    <!-- Void Payment Dialog -->
    <Dialog
      v-model:visible="voidDialog"
      :header="t('payments.void_payment')"
      :modal="true"
      :style="{ width: '30rem' }"
    >
      <div class="flex flex-col gap-4">
        <p>{{ t('payments.void_confirm') }}</p>

        <div class="flex flex-col gap-2">
          <label for="voidReason" class="font-semibold text-slate-700 dark:text-slate-200">
            {{ t('payments.void_reason') }}
          </label>
          <Textarea
            id="voidReason"
            v-model="voidReason"
            rows="3"
            :placeholder="t('payments.void_reason_placeholder')"
          />
        </div>
      </div>

      <template #footer>
        <Button
          :label="t('common.cancel')"
          severity="secondary"
          outlined
          @click="voidDialog = false"
        />
        <Button
          :label="t('payments.void_payment')"
          severity="danger"
          :loading="voiding"
          @click="handleVoid"
        />
      </template>
    </Dialog>

    <!-- Complete Payment Dialog -->
    <Dialog
      v-model:visible="completeDialog"
      :header="t('payments.complete_payment')"
      :modal="true"
      :style="{ width: '30rem' }"
    >
      <div class="flex flex-col gap-4">
        <p>{{ t('payments.complete_confirm') }}</p>

        <div class="flex flex-col gap-2">
          <label for="completeNotes" class="font-semibold text-slate-700 dark:text-slate-200">
            {{ t('common.notes') }} ({{ t('common.optional') }})
          </label>
          <Textarea
            id="completeNotes"
            v-model="completeNotes"
            rows="3"
            :placeholder="t('payments.complete_notes_placeholder')"
          />
        </div>
      </div>

      <template #footer>
        <Button
          :label="t('common.cancel')"
          severity="secondary"
          outlined
          @click="completeDialog = false"
        />
        <Button
          :label="t('payments.mark_as_paid')"
          severity="success"
          :loading="completing"
          @click="handleComplete"
        />
      </template>
    </Dialog>
  </div>
</template>
