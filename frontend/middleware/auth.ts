export default defineNuxtRouteMiddleware((to) => {
  // Skip during SSR to prevent hydration issues with localStorage
  if (import.meta.server) {
    return
  }

  const authStore = useAuthStore()

  // Allow access to public auth pages
  const publicAuthPages = ['/login', '/register', '/forgot-password', '/reset-password']
  if (publicAuthPages.includes(to.path)) {
    // Redirect to dashboard if already authenticated
    if (authStore.isAuthenticated) {
      return navigateTo('/')
    }
    return
  }

  // Require authentication for all other pages
  if (!authStore.isAuthenticated) {
    return navigateTo('/login')
  }
})
