<script setup lang="ts">
import { useVuelidate } from '@vuelidate/core'
import { required } from '@vuelidate/validators'
import type { Customer, Product, TaxRate } from '~/types'
import type { Invoice, InvoiceItem, UpdateInvoiceDto, UpdateInvoiceItemDto } from '~/types/billing'
import type { Warehouse } from '~/types/inventory'

definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const toast = useNotification()
const router = useRouter()
const route = useRoute()
const { getInvoiceById, updateInvoice } = useInvoice()
const { getAllTaxRates } = useTaxRate()
const { getAllCustomers } = useCustomer()
const { getAllProducts } = useProduct()
const { getAllWarehouses } = useWarehouse()

const loading = ref(false)
const initialLoading = ref(true)

const customers = ref<Customer[]>([])
const products = ref<Product[]>([])
const taxRates = ref<TaxRate[]>([])
const warehouses = ref<Warehouse[]>([])
const invoice = ref<Invoice | null>(null)

const formData = reactive({
  customerId: null as string | null,
  warehouseId: null as string | null,
  issueDate: '',
  notes: '',
  items: [] as UpdateInvoiceItemDto[],
})

onMounted(async () => {
  try {
    const id = route.params.id as string
    const [customerList, productList, taxRateList, warehouseList, invoiceData] = await Promise.all([
      getAllCustomers(),
      getAllProducts(),
      getAllTaxRates(),
      getAllWarehouses(),
      getInvoiceById(id),
    ])

    customers.value = customerList
    products.value = productList
    taxRates.value = taxRateList.filter((tr: TaxRate) => tr.isActive)
    warehouses.value = warehouseList.filter((w: Warehouse) => w.isActive)
    invoice.value = invoiceData

    // Check if invoice can be edited (only drafts)
    if (invoiceData.status !== 0) { // InvoiceStatus.Draft
      toast.showError(t('invoices.edit_error'), 'Only draft invoices can be edited')
      router.push(`/billing/invoices/${id}`)
      return
    }

    // Populate form with invoice data
    formData.customerId = invoiceData.customerId
    formData.warehouseId = invoiceData.warehouseId || null
    formData.issueDate = new Date(invoiceData.issueDate).toISOString().split('T')[0]
    formData.notes = invoiceData.notes || ''
    formData.items = invoiceData.items.map((item: InvoiceItem) => ({
      id: item.id,
      productId: item.productId,
      quantity: item.quantity,
      unitPrice: item.unitPrice,
      taxRateId: item.taxRateId,
      description: item.description,
    }))
  }
  catch {
    toast.showError(t('invoices.load_error'))
    router.push('/billing/invoices')
  }
  finally {
    initialLoading.value = false
  }
})

const rules = computed(() => ({
  customerId: { required },
  issueDate: { required },
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

function updateProductDetails(index: number) {
  const item = formData.items[index]
  if (!item)
    return

  const product = products.value.find((p: Product) => p.id === item.productId)
  if (product) {
    item.unitPrice = product.unitPrice
    item.description = product.description || ''
  }
}

// Calculate line item totals
function getLineItemTaxRate(taxRateId: string): number {
  const taxRate = taxRates.value.find((tr: TaxRate) => tr.id === taxRateId)
  return taxRate?.rate || 0
}

function calculateLineItemSubtotal(item: UpdateInvoiceItemDto): number {
  return item.quantity * item.unitPrice
}

function calculateLineItemTax(item: UpdateInvoiceItemDto): number {
  const subtotal = calculateLineItemSubtotal(item)
  const rate = getLineItemTaxRate(item.taxRateId)
  return subtotal * rate
}

function calculateLineItemTotal(item: UpdateInvoiceItemDto): number {
  return calculateLineItemSubtotal(item) + calculateLineItemTax(item)
}

// Calculate invoice totals
const invoiceSubtotal = computed(() => {
  return formData.items.reduce((sum, item) => sum + calculateLineItemSubtotal(item), 0)
})

const invoiceTax = computed(() => {
  return formData.items.reduce((sum, item) => sum + calculateLineItemTax(item), 0)
})

const invoiceTotal = computed(() => {
  return invoiceSubtotal.value + invoiceTax.value
})

function formatCurrency(amount: number): string {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
    minimumFractionDigits: 2,
  }).format(amount)
}

async function handleSubmit() {
  const isValid = await v$.value.$validate()
  if (!isValid || formData.items.length === 0) {
    toast.showError(t('validation.invalid_form'), t('invoices.add_items_error'))
    return
  }

  if (!invoice.value)
    return

  loading.value = true
  try {
    const payload: UpdateInvoiceDto = {
      customerId: formData.customerId!,
      warehouseId: formData.warehouseId || undefined,
      issueDate: formData.issueDate,
      notes: formData.notes || undefined,
      items: formData.items,
    }

    await updateInvoice(invoice.value.id, payload)
    toast.showSuccess(t('invoices.update_success'))
    router.push(`/billing/invoices/${invoice.value.id}`)
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('invoices.update_error'), errMessage)
  }
  finally {
    loading.value = false
  }
}

