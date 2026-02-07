<script setup lang="ts">
// Theme is automatically handled by color-mode module and app.vue
</script>

<template>
  <div class="min-h-screen flex items-center justify-center relative bg-gradient-to-br from-gray-50 via-teal-50 to-gray-50 dark:bg-gradient-to-br dark:from-gray-900 dark:via-gray-800 dark:to-gray-900">
    <!-- Theme Switcher - Fixed Top Right -->
    <div class="fixed top-4 right-4 z-[99999] grid grid-cols-2 md:flex md:flex-row items-center gap-2">
      <LanguageSwitcher />
      <ThemeSwitcher />
    </div>

    <!-- Animated Background Pattern -->
    <div class="absolute inset-0 opacity-30 pointer-events-none overflow-hidden">
      <div class="absolute top-20 left-20 w-72 h-72 bg-teal-400/20 rounded-full mix-blend-multiply filter blur-xl animate-pulse" />
      <div class="absolute top-40 right-20 w-72 h-72 bg-primary/20 rounded-full mix-blend-multiply filter blur-xl animate-pulse" style="animation-delay: 2s;" />
      <div class="absolute bottom-20 left-1/2 w-72 h-72 bg-teal-300/20 rounded-full mix-blend-multiply filter blur-xl animate-pulse" style="animation-delay: 4s;" />
    </div>

    <!-- Content Container -->
    <div class="w-full max-w-md p-6 relative z-10">
      <slot />
    </div>

    <!-- Toast for notifications - positioned below switchers -->
    <Toast position="top-right" :pt="{ root: { style: 'top: 5rem' } }" />
  </div>
</template>
