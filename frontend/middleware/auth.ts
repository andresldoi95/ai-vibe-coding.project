export default defineNuxtRouteMiddleware((to) => {
  // Skip during SSR to prevent hydration issues with localStorage
  if (import.meta.server) {
    return
  }

  const authStore = useAuthStore()

  // Allow access to login and register pages
  if (to.path === '/login' || to.path === '/register') {
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
