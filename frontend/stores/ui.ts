import { defineStore } from 'pinia'

export const useUiStore = defineStore('ui', () => {
  const sidebarVisible = ref(false)
  const loading = ref(false)
  const breadcrumbs = ref<{ label: string, to?: string }[]>([])

  const toggleSidebar = () => {
    sidebarVisible.value = !sidebarVisible.value
  }

  const showSidebar = () => {
    sidebarVisible.value = true
  }

  const hideSidebar = () => {
    sidebarVisible.value = false
  }

  const setLoading = (value: boolean) => {
    loading.value = value
  }

  const setBreadcrumbs = (items: { label: string, to?: string }[]) => {
    breadcrumbs.value = items
  }

  return {
    sidebarVisible,
    loading,
    breadcrumbs,
    toggleSidebar,
    showSidebar,
    hideSidebar,
    setLoading,
    setBreadcrumbs,
  }
})
