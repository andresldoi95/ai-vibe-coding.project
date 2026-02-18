import { beforeEach, describe, expect, it } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { useUiStore } from '~/stores/ui'

describe('ui store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  describe('initial state', () => {
    it('should have sidebarVisible as false', () => {
      // Arrange & Act
      const store = useUiStore()

      // Assert
      expect(store.sidebarVisible).toBe(false)
    })

    it('should have loading as false', () => {
      // Arrange & Act
      const store = useUiStore()

      // Assert
      expect(store.loading).toBe(false)
    })

    it('should have empty breadcrumbs array', () => {
      // Arrange & Act
      const store = useUiStore()

      // Assert
      expect(store.breadcrumbs).toEqual([])
    })
  })

  describe('toggleSidebar', () => {
    it('should toggle sidebarVisible from false to true', () => {
      // Arrange
      const store = useUiStore()
      expect(store.sidebarVisible).toBe(false)

      // Act
      store.toggleSidebar()

      // Assert
      expect(store.sidebarVisible).toBe(true)
    })

    it('should toggle sidebarVisible from true to false', () => {
      // Arrange
      const store = useUiStore()
      store.sidebarVisible = true

      // Act
      store.toggleSidebar()

      // Assert
      expect(store.sidebarVisible).toBe(false)
    })

    it('should toggle multiple times correctly', () => {
      // Arrange
      const store = useUiStore()

      // Act & Assert
      store.toggleSidebar()
      expect(store.sidebarVisible).toBe(true)
      store.toggleSidebar()
      expect(store.sidebarVisible).toBe(false)
      store.toggleSidebar()
      expect(store.sidebarVisible).toBe(true)
    })
  })

  describe('showSidebar', () => {
    it('should set sidebarVisible to true when false', () => {
      // Arrange
      const store = useUiStore()
      expect(store.sidebarVisible).toBe(false)

      // Act
      store.showSidebar()

      // Assert
      expect(store.sidebarVisible).toBe(true)
    })

    it('should keep sidebarVisible true when already true', () => {
      // Arrange
      const store = useUiStore()
      store.sidebarVisible = true

      // Act
      store.showSidebar()

      // Assert
      expect(store.sidebarVisible).toBe(true)
    })
  })

  describe('hideSidebar', () => {
    it('should set sidebarVisible to false when true', () => {
      // Arrange
      const store = useUiStore()
      store.sidebarVisible = true

      // Act
      store.hideSidebar()

      // Assert
      expect(store.sidebarVisible).toBe(false)
    })

    it('should keep sidebarVisible false when already false', () => {
      // Arrange
      const store = useUiStore()
      expect(store.sidebarVisible).toBe(false)

      // Act
      store.hideSidebar()

      // Assert
      expect(store.sidebarVisible).toBe(false)
    })
  })

  describe('setLoading', () => {
    it('should set loading to true', () => {
      // Arrange
      const store = useUiStore()
      expect(store.loading).toBe(false)

      // Act
      store.setLoading(true)

      // Assert
      expect(store.loading).toBe(true)
    })

    it('should set loading to false', () => {
      // Arrange
      const store = useUiStore()
      store.loading = true

      // Act
      store.setLoading(false)

      // Assert
      expect(store.loading).toBe(false)
    })

    it('should allow multiple loading state changes', () => {
      // Arrange
      const store = useUiStore()

      // Act & Assert
      store.setLoading(true)
      expect(store.loading).toBe(true)
      store.setLoading(false)
      expect(store.loading).toBe(false)
      store.setLoading(true)
      expect(store.loading).toBe(true)
    })
  })

  describe('setBreadcrumbs', () => {
    it('should set breadcrumbs with single item', () => {
      // Arrange
      const store = useUiStore()
      const breadcrumbs = [{ label: 'Home' }]

      // Act
      store.setBreadcrumbs(breadcrumbs)

      // Assert
      expect(store.breadcrumbs).toEqual(breadcrumbs)
    })

    it('should set breadcrumbs with multiple items', () => {
      // Arrange
      const store = useUiStore()
      const breadcrumbs = [
        { label: 'Home', to: '/' },
        { label: 'Products', to: '/products' },
        { label: 'Edit Product' },
      ]

      // Act
      store.setBreadcrumbs(breadcrumbs)

      // Assert
      expect(store.breadcrumbs).toEqual(breadcrumbs)
    })

    it('should replace existing breadcrumbs', () => {
      // Arrange
      const store = useUiStore()
      store.setBreadcrumbs([{ label: 'Old' }])

      const newBreadcrumbs = [{ label: 'New', to: '/new' }]

      // Act
      store.setBreadcrumbs(newBreadcrumbs)

      // Assert
      expect(store.breadcrumbs).toEqual(newBreadcrumbs)
    })

    it('should set empty breadcrumbs array', () => {
      // Arrange
      const store = useUiStore()
      store.setBreadcrumbs([{ label: 'Home' }])

      // Act
      store.setBreadcrumbs([])

      // Assert
      expect(store.breadcrumbs).toEqual([])
    })

    it('should handle breadcrumbs without "to" property', () => {
      // Arrange
      const store = useUiStore()
      const breadcrumbs = [
        { label: 'Home' },
        { label: 'Products' },
      ]

      // Act
      store.setBreadcrumbs(breadcrumbs)

      // Assert
      expect(store.breadcrumbs).toEqual(breadcrumbs)
    })
  })

  describe('state independence', () => {
    it('should not affect other state when changing sidebarVisible', () => {
      // Arrange
      const store = useUiStore()
      store.setLoading(true)
      store.setBreadcrumbs([{ label: 'Test' }])

      // Act
      store.toggleSidebar()

      // Assert
      expect(store.loading).toBe(true)
      expect(store.breadcrumbs).toEqual([{ label: 'Test' }])
    })

    it('should not affect other state when changing loading', () => {
      // Arrange
      const store = useUiStore()
      store.showSidebar()
      store.setBreadcrumbs([{ label: 'Test' }])

      // Act
      store.setLoading(true)

      // Assert
      expect(store.sidebarVisible).toBe(true)
      expect(store.breadcrumbs).toEqual([{ label: 'Test' }])
    })

    it('should not affect other state when changing breadcrumbs', () => {
      // Arrange
      const store = useUiStore()
      store.showSidebar()
      store.setLoading(true)

      // Act
      store.setBreadcrumbs([{ label: 'New' }])

      // Assert
      expect(store.sidebarVisible).toBe(true)
      expect(store.loading).toBe(true)
    })
  })
})
