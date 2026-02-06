<script setup lang="ts">
type ThemePref = 'system' | 'light' | 'dark'
interface ThemeOption { label: string, value: ThemePref, icon: string }

const { t } = useI18n()
const colorMode = useColorMode()

const themeOptions = computed<ThemeOption[]>(() => [
  { label: t('common.theme_system'), value: 'system', icon: 'pi pi-desktop' },
  { label: t('common.theme_light'), value: 'light', icon: 'pi pi-sun' },
  { label: t('common.theme_dark'), value: 'dark', icon: 'pi pi-moon' },
])

const optionByValue = computed(() => {
  const m = new Map<ThemePref, ThemeOption>()
  for (const opt of themeOptions.value) m.set(opt.value, opt)
  return m
})

const selectedTheme = computed<ThemePref>({
  get: () => (colorMode.preference as ThemePref) ?? 'system',
  set: (value) => { colorMode.preference = value },
})
</script>

<template>
  <Select
    v-model="selectedTheme"
    :options="themeOptions"
    optionLabel="label"
    optionValue="value"
    appendTo="body"
    class="w-full sm:w-36"
  >
    <template #value="{ value }">
      <div v-if="value" class="flex items-center gap-2">
        <i :class="optionByValue.get(value)?.icon" />
        <span class="hidden md:inline">{{ optionByValue.get(value)?.label }}</span>
      </div>
      <span v-else class="text-sm opacity-70">—</span>
    </template>

    <template #option="{ option }">
      <div class="flex items-center gap-2">
        <i :class="option.icon" />
        <span>{{ option.label }}</span>
      </div>
    </template>
  </Select>
</template>
