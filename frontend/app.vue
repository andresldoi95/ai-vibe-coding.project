<script setup lang="ts">
const { isDark } = useTheme()

useHead({
  htmlAttrs: {
    class: computed(() => isDark.value ? 'dark' : ''),
  },
})
</script>

<template>
  <NuxtLayout>
    <NuxtPage />
  </NuxtLayout>
</template>
