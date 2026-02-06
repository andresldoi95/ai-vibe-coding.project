<script setup lang="ts">
interface Props {
  message?: string
  overlay?: boolean
}

defineProps<Props>()
</script>

<template>
  <div
    class="flex flex-col items-center justify-center py-12"
    :class="[
      overlay && 'absolute inset-0 bg-white/80 dark:bg-gray-900/80 z-10',
    ]"
  >
    <ProgressSpinner />
    <p v-if="message" class="mt-4 text-sm text-gray-500 dark:text-gray-400">
      {{ message }}
    </p>
  </div>
</template>
