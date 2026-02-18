import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import PageHeader from '~/components/shared/PageHeader.vue'

describe('pageHeader component', () => {
  describe('rendering', () => {
    it('should render title', () => {
      // Arrange & Act
      const wrapper = mount(PageHeader, {
        props: {
          title: 'Products',
        },
      })

      // Assert
      expect(wrapper.text()).toContain('Products')
      const heading = wrapper.find('h1')
      expect(heading.exists()).toBe(true)
      expect(heading.text()).toBe('Products')
    })

    it('should render title and description', () => {
      // Arrange & Act
      const wrapper = mount(PageHeader, {
        props: {
          title: 'Dashboard',
          description: 'Welcome to your dashboard',
        },
      })

      // Assert
      expect(wrapper.text()).toContain('Dashboard')
      expect(wrapper.text()).toContain('Welcome to your dashboard')
    })

    it('should not render description when not provided', () => {
      // Arrange & Act
      const wrapper = mount(PageHeader, {
        props: {
          title: 'Settings',
        },
      })

      // Assert
      expect(wrapper.find('p').exists()).toBe(false)
    })

    it('should render description as paragraph', () => {
      // Arrange & Act
      const wrapper = mount(PageHeader, {
        props: {
          title: 'Users',
          description: 'Manage system users',
        },
      })

      // Assert
      const description = wrapper.find('p')
      expect(description.exists()).toBe(true)
      expect(description.text()).toBe('Manage system users')
    })
  })

  describe('slots', () => {
    it('should render actions slot', () => {
      // Arrange & Act
      const wrapper = mount(PageHeader, {
        props: {
          title: 'Products',
        },
        slots: {
          actions: '<button>Add Product</button>',
        },
      })

      // Assert
      expect(wrapper.html()).toContain('<button>Add Product</button>')
    })

    it('should render multiple action elements in slot', () => {
      // Arrange & Act
      const wrapper = mount(PageHeader, {
        props: {
          title: 'Invoices',
        },
        slots: {
          actions: `
            <button>Export</button>
            <button>Create</button>
          `,
        },
      })

      // Assert
      expect(wrapper.html()).toContain('<button>Export</button>')
      expect(wrapper.html()).toContain('<button>Create</button>')
    })

    it('should render without actions slot', () => {
      // Arrange & Act
      const wrapper = mount(PageHeader, {
        props: {
          title: 'Reports',
        },
      })

      // Assert
      const actionsContainer = wrapper.find('.flex.gap-2')
      expect(actionsContainer.exists()).toBe(true)
      expect(actionsContainer.text()).toBe('')
    })
  })

  describe('styling', () => {
    it('should apply large title styling', () => {
      // Arrange & Act
      const wrapper = mount(PageHeader, {
        props: {
          title: 'Page Title',
        },
      })

      // Assert
      const heading = wrapper.find('h1')
      expect(heading.classes()).toContain('text-3xl')
      expect(heading.classes()).toContain('font-bold')
    })

    it('should apply dark mode classes to title', () => {
      // Arrange & Act
      const wrapper = mount(PageHeader, {
        props: {
          title: 'Title',
        },
      })

      // Assert
      const heading = wrapper.find('h1')
      expect(heading.classes()).toContain('dark:text-gray-50')
    })

    it('should apply dark mode classes to description', () => {
      // Arrange & Act
      const wrapper = mount(PageHeader, {
        props: {
          title: 'Title',
          description: 'Description text',
        },
      })

      // Assert
      const description = wrapper.find('p')
      expect(description.classes()).toContain('dark:text-gray-400')
    })

    it('should apply spacing between title and description', () => {
      // Arrange & Act
      const wrapper = mount(PageHeader, {
        props: {
          title: 'Title',
          description: 'Description',
        },
      })

      // Assert
      const description = wrapper.find('p')
      expect(description.classes()).toContain('mt-2')
    })

    it('should apply layout classes', () => {
      // Arrange & Act
      const wrapper = mount(PageHeader, {
        props: {
          title: 'Title',
        },
      })

      // Assert
      const container = wrapper.find('.flex.justify-between.items-start')
      expect(container.exists()).toBe(true)
    })

    it('should apply bottom margin to header', () => {
      // Arrange & Act
      const wrapper = mount(PageHeader, {
        props: {
          title: 'Title',
        },
      })

      // Assert
      const header = wrapper.find('.mb-6')
      expect(header.exists()).toBe(true)
    })
  })

  describe('accessibility', () => {
    it('should use h1 for main title', () => {
      // Arrange & Act
      const wrapper = mount(PageHeader, {
        props: {
          title: 'Main Page Title',
        },
      })

      // Assert
      const heading = wrapper.find('h1')
      expect(heading.exists()).toBe(true)
      expect(heading.element.tagName).toBe('H1')
    })

    it('should have proper semantic structure', () => {
      // Arrange & Act
      const wrapper = mount(PageHeader, {
        props: {
          title: 'Page Title',
          description: 'Page description',
        },
      })

      // Assert
      const heading = wrapper.find('h1')
      const description = wrapper.find('p')
      expect(heading.exists()).toBe(true)
      expect(description.exists()).toBe(true)
    })
  })

  describe('combinations', () => {
    it('should render title, description, and actions together', () => {
      // Arrange & Act
      const wrapper = mount(PageHeader, {
        props: {
          title: 'Products',
          description: 'Manage your product catalog',
        },
        slots: {
          actions: '<button>Add Product</button>',
        },
      })

      // Assert
      expect(wrapper.text()).toContain('Products')
      expect(wrapper.text()).toContain('Manage your product catalog')
      expect(wrapper.html()).toContain('<button>Add Product</button>')
    })

    it('should render only title and actions', () => {
      // Arrange & Act
      const wrapper = mount(PageHeader, {
        props: {
          title: 'Dashboard',
        },
        slots: {
          actions: '<button>Refresh</button>',
        },
      })

      // Assert
      expect(wrapper.text()).toContain('Dashboard')
      expect(wrapper.html()).toContain('<button>Refresh</button>')
      expect(wrapper.findAll('p')).toHaveLength(0)
    })
  })
})
