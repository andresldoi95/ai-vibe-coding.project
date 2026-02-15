<script setup lang="ts">
definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const uiStore = useUiStore()

const modules = [
  {
    title: 'nav.invoices',
    description: 'module_descriptions.invoices',
    icon: 'pi pi-file',
    to: '/billing/invoices',
    color: 'bg-blue-500',
  },
  {
    title: 'nav.payments',
    description: 'module_descriptions.payments',
    icon: 'pi pi-money-bill',
    to: '/billing/payments',
    color: 'bg-emerald-500',
  },
  {
    title: 'nav.customers',
    description: 'module_descriptions.customers',
    icon: 'pi pi-users',
    to: '/billing/customers',
    color: 'bg-green-500',
  },
  {
    title: 'nav.establishments',
    description: 'module_descriptions.establishments',
    icon: 'pi pi-building',
    to: '/billing/establishments',
    color: 'bg-purple-500',
  },
  {
    title: 'nav.emission_points',
    description: 'module_descriptions.emission_points',
    icon: 'pi pi-map-marker',
    to: '/billing/emission-points',
    color: 'bg-orange-500',
  },
  {
    title: 'nav.tax_rates',
    description: 'module_descriptions.tax_rates',
    icon: 'pi pi-percentage',
    to: '/billing/tax-rates',
    color: 'bg-teal-500',
  },
  {
    title: 'nav.sri_configuration',
    description: 'module_descriptions.sri_configuration',
    icon: 'pi pi-shield',
    to: '/billing/sri-configuration',
    color: 'bg-indigo-500',
  },
]

onMounted(() => {
  uiStore.setBreadcrumbs([
    { label: t('nav.billing') },
  ])
})
</script>

<template>
  <div>
    <!-- Page Header -->
    <PageHeader
      :title="t('nav.billing')"
      description="Manage invoices, customers, and payments"
    />

    <!-- Module Cards Grid -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <Card
        v-for="module in modules"
        :key="module.to"
        class="cursor-pointer hover:shadow-lg transition-shadow"
        @click="navigateTo(module.to)"
      >
        <template #content>
          <div class="flex items-start gap-4">
            <div class="p-3 rounded-lg" :class="[module.color]">
              <i :class="module.icon" class="text-2xl text-white" />
            </div>
            <div class="flex-1">
              <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-2">
                {{ t(module.title) }}
              </h3>
              <p class="text-sm text-gray-600 dark:text-gray-400">
                {{ t(module.description) }}
              </p>
            </div>
            <i class="pi pi-chevron-right text-gray-400" />
          </div>
        </template>
      </Card>
    </div>
  </div>
</template>
