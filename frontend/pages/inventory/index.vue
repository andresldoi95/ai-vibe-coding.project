<script setup lang="ts">
definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const uiStore = useUiStore()

const modules = [
  {
    title: 'warehouses.title',
    description: 'warehouses.description',
    icon: 'pi pi-building',
    to: '/inventory/warehouses',
    color: 'bg-blue-500',
  },
  {
    title: 'inventory.products',
    description: 'Manage products, SKUs, and pricing',
    icon: 'pi pi-box',
    to: '/inventory/products',
    color: 'bg-green-500',
  },
  {
    title: 'Stock Movements',
    description: 'Track inventory movements and transfers',
    icon: 'pi pi-arrows-h',
    to: '/inventory/stock-movements',
    color: 'bg-purple-500',
  },
]

onMounted(() => {
  uiStore.setBreadcrumbs([
    { label: t('nav.inventory') },
  ])
})
</script>

<template>
  <div>
    <!-- Page Header -->
    <PageHeader
      :title="t('nav.inventory')"
      :description="t('dashboard.welcome')"
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
                {{ typeof module.description === 'string' && module.description.includes('.') ? t(module.description) : module.description }}
              </p>
            </div>
            <i class="pi pi-chevron-right text-gray-400" />
          </div>
        </template>
      </Card>
    </div>
  </div>
</template>
