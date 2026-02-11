<script setup lang="ts">
import { useVuelidate } from '@vuelidate/core'
import { required } from '@vuelidate/validators'
import type { CreateInvoiceDto, CreateInvoiceItemDto, Customer, Product, TaxRate } from '~/types'
import type { Warehouse } from '~/types/inventory'

definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const toast = useNotification()
const router = useRouter()
const { createInvoice } = useInvoice()
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

const formData = reactive({
  customerId: null as string | null,
  warehouseId: null as string | null,
  issueDate: new Date().toISOString().split('T')[0],
  notes: '',
  items: [] as CreateInvoiceItemDto[],
})

onMounted(async () => {
  try {
    const [customerList, productList, taxRateList, warehouseList] = await Promise.all([
      getAllCustomers(),
      getAllProducts(),
      getAllTaxRates(),
      getAllWarehouses(),
    ])

    customers.value = customerList
    products.value = productList
    taxRates.value = taxRateList.filter(tr => tr.isActive)
    warehouses.value = warehouseList.filter(w => w.isActive)
  }
  catch (error) {
    toast.showError(t('invoices.load_error'))
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

function updateProductDetails(index: number) {
  const item = formData.items[index]
  if (!item)
    return

  const product = products.value.find(p => p.id === item.productId)
  if (product) {
    item.unitPrice = product.unitPrice
    item.description = product.description || ''
  }
}

async function handleSubmit() {
  const isValid = await v$.value.$validate()
  if (!isValid || formData.items.length === 0) {
    toast.showError(t('common.validation_error'), t('invoices.add_items_error'))
    return
  }

  loading.value = true
  try {
    const payload: CreateInvoiceDto = {
      customerId: formData.customerId!,
      warehouseId: formData.warehouseId || undefined,
      issueDate: formData.issueDate,
      notes: formData.notes || undefined,
      items: formData.items,
    }

    await createInvoice(payload)
    toast.showSuccess(t('invoices.create_success'))
    router.push('/billing/invoices')
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('invoices.create_error'), errMessage)
  }
  finally {
    loading.value = false
  }
}

function handleCancel() {
  router.push('/billing/invoices')
}
</script>

<template>
  <div>
    <PageHeader
      :title="t('invoices.create')"
      :description="t('invoices.create_description')"
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

              <Column :header="t('invoices.product')" style="width: 25%">
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

              <Column :header="t('invoices.description')" style="width: 20%">
                <template #body="{ data }">
                  <InputText
                    v-model="data.description"
                    :placeholder="t('invoices.description_placeholder')"
                    class="w-full"
                  />
                </template>
              </Column>

              <Column :header="t('invoices.quantity')" style="width: 15%">
                <template #body="{ data }">
                  <InputNumber
                    v-model="data.quantity"
                    :min="1"
                    class="w-full"
                  />
                </template>
              </Column>

              <Column :header="t('invoices.unit_price')" style="width: 15%">
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

              <Column :header="t('invoices.tax_rate')" style="width: 15%">
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

              <Column :header="t('common.actions')" style="width: 10%">
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
