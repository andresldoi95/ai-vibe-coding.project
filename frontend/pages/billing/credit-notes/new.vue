<script setup lang="ts">
import { useVuelidate } from '@vuelidate/core'
import { required } from '@vuelidate/validators'
import type { CreateCreditNoteDto, CreateCreditNoteItemDto, Customer, Invoice, TaxRate } from '~/types'
import type { EmissionPoint } from '~/types/emission-point'

definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const toast = useNotification()
const router = useRouter()
const { createCreditNote } = useCreditNote()
const { getAllTaxRates } = useTaxRate()
const { getAllCustomers } = useCustomer()
const { getAllEmissionPoints } = useEmissionPoint()
const { getAllInvoices, getInvoiceById } = useInvoice()

const loading = ref(false)
const initialLoading = ref(true)
const importingInvoice = ref(false)

const customers = ref<Customer[]>([])
const taxRates = ref<TaxRate[]>([])
const emissionPoints = ref<EmissionPoint[]>([])
const availableInvoices = ref<Invoice[]>([])

const formData = reactive({
  customerId: null as string | null,
  originalInvoiceId: null as string | null,
  emissionPointId: null as string | null,
  issueDate: new Date() as Date | null,
  reason: '',
  notes: '',
  isPhysicalReturn: false,
  items: [] as CreateCreditNoteItemDto[],
})

// Stores the original invoice's display data after import
const originalInvoiceNumber = ref('')
const originalInvoiceDate = ref('')

onMounted(async () => {
  try {
    const [customerList, taxRateList, emissionPointList] = await Promise.all([
      getAllCustomers(),
      getAllTaxRates(),
      getAllEmissionPoints({ isActive: true }),
    ])

    customers.value = customerList
    taxRates.value = taxRateList.filter(tr => tr.isActive)
    emissionPoints.value = emissionPointList
  }
  catch {
    toast.showError(t('creditNotes.load_error'))
  }
  finally {
    initialLoading.value = false
  }
})

// When customer changes, reload authorized invoices for that customer
watch(() => formData.customerId, async (newCustomerId) => {
  formData.originalInvoiceId = null
  formData.items = []
  originalInvoiceNumber.value = ''
  originalInvoiceDate.value = ''
  availableInvoices.value = []

  if (newCustomerId) {
    try {
      // Load authorized invoices for this customer
      const invoices = await getAllInvoices({ customerId: newCustomerId, status: 3 /* Authorized */ })
      availableInvoices.value = invoices
    }
    catch {
      toast.showError(t('creditNotes.load_invoices_error'))
    }
  }
})

// When an invoice is selected, import its items
async function handleImportInvoice() {
  if (!formData.originalInvoiceId)
    return

  importingInvoice.value = true
  try {
    const invoice = await getInvoiceById(formData.originalInvoiceId)

    // Auto-populate items from the invoice
    formData.items = invoice.items.map(item => ({
      productId: item.productId,
      quantity: item.quantity,
      unitPrice: item.unitPrice,
      taxRateId: item.taxRateId,
      description: item.description || '',
    }))

    // Store reference data for display (read-only)
    originalInvoiceNumber.value = invoice.invoiceNumber
    originalInvoiceDate.value = invoice.issueDate

    toast.showSuccess(t('creditNotes.invoice_imported'))
  }
  catch {
    toast.showError(t('creditNotes.import_error'))
  }
  finally {
    importingInvoice.value = false
  }
}

const rules = computed(() => ({
  customerId: { required },
  originalInvoiceId: { required },
  emissionPointId: { required },
  issueDate: { required },
  reason: { required },
  items: { required },
}))

const v$ = useVuelidate(rules, formData)

function addLineItem() {
  const defaultTaxRate = taxRates.value.find(tr => tr.isDefault) || taxRates.value[0]
  formData.items.push({
    productId: '',
    quantity: 1,
    unitPrice: 0,
    taxRateId: defaultTaxRate?.id || '',
    description: '',
  })
}

function removeLineItem(index: number) {
  formData.items.splice(index, 1)
}

function getLineItemTaxRate(taxRateId: string): number {
  return taxRates.value.find(tr => tr.id === taxRateId)?.rate || 0
}

function calculateLineItemSubtotal(item: CreateCreditNoteItemDto): number {
  return item.quantity * item.unitPrice
}

function calculateLineItemTax(item: CreateCreditNoteItemDto): number {
  return calculateLineItemSubtotal(item) * getLineItemTaxRate(item.taxRateId)
}

function calculateLineItemTotal(item: CreateCreditNoteItemDto): number {
  return calculateLineItemSubtotal(item) + calculateLineItemTax(item)
}

const subtotal = computed(() => formData.items.reduce((sum, item) => sum + calculateLineItemSubtotal(item), 0))
const tax = computed(() => formData.items.reduce((sum, item) => sum + calculateLineItemTax(item), 0))
const total = computed(() => subtotal.value + tax.value)

