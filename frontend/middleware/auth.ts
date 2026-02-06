export default defineNuxtRouteMiddleware((to) => {
  const authStore = useAuthStore()

  // Allow access to login page
  if (to.path === '/login') {
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
