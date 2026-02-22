<script setup lang="ts">
import type { Invoice, Payment } from '~/types/billing'
import { InvoiceStatus, PaymentStatus } from '~/types/billing'

definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const route = useRoute()
const router = useRouter()
const toast = useNotification()
const { getInvoiceById, changeInvoiceStatus, generateXml, signDocument, submitToSri, checkAuthorization, generateRide, downloadXml, downloadRide } = useInvoice()
const { getPaymentsByInvoiceId } = usePayment()
const { can } = usePermissions()

const invoice = ref<Invoice | null>(null)
const payments = ref<Payment[]>([])
const loading = ref(true)
const paymentsLoading = ref(false)
const statusChangeDialog = ref(false)
const statusChangeLoading = ref(false)
const newStatus = ref<InvoiceStatus>(InvoiceStatus.Sent)

// SRI Workflow State
const sriLoading = ref({
  generateXml: false,
  sign: false,
  submit: false,
  checkAuth: false,
  generateRide: false,
})

onMounted(async () => {
  try {
    const id = route.params.id as string
    invoice.value = await getInvoiceById(id)
    await loadPayments(id)
  }
  catch {
    toast.showError(t('invoices.load_error'))
    router.push('/billing/invoices')
  }
  finally {
    loading.value = false
  }
})

async function loadPayments(invoiceId: string) {
  paymentsLoading.value = true
  try {
    payments.value = await getPaymentsByInvoiceId(invoiceId)
  }
  catch (error) {
    console.error('Failed to load payments:', error)
  }
  finally {
    paymentsLoading.value = false
  }
}

function handleBack() {
  router.push('/billing/invoices')
}

function openStatusChangeDialog() {
  if (!invoice.value)
    return

  // Set next logical status
  if (invoice.value.status === InvoiceStatus.Draft) {
    newStatus.value = InvoiceStatus.Sent
  }
  else if (invoice.value.status === InvoiceStatus.Sent) {
    newStatus.value = InvoiceStatus.Paid
  }
  else if (invoice.value.status === InvoiceStatus.Overdue) {
    newStatus.value = InvoiceStatus.Paid
  }

  statusChangeDialog.value = true
}

async function handleStatusChange() {
  if (!invoice.value)
    return

  statusChangeLoading.value = true
  try {
    const updated = await changeInvoiceStatus(invoice.value.id, newStatus.value)
    invoice.value = updated
    toast.showSuccess(t('invoices.status_change_success'))
    statusChangeDialog.value = false
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('invoices.status_change_error'), errMessage)
  }
  finally {
    statusChangeLoading.value = false
  }
}

function getStatusLabel(status: InvoiceStatus): string {
  const labels: Record<InvoiceStatus, string> = {
    [InvoiceStatus.Draft]: t('invoices.status_draft'),
    [InvoiceStatus.PendingSignature]: t('invoices.status_pending_signature'),
    [InvoiceStatus.PendingAuthorization]: t('invoices.status_pending_authorization'),
    [InvoiceStatus.Authorized]: t('invoices.status_authorized'),
    [InvoiceStatus.Rejected]: t('invoices.status_rejected'),
    [InvoiceStatus.Sent]: t('invoices.status_sent'),
    [InvoiceStatus.Paid]: t('invoices.status_paid'),
    [InvoiceStatus.Overdue]: t('invoices.status_overdue'),
    [InvoiceStatus.Cancelled]: t('invoices.status_cancelled'),
    [InvoiceStatus.Voided]: t('invoices.status_voided'),
  }
  return labels[status] || status.toString()
}

function getStatusSeverity(status: InvoiceStatus): string {
  const severities: Record<InvoiceStatus, string> = {
    [InvoiceStatus.Draft]: 'secondary',
    [InvoiceStatus.PendingSignature]: 'info',
    [InvoiceStatus.PendingAuthorization]: 'info',
    [InvoiceStatus.Authorized]: 'success',
    [InvoiceStatus.Rejected]: 'danger',
    [InvoiceStatus.Sent]: 'info',
    [InvoiceStatus.Paid]: 'success',
    [InvoiceStatus.Overdue]: 'danger',
    [InvoiceStatus.Cancelled]: 'warn',
    [InvoiceStatus.Voided]: 'warn',
  }
  return severities[status] || 'secondary'
}