function formatCurrency(amount: number): string {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', minimumFractionDigits: 2 }).format(amount)
}

async function handleSubmit() {
  const isValid = await v$.value.$validate()
  if (!isValid || formData.items.length === 0) {
    toast.showError(t('common.validation_error'), t('creditNotes.add_items_error'))
    return
  }

  loading.value = true
  try {
    const payload: CreateCreditNoteDto = {
      customerId: formData.customerId!,
      originalInvoiceId: formData.originalInvoiceId!,
      emissionPointId: formData.emissionPointId!,
      issueDate: formData.issueDate instanceof Date ? formData.issueDate.toISOString().split('T')[0] : '',
      reason: formData.reason,
      notes: formData.notes || undefined,
      isPhysicalReturn: formData.isPhysicalReturn,
      items: formData.items,
    }

    await createCreditNote(payload)
    toast.showSuccess(t('creditNotes.create_success'))
    router.push('/billing/credit-notes')
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('creditNotes.create_error'), errMessage)
  }
  finally {
    loading.value = false
  }
}

function handleCancel() {
  router.push('/billing/credit-notes')
}
</script>

<template>
  <div>
    <PageHeader
      :title="t('creditNotes.create')"
      :description="t('creditNotes.create_description')"
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
          <!-- Header Section: Customer + Invoice Selection -->
          <div class="mb-8">
            <h3 class="text-lg font-semibold mb-4">
              {{ t('creditNotes.header_section') }}
            </h3>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <!-- Customer -->
              <div>
                <label class="block text-sm font-medium mb-2">
                  {{ t('creditNotes.customer') }} *
                </label>
                <Dropdown
                  v-model="formData.customerId"
                  :options="customers"
                  option-label="name"
                  option-value="id"
                  :placeholder="t('creditNotes.select_customer')"
                  class="w-full"
                  :class="{ 'p-invalid': v$.customerId.$error }"
                  filter
                />
                <small v-if="v$.customerId.$error" class="p-error">{{ t('common.required') }}</small>
              </div>

              <!-- Emission Point -->
              <div>
                <label class="block text-sm font-medium mb-2">
                  {{ t('creditNotes.emission_point') }} *
                </label>
                <Dropdown
                  v-model="formData.emissionPointId"
                  :options="emissionPoints"
                  option-label="name"
                  option-value="id"
                  :placeholder="t('creditNotes.select_emission_point')"
                  class="w-full"
                  :class="{ 'p-invalid': v$.emissionPointId.$error }"
                />
                <small v-if="v$.emissionPointId.$error" class="p-error">{{ t('common.required') }}</small>
              </div>

              <!-- Issue Date -->
              <div>
                <label class="block text-sm font-medium mb-2">
                  {{ t('creditNotes.issue_date') }} *
                </label>
                <Calendar
                  v-model="formData.issueDate"
                  date-format="yy-mm-dd"
                  class="w-full"
                  :class="{ 'p-invalid': v$.issueDate.$error }"
                />
                <small v-if="v$.issueDate.$error" class="p-error">{{ t('common.required') }}</small>
              </div>
            </div>
          </div>

          <!-- Original Invoice Import Section -->
          <div class="mb-8">
            <h3 class="text-lg font-semibold mb-4">
              {{ t('creditNotes.original_document_section') }}
            </h3>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4 items-end">
              <div>
                <label class="block text-sm font-medium mb-2">
                  {{ t('creditNotes.original_invoice') }} *
                </label>
                <Dropdown
                  v-model="formData.originalInvoiceId"
                  :options="availableInvoices"
                  :option-label="(inv: Invoice) => `${inv.invoiceNumber} — ${formatCurrency(inv.totalAmount)}`"
                  option-value="id"
                  :placeholder="formData.customerId ? t('creditNotes.select_invoice') : t('creditNotes.select_customer_first')"
                  :disabled="!formData.customerId"
                  class="w-full"
                  :class="{ 'p-invalid': v$.originalInvoiceId.$error }"
                  filter
                />
                <small v-if="v$.originalInvoiceId.$error" class="p-error">{{ t('common.required') }}</small>
              </div>
              <div>
                <Button
                  type="button"
                  :label="importingInvoice ? t('creditNotes.importing') : t('creditNotes.import_items')"
                  icon="pi pi-download"
                  :disabled="!formData.originalInvoiceId || importingInvoice"
                  :loading="importingInvoice"
                  severity="secondary"
                  @click="handleImportInvoice"
                />
              </div>
              <div v-if="originalInvoiceNumber">
                <small class="text-surface-500">
                  {{ t('creditNotes.imported_from') }}: <strong>{{ originalInvoiceNumber }}</strong>
                  ({{ new Date(originalInvoiceDate).toLocaleDateString('es-EC') }})
                </small>
              </div>
            </div>
          </div>

          <!-- Reason -->
          <div class="mb-8">
            <h3 class="text-lg font-semibold mb-4">
              {{ t('creditNotes.reason_section') }}
            </h3>
            <div>
              <label class="block text-sm font-medium mb-2">
                {{ t('creditNotes.reason') }} *
              </label>
              <Textarea
                v-model="formData.reason"
                rows="3"
                class="w-full"
                :class="{ 'p-invalid': v$.reason.$error }"
                :placeholder="t('creditNotes.reason_placeholder')"
              />
              <small v-if="v$.reason.$error" class="p-error">{{ t('common.required') }}</small>
            </div>
            <div class="mt-4">
              <label class="block text-sm font-medium mb-2">
                {{ t('creditNotes.notes') }}
              </label>
              <Textarea
                v-model="formData.notes"
                rows="2"
                class="w-full"
                :placeholder="t('creditNotes.notes_placeholder')"
              />
            </div>

            <!-- Physical Return toggle -->
            <div class="mt-4 flex items-start gap-3 p-3 border rounded-lg bg-surface-50 dark:bg-surface-800">
              <Checkbox
                v-model="formData.isPhysicalReturn"
                input-id="isPhysicalReturn"
                :binary="true"
              />
              <div>
                <label for="isPhysicalReturn" class="font-medium cursor-pointer">
                  {{ t('creditNotes.is_physical_return') }}
                </label>
                <p class="text-sm text-surface-500 mt-0.5">
                  {{ t('creditNotes.is_physical_return_hint') }}
                </p>
              </div>
            </div>
          </div>

          <!-- Line Items -->
          <div class="mb-8">
            <div class="flex items-center justify-between mb-4">
              <h3 class="text-lg font-semibold">
                {{ t('creditNotes.items_section') }}
              </h3>
              <Button
                type="button"
                :label="t('creditNotes.add_item')"
                icon="pi pi-plus"
                size="small"
                severity="secondary"
                outlined
                @click="addLineItem"
              />
            </div>

            <div v-if="formData.items.length === 0" class="text-center py-6 text-surface-500 border border-dashed rounded">
              {{ t('creditNotes.no_items') }}
            </div>

            <DataTable v-else :value="formData.items" class="w-full">
              <Column :header="t('creditNotes.item_quantity')" style="width: 8rem">
                <template #body="{ index }">
                  <InputNumber
                    v-model="formData.items[index].quantity"
                    :min="0.001"
                    :min-fraction-digits="3"
                    :max-fraction-digits="6"
                    class="w-full"
                  />
                </template>
              </Column>
              <Column :header="t('creditNotes.item_description')">
                <template #body="{ index }">
                  <InputText
                    v-model="formData.items[index].description"
                    class="w-full"
                    :placeholder="t('creditNotes.item_description_placeholder')"
                  />
                </template>
              </Column>
              <Column :header="t('creditNotes.item_unit_price')" style="width: 10rem">
                <template #body="{ index }">
                  <InputNumber
                    v-model="formData.items[index].unitPrice"
                    :min="0"
                    :min-fraction-digits="2"
                    :max-fraction-digits="6"
                    mode="currency"
                    currency="USD"
                    class="w-full"
                  />
                </template>
              </Column>
              <Column :header="t('creditNotes.item_tax_rate')" style="width: 10rem">
                <template #body="{ index }">
                  <Dropdown
                    v-model="formData.items[index].taxRateId"
                    :options="taxRates"
                    option-label="name"
                    option-value="id"
                    class="w-full"
                  />
                </template>
              </Column>
              <Column :header="t('creditNotes.item_total')" style="width: 9rem">
                <template #body="{ data }">
                  <span class="font-medium">{{ formatCurrency(calculateLineItemTotal(data)) }}</span>
                </template>
              </Column>
              <Column style="width: 4rem">
                <template #body="{ index }">
                  <Button
                    icon="pi pi-trash"
                    size="small"
                    severity="danger"
                    text
                    @click="removeLineItem(index)"
                  />
                </template>
              </Column>
            </DataTable>

            <!-- Totals -->
            <div v-if="formData.items.length > 0" class="flex justify-end mt-4">
              <div class="w-64 space-y-2">
                <div class="flex justify-between text-sm">
                  <span>{{ t('creditNotes.subtotal') }}</span>
                  <span>{{ formatCurrency(subtotal) }}</span>
                </div>
                <div class="flex justify-between text-sm">
                  <span>{{ t('creditNotes.tax') }}</span>
                  <span>{{ formatCurrency(tax) }}</span>
                </div>
                <Divider />
                <div class="flex justify-between font-semibold">
                  <span>{{ t('creditNotes.value_modification') }}</span>
                  <span>{{ formatCurrency(total) }}</span>
                </div>
              </div>
            </div>
          </div>

          <!-- Actions -->
          <div class="flex justify-end gap-3">
            <Button
              type="button"
              :label="t('common.cancel')"
              severity="secondary"
              outlined
              @click="handleCancel"
            />
            <Button
              type="submit"
              :label="t('creditNotes.create')"
              :loading="loading"
            />
          </div>
        </form>
      </template>
    </Card>
  </div>
</template>
