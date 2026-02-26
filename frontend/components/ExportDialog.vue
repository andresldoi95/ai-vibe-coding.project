<script setup lang="ts">
/**
 * Reusable Export Dialog Component
 * Eliminates repetitive export dialog code across pages
 */

interface ExportFilter {
  name: string
  label: string
  type: 'text' | 'select' | 'date' | 'daterange'
  value?: unknown
  options?: Array<{ label: string, value: unknown }>
  placeholder?: string
}

interface Props {
  visible: boolean
  title?: string
  description?: string
  formats?: Array<{ label: string, value: string }>
  filters?: ExportFilter[]
  loading?: boolean
}

interface Emits {
  (e: 'update:visible', value: boolean): void
  (e: 'export', data: { format: string, filters: Record<string, unknown> }): void
  (e: 'cancel'): void
}

const props = withDefaults(defineProps<Props>(), {
  title: undefined,
  description: undefined,
  formats: () => [
    { label: 'Excel', value: 'excel' },
    { label: 'CSV', value: 'csv' },
  ],
  filters: () => [],
  loading: false,
})

const emit = defineEmits<Emits>()

const { t } = useI18n()

const dialogVisible = computed({
  get: () => props.visible,
  set: value => emit('update:visible', value),
})

const selectedFormat = ref(props.formats[0]?.value || 'excel')
const filterValues = reactive<Record<string, unknown>>({})

function getFilterValue(name: string): string {
  return filterValues[name] as string
}
function getFilterDate(name: string): Date | Date[] | null | undefined {
  return filterValues[name] as Date | null | undefined
}
function setFilterValue(name: string, val: unknown) {
  filterValues[name] = val
}

// Initialize filter values
watch(() => props.filters, (newFilters) => {
  newFilters.forEach((filter) => {
    if (!(filter.name in filterValues)) {
      filterValues[filter.name] = filter.value || ''
    }
  })
}, { immediate: true })

const dialogTitle = computed(() => props.title || t('common.export'))
const dialogDescription = computed(() => props.description || t('common.export_description'))

function handleExport() {
  emit('export', {
    format: selectedFormat.value,
    filters: { ...filterValues },
  })
}

function handleCancel() {
  dialogVisible.value = false
  emit('cancel')
}
</script>

<template>
  <Dialog
    v-model:visible="dialogVisible"
    :header="dialogTitle"
    :modal="true"
    :style="{ width: '600px' }"
  >
    <div class="space-y-4">
      <!-- Description -->
      <p v-if="dialogDescription" class="text-sm text-gray-600 dark:text-gray-400">
        {{ dialogDescription }}
      </p>

      <!-- Format Selection -->
      <div>
        <label class="block text-sm font-medium mb-2">{{ t('common.export_format') }}</label>
        <div class="flex gap-4">
          <div
            v-for="format in formats"
            :key="format.value"
            class="flex items-center"
          >
            <RadioButton
              :id="`format-${format.value}`"
              v-model="selectedFormat"
              :value="format.value"
            />
            <label :for="`format-${format.value}`" class="ml-2">{{ format.label }}</label>
          </div>
        </div>
      </div>

      <!-- Filters -->
      <template v-if="filters.length > 0">
        <Divider />

        <div>
          <h4 class="text-sm font-medium mb-3">
            {{ t('common.filters') }}
          </h4>

          <div class="space-y-3">
            <div v-for="filter in filters" :key="filter.name">
              <!-- Text Filter -->
              <template v-if="filter.type === 'text'">
                <label :for="filter.name" class="block text-sm mb-1">{{ filter.label }}</label>
                <InputText
                  :id="filter.name"
                  :model-value="getFilterValue(filter.name)"
                  :placeholder="filter.placeholder"
                  class="w-full"
                  @update:model-value="setFilterValue(filter.name, $event)"
                />
              </template>

              <!-- Select Filter -->
              <template v-else-if="filter.type === 'select'">
                <label :for="filter.name" class="block text-sm mb-1">{{ filter.label }}</label>
                <Select
                  :id="filter.name"
                  v-model="filterValues[filter.name]"
                  :options="filter.options || []"
                  option-label="label"
                  option-value="value"
                  :placeholder="filter.placeholder || t('common.select')"
                  class="w-full"
                  show-clear
                />
              </template>

              <!-- Date Filter -->
              <template v-else-if="filter.type === 'date'">
                <label :for="filter.name" class="block text-sm mb-1">{{ filter.label }}</label>
                <DatePicker
                  :id="filter.name"
                  :model-value="getFilterDate(filter.name)"
                  date-format="yy-mm-dd"
                  class="w-full"
                  show-icon
                  @update:model-value="setFilterValue(filter.name, $event)"
                />
              </template>
            </div>
          </div>
        </div>
      </template>
    </div>

    <template #footer>
      <Button
        :label="t('common.cancel')"
        severity="secondary"
        outlined
        :disabled="loading"
        @click="handleCancel"
      />
      <Button
        :label="loading ? t('common.exporting') : t('common.export')"
        icon="pi pi-download"
        :loading="loading"
        @click="handleExport"
      />
    </template>
  </Dialog>
</template>
