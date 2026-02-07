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
    description: 'Create and manage customer invoices',
    icon: 'pi pi-file',
    to: '/billing/invoices',
    color: 'bg-blue-500',
  },
  {
    title: 'nav.customers',
    description: 'Manage customer information and contacts',
    icon: 'pi pi-users',
    to: '/billing/customers',
    color: 'bg-green-500',
  },
  {
    title: 'nav.payments',
    description: 'Track payments and payment history',
    icon: 'pi pi-credit-card',
    to: '/billing/payments',
    color: 'bg-purple-500',
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
                {{ module.description }}
              </p>
            </div>
            <i class="pi pi-chevron-right text-gray-400" />
          </div>
        </template>
      </Card>
    </div>
  </div>
</template>
