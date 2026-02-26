<script setup lang="ts">
import { useVuelidate } from '@vuelidate/core'
import { required } from '@vuelidate/validators'
import type { TaxRate } from '~/types'
import type { CreditNote, CreditNoteItem, UpdateCreditNoteDto, UpdateCreditNoteItemDto } from '~/types/billing'

definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const toast = useNotification()
const router = useRouter()
const route = useRoute()
const { getCreditNoteById, updateCreditNote } = useCreditNote()
const { getAllTaxRates } = useTaxRate()

const loading = ref(false)
const initialLoading = ref(true)

const taxRates = ref<TaxRate[]>([])
const creditNote = ref<CreditNote | null>(null)

const formData = reactive({
  issueDate: null as Date | null,
  reason: '',
  notes: '',
  items: [] as UpdateCreditNoteItemDto[],
})

onMounted(async () => {
  try {
    const id = route.params.id as string
    const [taxRateList, creditNoteData] = await Promise.all([
      getAllTaxRates(),
      getCreditNoteById(id),
    ])

    taxRates.value = taxRateList.filter((tr: TaxRate) => tr.isActive)
    creditNote.value = creditNoteData

    // Guard: only draft credit notes can be edited
    if (!creditNoteData.isEditable) {
      toast.showError(t('creditNotes.edit_error'), t('creditNotes.only_draft_editable'))
      router.push(`/billing/credit-notes/${id}`)
      return
    }

    // Populate form
    formData.issueDate = new Date(creditNoteData.issueDate)
    formData.reason = creditNoteData.reason
    formData.notes = creditNoteData.notes || ''
    formData.items = creditNoteData.items.map((item: CreditNoteItem) => ({
      id: item.id,
      productId: item.productId,
      quantity: item.quantity,
      unitPrice: item.unitPrice,
      taxRateId: item.taxRateId,
      description: item.description || '',
    }))
  }
  catch {
    toast.showError(t('creditNotes.load_error'))
    router.push('/billing/credit-notes')
  }
  finally {
    initialLoading.value = false
  }
})

const rules = computed(() => ({
  issueDate: { required },
  reason: { required },
  items: { required },
}))

const v$ = useVuelidate(rules, formData)

function addLineItem() {
  const defaultTaxRate = taxRates.value.find((tr: TaxRate) => tr.isDefault) || taxRates.value[0]
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

function calculateLineSubtotal(item: UpdateCreditNoteItemDto): number {
  return item.quantity * item.unitPrice
}

function calculateLineTotal(item: UpdateCreditNoteItemDto): number {
  return calculateLineSubtotal(item) * (1 + getLineItemTaxRate(item.taxRateId))
}

const subtotal = computed(() => formData.items.reduce((s, i) => s + calculateLineSubtotal(i), 0))
const tax = computed(() => formData.items.reduce((s, i) => s + calculateLineSubtotal(i) * getLineItemTaxRate(i.taxRateId), 0))
const total = computed(() => subtotal.value + tax.value)

function formatCurrency(amount: number): string {
  return new Intl.NumberFormat('es-EC', { style: 'currency', currency: 'USD' }).format(amount)
}

async function handleSubmit() {
  const isValid = await v$.value.$validate()
  if (!isValid) {
    toast.showError(t('common.validation_error'))
    return
  }
  if (formData.items.length === 0) {
    toast.showError(t('creditNotes.add_items_error'))
    return
  }

  loading.value = true
  try {
    const payload: UpdateCreditNoteDto = {
      customerId: creditNote.value!.customerId,
      issueDate: formData.issueDate instanceof Date ? formData.issueDate.toISOString().split('T')[0] : '',
      reason: formData.reason,
      notes: formData.notes || undefined,
      items: formData.items,
    }

    await updateCreditNote(creditNote.value!.id, payload)
    toast.showSuccess(t('creditNotes.update_success'))
    router.push(`/billing/credit-notes/${creditNote.value!.id}`)
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('creditNotes.update_error'), errMessage)
  }
  finally {
    loading.value = false
  }
}

function handleCancel() {
  router.push(`/billing/credit-notes/${creditNote.value?.id}`)
}
</script>

<template>
  <div>
    <PageHeader
      :title="t('creditNotes.edit_title')"
      :description="t('creditNotes.edit_description')"
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
          <!-- Read-only: Original Document Reference -->
          <div class="mb-8 p-4 bg-surface-50 dark:bg-surface-800 rounded-lg">
            <p class="text-sm text-surface-500 mb-1">
              {{ t('creditNotes.original_document_section') }}
            </p>
            <p class="font-mono font-semibold">
              {{ creditNote?.originalInvoiceNumber }}
            </p>
            <p class="text-sm text-surface-500">
              {{ creditNote?.customerName }}
            </p>
          </div>

          <!-- Editable Fields -->
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mb-6">
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

          <!-- Reason -->
          <div class="mb-6">
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

          <!-- Notes -->
          <div class="mb-8">
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

            <DataTable v-else :value="formData.items">
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
                  <span class="font-medium">{{ formatCurrency(calculateLineTotal(data)) }}</span>
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
              :label="t('common.save')"
              :loading="loading"
            />
          </div>
        </form>
      </template>
    </Card>
  </div>
</template>
