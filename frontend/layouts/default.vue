<script setup lang="ts">
const { t } = useI18n()
const authStore = useAuthStore()
const tenantStore = useTenantStore()
const uiStore = useUiStore()

const menuItems = computed(() => [
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
])

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
            <Dropdown
              v-if="tenantStore.availableTenants.length > 0"
              v-model="tenantStore.currentTenantId"
              :options="tenantStore.availableTenants"
              optionLabel="name"
              optionValue="id"
              placeholder="Select Tenant"
              appendTo="body"
              class="w-48"
              @change="(event: Event) => tenantStore.selectTenant((event.target as HTMLSelectElement).value)"
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
