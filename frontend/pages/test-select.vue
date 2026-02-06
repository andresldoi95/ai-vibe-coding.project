<script setup lang="ts">
definePageMeta({
  layout: false,
  middleware: undefined,
})

const selectedCity = ref()
const cities = ref([
  { name: 'New York', code: 'NY' },
  { name: 'Rome', code: 'RM' },
  { name: 'London', code: 'LDN' },
  { name: 'Istanbul', code: 'IST' },
  { name: 'Paris', code: 'PRS' },
])

const selectedTheme = ref('light')
const themes = ref([
  { label: 'Light', value: 'light' },
  { label: 'Dark', value: 'dark' },
  { label: 'System', value: 'system' },
])

const password = ref('')
</script>

<template>
  <div class="p-8 space-y-8">
    <h1 class="text-2xl font-bold">
      Test Select Components
    </h1>

    <div class="space-y-4">
      <h2 class="text-xl">
        Basic Select (from docs)
      </h2>
      <Select
        v-model="selectedCity"
        :options="cities"
        optionLabel="name"
        placeholder="Select a City"
        class="w-full md:w-56"
      />
      <p>Selected: {{ selectedCity?.name || 'None' }}</p>
    </div>

    <div class="space-y-4">
      <h2 class="text-xl">
        Select with optionValue
      </h2>
      <Select
        v-model="selectedTheme"
        :options="themes"
        optionLabel="label"
        optionValue="value"
        placeholder="Select a Theme"
        class="w-full md:w-56"
      />
      <p>Selected: {{ selectedTheme }}</p>
    </div>

    <div class="space-y-4">
      <h2 class="text-xl">
        Password with toggle
      </h2>
      <Password
        v-model="password"
        toggleMask
        placeholder="Enter password"
        class="w-full md:w-56"
      />
    </div>
  </div>
</template>
