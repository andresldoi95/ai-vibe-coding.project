<script setup lang="ts">
import type { CreditNote, SriErrorLog } from '~/types/billing'
import { InvoiceStatus } from '~/types/billing'

definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const route = useRoute()
const router = useRouter()
const toast = useNotification()
const {
  getCreditNoteById,
  generateXml,
  signDocument,
  submitToSri,
  checkAuthorization,
  generateRide,
  downloadXml,
  downloadRide,
  getSriErrors,
  deleteCreditNote,
} = useCreditNote()
const { can } = usePermissions()

const creditNote = ref<CreditNote | null>(null)
const sriErrors = ref<SriErrorLog[]>([])
const loading = ref(true)
const sriErrorsLoading = ref(false)
const deleteLoading = ref(false)
const deleteDialog = ref(false)

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
    creditNote.value = await getCreditNoteById(id)
    await loadSriErrors(id)
  }
  catch {
    toast.showError(t('creditNotes.load_error'))
    router.push('/billing/credit-notes')
  }
  finally {
    loading.value = false
  }
})

async function loadSriErrors(id: string) {
  sriErrorsLoading.value = true
  try {
    sriErrors.value = await getSriErrors(id)
  }
  catch (error) {
    console.error('Failed to load SRI errors:', error)
  }
  finally {
    sriErrorsLoading.value = false
  }
}

function handleBack() {
  router.push('/billing/credit-notes')
}

function handleEdit() {
  router.push(`/billing/credit-notes/${creditNote.value?.id}/edit`)
}

async function handleDelete() {
  if (!creditNote.value)
    return

  deleteLoading.value = true
  try {
    await deleteCreditNote(creditNote.value.id)
    toast.showSuccess(t('creditNotes.delete_success'))
    router.push('/billing/credit-notes')
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('creditNotes.delete_error'), errMessage)
  }
  finally {
    deleteLoading.value = false
    deleteDialog.value = false
  }
}

// SRI Workflow Handlers
async function handleGenerateXml() {
  if (!creditNote.value)
    return
  sriLoading.value.generateXml = true
  try {
    creditNote.value = await generateXml(creditNote.value.id)
    toast.showSuccess(t('creditNotes.sri.xml_generated'))
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : t('creditNotes.sri.xml_generation_error')
    toast.showError(t('creditNotes.sri.xml_generation_error'), errMessage)
  }
  finally {
    sriLoading.value.generateXml = false
  }
}

async function handleSignDocument() {
  if (!creditNote.value)
    return
  sriLoading.value.sign = true
  try {
    creditNote.value = await signDocument(creditNote.value.id)
    toast.showSuccess(t('creditNotes.sri.document_signed'))
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : t('creditNotes.sri.sign_error')
    toast.showError(t('creditNotes.sri.sign_error'), errMessage)
  }
  finally {
    sriLoading.value.sign = false
  }
}

async function handleSubmitToSri() {
  if (!creditNote.value)
    return
  sriLoading.value.submit = true
  try {
    creditNote.value = await submitToSri(creditNote.value.id)
    toast.showSuccess(t('creditNotes.sri.submitted'))
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : t('creditNotes.sri.submit_error')
    toast.showError(t('creditNotes.sri.submit_error'), errMessage)
  }
  finally {
    sriLoading.value.submit = false
  }
}

async function handleCheckAuthorization() {
  if (!creditNote.value)
    return
  sriLoading.value.checkAuth = true
  try {
    creditNote.value = await checkAuthorization(creditNote.value.id)
    await loadSriErrors(creditNote.value.id)
    if (creditNote.value.status === InvoiceStatus.Authorized) {
      toast.showSuccess(t('creditNotes.sri.authorized'))
    }
    else if (creditNote.value.status === InvoiceStatus.Rejected) {
      toast.showError(t('creditNotes.sri.rejected'))
    }
    else {
      toast.showInfo(t('creditNotes.sri.pending_authorization'))
    }
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : t('creditNotes.sri.check_auth_error')
    toast.showError(t('creditNotes.sri.check_auth_error'), errMessage)
  }
  finally {
    sriLoading.value.checkAuth = false
  }
}

async function handleGenerateRide() {
  if (!creditNote.value)
    return
  sriLoading.value.generateRide = true
  try {
    creditNote.value = await generateRide(creditNote.value.id)
    toast.showSuccess(t('creditNotes.sri.ride_generated'))
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : t('creditNotes.sri.ride_generation_error')
    toast.showError(t('creditNotes.sri.ride_generation_error'), errMessage)
  }
  finally {
    sriLoading.value.generateRide = false
  }
}

