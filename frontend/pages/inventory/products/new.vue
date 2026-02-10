<script setup lang="ts">
import { useVuelidate } from '@vuelidate/core'
import { helpers, maxLength, minValue, required } from '@vuelidate/validators'

definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const uiStore = useUiStore()
const toast = useNotification()
const router = useRouter()
const { createProduct } = useProduct()
const { getAllWarehouses } = useWarehouse()

const loading = ref(false)
const warehouses = ref<Array<{ id: string, name: string }>>([])

const formData = reactive({
  name: '',
  code: '',
  sku: '',
  description: '',
  category: '',
  brand: '',
  unitPrice: null as number | null,
  costPrice: null as number | null,
  minimumStockLevel: null as number | null,
  initialQuantity: null as number | null,
  initialWarehouseId: null as string | null,
  weight: null as number | null,
  dimensions: '',
  isActive: true,
})

// Validation rules
const codeFormat = helpers.regex(/^[A-Z0-9-]+$/)
const skuFormat = helpers.regex(/^[A-Z0-9-]+$/)

const rules = computed(() => ({
  name: {
    required,
    maxLength: maxLength(200),
  },
  code: {
    required,
    maxLength: maxLength(50),
    codeFormat: helpers.withMessage(t('validation.product_code_format'), codeFormat),
  },
  sku: {
    required,
    maxLength: maxLength(50),
    skuFormat: helpers.withMessage(t('validation.sku_format'), skuFormat),
  },
  description: {
    maxLength: maxLength(1000),
  },
  category: {
    maxLength: maxLength(100),
  },
  brand: {
    maxLength: maxLength(100),
  },
  unitPrice: {
    required,
    minValue: minValue(0),
  },
  costPrice: {
    required,
    minValue: minValue(0),
  },
  minimumStockLevel: {
    required,
    minValue: minValue(0),
  },
  initialQuantity: {
    minValue: minValue(1),
  },
  initialWarehouseId: {
    // Required if initialQuantity is provided
    required: helpers.withMessage(
      t('validation.initial_warehouse_required'),
      (value: any) => !formData.initialQuantity || !!value
    ),
  },
  weight: {
    minValue: minValue(0),
  },
  dimensions: {
    maxLength: maxLength(100),
  },
}))

const v$ = useVuelidate(rules, formData)

// Load warehouses on mount
onMounted(async () => {
  try {
    const warehouseList = await getAllWarehouses()
    warehouses.value = warehouseList.map(w => ({ id: w.id, name: w.name }))
  }
  catch (error) {
    console.error('Error loading warehouses:', error)
  }
})

