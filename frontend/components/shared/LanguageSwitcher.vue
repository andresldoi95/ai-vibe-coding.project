<script setup lang="ts">
const { locale, locales, setLocale } = useI18n()

interface LocaleInfo {
  code: string
  name: string
  iso?: string
  file?: string
}

const availableLocales = computed(() =>
  (locales.value as LocaleInfo[]).map(l => ({
    label: l.name,
    value: l.code,
  })),
)

const currentLocale = computed({
  get: () => locale.value,
  set: value => setLocale(value),
})
</script>

<template>
  <Select
    v-model="currentLocale"
    :options="availableLocales"
    optionLabel="label"
    optionValue="value"
    placeholder="Language"
    appendTo="body"
    class="w-full sm:w-40"
  >
    <template #value="{ value }">
      <div class="flex items-center gap-2">
        <i class="pi pi-globe" />
        <span class="hidden sm:inline">{{
          availableLocales.find((l) => l.value === value)?.label
        }}</span>
      </div>
    </template>
  </Select>
</template>
