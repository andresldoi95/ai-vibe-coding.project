<script setup lang="ts">
interface Props {
  icon?: string
  title: string
  description?: string
  actionLabel?: string
  actionIcon?: string
}

defineProps<Props>()
const emit = defineEmits<{
  action: []
}>()
</script>

<template>
  <div class="flex flex-col items-center justify-center py-12 text-center">
    <i
      v-if="icon"
      :class="icon"
      class="text-6xl text-gray-400 dark:text-gray-600 mb-4"
    />
    <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-50 mb-2">
      {{ title }}
    </h3>
    <p
      v-if="description"
      class="text-sm text-gray-500 dark:text-gray-400 mb-6 max-w-md"
    >
      {{ description }}
    </p>
    <Button
      v-if="actionLabel"
      :label="actionLabel"
      :icon="actionIcon"
      @click="emit('action')"
    />
  </div>
</template>
