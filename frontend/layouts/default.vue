<script setup lang="ts">
const { t } = useI18n()
const authStore = useAuthStore()
const tenantStore = useTenantStore()
const uiStore = useUiStore()
const { hasPermission } = usePermissions()
const { showSuccess, showError } = useNotification()

// Handle tenant selection
async function handleTenantChange(event: { value: string | null }) {
  const tenantId = event.value
  if (tenantId && tenantId !== tenantStore.currentTenantId) {
    try {
      await authStore.selectTenant(tenantId)
      showSuccess('Company switched successfully')

      // Reload page to ensure all data is fetched with new tenant context
      window.location.reload()
    }
    catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Failed to switch company'
      console.error('Failed to switch company:', error)
      showError(errorMessage)
    }
  }
}

const menuItems = computed(() => {
  const items = [
    {
      label: t('nav.dashboard'),
      icon: 'pi pi-home',
      command: () => navigateTo('/'),
    },
    {
      label: t('nav.billing'),
      icon: 'pi pi-dollar',
      command: () => navigateTo('/billing'),
      items: [
        { label: t('nav.invoices'), icon: 'pi pi-file', command: () => navigateTo('/billing/invoices') },
        {
          label: t('nav.customers'),
          icon: 'pi pi-users',
          command: () => navigateTo('/billing/customers'),
        },
        {
          label: t('nav.payments'),
          icon: 'pi pi-credit-card',
          command: () => navigateTo('/billing/payments'),
        },
        {
          label: t('nav.tax_rates'),
          icon: 'pi pi-percentage',
          command: () => navigateTo('/billing/tax-rates'),
        },
        {
          label: t('nav.invoice_configuration'),
          icon: 'pi pi-cog',
          command: () => navigateTo('/billing/invoice-configuration'),
        },
      ],
    },
    {
      label: t('nav.inventory'),
      icon: 'pi pi-box',
      command: () => navigateTo('/inventory'),
      items: [
        {
          label: t('nav.products'),
          icon: 'pi pi-shopping-cart',
          command: () => navigateTo('/inventory/products'),
        },
        {
          label: t('nav.warehouses'),
          icon: 'pi pi-building',
          command: () => navigateTo('/inventory/warehouses'),
        },
        {
          label: t('nav.stock_movements'),
          icon: 'pi pi-arrows-h',
          command: () => navigateTo('/inventory/stock-movements'),
        },
      ],
    },
  ]

  // Add Settings menu only if user has roles.read permission
  const settingsItems = []
  if (hasPermission('roles.read')) {
    settingsItems.push({
      label: t('nav.roles'),
      icon: 'pi pi-shield',
      command: () => navigateTo('/settings/roles'),
    })
  }

  // Only show Settings menu if there are items
  if (settingsItems.length > 0) {
    items.push({
      label: t('nav.settings'),
      icon: 'pi pi-cog',
      command: () => navigateTo('/settings'),
      items: settingsItems,
    })
  }

  return items
})

const userMenu = ref()

const userMenuItems = ref([
  {
    label: t('nav.profile'),
    icon: 'pi pi-user',
    command: () => navigateTo('/profile'),
  },
  {
    label: t('nav.settings'),
    icon: 'pi pi-cog',
    command: () => navigateTo('/settings'),
  },
  {
    separator: true,
  },
  {
    label: t('nav.logout'),
    icon: 'pi pi-sign-out',
    command: () => {
      authStore.logout()
      navigateTo('/login')
    },
  },
])

function toggleUserMenu(event: Event) {
  userMenu.value.toggle(event)
}

const breadcrumbHome = { icon: 'pi pi-home', to: '/' }
</script>

<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-900">
    <!-- Top Navigation -->
    <Menubar :model="menuItems" class="rounded-none border-x-0 border-t-0">
      <template #start>
        <div class="flex items-center gap-3 px-3">
          <i class="pi pi-chart-line text-2xl text-primary" />
          <span class="font-bold text-xl hidden md:inline">{{
            t("app.title")
          }}</span>
        </div>
      </template>

      <template #end>
        <ClientOnly>
          <div class="flex items-center gap-2">
            <!-- Tenant Selector -->
            <Select
              v-if="tenantStore.availableTenants.length > 0"
              :modelValue="tenantStore.currentTenantId"
              :options="tenantStore.availableTenants"
              optionLabel="name"
              optionValue="id"
              placeholder="Select Company"
              class="w-48"
              @change="handleTenantChange"
            >
              <template #value="{ value }">
                <div v-if="value" class="flex items-center gap-2">
                  <i class="pi pi-briefcase" />
                  <span>{{ tenantStore.currentTenant?.name }}</span>
                </div>
              </template>
            </Select>

            <!-- Language Switcher -->
            <LanguageSwitcher />

            <!-- Theme Switcher -->
            <ThemeSwitcher />

            <!-- User Menu -->
            <Button
              icon="pi pi-user"
              text
              rounded
              aria-haspopup="true"
              aria-controls="user_menu"
              @click="toggleUserMenu"
            />
            <Menu
              id="user_menu"
              ref="userMenu"
              :model="userMenuItems"
              :popup="true"
            />
          </div>

          <template #fallback>
            <div class="flex items-center gap-2">
              <!-- Placeholder to maintain layout during SSR -->
              <div class="w-48 h-10" />
              <div class="w-40 h-10" />
              <div class="w-36 h-10" />
              <div class="w-10 h-10" />
            </div>
          </template>
        </ClientOnly>
      </template>
    </Menubar>

    <!-- Main Content Area -->
    <div class="max-w-7xl mx-auto px-4 py-6">
      <!-- Breadcrumb -->
      <Breadcrumb
        v-if="uiStore.breadcrumbs.length > 0"
        :home="breadcrumbHome"
        :model="uiStore.breadcrumbs"
        class="mb-4 bg-transparent border-0"
      >
        <template #item="{ item }">
          <NuxtLink
            v-if="item.to"
            :to="item.to"
            class="text-primary hover:underline"
          >
            {{ item.label }}
          </NuxtLink>
          <span v-else class="text-gray-600 dark:text-gray-400">
            {{ item.label }}
          </span>
        </template>
      </Breadcrumb>

      <!-- Page Content -->
      <slot />
    </div>

    <!-- Toast for notifications -->
    <Toast position="top-right" />
  </div>
</template>
