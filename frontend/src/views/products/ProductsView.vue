<template>
  <div>
    <div class="flex items-center justify-between mb-6">
      <h1 class="text-3xl font-bold text-gray-800">Gestión de Productos</h1>
      <Button
        label="Nuevo Producto"
        icon="pi pi-plus"
        @click="openCreateDialog"
      />
    </div>

    <!-- Filters -->
    <Card class="mb-6">
      <template #content>
        <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
          <div class="flex flex-col gap-2">
            <label for="search" class="font-medium text-gray-700">Buscar</label>
            <InputText
              id="search"
              v-model="filters.search"
              placeholder="Código, nombre o código de barras"
              @input="debouncedSearch"
            />
          </div>
          <div class="flex flex-col gap-2">
            <label for="category" class="font-medium text-gray-700">Categoría</label>
            <Dropdown
              id="category"
              v-model="filters.category"
              :options="categories"
              placeholder="Todas"
              showClear
              @change="loadProducts"
            />
          </div>
          <div class="flex flex-col gap-2">
            <label for="status" class="font-medium text-gray-700">Estado</label>
            <Dropdown
              id="status"
              v-model="filters.status"
              :options="statusOptions"
              optionLabel="label"
              optionValue="value"
              placeholder="Todos"
              showClear
              @change="loadProducts"
            />
          </div>
          <div class="flex items-end">
            <Button
              label="Limpiar Filtros"
              icon="pi pi-filter-slash"
              severity="secondary"
              outlined
              @click="clearFilters"
            />
          </div>
        </div>
      </template>
    </Card>

    <!-- Products Table -->
    <Card>
      <template #content>
        <DataTable
          :value="products"
          :loading="loading"
          :paginator="true"
          :rows="20"
          :totalRecords="totalRecords"
          :lazy="true"
          @page="onPage"
          responsiveLayout="scroll"
          stripedRows
        >
          <Column field="code" header="Código" style="min-width: 120px"></Column>
          <Column field="barcode" header="Código de Barras" style="min-width: 150px"></Column>
          <Column field="name" header="Nombre" style="min-width: 200px">
            <template #body="slotProps">
              <div class="font-medium">{{ slotProps.data.name }}</div>
              <div v-if="slotProps.data.category" class="text-sm text-gray-500">{{ slotProps.data.category }}</div>
            </template>
          </Column>
          <Column field="unit" header="Unidad" style="min-width: 100px"></Column>
          <Column field="price" header="Precio" style="min-width: 120px">
            <template #body="slotProps">
              {{ formatCurrency(slotProps.data.price) }}
            </template>
          </Column>
          <Column field="cost" header="Costo" style="min-width: 120px">
            <template #body="slotProps">
              {{ formatCurrency(slotProps.data.cost) }}
            </template>
          </Column>
          <Column field="hasInventory" header="Control Inventario" style="min-width: 150px">
            <template #body="slotProps">
              <Tag 
                :value="slotProps.data.hasInventory ? 'Sí' : 'No'" 
                :severity="slotProps.data.hasInventory ? 'success' : 'secondary'"
              />
            </template>
          </Column>
          <Column field="status" header="Estado" style="min-width: 100px">
            <template #body="slotProps">
              <Tag 
                :value="slotProps.data.status === 'active' ? 'Activo' : 'Inactivo'" 
                :severity="slotProps.data.status === 'active' ? 'success' : ''" 
              />
            </template>
          </Column>
          <Column header="Acciones" style="min-width: 150px">
            <template #body="slotProps">
              <div class="flex gap-2">
                <Button
                  icon="pi pi-pencil"
                  size="small"
                  outlined
                  @click="openEditDialog(slotProps.data)"
                />
                <Button
                  icon="pi pi-trash"
                  size="small"
                  severity="danger"
                  outlined
                  @click="confirmDelete(slotProps.data)"
                />
              </div>
            </template>
          </Column>
        </DataTable>
      </template>
    </Card>

    <!-- Create/Edit Dialog -->
    <Dialog
      :visible="dialogVisible"
      :header="editingProduct ? 'Editar Producto' : 'Nuevo Producto'"
      :modal="true"
      :style="{ width: '600px', maxWidth: '90vw' }"
      :contentStyle="{ overflowX: 'hidden' }"
      @update:visible="(val) => dialogVisible = val"
      @hide="resetForm"
    >
      <div class="flex flex-col gap-4 py-4">
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div class="flex flex-col gap-2">
            <label for="code" class="font-medium text-gray-700">Código *</label>
            <InputText
              id="code"
              v-model="form.code"
              :class="{ 'p-invalid': errors.code }"
            />
            <small v-if="errors.code" class="p-error">{{ errors.code }}</small>
          </div>
          <div class="flex flex-col gap-2">
            <label for="barcode" class="font-medium text-gray-700">Código de Barras</label>
            <InputText
              id="barcode"
              v-model="form.barcode"
            />
          </div>
        </div>

        <div class="flex flex-col gap-2">
          <label for="name" class="font-medium text-gray-700">Nombre *</label>
          <InputText
            id="name"
            v-model="form.name"
            :class="{ 'p-invalid': errors.name }"
          />
          <small v-if="errors.name" class="p-error">{{ errors.name }}</small>
        </div>

        <div class="flex flex-col gap-2">
          <label for="description" class="font-medium text-gray-700">Descripción</label>
          <Textarea
            id="description"
            v-model="form.description"
            rows="3"
          />
        </div>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div class="flex flex-col gap-2">
            <label for="category" class="font-medium text-gray-700">Categoría</label>
            <InputText
              id="category"
              v-model="form.category"
            />
          </div>
          <div class="flex flex-col gap-2">
            <label for="unit" class="font-medium text-gray-700">Unidad *</label>
            <Dropdown
              id="unit"
              v-model="form.unit"
              :options="unitOptions"
              optionLabel="label"
              optionValue="value"
              :class="{ 'p-invalid': errors.unit }"
            />
            <small v-if="errors.unit" class="p-error">{{ errors.unit }}</small>
          </div>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div class="flex flex-col gap-2">
            <label for="cost" class="font-medium text-gray-700">Costo *</label>
            <InputNumber
              id="cost"
              v-model="form.cost"
              mode="currency"
              currency="USD"
              locale="es-EC"
              :minFractionDigits="2"
              :class="{ 'p-invalid': errors.cost }"
            />
            <small v-if="errors.cost" class="p-error">{{ errors.cost }}</small>
          </div>
          <div class="flex flex-col gap-2">
            <label for="price" class="font-medium text-gray-700">Precio *</label>
            <InputNumber
              id="price"
              v-model="form.price"
              mode="currency"
              currency="USD"
              locale="es-EC"
              :minFractionDigits="2"
              :class="{ 'p-invalid': errors.price }"
            />
            <small v-if="errors.price" class="p-error">{{ errors.price }}</small>
          </div>
        </div>

        <div class="flex flex-col gap-2">
          <label for="taxType" class="font-medium text-gray-700">Tipo de IVA *</label>
          <Dropdown
            id="taxType"
            v-model="form.taxTypeId"
            :options="taxTypes"
            optionLabel="name"
            optionValue="id"
            :class="{ 'p-invalid': errors.taxTypeId }"
          />
          <small v-if="errors.taxTypeId" class="p-error">{{ errors.taxTypeId }}</small>
        </div>

        <div class="flex items-center gap-2">
          <Checkbox
            id="hasInventory"
            v-model="form.hasInventory"
            :binary="true"
          />
          <label for="hasInventory" class="cursor-pointer">Controlar inventario</label>
        </div>

        <div v-if="form.hasInventory" class="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div class="flex flex-col gap-2">
            <label for="minStock" class="font-medium text-gray-700">Stock Mínimo</label>
            <InputNumber
              id="minStock"
              v-model="form.minStock"
              :min="0"
            />
          </div>
          <div class="flex flex-col gap-2">
            <label for="maxStock" class="font-medium text-gray-700">Stock Máximo</label>
            <InputNumber
              id="maxStock"
              v-model="form.maxStock"
              :min="0"
            />
          </div>
          <div class="flex flex-col gap-2">
            <label for="reorderPoint" class="font-medium text-gray-700">Punto de Reorden</label>
            <InputNumber
              id="reorderPoint"
              v-model="form.reorderPoint"
              :min="0"
            />
          </div>
        </div>
      </div>

      <template #footer>
        <div class="flex justify-end gap-2">
          <Button
            label="Cancelar"
            severity="secondary"
            outlined
            @click="dialogVisible = false"
          />
          <Button
            label="Guardar"
            :loading="saving"
            @click="saveProduct"
          />
        </div>
      </template>
    </Dialog>

    <!-- Delete Confirmation Dialog -->
    <ConfirmDialog />
    <Toast />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useToast } from 'primevue/usetoast'