async function handleSubmit() {
  const isValid = await v$.value.$validate()
  if (!isValid) {
    toast.showWarning(t('validation.invalid_form'), t('validation.fix_errors'))
    return
  }

  loading.value = true
  try {
    await createProduct({
      name: formData.name,
      code: formData.code,
      sku: formData.sku,
      description: formData.description || undefined,
      category: formData.category || undefined,
      brand: formData.brand || undefined,
      unitPrice: formData.unitPrice!,
      costPrice: formData.costPrice!,
      minimumStockLevel: formData.minimumStockLevel!,
      initialQuantity: formData.initialQuantity || undefined,
      initialWarehouseId: formData.initialWarehouseId || undefined,
      weight: formData.weight || undefined,
      dimensions: formData.dimensions || undefined,
      isActive: formData.isActive,
    })

    toast.showSuccess(t('messages.success_create'), t('products.created_successfully'))
    router.push('/inventory/products')
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
  router.push('/inventory/products')
}

onMounted(() => {
  uiStore.setBreadcrumbs([
    { label: t('nav.inventory'), to: '/inventory' },
    { label: t('products.title'), to: '/inventory/products' },
    { label: t('products.create') },
  ])
})
</script>

<template>
  <div>
    <!-- Page Header -->
    <PageHeader
      :title="t('products.create')"
      :description="t('products.create_description')"
    />

    <!-- Form Card -->
    <Card>
      <template #content>
        <form class="flex flex-col gap-6" @submit.prevent="handleSubmit">
          <!-- Basic Information Section -->
          <div class="mb-4">
            <h3 class="mb-4 text-lg font-semibold text-slate-900 dark:text-white">
              {{ t('common.basic_info') }}
            </h3>

            <div class="grid grid-cols-1 gap-6 md:grid-cols-2">
              <!-- Name -->
              <div class="flex flex-col gap-2">
                <label for="name" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('products.name') }} *
                </label>
                <InputText
                  id="name"
                  v-model="formData.name"
                  :invalid="v$.name.$error"
                  :placeholder="t('products.name_placeholder')"
                  @blur="v$.name.$touch()"
                />
                <small v-if="v$.name.$error" class="text-red-600 dark:text-red-400">
                  {{ v$.name.$errors[0].$message }}
                </small>
              </div>

              <!-- Code -->
              <div class="flex flex-col gap-2">
                <label for="code" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('products.code') }} *
                </label>
                <InputText
                  id="code"
                  v-model="formData.code"
                  :invalid="v$.code.$error"
                  :placeholder="t('products.code_placeholder')"
                  @blur="v$.code.$touch()"
                />
                <small v-if="v$.code.$error" class="text-red-600 dark:text-red-400">
                  {{ v$.code.$errors[0].$message }}
                </small>
                <small v-else class="text-slate-500 dark:text-slate-400">
                  {{ t('products.code_helper') }}
                </small>
              </div>

              <!-- SKU -->
              <div class="flex flex-col gap-2">
                <label for="sku" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('products.sku') }} *
                </label>
                <InputText
                  id="sku"
                  v-model="formData.sku"
                  :invalid="v$.sku.$error"
                  :placeholder="t('products.sku_placeholder')"
                  @blur="v$.sku.$touch()"
                />
                <small v-if="v$.sku.$error" class="text-red-600 dark:text-red-400">
                  {{ v$.sku.$errors[0].$message }}
                </small>
                <small v-else class="text-slate-500 dark:text-slate-400">
                  {{ t('products.sku_helper') }}
                </small>
              </div>
            </div>

            <!-- Description -->
            <div class="mt-6 flex flex-col gap-2">
              <label for="description" class="font-semibold text-slate-700 dark:text-slate-200">
                {{ t('products.description') }}
              </label>
              <Textarea
                id="description"
                v-model="formData.description"
                :invalid="v$.description.$error"
                :placeholder="t('products.description_placeholder')"
                rows="3"
                @blur="v$.description.$touch()"
              />
            </div>
          </div>

          <!-- Classification Section -->
          <div class="mb-4">
            <h3 class="mb-4 text-lg font-semibold text-slate-900 dark:text-white">
              {{ t('products.classification') }}
            </h3>

            <div class="grid grid-cols-1 gap-6 md:grid-cols-2">
              <!-- Category -->
              <div class="flex flex-col gap-2">
                <label for="category" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('products.category') }}
                </label>
                <InputText
                  id="category"
                  v-model="formData.category"
                  :placeholder="t('products.category_placeholder')"
                />
              </div>

              <!-- Brand -->
              <div class="flex flex-col gap-2">
                <label for="brand" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('products.brand') }}
                </label>
                <InputText
                  id="brand"
                  v-model="formData.brand"
                  :placeholder="t('products.brand_placeholder')"
                />
              </div>
            </div>
          </div>

          <!-- Pricing Section -->
          <div class="mb-4">
            <h3 class="mb-4 text-lg font-semibold text-slate-900 dark:text-white">
              {{ t('products.pricing') }}
            </h3>

            <div class="grid grid-cols-1 gap-6 md:grid-cols-2">
              <!-- Unit Price -->
              <div class="flex flex-col gap-2">
                <label for="unitPrice" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('products.unit_price') }} *
                </label>
                <InputNumber
                  id="unitPrice"
                  v-model="formData.unitPrice"
                  :invalid="v$.unitPrice.$error"
                  :placeholder="t('products.unit_price_placeholder')"
                  mode="currency"
                  currency="USD"
                  :min="0"
                  :min-fraction-digits="2"
                  :max-fraction-digits="2"
                  @blur="v$.unitPrice.$touch()"
                />
                <small v-if="v$.unitPrice.$error" class="text-red-600 dark:text-red-400">
                  {{ v$.unitPrice.$errors[0].$message }}
                </small>
              </div>

              <!-- Cost Price -->
              <div class="flex flex-col gap-2">
                <label for="costPrice" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('products.cost_price') }} *
                </label>
                <InputNumber
                  id="costPrice"
                  v-model="formData.costPrice"
                  :invalid="v$.costPrice.$error"
                  :placeholder="t('products.cost_price_placeholder')"
                  mode="currency"
                  currency="USD"
                  :min="0"
                  :min-fraction-digits="2"
                  :max-fraction-digits="2"
                  @blur="v$.costPrice.$touch()"
                />
                <small v-if="v$.costPrice.$error" class="text-red-600 dark:text-red-400">
                  {{ v$.costPrice.$errors[0].$message }}
                </small>
              </div>
            </div>
          </div>

          <!-- Inventory Section -->
          <div class="mb-4">
            <h3 class="mb-4 text-lg font-semibold text-slate-900 dark:text-white">
              {{ t('nav.inventory') }}
            </h3>

            <div class="grid grid-cols-1 gap-6 md:grid-cols-2">
              <!-- Minimum Stock Level -->
              <div class="flex flex-col gap-2">
                <label for="minimumStockLevel" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('products.minimum_stock_level') }} *
                </label>
                <InputNumber
                  id="minimumStockLevel"
                  v-model="formData.minimumStockLevel"
                  :invalid="v$.minimumStockLevel.$error"
                  :placeholder="t('products.minimum_stock_level_placeholder')"
                  :min="0"
                  @blur="v$.minimumStockLevel.$touch()"
                />
                <small v-if="v$.minimumStockLevel.$error" class="text-red-600 dark:text-red-400">
                  {{ v$.minimumStockLevel.$errors[0].$message }}
                </small>
              </div>

              <!-- Initial Quantity (Optional) -->
              <div class="flex flex-col gap-2">
                <label for="initialQuantity" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('products.initial_quantity') }}
                </label>
                <InputNumber
                  id="initialQuantity"
                  v-model="formData.initialQuantity"
                  :invalid="v$.initialQuantity.$error"
                  :placeholder="t('products.initial_quantity_placeholder')"
                  :min="1"
                  @blur="v$.initialQuantity.$touch()"
                />
                <small v-if="v$.initialQuantity.$error" class="text-red-600 dark:text-red-400">
                  {{ v$.initialQuantity.$errors[0].$message }}
                </small>
                <small class="text-slate-500 dark:text-slate-400">
                  {{ t('products.initial_quantity_hint') }}
                </small>
              </div>

              <!-- Initial Warehouse (Required if initial quantity is set) -->
              <div class="flex flex-col gap-2">
                <label for="initialWarehouseId" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('products.initial_warehouse') }}
                  <span v-if="formData.initialQuantity" class="text-red-600">*</span>
                </label>
                <Dropdown
                  id="initialWarehouseId"
                  v-model="formData.initialWarehouseId"
                  :options="warehouses"
                  option-label="name"
                  option-value="id"
                  :invalid="v$.initialWarehouseId.$error"
                  :placeholder="t('products.initial_warehouse_placeholder')"
                  @blur="v$.initialWarehouseId.$touch()"
                />
                <small v-if="v$.initialWarehouseId.$error" class="text-red-600 dark:text-red-400">
                  {{ v$.initialWarehouseId.$errors[0].$message }}
                </small>
                <small v-if="formData.initialQuantity" class="text-slate-500 dark:text-slate-400">
                  {{ t('products.initial_warehouse_hint') }}
                </small>
              </div>
            </div>
          </div>

          <!-- Physical Properties Section -->
          <div class="mb-4">
            <h3 class="mb-4 text-lg font-semibold text-slate-900 dark:text-white">
              {{ t('products.physical_properties') }}
            </h3>

            <div class="grid grid-cols-1 gap-6 md:grid-cols-2">
              <!-- Weight -->
              <div class="flex flex-col gap-2">
                <label for="weight" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('products.weight') }}
                </label>
                <InputNumber
                  id="weight"
                  v-model="formData.weight"
                  :placeholder="t('products.weight_placeholder')"
                  suffix=" kg"
                  :min="0"
                  :min-fraction-digits="0"
                  :max-fraction-digits="2"
                />
              </div>

              <!-- Dimensions -->
              <div class="flex flex-col gap-2">
                <label for="dimensions" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('products.dimensions') }}
                </label>
                <InputText
                  id="dimensions"
                  v-model="formData.dimensions"
                  :placeholder="t('products.dimensions_placeholder')"
                />
              </div>
            </div>
          </div>

          <!-- Status Section -->
          <div class="mb-4">
            <h3 class="mb-4 text-lg font-semibold text-slate-900 dark:text-white">
              {{ t('common.status') }}
            </h3>

            <!-- Is Active -->
            <div class="flex items-center gap-2">
              <Checkbox
                id="isActive"
                v-model="formData.isActive"
                :binary="true"
              />
              <label for="isActive" class="font-semibold text-slate-700 dark:text-slate-200">
                {{ t('products.is_active') }}
              </label>
            </div>
          </div>

          <!-- Action Buttons -->
          <div class="flex justify-end gap-3 pt-4">
            <Button
              :label="t('common.cancel')"
              severity="secondary"
              outlined
              @click="cancel"
            />
            <Button
              type="submit"
              :label="t('common.create')"
              :loading="loading"
              icon="pi pi-check"
            />
          </div>
        </form>
      </template>
    </Card>
  </div>
</template>