function handleCancel() {
  if (invoice.value) {
    router.push(`/billing/invoices/${invoice.value.id}`)
  }
  else {
    router.push('/billing/invoices')
  }
}
</script>

<template>
  <div>
    <PageHeader
      :title="t('invoices.edit')"
      :description="t('invoices.edit_description')"
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
          <!-- Invoice Header -->
          <div class="mb-8">
            <h3 class="text-lg font-semibold mb-4">
              {{ t('invoices.header_section') }}
            </h3>

            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div class="field">
                <label for="customerId" class="font-semibold">
                  {{ t('invoices.customer') }} <span class="text-red-500">*</span>
                </label>
                <Select
                  id="customerId"
                  v-model="formData.customerId"
                  :options="customers"
                  option-label="name"
                  option-value="id"
                  :placeholder="t('invoices.select_customer')"
                  :class="{ 'p-invalid': v$.customerId.$error }"
                  class="w-full"
                  filter
                />
                <small v-if="v$.customerId.$error" class="p-error">
                  {{ v$.customerId.$errors[0].$message }}
                </small>
              </div>

              <div class="field">
                <label for="warehouseId" class="font-semibold">
                  {{ t('invoices.warehouse') }}
                </label>
                <Select
                  id="warehouseId"
                  v-model="formData.warehouseId"
                  :options="warehouses"
                  option-label="name"
                  option-value="id"
                  :placeholder="t('invoices.select_warehouse')"
                  class="w-full"
                  show-clear
                />
              </div>

              <div class="field">
                <label for="issueDate" class="font-semibold">
                  {{ t('invoices.issue_date') }} <span class="text-red-500">*</span>
                </label>
                <InputText
                  id="issueDate"
                  v-model="formData.issueDate"
                  type="date"
                  :class="{ 'p-invalid': v$.issueDate.$error }"
                  class="w-full"
                />
                <small v-if="v$.issueDate.$error" class="p-error">
                  {{ v$.issueDate.$errors[0].$message }}
                </small>
              </div>

              <div class="field md:col-span-2">
                <label for="notes" class="font-semibold">
                  {{ t('invoices.notes') }}
                </label>
                <Textarea
                  id="notes"
                  v-model="formData.notes"
                  :placeholder="t('invoices.notes_placeholder')"
                  rows="3"
                  class="w-full"
                />
              </div>
            </div>
          </div>

          <!-- Invoice Items -->
          <div class="mb-8">
            <div class="flex justify-between items-center mb-4">
              <h3 class="text-lg font-semibold">
                {{ t('invoices.items_section') }}
              </h3>
              <Button
                :label="t('invoices.add_item')"
                icon="pi pi-plus"
                size="small"
                @click="addLineItem"
              />
            </div>

            <DataTable
              :value="formData.items"
              class="mb-4"
            >
              <template #empty>
                <div class="text-center py-6 text-surface-500">
                  {{ t('invoices.no_items') }}
                </div>
              </template>

              <Column :header="t('invoices.product')" style="width: 18%">
                <template #body="{ data, index }">
                  <Select
                    v-model="data.productId"
                    :options="products"
                    option-label="name"
                    option-value="id"
                    :placeholder="t('invoices.select_product')"
                    class="w-full"
                    filter
                    @change="updateProductDetails(index)"
                  />
                </template>
              </Column>

              <Column :header="t('invoices.description')" style="width: 16%">
                <template #body="{ data }">
                  <InputText
                    v-model="data.description"
                    :placeholder="t('invoices.description_placeholder')"
                    class="w-full"
                  />
                </template>
              </Column>

              <Column :header="t('invoices.quantity')" style="width: 10%">
                <template #body="{ data }">
                  <InputNumber
                    v-model="data.quantity"
                    :min="1"
                    class="w-full"
                  />
                </template>
              </Column>

              <Column :header="t('invoices.unit_price')" style="width: 12%">
                <template #body="{ data }">
                  <InputNumber
                    v-model="data.unitPrice"
                    mode="currency"
                    currency="USD"
                    :min="0"
                    :min-fraction-digits="2"
                    class="w-full"
                  />
                </template>
              </Column>

              <Column :header="t('invoices.tax_rate')" style="width: 12%">
                <template #body="{ data }">
                  <Select
                    v-model="data.taxRateId"
                    :options="taxRates"
                    option-label="name"
                    option-value="id"
                    class="w-full"
                  />
                </template>
              </Column>

              <Column :header="t('invoices.subtotal')" style="width: 10%">
                <template #body="{ data }">
                  <span class="font-semibold">
                    {{ formatCurrency(calculateLineItemSubtotal(data)) }}
                  </span>
                </template>
              </Column>

              <Column :header="t('invoices.tax')" style="width: 10%">
                <template #body="{ data }">
                  <span>{{ formatCurrency(calculateLineItemTax(data)) }}</span>
                </template>
              </Column>

              <Column :header="t('invoices.total')" style="width: 12%">
                <template #body="{ data }">
                  <span class="font-semibold text-primary-600">
                    {{ formatCurrency(calculateLineItemTotal(data)) }}
                  </span>
                </template>
              </Column>

              <Column :header="t('common.actions')" style="width: 8%">
                <template #body="{ index }">
                  <Button
                    icon="pi pi-trash"
                    severity="danger"
                    text
                    rounded
                    @click="removeLineItem(index)"
                  />
                </template>
              </Column>
            </DataTable>

            <!-- Invoice Totals Summary -->
            <div class="flex justify-end mt-6">
              <Card class="w-full md:w-96">
                <template #content>
                  <div class="space-y-3">
                    <div class="flex justify-between items-center">
                      <span class="text-surface-600 dark:text-surface-400">
                        {{ t('invoices.subtotal_amount') }}
                      </span>
                      <span class="font-semibold text-lg">
                        {{ formatCurrency(invoiceSubtotal) }}
                      </span>
                    </div>

                    <div class="flex justify-between items-center">
                      <span class="text-surface-600 dark:text-surface-400">
                        {{ t('invoices.tax_amount') }}
                      </span>
                      <span class="font-semibold text-lg">
                        {{ formatCurrency(invoiceTax) }}
                      </span>
                    </div>

                    <Divider />

                    <div class="flex justify-between items-center">
                      <span class="text-xl font-semibold text-surface-900 dark:text-surface-50">
                        {{ t('invoices.total_amount') }}
                      </span>
                      <span class="text-2xl font-bold text-primary-600">
                        {{ formatCurrency(invoiceTotal) }}
                      </span>
                    </div>
                  </div>
                </template>
              </Card>
            </div>
          </div>

          <!-- Form Actions -->
          <div class="flex justify-end gap-3 pt-6 border-t">
            <Button
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
