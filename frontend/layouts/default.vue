<script setup lang="ts">
const { t } = useI18n()
const { isDark } = useTheme()
const authStore = useAuthStore()
const tenantStore = useTenantStore()
const uiStore = useUiStore()

const menuItems = computed(() => [
  {
    label: t('nav.dashboard'),
    icon: 'pi pi-home',
    to: '/',
  },
  {
    label: t('nav.billing'),
    icon: 'pi pi-dollar',
    items: [
      { label: t('nav.invoices'), icon: 'pi pi-file', to: '/billing/invoices' },
      {
        label: t('nav.customers'),
        icon: 'pi pi-users',
        to: '/billing/customers',
      },
      {
        label: t('nav.payments'),
        icon: 'pi pi-credit-card',
        to: '/billing/payments',
      },
    ],
  },
  {
    label: t('nav.inventory'),
    icon: 'pi pi-box',
    items: [
      {
        label: t('nav.products'),
        icon: 'pi pi-shopping-cart',
        to: '/inventory/products',
      },
      {
        label: t('nav.warehouses'),
        icon: 'pi pi-building',
        to: '/inventory/warehouses',
      },
      {
        label: t('nav.stock_movements'),
        icon: 'pi pi-arrows-h',
        to: '/inventory/stock-movements',
      },
    ],
  },
])

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

const userMenuVisible = ref(false)

function toggleUserMenu(_event: Event) {
  userMenuVisible.value = !userMenuVisible.value
}

const breadcrumbHome = { icon: 'pi pi-home', to: '/' }
</script>

<template>
  <div class="min-h-screen" :class="isDark ? 'dark bg-gray-900' : 'bg-gray-50'">
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
        <div class="flex items-center gap-2">
          <!-- Tenant Selector -->
          <Dropdown
            v-if="tenantStore.availableTenants.length > 0"
            v-model="tenantStore.currentTenantId"
            :options="tenantStore.availableTenants"
            optionLabel="name"
            optionValue="id"
            placeholder="Select Tenant"
            appendTo="body"
            class="w-48"
            @change="(event) => tenantStore.setTenant(event.value)"
          >
            <template #value="{ value }">
              <div v-if="value" class="flex items-center gap-2">
                <i class="pi pi-briefcase" />
                <span>{{ tenantStore.currentTenant?.name }}</span>
              </div>
            </template>
          </Dropdown>

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
            v-model:visible="userMenuVisible"
            :model="userMenuItems"
            :popup="true"
          />
        </div>
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
      />

      <!-- Page Content -->
      <slot />
    </div>

    <!-- Toast for notifications -->
    <Toast position="top-right" />
  </div>
</template>
