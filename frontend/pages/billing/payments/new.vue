<script setup lang="ts">
import { useVuelidate } from '@vuelidate/core'
import { helpers, maxLength, minValue, required } from '@vuelidate/validators'
import { PaymentStatus, SriPaymentMethod } from '~/types/billing'

definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const uiStore = useUiStore()
const toast = useNotification()
const router = useRouter()
const route = useRoute()
const { createPayment } = usePayment()
const { getAllInvoices } = useInvoice()

const loading = ref(false)
const loadingInvoices = ref(false)
const unpaidInvoices = ref<any[]>([])

const formData = reactive({
  invoiceId: '',
  amount: 0,
  paymentDate: new Date().toISOString().split('T')[0],
  paymentMethod: SriPaymentMethod.Cash,
  status: PaymentStatus.Completed,
  transactionId: '',
  notes: '',
})

// Load invoices on mount
onMounted(async () => {
  // Set breadcrumbs
  uiStore.setBreadcrumbs([
    { label: t('nav.billing'), to: '/billing' },
    { label: t('payments.title'), to: '/billing/payments' },
    { label: t('payments.create') },
  ])

  // Load unpaid invoices
  loadingInvoices.value = true
  try {
    const allInvoices = await getAllInvoices()
    unpaidInvoices.value = allInvoices.filter(
      inv => inv.status !== 'paid' && inv.status !== 'cancelled' && inv.status !== 'voided',
    )

    // Pre-fill invoice if provided in query
    const invoiceId = route.query.invoiceId as string | undefined
    if (invoiceId) {
      const invoice = unpaidInvoices.value.find(i => i.id === invoiceId)
      if (invoice) {
        formData.invoiceId = invoiceId
        // Set amount to total amount (API will validate against remaining balance)
        formData.amount = invoice.totalAmount
      }
    }
  }
  catch (error) {
    console.error('Error loading invoices:', error)
    toast.showError(t('common.error'), t('invoices.load_error'))
  }
  finally {
    loadingInvoices.value = false
  }
})

// Validation rules
const rules = computed(() => ({
  invoiceId: {
    required,
  },
  amount: {
    required,
    minValue: minValue(0.01),
  },
  paymentDate: {
    required,
  },
  paymentMethod: {
    required,
  },
  status: {
    required,
  },
  transactionId: {
    maxLength: maxLength(256),
  },
  notes: {
    maxLength: maxLength(1000),
  },
}))

const v$ = useVuelidate(rules, formData)

const paymentMethodOptions = computed(() => [
  { label: t('payments.payment_methods.cash'), value: SriPaymentMethod.Cash },
  { label: t('payments.payment_methods.check'), value: SriPaymentMethod.Check },
  { label: t('payments.payment_methods.bank_transfer'), value: SriPaymentMethod.BankTransfer },
  { label: t('payments.payment_methods.account_deposit'), value: SriPaymentMethod.AccountDeposit },
  { label: t('payments.payment_methods.debit_card'), value: SriPaymentMethod.DebitCard },
  { label: t('payments.payment_methods.credit_card'), value: SriPaymentMethod.CreditCard },
  { label: t('payments.payment_methods.electronic_money'), value: SriPaymentMethod.ElectronicMoney },
  { label: t('payments.payment_methods.prepaid_card'), value: SriPaymentMethod.PrepaidCard },
  { label: t('payments.payment_methods.other'), value: SriPaymentMethod.Other },
])

const statusOptions = computed(() => [
  { label: t('payments.status.pending'), value: PaymentStatus.Pending },
  { label: t('payments.status.completed'), value: PaymentStatus.Completed },
])

const invoiceOptions = computed(() =>
  unpaidInvoices.value.map(inv => ({
    label: `${inv.invoiceNumber} - ${inv.customerName} ($${inv.totalAmount.toFixed(2)})`,
    value: inv.id,
  })),
)

