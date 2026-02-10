/**
 * Composable for common CRUD page functionality
 * Eliminates repetitive code across list/index pages
 */

import type { Ref } from 'vue'

export interface CrudPageOptions<T> {
  /**
   * Resource name for breadcrumbs and i18n (e.g., 'warehouses', 'products')
   */
  resourceName: string

  /**
   * Parent route for breadcrumbs (e.g., 'inventory', 'billing')
   */
  parentRoute?: string

  /**
   * Base path for navigation (e.g., '/inventory/warehouses')
   */
  basePath: string

  /**
   * Function to load all items
   */
  loadItems: () => Promise<T[]>

  /**
   * Function to delete an item
   */
  deleteItem: (id: string) => Promise<void>

  /**
   * Function to get item display name for delete confirmation
   */
  getItemName?: (item: T) => string
}

export interface CrudPageState<T> {
  items: Ref<T[]>
  loading: Ref<boolean>
  deleteDialog: Ref<boolean>
  selectedItem: Ref<T | null>
  loadData: () => Promise<void>
  handleCreate: () => void
  handleView: (item: T) => void
  handleEdit: (item: T) => void
  confirmDelete: (item: T) => void
  handleDelete: () => Promise<void>
  getItemId: (item: T) => string
}

export function useCrudPage<T extends { id: string, name?: string }>(
  options: CrudPageOptions<T>,
): CrudPageState<T> {
  const { t } = useI18n()
  const uiStore = useUiStore()
  const toast = useNotification()

  // State
  const items = ref<T[]>([]) as Ref<T[]>
  const loading = ref(false)
  const deleteDialog = ref(false)
  const selectedItem = ref<T | null>(null) as Ref<T | null>

  // Setup breadcrumbs
  const breadcrumbs = []
  if (options.parentRoute) {
    breadcrumbs.push({
      label: t(`nav.${options.parentRoute}`),
      to: `/${options.parentRoute}`,
    })
  }
  breadcrumbs.push({
    label: t(`${options.resourceName}.title`),
  })

  // Load data with error handling
  async function loadData() {
    loading.value = true
    try {
      items.value = await options.loadItems()
    }
    catch (error) {
      const errMessage = error instanceof Error ? error.message : 'Unknown error'
      toast.showError(t('messages.error_load'), errMessage)
    }
    finally {
      loading.value = false
    }
  }

  // Navigation functions
  function handleCreate() {
    navigateTo(`${options.basePath}/new`)
  }

  function handleView(item: T) {
    navigateTo(`${options.basePath}/${item.id}`)
  }

  function handleEdit(item: T) {
    navigateTo(`${options.basePath}/${item.id}/edit`)
  }

  // Delete functions
  function confirmDelete(item: T) {
    selectedItem.value = item
    deleteDialog.value = true
  }

  async function handleDelete() {
    if (!selectedItem.value)
      return

    try {
      await options.deleteItem(selectedItem.value.id)

      const itemName = options.getItemName
        ? options.getItemName(selectedItem.value)
        : selectedItem.value.name || selectedItem.value.id

      toast.showSuccess(
        t('messages.success_delete'),
        t(`${options.resourceName}.deleted_successfully`, { name: itemName }),
      )

      await loadData()
    }
    catch (error) {
      const errMessage = error instanceof Error ? error.message : 'Unknown error'
      toast.showError(t('messages.error_delete'), errMessage)
    }
    finally {
      deleteDialog.value = false
      selectedItem.value = null
    }
  }

  function getItemId(item: T): string {
    return item.id
  }

  // Initialize
  onMounted(() => {
    uiStore.setBreadcrumbs(breadcrumbs)
    loadData()
  })

  return {
    items,
    loading,
    deleteDialog,
    selectedItem,
    loadData,
    handleCreate,
    handleView,
    handleEdit,
    confirmDelete,
    handleDelete,
    getItemId,
  }
}