import { useConfirm } from 'primevue/useconfirm'
import { productsAPI } from '../../api/products'
import { useCurrency } from '../../composables/useCurrency'
import type { IProduct, ICreateProductDTO, IProductFilters } from '../../types'
import Card from 'primevue/card'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import InputNumber from 'primevue/inputnumber'
import Dropdown from 'primevue/dropdown'
import Textarea from 'primevue/textarea'
import Checkbox from 'primevue/checkbox'
import Dialog from 'primevue/dialog'
import Tag from 'primevue/tag'
import Toast from 'primevue/toast'
import ConfirmDialog from 'primevue/confirmdialog'

const toast = useToast()
const confirm = useConfirm()
const { formatCurrency } = useCurrency()

// State
const products = ref<IProduct[]>([])
const loading = ref(false)
const saving = ref(false)
const dialogVisible = ref(false)
const editingProduct = ref<IProduct | null>(null)
const totalRecords = ref(0)
const currentPage = ref(1)
const categories = ref<string[]>([])

// Filters
const filters = ref<IProductFilters>({
  search: '',
  category: undefined,
  status: undefined,
  page: 1,
  limit: 20
})

const statusOptions = [
  { label: 'Activo', value: 'active' },
  { label: 'Inactivo', value: 'inactive' }
]