function formatCurrency(amount: number): string {
  return new Intl.NumberFormat('es-EC', {
    style: 'currency',
    currency: 'USD',
  }).format(amount)
}

const statusOptions = computed(() => [
  { value: InvoiceStatus.Draft, label: t('invoices.status_draft') },
  { value: InvoiceStatus.PendingSignature, label: t('invoices.status_pending_signature') },
  { value: InvoiceStatus.PendingAuthorization, label: t('invoices.status_pending_authorization') },
  { value: InvoiceStatus.Authorized, label: t('invoices.status_authorized') },
  { value: InvoiceStatus.Rejected, label: t('invoices.status_rejected') },
  { value: InvoiceStatus.Sent, label: t('invoices.status_sent') },
  { value: InvoiceStatus.Paid, label: t('invoices.status_paid') },
  { value: InvoiceStatus.Overdue, label: t('invoices.status_overdue') },
  { value: InvoiceStatus.Cancelled, label: t('invoices.status_cancelled') },
  { value: InvoiceStatus.Voided, label: t('invoices.status_voided') },
])

const totalPaid = computed(() => {
  return payments.value
    .filter(p => p.status === PaymentStatus.Completed)
    .reduce((sum, p) => sum + p.amount, 0)
})

const remainingBalance = computed(() => {
  if (!invoice.value)
    return 0
  return invoice.value.totalAmount - totalPaid.value
})

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