async function handleDownloadXml() {
  if (!creditNote.value)
    return
  try {
    const blob = await downloadXml(creditNote.value.id)
    const url = window.URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `${creditNote.value.creditNoteNumber}_signed.xml`
    document.body.appendChild(a)
    a.click()
    window.URL.revokeObjectURL(url)
    document.body.removeChild(a)
    toast.showSuccess(t('creditNotes.sri.xml_downloaded'))
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : t('creditNotes.sri.download_error')
    toast.showError(t('creditNotes.sri.download_error'), errMessage)
  }
}

async function handleDownloadRide() {
  if (!creditNote.value)
    return
  try {
    const blob = await downloadRide(creditNote.value.id)
    const url = window.URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `${creditNote.value.creditNoteNumber}_ride.pdf`
    document.body.appendChild(a)
    a.click()
    window.URL.revokeObjectURL(url)
    document.body.removeChild(a)
    toast.showSuccess(t('creditNotes.sri.ride_downloaded'))
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : t('creditNotes.sri.download_error')
    toast.showError(t('creditNotes.sri.download_error'), errMessage)
  }
}

// Computed flags
const canGenerateXml = computed(() => {
  const s = creditNote.value?.status
  return s === InvoiceStatus.Draft
    || s === InvoiceStatus.PendingSignature
    || s === InvoiceStatus.PendingAuthorization
})

const canSign = computed(() => {
  const s = creditNote.value?.status
  return s === InvoiceStatus.PendingSignature || s === InvoiceStatus.PendingAuthorization
})

const canSubmitToSri = computed(() => {
  return creditNote.value?.status === InvoiceStatus.PendingAuthorization && !!creditNote.value.signedXmlFilePath
})

const canCheckAuthorization = computed(() => {
  return creditNote.value?.status === InvoiceStatus.PendingAuthorization
})

const canGenerateRide = computed(() => {
  return creditNote.value?.status === InvoiceStatus.Authorized && !creditNote.value.rideFilePath
})

const canDownloadXml = computed(() => !!creditNote.value?.signedXmlFilePath)
const canDownloadRide = computed(() => !!creditNote.value?.rideFilePath)

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
  return labels[status] ?? status.toString()
}

