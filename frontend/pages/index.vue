<script setup lang="ts">
definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const uiStore = useUiStore()

onMounted(() => {
  uiStore.setBreadcrumbs([{ label: t('nav.dashboard') }])
})
</script>

<template>
  <div>
    <PageHeader
      :title="t('nav.dashboard')"
      :description="t('dashboard.welcome')"
    />

    <!-- Stats Grid - Using spacing standard: gap-6 (24px between cards) -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-6">
      <StatCard
        :title="t('dashboard.total_invoices')"
        :value="0"
        icon="pi pi-file"
        severity="info"
      />

      <StatCard
        :title="t('dashboard.total_revenue')"
        value="$0.00"
        icon="pi pi-dollar"
        severity="success"
      />

      <StatCard
        :title="t('dashboard.total_products')"
        :value="0"
        icon="pi pi-shopping-cart"
        severity="info"
      />

      <StatCard
        :title="t('dashboard.low_stock')"
        :value="0"
        icon="pi pi-exclamation-triangle"
        severity="warning"
      />
    </div>

    <!-- Content Grid - Using spacing standard: gap-6 -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <!-- Recent Invoices -->
      <Card>
        <template #title>
          {{ t("dashboard.recent_invoices") }}
        </template>
        <template #content>
          <EmptyState
            icon="pi pi-file"
            :title="t('common.no_data')"
            description="No recent invoices to display"
          />
        </template>
      </Card>

      <!-- Recent Stock Movements -->
      <Card>
        <template #title>
          {{ t("dashboard.recent_stock_movements") }}
        </template>
        <template #content>
          <EmptyState
            icon="pi pi-box"
            :title="t('common.no_data')"
            description="No recent stock movements to display"
          />
        </template>
      </Card>
    </div>
  </div>
</template>