function getPaymentStatusSeverity(status: PaymentStatus): 'success' | 'warning' | 'secondary' {
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

function recordPayment() {
  if (!invoice.value)
    return
  router.push(`/billing/payments/new?invoiceId=${invoice.value.id}`)
}

function viewPayment(paymentId: string) {
  router.push(`/billing/payments/${paymentId}`)
}

const canChangeStatus = computed(() => {
  if (!invoice.value)
    return false
  return invoice.value.status !== InvoiceStatus.Paid && invoice.value.status !== InvoiceStatus.Cancelled
})

// SRI Workflow Handlers
async function handleGenerateXml() {
  if (!invoice.value)
    return
  sriLoading.value.generateXml = true
  try {
    invoice.value = await generateXml(invoice.value.id)
    toast.showSuccess(t('invoices.sri.xml_generated'))
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : t('invoices.sri.xml_generation_error')
    toast.showError(t('invoices.sri.xml_generation_error'), errMessage)
  }
  finally {
    sriLoading.value.generateXml = false
  }
}

async function handleSignDocument() {
  if (!invoice.value)
    return
  sriLoading.value.sign = true
  try {
    invoice.value = await signDocument(invoice.value.id)
    toast.showSuccess(t('invoices.sri.document_signed'))
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : t('invoices.sri.sign_error')
    toast.showError(t('invoices.sri.sign_error'), errMessage)
  }
  finally {
    sriLoading.value.sign = false
  }
}

async function handleSubmitToSri() {
  if (!invoice.value)
    return
  sriLoading.value.submit = true
  try {
    invoice.value = await submitToSri(invoice.value.id)
    toast.showSuccess(t('invoices.sri.submitted'))
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : t('invoices.sri.submit_error')
    toast.showError(t('invoices.sri.submit_error'), errMessage)
  }
  finally {
    sriLoading.value.submit = false
  }
}

async function handleCheckAuthorization() {
  if (!invoice.value)
    return
  sriLoading.value.checkAuth = true
  try {
    invoice.value = await checkAuthorization(invoice.value.id)
    if (invoice.value.status === InvoiceStatus.Authorized) {
      toast.showSuccess(t('invoices.sri.authorized'))
    }
    else if (invoice.value.status === InvoiceStatus.Rejected) {
      toast.showError(t('invoices.sri.rejected'))
    }
    else {
      toast.showInfo(t('invoices.sri.pending_authorization'))
    }
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : t('invoices.sri.check_auth_error')
    toast.showError(t('invoices.sri.check_auth_error'), errMessage)
  }
  finally {
    sriLoading.value.checkAuth = false
  }
}

async function handleGenerateRide() {
  if (!invoice.value)
    return
  sriLoading.value.generateRide = true
  try {
    invoice.value = await generateRide(invoice.value.id)
    toast.showSuccess(t('invoices.sri.ride_generated'))
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : t('invoices.sri.ride_generation_error')
    toast.showError(t('invoices.sri.ride_generation_error'), errMessage)
  }
  finally {
    sriLoading.value.generateRide = false
  }
}

async function handleDownloadXml() {
  if (!invoice.value)
    return
  try {
    const blob = await downloadXml(invoice.value.id)
    const url = window.URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `${invoice.value.invoiceNumber}_signed.xml`
    document.body.appendChild(a)
    a.click()
    window.URL.revokeObjectURL(url)
    document.body.removeChild(a)
    toast.showSuccess(t('invoices.sri.xml_downloaded'))
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : t('invoices.sri.download_error')
    toast.showError(t('invoices.sri.download_error'), errMessage)
  }
}

async function handleDownloadRide() {
  if (!invoice.value)
    return
  try {
    const blob = await downloadRide(invoice.value.id)
    const url = window.URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `${invoice.value.invoiceNumber}_ride.pdf`
    document.body.appendChild(a)
    a.click()
    window.URL.revokeObjectURL(url)
    document.body.removeChild(a)
    toast.showSuccess(t('invoices.sri.ride_downloaded'))
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : t('invoices.sri.download_error')
    toast.showError(t('invoices.sri.download_error'), errMessage)
  }
}

// Computed flags for button visibility
// Allow re-generation from any non-final status so users can fix and resubmit
const canGenerateXml = computed(() => {
  const status = invoice.value?.status
  return status === InvoiceStatus.Draft
    || status === InvoiceStatus.PendingSignature
    || status === InvoiceStatus.PendingAuthorization
})

const canSign = computed(() => {
  const status = invoice.value?.status
  return status === InvoiceStatus.PendingSignature
    || status === InvoiceStatus.PendingAuthorization
})

const canSubmitToSri = computed(() => {
  return invoice.value?.status === InvoiceStatus.PendingAuthorization && invoice.value.signedXmlFilePath
})

const canCheckAuthorization = computed(() => {
  return invoice.value?.status === InvoiceStatus.PendingAuthorization
})

const canGenerateRide = computed(() => {
  return invoice.value?.status === InvoiceStatus.Authorized && !invoice.value.rideFilePath
})

const canDownloadXml = computed(() => {
  return !!invoice.value?.signedXmlFilePath
})

const canDownloadRide = computed(() => {
  return !!invoice.value?.rideFilePath
})
</script>

<template>
  <div>
    <PageHeader
      :title="t('invoices.view_title')"
      :description="t('invoices.view_description')"
    >
      <template #actions>
        <Button
          v-if="can.updateInvoice() && canChangeStatus"
          :label="t('invoices.change_status')"
          icon="pi pi-refresh"
          @click="openStatusChangeDialog"
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

    <div v-else-if="invoice" class="space-y-6">
      <!-- Invoice Header -->
      <Card>
        <template #header>
          <div class="p-6 pb-0">
            <div class="flex justify-between items-start">
              <div>
                <h2 class="text-2xl font-bold mb-2">
                  {{ invoice.invoiceNumber }}
                </h2>
                <Tag
                  :value="getStatusLabel(invoice.status)"
                  :severity="getStatusSeverity(invoice.status)"
                  class="text-base"
                />
              </div>
              <div class="text-right">
                <p class="text-3xl font-bold text-teal-600">
                  {{ formatCurrency(invoice.totalAmount) }}
                </p>
              </div>
            </div>
          </div>
        </template>

        <template #content>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div class="field">
              <label class="font-semibold text-surface-700">{{ t('invoices.customer') }}</label>
              <p class="mt-1 text-lg">
                {{ invoice.customerName }}
              </p>
              <p v-if="invoice.customerTaxId" class="text-surface-600 text-sm">
                {{ t('invoices.tax_id') }}: {{ invoice.customerTaxId }}
              </p>
            </div>

            <div class="field">
              <label class="font-semibold text-surface-700">{{ t('invoices.warehouse') }}</label>
              <p class="mt-1">
                {{ invoice.warehouseName || t('common.not_specified') }}
              </p>
            </div>

            <div class="field">
              <label class="font-semibold text-surface-700">{{ t('invoices.emission_point') }}</label>
              <p v-if="invoice.emissionPointCode && invoice.establishmentCode" class="mt-1">
                <span class="font-mono">{{ invoice.establishmentCode }}-{{ invoice.emissionPointCode }}</span>
                <span v-if="invoice.emissionPointName" class="text-surface-600 ml-2">
                  ({{ invoice.emissionPointName }})
                </span>
              </p>
              <p v-else class="mt-1 text-surface-400">
                {{ t('common.not_specified') }}
              </p>
            </div>

            <div class="field">
              <label class="font-semibold text-surface-700">{{ t('invoices.issue_date') }}</label>
              <p class="mt-1">
                {{ new Date(invoice.issueDate).toLocaleDateString() }}
              </p>
            </div>

            <div class="field">
              <label class="font-semibold text-surface-700">{{ t('invoices.due_date') }}</label>
              <p class="mt-1">
                {{ new Date(invoice.dueDate).toLocaleDateString() }}
              </p>
            </div>

            <div v-if="invoice.paidDate" class="field">
              <label class="font-semibold text-surface-700">{{ t('invoices.paid_date') }}</label>
              <p class="mt-1">
                {{ new Date(invoice.paidDate).toLocaleDateString() }}
              </p>
            </div>

            <div v-if="invoice.sriAuthorization" class="field">
              <label class="font-semibold text-surface-700">{{ t('invoices.sri_authorization') }}</label>
              <p class="mt-1 font-mono">
                {{ invoice.sriAuthorization }}
              </p>
            </div>

            <div v-if="invoice.notes" class="field md:col-span-2">
              <label class="font-semibold text-surface-700">{{ t('invoices.notes') }}</label>
              <p class="mt-1">
                {{ invoice.notes }}
              </p>
            </div>
          </div>
        </template>
      </Card>

      <!-- SRI Electronic Invoicing Workflow -->
      <Card v-if="invoice.status !== InvoiceStatus.Draft || canGenerateXml">
        <template #header>
          <div class="p-6 pb-0">
            <h3 class="text-lg font-semibold flex items-center gap-2">
              <i class="pi pi-file-pdf text-teal-600" />
              {{ t('invoices.sri.title') }}
            </h3>
          </div>
        </template>

        <template #content>
          <div class="space-y-4">
            <!-- Workflow Status -->
            <div class="bg-surface-50 dark:bg-surface-800 p-4 rounded-lg">
              <div class="flex items-center justify-between">
                <div>
                  <p class="text-sm text-surface-600 dark:text-surface-400 mb-1">
                    {{ t('invoices.sri.workflow_status') }}
                  </p>
                  <p class="font-semibold">
                    {{ getStatusLabel(invoice.status) }}
                  </p>
                </div>
                <Tag
                  :value="getStatusLabel(invoice.status)"
                  :severity="getStatusSeverity(invoice.status)"
                  class="text-base"
                />
              </div>

              <div v-if="invoice.accessKey" class="mt-3 pt-3 border-t border-surface-200 dark:border-surface-700">
                <p class="text-sm text-surface-600 dark:text-surface-400 mb-1">
                  {{ t('invoices.sri.access_key') }}
                </p>
                <p class="font-mono text-sm">
                  {{ invoice.accessKey }}
                </p>
              </div>

              <div v-if="invoice.sriAuthorization" class="mt-3 pt-3 border-t border-surface-200 dark:border-surface-700">
                <p class="text-sm text-surface-600 dark:text-surface-400 mb-1">
                  {{ t('invoices.sri.authorization_number') }}
                </p>
                <p class="font-mono text-sm">
                  {{ invoice.sriAuthorization }}
                </p>
              </div>
            </div>

            <!-- Action Buttons -->
            <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
              <!-- Generate XML -->
              <Button
                v-if="canGenerateXml"
                :label="t('invoices.sri.actions.generate_xml')"
                icon="pi pi-file-code"
                :loading="sriLoading.generateXml"
                :disabled="sriLoading.generateXml"
                severity="secondary"
                outlined
                @click="handleGenerateXml"
              />

              <!-- Sign Document -->
              <Button
                v-if="canSign"
                :label="t('invoices.sri.actions.sign_document')"
                icon="pi pi-verified"
                :loading="sriLoading.sign"
                :disabled="sriLoading.sign"
                severity="info"
                outlined
                @click="handleSignDocument"
              />

              <!-- Submit to SRI -->
              <Button
                v-if="canSubmitToSri"
                :label="t('invoices.sri.actions.submit_to_sri')"
                icon="pi pi-send"
                :loading="sriLoading.submit"
                :disabled="sriLoading.submit"
                severity="warning"
                outlined
                @click="handleSubmitToSri"
              />

              <!-- Check Authorization -->
              <Button
                v-if="canCheckAuthorization"
                :label="t('invoices.sri.actions.check_authorization')"
                icon="pi pi-sync"
                :loading="sriLoading.checkAuth"
                :disabled="sriLoading.checkAuth"
                severity="help"
                outlined
                @click="handleCheckAuthorization"
              />

              <!-- Generate RIDE PDF -->
              <Button
                v-if="canGenerateRide"
                :label="t('invoices.sri.actions.generate_ride')"
                icon="pi pi-file-pdf"
                :loading="sriLoading.generateRide"
                :disabled="sriLoading.generateRide"
                severity="success"
                outlined
                @click="handleGenerateRide"
              />

              <!-- Download XML -->
              <Button
                v-if="canDownloadXml"
                :label="t('invoices.sri.actions.download_xml')"
                icon="pi pi-download"
                severity="secondary"
                text
                @click="handleDownloadXml"
              />

              <!-- Download RIDE PDF -->
              <Button
                v-if="canDownloadRide"
                :label="t('invoices.sri.actions.download_ride')"
                icon="pi pi-file-pdf"
                severity="success"
                text
                @click="handleDownloadRide"
              />
            </div>

            <!-- Info Message -->
            <Message v-if="invoice.status === InvoiceStatus.Draft" severity="info" :closable="false">
              {{ t('invoices.sri.info_draft') }}
            </Message>

            <Message v-else-if="invoice.status === InvoiceStatus.PendingSignature" severity="info" :closable="false">
              {{ t('invoices.sri.info_pending_signature') }}
            </Message>

            <Message v-else-if="invoice.status === InvoiceStatus.PendingAuthorization" severity="warn" :closable="false">
              {{ t('invoices.sri.info_pending_authorization') }}
            </Message>

            <Message v-else-if="invoice.status === InvoiceStatus.Authorized" severity="success" :closable="false">
              {{ t('invoices.sri.info_authorized') }}
            </Message>

            <Message v-else-if="invoice.status === InvoiceStatus.Rejected" severity="error" :closable="false">
              {{ t('invoices.sri.info_rejected') }}
            </Message>
          </div>
        </template>
      </Card>

      <!-- Invoice Items -->
      <Card>
        <template #header>
          <div class="p-6 pb-0">
            <h3 class="text-lg font-semibold">
              {{ t('invoices.items') }}
            </h3>
          </div>
        </template>

        <template #content>
          <DataTable :value="invoice.items" striped-rows>
            <Column field="productName" :header="t('invoices.product')" />
            <Column field="description" :header="t('invoices.description')" />
            <Column field="quantity" :header="t('invoices.quantity')" />

            <Column field="unitPrice" :header="t('invoices.unit_price')">
              <template #body="{ data }">
                {{ formatCurrency(data.unitPrice) }}
              </template>
            </Column>

            <Column field="taxRate" :header="t('invoices.tax_rate')">
              <template #body="{ data }">
                {{ (data.taxRate * 100).toFixed(2) }}%
              </template>
            </Column>

            <Column field="subtotalAmount" :header="t('invoices.subtotal')">
              <template #body="{ data }">
                {{ formatCurrency(data.subtotalAmount) }}
              </template>
            </Column>

            <Column field="taxAmount" :header="t('invoices.tax')">
              <template #body="{ data }">
                {{ formatCurrency(data.taxAmount) }}
              </template>
            </Column>

            <Column field="totalAmount" :header="t('invoices.total')">
              <template #body="{ data }">
                <span class="font-semibold">{{ formatCurrency(data.totalAmount) }}</span>
              </template>
            </Column>
          </DataTable>

          <!-- Totals Summary -->
          <div class="mt-6 flex justify-end">
            <div class="w-full md:w-1/3 space-y-2">
              <div class="flex justify-between py-2 border-b">
                <span class="font-semibold">{{ t('invoices.subtotal') }}:</span>
                <span>{{ formatCurrency(invoice.subtotalAmount) }}</span>
              </div>
              <div class="flex justify-between py-2 border-b">
                <span class="font-semibold">{{ t('invoices.tax') }}:</span>
                <span>{{ formatCurrency(invoice.taxAmount) }}</span>
              </div>
              <div class="flex justify-between py-3 border-t-2 border-surface-700">
                <span class="text-lg font-bold">{{ t('invoices.total') }}:</span>
                <span class="text-lg font-bold text-teal-600">{{ formatCurrency(invoice.totalAmount) }}</span>
              </div>
            </div>
          </div>
        </template>
      </Card>

      <!-- Payments Section -->
      <Card>
        <template #header>
          <div class="p-6 pb-0 flex justify-between items-center">
            <h3 class="text-lg font-semibold">
              {{ t('payments.title') }}
            </h3>
            <Button
              v-if="can.createPayment() && remainingBalance > 0"
              :label="t('payments.record_payment')"
              icon="pi pi-plus"
              size="small"
              @click="recordPayment"
            />
          </div>
        </template>

        <template #content>
          <div v-if="paymentsLoading" class="flex justify-center py-8">
            <ProgressSpinner />
          </div>

          <div v-else-if="payments.length > 0">
            <DataTable :value="payments" striped-rows>
              <Column field="paymentDate" :header="t('payments.payment_date')">
                <template #body="{ data }">
                  {{ new Date(data.paymentDate).toLocaleDateString() }}
                </template>
              </Column>

              <Column field="amount" :header="t('payments.amount')">
                <template #body="{ data }">
                  {{ formatCurrency(data.amount) }}
                </template>
              </Column>

              <Column field="paymentMethod" :header="t('payments.payment_method')">
                <template #body="{ data }">
                  {{ getPaymentMethodLabel(data.paymentMethod) }}
                </template>
              </Column>

              <Column field="status" :header="t('payments.status.label')">
                <template #body="{ data }">
                  <Tag
                    :value="t(`payments.status.${data.status}`)"
                    :severity="getPaymentStatusSeverity(data.status)"
                  />
                </template>
              </Column>

              <Column field="transactionId" :header="t('payments.transaction_id')">
                <template #body="{ data }">
                  {{ data.transactionId || '—' }}
                </template>
              </Column>

              <Column :header="t('common.actions')">
                <template #body="{ data }">
                  <Button
                    icon="pi pi-eye"
                    severity="secondary"
                    text
                    rounded
                    @click="viewPayment(data.id)"
                  />
                </template>
              </Column>
            </DataTable>

            <!-- Payment Summary -->
            <div class="mt-6 flex justify-end">
              <div class="w-full md:w-1/3 space-y-2">
                <div class="flex justify-between py-2 border-b">
                  <span class="font-semibold">{{ t('payments.total_paid') }}:</span>
                  <span class="text-teal-600">{{ formatCurrency(totalPaid) }}</span>
                </div>
                <div class="flex justify-between py-2 border-b">
                  <span class="font-semibold">{{ t('payments.remaining_balance') }}:</span>
                  <span :class="remainingBalance > 0 ? 'text-orange-600' : 'text-teal-600'">
                    {{ formatCurrency(remainingBalance) }}
                  </span>
                </div>
              </div>
            </div>
          </div>

          <div v-else class="py-8 text-center text-surface-500">
            <i class="pi pi-wallet text-4xl mb-3 block" />
            <p>{{ t('payments.no_payments') }}</p>
            <Button
              v-if="can.createPayment()"
              :label="t('payments.record_first_payment')"
              icon="pi pi-plus"
              class="mt-4"
              @click="recordPayment"
            />
          </div>
        </template>
      </Card>
    </div>

    <!-- Status Change Dialog -->
    <Dialog
      v-model:visible="statusChangeDialog"
      :header="t('invoices.change_status_title')"
      :modal="true"
      class="w-[450px]"
    >
      <div class="space-y-4">
        <p>{{ t('invoices.change_status_message') }}</p>

        <div class="field">
          <label for="newStatus" class="font-semibold">{{ t('invoices.new_status') }}</label>
          <Select
            id="newStatus"
            v-model="newStatus"
            :options="statusOptions"
            option-label="label"
            option-value="value"
            class="w-full mt-2"
          />
        </div>
      </div>

      <template #footer>
        <Button
          :label="t('common.cancel')"
          severity="secondary"
          text
          :disabled="statusChangeLoading"
          @click="statusChangeDialog = false"
        />
        <Button
          :label="t('common.save')"
          :loading="statusChangeLoading"
          :disabled="statusChangeLoading"
          @click="handleStatusChange"
        />
      </template>
    </Dialog>
  </div>
</template>
