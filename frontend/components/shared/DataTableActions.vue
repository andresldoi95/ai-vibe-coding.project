<script setup lang="ts">
interface Props {
  showView?: boolean
  showEdit?: boolean
  showDelete?: boolean
}

withDefaults(defineProps<Props>(), {
  showView: true,
  showEdit: true,
  showDelete: true,
})

const emit = defineEmits<{
  view: []
  edit: []
  delete: []
}>()
</script>

<template>
  <div class="flex gap-2">
    <Button
      v-if="showView"
      icon="pi pi-eye"
      severity="info"
      text
      rounded
      aria-label="View"
      @click="emit('view')"
    />
    <Button
      v-if="showEdit"
      icon="pi pi-pencil"
      severity="warning"
      text
      rounded
      aria-label="Edit"
      @click="emit('edit')"
    />
    <Button
      v-if="showDelete"
      icon="pi pi-trash"
      severity="danger"
      text
      rounded
      aria-label="Delete"
      @click="emit('delete')"
    />
  </div>
</template>
