<template>
  <div class="flex flex-col h-screen bg-gray-100">
    <!-- Top Navigation -->
    <header class="fixed top-0 left-0 right-0 h-15 flex items-center justify-between px-6 bg-gradient-primary text-white shadow-lg z-[1030]">
      <div class="flex items-center gap-4">
        <button 
          @click="toggleSidebar" 
          class="p-2 rounded-lg transition-colors hover:bg-white/20"
        >
          <i class="pi pi-bars text-xl"></i>
        </button>
        <h1 class="text-xl font-semibold flex items-center gap-2">
          <i class="pi pi-receipt text-2xl"></i>
          <span class="hidden sm:inline">Sistema de Facturación</span>
        </h1>
      </div>

      <div class="flex items-center gap-4">
        <!-- Current Company Display -->
        <div v-if="authStore.currentCompany" class="hidden md:flex items-center gap-2 bg-white/10 px-4 py-2 rounded-lg border border-white/20">
          <i class="pi pi-building text-sm"></i>
          <div class="flex flex-col">
            <span class="text-sm font-semibold">{{ authStore.currentCompany.name }}</span>
            <span class="text-xs opacity-80">RUC: {{ authStore.currentCompany.ruc }}</span>
          </div>
        </div>

        <!-- User Menu -->
        <Menu ref="userMenu" :model="userMenuItems" :popup="true" />
        <button 
          @click="toggleUserMenu" 
          class="flex items-center gap-2 bg-white/10 border border-white/20 text-white px-4 py-2 rounded-full transition-all hover:bg-white/20"
        >
          <Avatar icon="pi pi-user" shape="circle" />
          <span class="font-medium hidden sm:inline">{{ userName }}</span>
          <i class="pi pi-angle-down"></i>
        </button>
      </div>
    </header>

    <!-- Sidebar Navigation -->
    <aside 
      :class="[
        'fixed top-15 left-0 h-[calc(100vh-3.75rem)] bg-white border-r border-gray-200 transition-width duration-300 overflow-hidden z-[1020]',
        sidebarCollapsed ? 'w-[70px]' : 'w-[250px]'
      ]"
    >
      <nav class="flex flex-col py-4">
        <router-link
          v-for="item in menuItems"
          :key="item.to"
          :to="item.to"
          class="flex items-center gap-4 px-6 py-4 text-gray-700 no-underline transition-all whitespace-nowrap hover:bg-gray-50 hover:text-primary"
          active-class="bg-primary/10 text-primary border-l-[3px] border-primary"
        >
          <i :class="[item.icon, 'text-xl min-w-[24px] flex items-center justify-center']"></i>
          <span v-if="!sidebarCollapsed" class="font-medium">{{ item.label }}</span>
        </router-link>
      </nav>
    </aside>

    <!-- Main Content -->
    <main 
      :class="[
        'mt-15 min-h-[calc(100vh-3.75rem)] transition-[margin-left] duration-300 bg-gray-100',
        sidebarCollapsed ? 'ml-[70px]' : 'ml-[250px]'
      ]"
    >
      <div class="p-8 max-w-screen-xl mx-auto">
        <router-view />
      </div>
    </main>

    <!-- Toast for notifications -->
    <Toast />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import Dropdown from 'primevue/dropdown'
import Avatar from 'primevue/avatar'
import Menu from 'primevue/menu'
import Toast from 'primevue/toast'
import type { MenuItem } from 'primevue/menuitem'

const router = useRouter()
const authStore = useAuthStore()

// Sidebar state
const sidebarCollapsed = ref(false)

// Company selection
interface ICompany {
  id: string
  businessName: string
  tradeName: string
  ruc: string
}

const selectedCompany = ref<ICompany | null>(null)
const companies = ref<ICompany[]>([
  // Mock data - will be replaced with actual API call
  { id: '1', businessName: 'MI EMPRESA S.A.', tradeName: 'Mi Empresa', ruc: '1234567890001' }
])

// User info
const userName = computed(() => authStore.user?.fullName || 'Usuario')

// User menu reference
const userMenu = ref()

// Menu items for sidebar
const menuItems = [
  { label: 'Dashboard', icon: 'pi pi-home', to: '/dashboard' },
  { label: 'Facturas', icon: 'pi pi-file-edit', to: '/invoices' },
  { label: 'Productos', icon: 'pi pi-box', to: '/products' },
  { label: 'Inventario', icon: 'pi pi-database', to: '/inventory' },
  { label: 'Clientes', icon: 'pi pi-users', to: '/customers' },
  { label: 'Reportes', icon: 'pi pi-chart-line', to: '/reports' }
]

// User menu items
const userMenuItems: MenuItem[] = [
  {
    label: 'Perfil',
    icon: 'pi pi-user',
    command: () => {
      // Navigate to profile
    }
  },
  {
    label: 'Configuración',
    icon: 'pi pi-cog',
    command: () => {
      // Navigate to settings
    }
  },
  {
    separator: true
  },
  {
    label: 'Cerrar Sesión',
    icon: 'pi pi-sign-out',
    command: () => {
      logout()
    }
  }
]

// Toggle sidebar
const toggleSidebar = () => {
  sidebarCollapsed.value = !sidebarCollapsed.value
}

// Toggle user menu
const toggleUserMenu = (event: Event) => {
  userMenu.value.toggle(event)
}

// Logout
const logout = () => {
  authStore.logout()
  router.push('/login')
}
</script>