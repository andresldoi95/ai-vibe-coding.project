<script setup lang="ts">
const { isDark } = useTheme()

// Apply dark class to both html and body for better compatibility
useHead({
  htmlAttrs: {
    class: computed(() => isDark.value ? 'dark' : ''),
  },
  bodyAttrs: {
    class: computed(() => isDark.value ? 'dark' : ''),
  },
})
</script>

<template>
  <NuxtLayout>
    <NuxtPage />
  </NuxtLayout>
</template>
