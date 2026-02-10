<script setup lang="ts">
/**
 * Reusable Delete Confirmation Dialog Component
 * Eliminates repetitive delete dialog code across CRUD pages
 */

interface Props {
  visible: boolean
  itemName?: string
  title?: string
  message?: string
  confirmLabel?: string
  cancelLabel?: string
  loading?: boolean
}

interface Emits {
  (e: 'update:visible', value: boolean): void
  (e: 'confirm'): void
  (e: 'cancel'): void
}

const props = withDefaults(defineProps<Props>(), {
  itemName: '',
  title: undefined,
  message: undefined,
  confirmLabel: undefined,
  cancelLabel: undefined,
  loading: false,
})

const emit = defineEmits<Emits>()

const { t } = useI18n()

const dialogVisible = computed({
  get: () => props.visible,
  set: value => emit('update:visible', value),
})

const dialogTitle = computed(() => props.title || t('common.confirm'))
const dialogMessage = computed(() => {
  if (props.message)
    return props.message
  if (props.itemName) {
    return t('common.confirm_delete_item', { name: props.itemName })
  }
  return t('common.confirm_delete')
})
const confirmButtonLabel = computed(() => props.confirmLabel || t('common.delete'))
const cancelButtonLabel = computed(() => props.cancelLabel || t('common.cancel'))

function handleConfirm() {
  emit('confirm')
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
    :style="{ width: '450px' }"
  >
    <div class="flex items-center gap-4">
      <i class="pi pi-exclamation-triangle text-3xl text-orange-500" />
      <span>{{ dialogMessage }}</span>
    </div>

    <template #footer>
      <Button
        :label="cancelButtonLabel"
        icon="pi pi-times"
        text
        :disabled="loading"
        @click="handleCancel"
      />
      <Button
        :label="confirmButtonLabel"
        icon="pi pi-trash"
        severity="danger"
        :loading="loading"
        @click="handleConfirm"
      />
    </template>
  </Dialog>
</template>
