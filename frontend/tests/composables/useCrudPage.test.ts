import { beforeEach, describe, expect, it, vi } from 'vitest'
import { useCrudPage } from '~/composables/useCrudPage'
import type { CrudPageOptions } from '~/composables/useCrudPage'

// Mock dependencies
const mockNavigateTo = vi.fn()
globalThis.navigateTo = mockNavigateTo

const mockUiStore = {
  setBreadcrumbs: vi.fn(),
}
globalThis.useUiStore = vi.fn(() => mockUiStore)

const mockToast = {
  showSuccess: vi.fn(),
  showError: vi.fn(),
}
globalThis.useNotification = vi.fn(() => mockToast)

const mockOnMounted = vi.fn((callback: () => void) => callback())
globalThis.onMounted = mockOnMounted

// Mock i18n
const mockT = vi.fn((key: string, params?: Record<string, unknown>) => {
  if (params) {
    let result = key
    Object.entries(params).forEach(([k, v]) => {
      result = result.replace(`{${k}}`, String(v))
    })
    return result
  }
  return key
})
globalThis.useI18n = vi.fn(() => ({ t: mockT }))

interface TestItem {
  id: string
  name: string
}

describe('useCrudPage', () => {
  let loadItemsMock: ReturnType<typeof vi.fn>
  let deleteItemMock: ReturnType<typeof vi.fn>
  let options: CrudPageOptions<TestItem>

  beforeEach(() => {
    vi.clearAllMocks()

    loadItemsMock = vi.fn()
    deleteItemMock = vi.fn()

    options = {
      resourceName: 'warehouses',
      parentRoute: 'inventory',
      basePath: '/inventory/warehouses',
      // eslint-disable-next-line ts/no-explicit-any
      loadItems: loadItemsMock as any,
      // eslint-disable-next-line ts/no-explicit-any
      deleteItem: deleteItemMock as any,
    }
  })

  describe('initialization', () => {
    it('should set breadcrumbs with parent route', () => {
      loadItemsMock.mockResolvedValue([])

      useCrudPage(options)

      expect(mockUiStore.setBreadcrumbs).toHaveBeenCalledWith([
        { label: 'nav.inventory', to: '/inventory' },
        { label: 'warehouses.title' },
      ])
    })

    it('should set breadcrumbs without parent route', () => {
      loadItemsMock.mockResolvedValue([])
      const optionsWithoutParent = { ...options, parentRoute: undefined }

      useCrudPage(optionsWithoutParent)

      expect(mockUiStore.setBreadcrumbs).toHaveBeenCalledWith([
        { label: 'warehouses.title' },
      ])
    })

    it('should load data on mount', () => {
      loadItemsMock.mockResolvedValue([
        { id: '1', name: 'Item 1' },
        { id: '2', name: 'Item 2' },
      ])

      useCrudPage(options)

      expect(loadItemsMock).toHaveBeenCalled()
      expect(mockOnMounted).toHaveBeenCalled()
    })
  })

  describe('loadData', () => {
    it('should load items successfully', async () => {
      const mockItems = [
        { id: '1', name: 'Item 1' },
        { id: '2', name: 'Item 2' },
      ]
      loadItemsMock.mockResolvedValue(mockItems)

      const { loadData, items, loading } = useCrudPage(options)

      // Wait for initial mount load to complete
      await vi.waitFor(() => expect(loading.value).toBe(false))

      await loadData()

      expect(items.value).toEqual(mockItems)
      expect(loading.value).toBe(false)
    })

    it('should set loading state during load', async () => {
      loadItemsMock.mockImplementation(() => new Promise(resolve => setTimeout(resolve, 100)))

      const { loadData, loading } = useCrudPage(options)

      const loadPromise = loadData()
      expect(loading.value).toBe(true)

      await loadPromise
      expect(loading.value).toBe(false)
    })

    it('should handle load errors', async () => {
      const error = new Error('Network error')
      loadItemsMock.mockRejectedValue(error)

      const { loadData, items } = useCrudPage(options)

      await loadData()

      expect(items.value).toEqual([])
      expect(mockToast.showError).toHaveBeenCalledWith(
        'messages.error_load',
        'Network error',
      )
    })

    it('should handle unknown errors', async () => {
      loadItemsMock.mockRejectedValue('Unknown error')

      const { loadData } = useCrudPage(options)

      await loadData()

      expect(mockToast.showError).toHaveBeenCalledWith(
        'messages.error_load',
        'Unknown error',
      )
    })
  })

  describe('navigation', () => {
    it('should navigate to create page', () => {
      loadItemsMock.mockResolvedValue([])

      const { handleCreate } = useCrudPage(options)

      handleCreate()

      expect(mockNavigateTo).toHaveBeenCalledWith('/inventory/warehouses/new')
    })

    it('should navigate to view page', () => {
      loadItemsMock.mockResolvedValue([])

      const { handleView } = useCrudPage(options)
      const item = { id: '123', name: 'Test Item' }

      handleView(item)

      expect(mockNavigateTo).toHaveBeenCalledWith('/inventory/warehouses/123')
    })

    it('should navigate to edit page', () => {
      loadItemsMock.mockResolvedValue([])

      const { handleEdit } = useCrudPage(options)
      const item = { id: '456', name: 'Test Item' }

      handleEdit(item)

      expect(mockNavigateTo).toHaveBeenCalledWith('/inventory/warehouses/456/edit')
    })
  })

  describe('delete', () => {
    it('should open delete dialog when confirmDelete is called', () => {
      loadItemsMock.mockResolvedValue([])

      const { confirmDelete, deleteDialog, selectedItem } = useCrudPage(options)
      const item = { id: '1', name: 'Test Item' }

      confirmDelete(item)

      expect(deleteDialog.value).toBe(true)
      expect(selectedItem.value).toEqual(item)
    })

    it('should delete item successfully with name', async () => {
      const mockItems = [{ id: '1', name: 'Test Item' }]
      loadItemsMock.mockResolvedValue(mockItems)
      deleteItemMock.mockResolvedValue(undefined)

      const { confirmDelete, handleDelete, deleteDialog, selectedItem } = useCrudPage(options)
      const item = { id: '1', name: 'Test Warehouse' }

      confirmDelete(item)
      await handleDelete()

      expect(deleteItemMock).toHaveBeenCalledWith('1')
      expect(mockToast.showSuccess).toHaveBeenCalledWith(
        'messages.success_delete',
        'warehouses.deleted_successfully',
      )
      expect(deleteDialog.value).toBe(false)
      expect(selectedItem.value).toBeNull()
      expect(loadItemsMock).toHaveBeenCalledTimes(2) // Initial + after delete
    })

    it('should delete item using custom getItemName', async () => {
      const mockItems = [{ id: '1', name: 'Test Item' }]
      loadItemsMock.mockResolvedValue(mockItems)
      deleteItemMock.mockResolvedValue(undefined)

      const optionsWithCustomName = {
        ...options,
        getItemName: (item: TestItem) => `Custom ${item.name}`,
      }

      const { confirmDelete, handleDelete } = useCrudPage(optionsWithCustomName)
      const item = { id: '1', name: 'Warehouse' }

      confirmDelete(item)
      await handleDelete()

      expect(mockToast.showSuccess).toHaveBeenCalledWith(
        'messages.success_delete',
        'warehouses.deleted_successfully',
      )
    })

    it('should use id as fallback when name is missing', async () => {
      const mockItems = [{ id: '1', name: 'Test' }]
      loadItemsMock.mockResolvedValue(mockItems)
      deleteItemMock.mockResolvedValue(undefined)

      const { confirmDelete, handleDelete } = useCrudPage(options)
      const item = { id: '1', name: '' } as TestItem

      confirmDelete(item)
      await handleDelete()

      expect(deleteItemMock).toHaveBeenCalledWith('1')
    })

    it('should handle delete errors', async () => {
      loadItemsMock.mockResolvedValue([])
      const error = new Error('Delete failed')
      deleteItemMock.mockRejectedValue(error)

      const { confirmDelete, handleDelete, deleteDialog, selectedItem } = useCrudPage(options)
      const item = { id: '1', name: 'Test Item' }

      confirmDelete(item)
      await handleDelete()

      expect(mockToast.showError).toHaveBeenCalledWith(
        'messages.error_delete',
        'Delete failed',
      )
      expect(deleteDialog.value).toBe(false)
      expect(selectedItem.value).toBeNull()
    })

    it('should do nothing when handleDelete is called without selected item', async () => {
      loadItemsMock.mockResolvedValue([])

      const { handleDelete } = useCrudPage(options)

      await handleDelete()

      expect(deleteItemMock).not.toHaveBeenCalled()
      expect(mockToast.showSuccess).not.toHaveBeenCalled()
    })
  })

  describe('getItemId', () => {
    it('should return item id', () => {
      loadItemsMock.mockResolvedValue([])

      const { getItemId } = useCrudPage(options)
      const item = { id: '123', name: 'Test' }

      expect(getItemId(item)).toBe('123')
    })
  })

  describe('state', () => {
    it('should expose reactive items', async () => {
      const mockItems = [{ id: '1', name: 'Item 1' }]
      loadItemsMock.mockResolvedValue(mockItems)

      const { items, loadData } = useCrudPage(options)

      expect(items.value).toEqual([])

      await loadData()

      expect(items.value).toEqual(mockItems)
    })

    it('should expose reactive loading state', async () => {
      loadItemsMock.mockImplementation(() => new Promise(resolve => setTimeout(resolve, 50)))

      const { loading, loadData } = useCrudPage(options)

      // Wait for initial mount load to complete
      await vi.waitFor(() => expect(loading.value).toBe(false))

      const loadPromise = loadData()
      expect(loading.value).toBe(true)

      await loadPromise
      expect(loading.value).toBe(false)
    })

    it('should expose reactive deleteDialog state', () => {
      loadItemsMock.mockResolvedValue([])

      const { deleteDialog, confirmDelete } = useCrudPage(options)
      const item = { id: '1', name: 'Test' }

      expect(deleteDialog.value).toBe(false)

      confirmDelete(item)

      expect(deleteDialog.value).toBe(true)
    })

    it('should expose reactive selectedItem state', () => {
      loadItemsMock.mockResolvedValue([])

      const { selectedItem, confirmDelete } = useCrudPage(options)
      const item = { id: '1', name: 'Test' }

      expect(selectedItem.value).toBeNull()

      confirmDelete(item)

      expect(selectedItem.value).toEqual(item)
    })
  })
})
