import { createRouter, createWebHistory } from 'vue-router'
import type { RouteRecordRaw } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import AppLayout from '@/components/AppLayout.vue'

const routes: RouteRecordRaw[] = [
  // Auth routes (no layout)
  {
    path: '/login',
    name: 'login',
    component: () => import('@/views/auth/LoginView.vue'),
    meta: { requiresAuth: false }
  },
  {
    path: '/register',
    name: 'register',
    component: () => import('@/views/auth/RegisterView.vue'),
    meta: { requiresAuth: false }
  },
  // App routes (with layout)
  {
    path: '/',
    component: AppLayout,
    meta: { requiresAuth: true },
    children: [
      {
        path: '',
        name: 'home',
        redirect: '/dashboard'
      },
      {
        path: 'dashboard',
        name: 'dashboard',
        component: () => import('@/views/DashboardView.vue'),
      },
      {
        path: 'invoices',
        name: 'invoices',
        component: () => import('@/views/invoices/InvoicesView.vue'),
      },
      {
        path: 'products',
        name: 'products',
        component: () => import('@/views/products/ProductsView.vue'),
      },
      {
        path: 'inventory',
        name: 'inventory',
        component: () => import('@/views/inventory/InventoryView.vue'),
      },
      {
        path: 'customers',
        name: 'customers',
        component: () => import('@/views/customers/CustomersView.vue'),
      },
      {
        path: 'reports',
        name: 'reports',
        component: () => import('@/views/reports/ReportsView.vue'),
      }
    ]
  }
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes
})

// Navigation guard for authentication
router.beforeEach((to, _from, next) => {
  const authStore = useAuthStore()
  
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    next({ name: 'login', query: { redirect: to.fullPath } })
  } else if ((to.name === 'login' || to.name === 'register') && authStore.isAuthenticated) {
    next({ name: 'dashboard' })
  } else {
    next()
  }
})

export default router