const unitOptions = [
  { label: 'Unidad (UND)', value: 'UND' },
  { label: 'Kilogramo (KG)', value: 'KG' },
  { label: 'Litro (LT)', value: 'LT' },
  { label: 'Metro (MT)', value: 'MT' },
  { label: 'Metro Cuadrado (M2)', value: 'M2' },
  { label: 'Metro Cúbico (M3)', value: 'M3' },
  { label: 'Paquete (PKG)', value: 'PKG' },
  { label: 'Caja (BOX)', value: 'BOX' },
  { label: 'Docena (DOZ)', value: 'DOZ' }
]

// Mock tax types - in real app, fetch from API
const taxTypes = ref([
  { id: '1', name: 'IVA 12%', code: 'IVA_12' },
  { id: '2', name: 'IVA 0%', code: 'IVA_0' },
  { id: '3', name: 'No IVA', code: 'NO_IVA' }
])

// Form
const form = ref<ICreateProductDTO>({
  code: '',
  barcode: '',
  name: '',
  description: '',
  category: '',
  unit: 'UND',
  price: 0,
  cost: 0,
  taxTypeId: '1',
  hasInventory: true,
  minStock: 0,
  maxStock: 0,
  reorderPoint: 0
})

const errors = ref<Record<string, string>>({})

// Methods
const loadProducts = async () => {
  loading.value = true
  try {
    const result = await productsAPI.getProducts(filters.value)
    products.value = result.products
    totalRecords.value = result.total
  } catch (error: any) {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: error.response?.data?.error || 'Error al cargar productos',
      life: 3000
    })
  } finally {
    loading.value = false
  }
}

