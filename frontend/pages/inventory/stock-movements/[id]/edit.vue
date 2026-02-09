<script setup lang="ts">
import { useVuelidate } from '@vuelidate/core'
import { maxLength, minValue, required } from '@vuelidate/validators'
import { MovementType, MovementTypeLabels } from '~/types/inventory'
import type { StockMovement } from '~/types/inventory'

definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const uiStore = useUiStore()
const toast = useNotification()
const router = useRouter()
const route = useRoute()
const { getStockMovementById, updateStockMovement } = useStockMovement()
const { getAllProducts } = useProduct()
const { getAllWarehouses } = useWarehouse()

const loading = ref(false)
const loadingData = ref(false)
const products = ref<any[]>([])
const warehouses = ref<any[]>([])
const stockMovement = ref<StockMovement | null>(null)

const formData = reactive({
  productId: '',
  warehouseId: '',
  destinationWarehouseId: '',
  movementType: MovementType.Purchase,
  quantity: null as number | null,
  unitCost: null as number | null,
  totalCost: null as number | null,
  reference: '',
  notes: '',
  movementDate: '',
})

// Movement type options
const movementTypeOptions = Object.values(MovementType)
  .filter(v => typeof v === 'number')
  .map(value => ({
    label: MovementTypeLabels[value as MovementType],
    value: value as MovementType,
  }))

// Validation rules
const rules = computed(() => ({
  productId: { required },
  warehouseId: { required },
  destinationWarehouseId: {},
  movementType: { required },
  quantity: {
    required,
  },
  unitCost: {
    minValue: minValue(0),
  },
  totalCost: {
    minValue: minValue(0),
  },
  reference: {
    maxLength: maxLength(100),
  },
  notes: {
    maxLength: maxLength(500),
  },
  movementDate: { required },
}))

const v$ = useVuelidate(rules, formData)

// Auto-calculate total cost
watch([() => formData.quantity, () => formData.unitCost], () => {
  if (formData.quantity && formData.unitCost) {
    formData.totalCost = formData.quantity * formData.unitCost
  }
})

// Show/hide destination warehouse based on movement type
const showDestinationWarehouse = computed(() => {
  return formData.movementType === MovementType.Transfer
})

async function loadData() {
  loadingData.value = true
  try {
    const id = route.params.id as string
    const [movementData, productsData, warehousesData] = await Promise.all([
      getStockMovementById(id),
      getAllProducts(),
      getAllWarehouses(),
    ])

    stockMovement.value = movementData
    products.value = productsData
    warehouses.value = warehousesData

    // Populate form with existing data
    formData.productId = movementData.productId
    formData.warehouseId = movementData.warehouseId
    formData.destinationWarehouseId = movementData.destinationWarehouseId || ''
    formData.movementType = movementData.movementType
    formData.quantity = movementData.quantity
    formData.unitCost = movementData.unitCost || null
    formData.totalCost = movementData.totalCost || null
    formData.reference = movementData.reference || ''
    formData.notes = movementData.notes || ''
    formData.movementDate = movementData.movementDate.split('T')[0]

    // Set breadcrumbs
    uiStore.setBreadcrumbs([
      { label: t('nav.inventory'), to: '/inventory' },
      { label: t('stock_movements.title'), to: '/inventory/stock-movements' },
      { label: t('stock_movements.edit') },
    ])
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('messages.error_load'), errMessage)
    router.push('/inventory/stock-movements')
  }
  finally {
    loadingData.value = false
  }
}

async function handleSubmit() {
  const isValid = await v$.value.$validate()
  if (!isValid) {
    toast.showWarning(t('validation.invalid_form'), t('validation.fix_errors'))
    return
  }

  loading.value = true
  try {
    const id = route.params.id as string
    await updateStockMovement({
      id,
      productId: formData.productId,
      warehouseId: formData.warehouseId,
      destinationWarehouseId: formData.destinationWarehouseId || undefined,
      movementType: formData.movementType,
      quantity: formData.quantity!,
      unitCost: formData.unitCost || undefined,
      totalCost: formData.totalCost || undefined,
      reference: formData.reference || undefined,
      notes: formData.notes || undefined,
      movementDate: formData.movementDate,
    })

    toast.showSuccess(t('messages.success_update'), t('stock_movements.updated_successfully'))
    router.push(`/inventory/stock-movements/${id}`)
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('messages.error_update'), errMessage)
  }
  finally {
    loading.value = false
  }
}

