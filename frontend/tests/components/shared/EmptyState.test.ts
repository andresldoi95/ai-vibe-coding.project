import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import EmptyState from '~/components/shared/EmptyState.vue'

describe('emptyState component', () => {
  describe('rendering', () => {
    it('should render title', () => {
      // Arrange & Act
      const wrapper = mount(EmptyState, {
        props: {
          title: 'No data found',
        },
      })

      // Assert
      expect(wrapper.text()).toContain('No data found')
      expect(wrapper.find('h3').text()).toBe('No data found')
    })

    it('should render title and description', () => {
      // Arrange & Act
      const wrapper = mount(EmptyState, {
        props: {
          title: 'No items',
          description: 'Add your first item to get started',
        },
      })

      // Assert
      expect(wrapper.text()).toContain('No items')
      expect(wrapper.text()).toContain('Add your first item to get started')
    })

    it('should render icon when provided', () => {
      // Arrange & Act
      const wrapper = mount(EmptyState, {
        props: {
          title: 'Empty',
          icon: 'pi pi-inbox',
        },
      })

      // Assert
      const icon = wrapper.find('i')
      expect(icon.exists()).toBe(true)
      expect(icon.classes()).toContain('pi')
      expect(icon.classes()).toContain('pi-inbox')
    })

    it('should not render icon when not provided', () => {
      // Arrange & Act
      const wrapper = mount(EmptyState, {
        props: {
          title: 'Empty',
        },
      })

      // Assert
      expect(wrapper.find('i').exists()).toBe(false)
    })

    it('should render action button when actionLabel provided', () => {
      // Arrange & Act
      const wrapper = mount(EmptyState, {
        props: {
          title: 'No products',
          actionLabel: 'Add Product',
        },
        global: {
          stubs: {
            Button: {
              template: '<button><slot /></button>',
              props: ['label', 'icon'],
            },
          },
        },
      })

      // Assert
      expect(wrapper.find('button').exists()).toBe(true)
    })

    it('should not render action button when actionLabel not provided', () => {
      // Arrange & Act
      const wrapper = mount(EmptyState, {
        props: {
          title: 'Empty',
        },
        global: {
          stubs: {
            Button: {
              template: '<button><slot /></button>',
            },
          },
        },
      })

      // Assert
      expect(wrapper.find('button').exists()).toBe(false)
    })

    it('should render button with icon when actionIcon provided', () => {
      // Arrange & Act
      const wrapper = mount(EmptyState, {
        props: {
          title: 'No data',
          actionLabel: 'Create',
          actionIcon: 'pi pi-plus',
        },
        global: {
          stubs: {
            Button: {
              template: '<button :data-icon="icon"><slot /></button>',
              props: ['label', 'icon'],
            },
          },
        },
      })

      // Assert
      const button = wrapper.find('button')
      expect(button.exists()).toBe(true)
      expect(button.attributes('data-icon')).toBe('pi pi-plus')
    })
  })

  describe('events', () => {
    it('should emit action event when button clicked', async () => {
      // Arrange
      const wrapper = mount(EmptyState, {
        props: {
          title: 'Empty',
          actionLabel: 'Add Item',
        },
        global: {
          stubs: {
            Button: {
              template: '<button @click="$attrs.onClick"><slot /></button>',
              props: ['label', 'icon'],
            },
          },
        },
      })

      // Act
      await wrapper.find('button').trigger('click')

      // Assert
      expect(wrapper.emitted('action')).toBeTruthy()
      expect(wrapper.emitted('action')).toHaveLength(1)
    })
  })

  describe('styling', () => {
    it('should apply correct layout classes', () => {
      // Arrange & Act
      const wrapper = mount(EmptyState, {
        props: {
          title: 'Empty',
        },
      })

      // Assert
      const container = wrapper.find('div')
      expect(container.classes()).toContain('flex')
      expect(container.classes()).toContain('flex-col')
      expect(container.classes()).toContain('items-center')
      expect(container.classes()).toContain('justify-center')
    })

    it('should apply dark mode classes to icon', () => {
      // Arrange & Act
      const wrapper = mount(EmptyState, {
        props: {
          title: 'Empty',
          icon: 'pi pi-inbox',
        },
      })

      // Assert
      const icon = wrapper.find('i')
      expect(icon.classes()).toContain('dark:text-gray-600')
    })

    it('should apply dark mode classes to title', () => {
      // Arrange & Act
      const wrapper = mount(EmptyState, {
        props: {
          title: 'No items',
        },
      })

      // Assert
      const title = wrapper.find('h3')
      expect(title.classes()).toContain('dark:text-gray-50')
    })

    it('should apply dark mode classes to description', () => {
      // Arrange & Act
      const wrapper = mount(EmptyState, {
        props: {
          title: 'Empty',
          description: 'No items available',
        },
      })

      // Assert
      const description = wrapper.find('p')
      expect(description.classes()).toContain('dark:text-gray-400')
    })
  })

  describe('accessibility', () => {
    it('should have proper heading hierarchy', () => {
      // Arrange & Act
      const wrapper = mount(EmptyState, {
        props: {
          title: 'No results',
        },
      })

      // Assert
      const heading = wrapper.find('h3')
      expect(heading.exists()).toBe(true)
      expect(heading.element.tagName).toBe('H3')
    })

    it('should have descriptive text structure', () => {
      // Arrange & Act
      const wrapper = mount(EmptyState, {
        props: {
          title: 'Empty',
          description: 'Description text',
        },
      })

      // Assert
      const description = wrapper.find('p')
      expect(description.exists()).toBe(true)
      expect(description.text()).toBe('Description text')
    })
  })
})