const loadCategories = async () => {
  try {
    categories.value = await productsAPI.getCategories()
  } catch (error) {
    console.error('Error loading categories:', error)
  }
}

let searchTimeout: number
const debouncedSearch = () => {
  clearTimeout(searchTimeout)
  searchTimeout = window.setTimeout(() => {
    filters.value.page = 1
    loadProducts()
  }, 300)
}

const clearFilters = () => {
  filters.value = {
    search: '',
    category: undefined,
    status: undefined,
    page: 1,
    limit: 20
  }
  loadProducts()
}

const onPage = (event: any) => {
  filters.value.page = event.page + 1
  loadProducts()
}

const openCreateDialog = () => {
  editingProduct.value = null
  resetForm()
  dialogVisible.value = true
}

const openEditDialog = (product: IProduct) => {
  editingProduct.value = product
  form.value = {
    code: product.code,
    barcode: product.barcode,
    name: product.name,
    description: product.description,
    category: product.category,
    unit: product.unit,
    price: product.price,
    cost: product.cost,
    taxTypeId: product.taxTypeId,
    hasInventory: product.hasInventory,
    minStock: product.minStock,
    maxStock: product.maxStock,
    reorderPoint: product.reorderPoint
  }
  dialogVisible.value = true
}

const resetForm = () => {
  form.value = {
    code: '',
    barcode: '',
    name: '',
    description: '',
    category: '',
    unit: 'UND',
    price: 0,
    cost: 0,
    taxTypeId: '1',
    hasInventory: true,
    minStock: 0,
    maxStock: 0,
    reorderPoint: 0
  }
  errors.value = {}
}

const validateForm = (): boolean => {
  errors.value = {}
  
  if (!form.value.code) errors.value.code = 'El código es requerido'
  if (!form.value.name) errors.value.name = 'El nombre es requerido'
  if (!form.value.unit) errors.value.unit = 'La unidad es requerida'
  if (form.value.price < 0) errors.value.price = 'El precio debe ser mayor o igual a 0'
  if (form.value.cost < 0) errors.value.cost = 'El costo debe ser mayor o igual a 0'
  if (!form.value.taxTypeId) errors.value.taxTypeId = 'El tipo de IVA es requerido'
  
  return Object.keys(errors.value).length === 0
}

const saveProduct = async () => {
  if (!validateForm()) return
  
  saving.value = true
  try {
    if (editingProduct.value) {
      await productsAPI.updateProduct(editingProduct.value.id, form.value)
      toast.add({
        severity: 'success',
        summary: 'Éxito',
        detail: 'Producto actualizado correctamente',
        life: 3000
      })
    } else {
      await productsAPI.createProduct(form.value)
      toast.add({
        severity: 'success',
        summary: 'Éxito',
        detail: 'Producto creado correctamente',
        life: 3000
      })
    }
    dialogVisible.value = false
    loadProducts()
    loadCategories()
  } catch (error: any) {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: error.response?.data?.error || 'Error al guardar producto',
      life: 3000
    })
  } finally {
    saving.value = false
  }
}

const confirmDelete = (product: IProduct) => {
  confirm.require({
    message: `¿Está seguro de eliminar el producto "${product.name}"?`,
    header: 'Confirmar Eliminación',
    icon: 'pi pi-exclamation-triangle',
    acceptLabel: 'Sí, eliminar',
    rejectLabel: 'Cancelar',
    acceptClass: 'p-button-danger',
    accept: () => deleteProduct(product.id)
  })
}

const deleteProduct = async (id: string) => {
  try {
    await productsAPI.deleteProduct(id)
    toast.add({
      severity: 'success',
      summary: 'Éxito',
      detail: 'Producto eliminado correctamente',
      life: 3000
    })
    loadProducts()
  } catch (error: any) {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: error.response?.data?.error || 'Error al eliminar producto',
      life: 3000
    })
  }
}

onMounted(() => {
  loadProducts()
  loadCategories()
})
</script>
