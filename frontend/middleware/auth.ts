import type { NavigationGuard, RouteLocationNormalized } from 'vue-router'
import { navigateTo as _navigateTo } from '#app'

export default defineNuxtRouteMiddleware((to: RouteLocationNormalized, _from: RouteLocationNormalized): ReturnType<NavigationGuard> => {
  // Skip during SSR to prevent hydration issues with localStorage
  if (import.meta.server) {
    return
  }

  const authStore = useAuthStore() as unknown as { isAuthenticated: boolean }

  // Allow access to public auth pages
  const publicAuthPages = ['/login', '/register', '/forgot-password', '/reset-password']
  if (publicAuthPages.includes(to.path)) {
    // Redirect to dashboard if already authenticated
    if (authStore.isAuthenticated) {
      return _navigateTo('/')
    }
    return
  }

  // Require authentication for all other pages
  if (!authStore.isAuthenticated) {
    return _navigateTo('/login')
  }
})