async function handleSubmit() {
  const isValid = await v$.value.$validate()
  if (!isValid) {
    toast.showWarning(t('validation.invalid_form'), t('validation.fix_errors'))
    return
  }

  loading.value = true
  try {
    await createPayment({
      invoiceId: formData.invoiceId,
      amount: formData.amount,
      paymentDate: formData.paymentDate,
      paymentMethod: formData.paymentMethod,
      status: formData.status,
      transactionId: formData.transactionId || undefined,
      notes: formData.notes || undefined,
    })

    toast.showSuccess(t('messages.success_create'), t('payments.created_successfully'))
    router.push('/billing/payments')
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
  router.push('/billing/payments')
}

onMounted(() => {
  uiStore.setBreadcrumbs([
    { label: t('nav.billing'), to: '/billing' },
    { label: t('payments.title'), to: '/billing/payments' },
    { label: t('payments.create') },
  ])
})
</script>

<template>
  <div>
    <PageHeader
      :title="t('payments.create')"
      :description="t('payments.create_description')"
    />

    <Card>
      <template #content>
        <form class="flex flex-col gap-6" @submit.prevent="handleSubmit">
          <!-- Payment Information Section -->
          <div class="mb-4">
            <h3 class="mb-4 text-lg font-semibold text-slate-900 dark:text-white">
              {{ t('payments.payment_info') }}
            </h3>

            <div class="grid grid-cols-1 gap-6 md:grid-cols-2">
              <!-- Invoice -->
              <div class="flex flex-col gap-2">
                <label for="invoiceId" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('payments.invoice') }} *
                </label>
                <Select
                  id="invoiceId"
                  v-model="formData.invoiceId"
                  :options="invoiceOptions"
                  option-label="label"
                  option-value="value"
                  :invalid="v$.invoiceId.$error"
                  :placeholder="t('payments.select_invoice')"
                  @blur="v$.invoiceId.$touch()"
                />
                <small v-if="v$.invoiceId.$error" class="text-red-600 dark:text-red-400">
                  {{ v$.invoiceId.$errors[0].$message }}
                </small>
              </div>

              <!-- Amount -->
              <div class="flex flex-col gap-2">
                <label for="amount" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('payments.amount') }} *
                </label>
                <InputNumber
                  id="amount"
                  v-model="formData.amount"
                  mode="currency"
                  currency="USD"
                  locale="en-US"
                  :invalid="v$.amount.$error"
                  :min="0.01"
                  @blur="v$.amount.$touch()"
                />
                <small v-if="v$.amount.$error" class="text-red-600 dark:text-red-400">
                  {{ v$.amount.$errors[0].$message }}
                </small>
              </div>

              <!-- Payment Date -->
              <div class="flex flex-col gap-2">
                <label for="paymentDate" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('payments.payment_date') }} *
                </label>
                <DatePicker
                  id="paymentDate"
                  v-model="formData.paymentDate"
                  :invalid="v$.paymentDate.$error"
                  date-format="yy-mm-dd"
                  @blur="v$.paymentDate.$touch()"
                />
                <small v-if="v$.paymentDate.$error" class="text-red-600 dark:text-red-400">
                  {{ v$.paymentDate.$errors[0].$message }}
                </small>
              </div>

              <!-- Payment Method -->
              <div class="flex flex-col gap-2">
                <label for="paymentMethod" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('payments.payment_method') }} *
                </label>
                <Select
                  id="paymentMethod"
                  v-model="formData.paymentMethod"
                  :options="paymentMethodOptions"
                  option-label="label"
                  option-value="value"
                  :invalid="v$.paymentMethod.$error"
                  @blur="v$.paymentMethod.$touch()"
                />
                <small v-if="v$.paymentMethod.$error" class="text-red-600 dark:text-red-400">
                  {{ v$.paymentMethod.$errors[0].$message }}
                </small>
              </div>

              <!-- Status -->
              <div class="flex flex-col gap-2">
                <label for="status" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('common.status') }} *
                </label>
                <Select
                  id="status"
                  v-model="formData.status"
                  :options="statusOptions"
                  option-label="label"
                  option-value="value"
                  :invalid="v$.status.$error"
                  @blur="v$.status.$touch()"
                />
                <small v-if="v$.status.$error" class="text-red-600 dark:text-red-400">
                  {{ v$.status.$errors[0].$message }}
                </small>
              </div>

              <!-- Transaction ID -->
              <div class="flex flex-col gap-2">
                <label for="transactionId" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('payments.transaction_id') }}
                </label>
                <InputText
                  id="transactionId"
                  v-model="formData.transactionId"
                  :invalid="v$.transactionId.$error"
                  :placeholder="t('payments.transaction_id_placeholder')"
                  @blur="v$.transactionId.$touch()"
                />
                <small v-if="v$.transactionId.$error" class="text-red-600 dark:text-red-400">
                  {{ v$.transactionId.$errors[0].$message }}
                </small>
              </div>
            </div>

            <!-- Notes -->
            <div class="mt-6 flex flex-col gap-2">
              <label for="notes" class="font-semibold text-slate-700 dark:text-slate-200">
                {{ t('common.notes') }}
              </label>
              <Textarea
                id="notes"
                v-model="formData.notes"
                :invalid="v$.notes.$error"
                :placeholder="t('payments.notes_placeholder')"
                rows="3"
                auto-resize
                @blur="v$.notes.$touch()"
              />
              <small v-if="v$.notes.$error" class="text-red-600 dark:text-red-400">
                {{ v$.notes.$errors[0].$message }}
              </small>
            </div>
          </div>

          <!-- Form Actions -->
          <div class="flex justify-end gap-3">
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
            />
          </div>
        </form>
      </template>
    </Card>
  </div>
</template>
