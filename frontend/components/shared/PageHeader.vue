<script setup lang="ts">
interface Props {
  title: string
  description?: string
}

defineProps<Props>()
</script>

<template>
  <div class="mb-6">
    <div class="flex justify-between items-start">
      <div>
        <h1 class="text-3xl font-bold text-gray-900 dark:text-gray-50">
          {{ title }}
        </h1>
        <p
          v-if="description"
          class="mt-2 text-sm text-gray-500 dark:text-gray-400"
        >
          {{ description }}
        </p>
      </div>
      <div class="flex gap-2">
        <slot name="actions" />
      </div>
    </div>
  </div>
</template>