function getStatusSeverity(status: InvoiceStatus): string {
  const map: Record<InvoiceStatus, string> = {
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
  return map[status] ?? 'secondary'
}

function formatCurrency(amount: number): string {
  return new Intl.NumberFormat('es-EC', { style: 'currency', currency: 'USD' }).format(amount)
}
</script>

<template>
  <div>
    <PageHeader
      :title="t('creditNotes.view_title')"
      :description="t('creditNotes.view_description')"
    >
      <template #actions>
        <Button
          v-if="creditNote?.isEditable && can.updateCreditNote()"
          :label="t('common.edit')"
          icon="pi pi-pencil"
          severity="secondary"
          @click="handleEdit"
        />
        <Button
          v-if="creditNote?.isEditable && can.deleteCreditNote()"
          :label="t('common.delete')"
          icon="pi pi-trash"
          severity="danger"
          outlined
          @click="deleteDialog = true"
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

    <div v-else-if="creditNote" class="space-y-6">
      <!-- Header Card -->
      <Card>
        <template #header>
          <div class="p-6 pb-0">
            <div class="flex justify-between items-start">
              <div>
                <h2 class="text-2xl font-bold mb-2">
                  {{ creditNote.creditNoteNumber }}
                </h2>
                <Tag
                  :value="getStatusLabel(creditNote.status)"
                  :severity="getStatusSeverity(creditNote.status)"
                  class="text-base"
                />
              </div>
              <div class="text-right">
                <p class="text-sm text-surface-500">
                  {{ t('creditNotes.value_modification') }}
                </p>
                <p class="text-3xl font-bold text-teal-600">
                  {{ formatCurrency(creditNote.valueModification) }}
                </p>
              </div>
            </div>
          </div>
        </template>

        <template #content>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div class="field">
              <label class="font-semibold text-surface-700">{{ t('creditNotes.customer') }}</label>
              <p class="mt-1 text-lg">
                {{ creditNote.customerName }}
              </p>
            </div>

            <div class="field">
              <label class="font-semibold text-surface-700">{{ t('creditNotes.issue_date') }}</label>
              <p class="mt-1">
                {{ new Date(creditNote.issueDate).toLocaleDateString('es-EC') }}
              </p>
            </div>

            <div class="field">
              <label class="font-semibold text-surface-700">{{ t('creditNotes.original_invoice') }}</label>
              <p class="mt-1 font-mono">
                {{ creditNote.originalInvoiceNumber }}
              </p>
              <p v-if="creditNote.originalInvoiceDate" class="text-sm text-surface-500">
                {{ new Date(creditNote.originalInvoiceDate).toLocaleDateString('es-EC') }}
              </p>
            </div>

            <div class="field">
              <label class="font-semibold text-surface-700">{{ t('creditNotes.emission_point') }}</label>
              <p v-if="creditNote.emissionPointCode && creditNote.establishmentCode" class="mt-1 font-mono">
                {{ creditNote.establishmentCode }}-{{ creditNote.emissionPointCode }}
              </p>
              <p v-else class="mt-1 text-surface-400">
                {{ t('common.not_specified') }}
              </p>
            </div>

            <div class="field md:col-span-2">
              <label class="font-semibold text-surface-700">{{ t('creditNotes.reason') }}</label>
              <p class="mt-1 bg-surface-50 dark:bg-surface-800 p-3 rounded">
                {{ creditNote.reason }}
              </p>
            </div>

            <div v-if="creditNote.notes" class="field md:col-span-2">
              <label class="font-semibold text-surface-700">{{ t('creditNotes.notes') }}</label>
              <p class="mt-1">
                {{ creditNote.notes }}
              </p>
            </div>
          </div>
        </template>
      </Card>

      <!-- Line Items -->
      <Card>
        <template #header>
          <div class="p-6 pb-0">
            <h3 class="text-lg font-semibold">
              {{ t('creditNotes.items_section') }}
            </h3>
          </div>
        </template>
        <template #content>
          <DataTable :value="creditNote.items">
            <Column :header="t('creditNotes.item_description')" field="description" />
            <Column :header="t('creditNotes.item_quantity')" field="quantity" style="width: 8rem">
              <template #body="{ data }">
                {{ data.quantity }}
              </template>
            </Column>
            <Column :header="t('creditNotes.item_unit_price')" style="width: 10rem">
              <template #body="{ data }">
                {{ formatCurrency(data.unitPrice) }}
              </template>
            </Column>
            <Column :header="t('creditNotes.item_tax_rate')" style="width: 8rem">
              <template #body="{ data }">
                {{ data.taxRateName || data.taxRateId }}
              </template>
            </Column>
            <Column :header="t('creditNotes.item_total')" style="width: 10rem">
              <template #body="{ data }">
                <span class="font-medium">{{ formatCurrency(data.total) }}</span>
              </template>
            </Column>
          </DataTable>

          <!-- Totals Summary -->
          <div class="flex justify-end mt-4">
            <div class="w-72 space-y-2">
              <div class="flex justify-between text-sm">
                <span>{{ t('creditNotes.subtotal') }}</span>
                <span>{{ formatCurrency(creditNote.subtotal) }}</span>
              </div>
              <div class="flex justify-between text-sm">
                <span>{{ t('creditNotes.tax') }}</span>
                <span>{{ formatCurrency(creditNote.taxAmount) }}</span>
              </div>
              <Divider />
              <div class="flex justify-between font-semibold text-lg">
                <span>{{ t('creditNotes.value_modification') }}</span>
                <span class="text-teal-600">{{ formatCurrency(creditNote.valueModification) }}</span>
              </div>
            </div>
          </div>
        </template>
      </Card>

      <!-- SRI Workflow Card -->
      <Card>
        <template #header>
          <div class="p-6 pb-0">
            <h3 class="text-lg font-semibold flex items-center gap-2">
              <i class="pi pi-file-pdf text-teal-600" />
              {{ t('creditNotes.sri.title') }}
            </h3>
          </div>
        </template>

        <template #content>
          <div class="space-y-4">
            <!-- Status Bar -->
            <div class="bg-surface-50 dark:bg-surface-800 p-4 rounded-lg">
              <div class="flex items-center justify-between">
                <div>
                  <p class="text-sm text-surface-500 mb-1">
                    {{ t('creditNotes.sri.workflow_status') }}
                  </p>
                  <p class="font-semibold">
                    {{ getStatusLabel(creditNote.status) }}
                  </p>
                </div>
                <Tag
                  :value="getStatusLabel(creditNote.status)"
                  :severity="getStatusSeverity(creditNote.status)"
                />
              </div>

              <div v-if="creditNote.accessKey" class="mt-3 pt-3 border-t border-surface-200 dark:border-surface-700">
                <p class="text-xs text-surface-500 mb-1">
                  {{ t('creditNotes.sri.access_key') }}
                </p>
                <p class="font-mono text-xs break-all">
                  {{ creditNote.accessKey }}
                </p>
              </div>

              <div v-if="creditNote.sriAuthorization" class="mt-3 pt-3 border-t border-surface-200 dark:border-surface-700">
                <p class="text-xs text-surface-500 mb-1">
                  {{ t('creditNotes.sri.authorization_number') }}
                </p>
                <p class="font-mono text-sm">
                  {{ creditNote.sriAuthorization }}
                </p>
              </div>
            </div>

            <!-- SRI Action Buttons -->
            <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
              <Button
                v-if="canGenerateXml && can.manageCreditNote()"
                :label="t('creditNotes.sri.generate_xml')"
                icon="pi pi-file-code"
                :loading="sriLoading.generateXml"
                severity="secondary"
                outlined
                @click="handleGenerateXml"
              />

              <Button
                v-if="canSign && can.manageCreditNote()"
                :label="t('creditNotes.sri.sign')"
                icon="pi pi-verified"
                :loading="sriLoading.sign"
                severity="info"
                outlined
                @click="handleSignDocument"
              />

              <Button
                v-if="canSubmitToSri && can.manageCreditNote()"
                :label="t('creditNotes.sri.submit_to_sri')"
                icon="pi pi-send"
                :loading="sriLoading.submit"
                severity="warning"
                outlined
                @click="handleSubmitToSri"
              />

              <Button
                v-if="canCheckAuthorization && can.manageCreditNote()"
                :label="t('creditNotes.sri.check_authorization')"
                icon="pi pi-refresh"
                :loading="sriLoading.checkAuth"
                severity="info"
                @click="handleCheckAuthorization"
              />

              <Button
                v-if="canGenerateRide && can.manageCreditNote()"
                :label="t('creditNotes.sri.generate_ride')"
                icon="pi pi-file-pdf"
                :loading="sriLoading.generateRide"
                severity="success"
                outlined
                @click="handleGenerateRide"
              />

              <Button
                v-if="canDownloadXml"
                :label="t('creditNotes.sri.download_xml')"
                icon="pi pi-download"
                severity="secondary"
                text
                @click="handleDownloadXml"
              />

              <Button
                v-if="canDownloadRide"
                :label="t('creditNotes.sri.download_ride')"
                icon="pi pi-file-pdf"
                severity="secondary"
                text
                @click="handleDownloadRide"
              />
            </div>
          </div>
        </template>
      </Card>

      <!-- SRI Errors -->
      <Card v-if="sriErrors.length > 0 || sriErrorsLoading">
        <template #header>
          <div class="p-6 pb-0">
            <h3 class="text-lg font-semibold text-red-600 flex items-center gap-2">
              <i class="pi pi-exclamation-triangle" />
              {{ t('creditNotes.sri.errors_title') }}
            </h3>
          </div>
        </template>
        <template #content>
          <div v-if="sriErrorsLoading" class="py-4 text-center">
            <ProgressSpinner style="width: 30px; height: 30px;" />
          </div>
          <DataTable v-else :value="sriErrors">
            <Column :header="t('creditNotes.sri.error_operation')" field="operation" style="width: 12rem" />
            <Column :header="t('creditNotes.sri.error_code')" field="errorCode" style="width: 12rem" />
            <Column :header="t('creditNotes.sri.error_message')" field="message" />
            <Column :header="t('creditNotes.sri.error_date')" style="width: 12rem">
              <template #body="{ data }">
                {{ new Date(data.occurredAt).toLocaleString('es-EC') }}
              </template>
            </Column>
          </DataTable>
        </template>
      </Card>
    </div>

    <!-- Delete Confirmation -->
    <Dialog
      v-model:visible="deleteDialog"
      modal
      :header="t('creditNotes.delete_title')"
      :style="{ width: '400px' }"
    >
      <p>{{ t('creditNotes.delete_confirm', { number: creditNote?.creditNoteNumber }) }}</p>
      <template #footer>
        <Button
          :label="t('common.cancel')"
          severity="secondary"
          outlined
          @click="deleteDialog = false"
        />
        <Button
          :label="t('common.delete')"
          severity="danger"
          :loading="deleteLoading"
          @click="handleDelete"
        />
      </template>
    </Dialog>
  </div>
</template>