function handleCancel() {
  const id = route.params.id as string
  router.push(`/inventory/stock-movements/${id}`)
}

onMounted(() => {
  loadData()
})
</script>

<template>
  <div class="space-y-6">
    <!-- Page Header -->
    <PageHeader
      :title="t('stock_movements.edit')"
      :description="t('stock_movements.edit_description')"
    />

    <LoadingState v-if="loadingData" :message="t('common.loading')" />

    <!-- Form -->
    <form v-else @submit.prevent="handleSubmit">
      <div class="grid gap-6">
        <!-- Basic Information -->
        <Card>
          <template #title>
            <h2 class="text-xl font-semibold">
              {{ t('stock_movements.basic_info') }}
            </h2>
          </template>
          <template #content>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <!-- Movement Type -->
              <div class="flex flex-col gap-2">
                <label for="movementType" class="font-medium">
                  {{ t('stock_movements.movement_type') }}
                  <span class="text-red-500">*</span>
                </label>
                <Dropdown
                  id="movementType"
                  v-model="formData.movementType"
                  :options="movementTypeOptions"
                  option-label="label"
                  option-value="value"
                  :placeholder="t('stock_movements.select_movement_type')"
                  class="w-full"
                  :class="{ 'p-invalid': v$.movementType.$error }"
                />
                <small v-if="v$.movementType.$error" class="text-red-500">
                  {{ v$.movementType.$errors[0].$message }}
                </small>
              </div>

              <!-- Product -->
              <div class="flex flex-col gap-2">
                <label for="productId" class="font-medium">
                  {{ t('stock_movements.product') }}
                  <span class="text-red-500">*</span>
                </label>
                <Dropdown
                  id="productId"
                  v-model="formData.productId"
                  :options="products"
                  option-label="name"
                  option-value="id"
                  filter
                  :placeholder="t('stock_movements.select_product')"
                  class="w-full"
                  :class="{ 'p-invalid': v$.productId.$error }"
                />
                <small v-if="v$.productId.$error" class="text-red-500">
                  {{ v$.productId.$errors[0].$message }}
                </small>
              </div>

              <!-- Warehouse -->
              <div class="flex flex-col gap-2">
                <label for="warehouseId" class="font-medium">
                  {{ t('stock_movements.warehouse') }}
                  <span class="text-red-500">*</span>
                </label>
                <Dropdown
                  id="warehouseId"
                  v-model="formData.warehouseId"
                  :options="warehouses"
                  option-label="name"
                  option-value="id"
                  filter
                  :placeholder="t('stock_movements.select_warehouse')"
                  class="w-full"
                  :class="{ 'p-invalid': v$.warehouseId.$error }"
                />
                <small v-if="v$.warehouseId.$error" class="text-red-500">
                  {{ v$.warehouseId.$errors[0].$message }}
                </small>
              </div>

              <!-- Destination Warehouse (for transfers) -->
              <div v-if="showDestinationWarehouse" class="flex flex-col gap-2">
                <label for="destinationWarehouseId" class="font-medium">
                  {{ t('stock_movements.destination_warehouse') }}
                </label>
                <Dropdown
                  id="destinationWarehouseId"
                  v-model="formData.destinationWarehouseId"
                  :options="warehouses.filter(w => w.id !== formData.warehouseId)"
                  option-label="name"
                  option-value="id"
                  filter
                  :placeholder="t('stock_movements.select_destination_warehouse')"
                  class="w-full"
                />
              </div>

              <!-- Movement Date -->
              <div class="flex flex-col gap-2">
                <label for="movementDate" class="font-medium">
                  {{ t('stock_movements.movement_date') }}
                  <span class="text-red-500">*</span>
                </label>
                <Calendar
                  id="movementDate"
                  v-model="formData.movementDate"
                  date-format="yy-mm-dd"
                  :placeholder="t('stock_movements.select_date')"
                  class="w-full"
                  :class="{ 'p-invalid': v$.movementDate.$error }"
                />
                <small v-if="v$.movementDate.$error" class="text-red-500">
                  {{ v$.movementDate.$errors[0].$message }}
                </small>
              </div>

              <!-- Quantity -->
              <div class="flex flex-col gap-2">
                <label for="quantity" class="font-medium">
                  {{ t('stock_movements.quantity') }}
                  <span class="text-red-500">*</span>
                </label>
                <InputNumber
                  id="quantity"
                  v-model="formData.quantity"
                  mode="decimal"
                  :use-grouping="false"
                  :placeholder="t('stock_movements.enter_quantity')"
                  class="w-full"
                  :class="{ 'p-invalid': v$.quantity.$error }"
                />
                <small v-if="v$.quantity.$error" class="text-red-500">
                  {{ v$.quantity.$errors[0].$message }}
                </small>
              </div>
            </div>
          </template>
        </Card>

        <!-- Cost Information -->
        <Card>
          <template #title>
            <h2 class="text-xl font-semibold">
              {{ t('stock_movements.cost_info') }}
            </h2>
          </template>
          <template #content>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <!-- Unit Cost -->
              <div class="flex flex-col gap-2">
                <label for="unitCost" class="font-medium">
                  {{ t('stock_movements.unit_cost') }}
                </label>
                <InputNumber
                  id="unitCost"
                  v-model="formData.unitCost"
                  mode="currency"
                  currency="USD"
                  locale="en-US"
                  :placeholder="t('stock_movements.enter_unit_cost')"
                  class="w-full"
                  :class="{ 'p-invalid': v$.unitCost.$error }"
                />
                <small v-if="v$.unitCost.$error" class="text-red-500">
                  {{ v$.unitCost.$errors[0].$message }}
                </small>
              </div>

              <!-- Total Cost -->
              <div class="flex flex-col gap-2">
                <label for="totalCost" class="font-medium">
                  {{ t('stock_movements.total_cost') }}
                </label>
                <InputNumber
                  id="totalCost"
                  v-model="formData.totalCost"
                  mode="currency"
                  currency="USD"
                  locale="en-US"
                  :placeholder="t('stock_movements.auto_calculated')"
                  class="w-full"
                  :class="{ 'p-invalid': v$.totalCost.$error }"
                />
                <small v-if="v$.totalCost.$error" class="text-red-500">
                  {{ v$.totalCost.$errors[0].$message }}
                </small>
                <small class="text-gray-500">
                  {{ t('stock_movements.total_cost_help') }}
                </small>
              </div>
            </div>
          </template>
        </Card>

        <!-- Additional Information -->
        <Card>
          <template #title>
            <h2 class="text-xl font-semibold">
              {{ t('stock_movements.additional_info') }}
            </h2>
          </template>
          <template #content>
            <div class="grid grid-cols-1 gap-4">
              <!-- Reference -->
              <div class="flex flex-col gap-2">
                <label for="reference" class="font-medium">
                  {{ t('stock_movements.reference') }}
                </label>
                <InputText
                  id="reference"
                  v-model="formData.reference"
                  :placeholder="t('stock_movements.reference_placeholder')"
                  :class="{ 'p-invalid': v$.reference.$error }"
                />
                <small v-if="v$.reference.$error" class="text-red-500">
                  {{ v$.reference.$errors[0].$message }}
                </small>
                <small class="text-gray-500">
                  {{ t('stock_movements.reference_help') }}
                </small>
              </div>

              <!-- Notes -->
              <div class="flex flex-col gap-2">
                <label for="notes" class="font-medium">
                  {{ t('stock_movements.notes') }}
                </label>
                <Textarea
                  id="notes"
                  v-model="formData.notes"
                  rows="3"
                  :placeholder="t('stock_movements.notes_placeholder')"
                  :class="{ 'p-invalid': v$.notes.$error }"
                />
                <small v-if="v$.notes.$error" class="text-red-500">
                  {{ v$.notes.$errors[0].$message }}
                </small>
              </div>
            </div>
          </template>
        </Card>

        <!-- Form Actions -->
        <div class="flex gap-3 justify-end">
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
      </div>
    </form>
  </div>
</template>
