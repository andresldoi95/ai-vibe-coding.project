import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import StatCard from '~/components/shared/StatCard.vue'

// Reusable Card stub that renders slot content and preserves classes
const CardStub = {
  template: '<div :class="$attrs.class"><slot name="content" /></div>',
}

describe('statCard component', () => {
  describe('rendering', () => {
    it('should render title and value', () => {
      // Arrange & Act
      const wrapper = mount(StatCard, {
        props: {
          title: 'Total Sales',
          value: '1,234',
        },
        global: {
          stubs: {
            Card: CardStub,
          },
        },
      })

      // Assert
      expect(wrapper.text()).toContain('Total Sales')
      expect(wrapper.text()).toContain('1,234')
    })

    it('should render numeric value', () => {
      // Arrange & Act
      const wrapper = mount(StatCard, {
        props: {
          title: 'Products',
          value: 42,
        },
        global: {
          stubs: {
            Card: CardStub,
          },
        },
      })

      // Assert
      expect(wrapper.text()).toContain('42')
    })

    it('should render string value', () => {
      // Arrange & Act
      const wrapper = mount(StatCard, {
        props: {
          title: 'Status',
          value: 'Active',
        },
        global: {
          stubs: {
            Card: CardStub,
          },
        },
      })

      // Assert
      expect(wrapper.text()).toContain('Active')
    })

    it('should render icon when provided', () => {
      // Arrange & Act
      const wrapper = mount(StatCard, {
        props: {
          title: 'Revenue',
          value: '$12,345',
          icon: 'pi pi-dollar',
        },
        global: {
          stubs: {
            Card: CardStub,
          },
        },
      })

      // Assert
      const icon = wrapper.find('i.pi-dollar')
      expect(icon.exists()).toBe(true)
    })

    it('should not render icon container when icon not provided', () => {
      // Arrange & Act
      const wrapper = mount(StatCard, {
        props: {
          title: 'Count',
          value: 100,
        },
        global: {
          stubs: {
            Card: CardStub,
          },
        },
      })

      // Assert
      const iconContainer = wrapper.find('.rounded-lg.p-3')
      expect(iconContainer.exists()).toBe(false)
    })
  })

  describe('trend indicator', () => {
    it('should render positive trend', () => {
      // Arrange & Act
      const wrapper = mount(StatCard, {
        props: {
          title: 'Sales',
          value: 1000,
          trend: 15,
          trendLabel: '+15% from last month',
        },
        global: {
          stubs: {
            Card: CardStub,
          },
        },
      })

      // Assert
      expect(wrapper.text()).toContain('+15% from last month')
      const trendIcon = wrapper.find('.pi-arrow-up')
      expect(trendIcon.exists()).toBe(true)
      expect(trendIcon.classes()).toContain('text-green-600')
    })

    it('should render negative trend', () => {
      // Arrange & Act
      const wrapper = mount(StatCard, {
        props: {
          title: 'Orders',
          value: 50,
          trend: -10,
          trendLabel: '-10% from last month',
        },
        global: {
          stubs: {
            Card: CardStub,
          },
        },
      })

      // Assert
      expect(wrapper.text()).toContain('-10% from last month')
      const trendIcon = wrapper.find('.pi-arrow-down')
      expect(trendIcon.exists()).toBe(true)
      expect(trendIcon.classes()).toContain('text-red-600')
    })

    it('should not render trend when not provided', () => {
      // Arrange & Act
      const wrapper = mount(StatCard, {
        props: {
          title: 'Total',
          value: 100,
        },
        global: {
          stubs: {
            Card: CardStub,
          },
        },
      })

      // Assert
      expect(wrapper.find('.pi-arrow-up').exists()).toBe(false)
      expect(wrapper.find('.pi-arrow-down').exists()).toBe(false)
    })

    it('should render trend with zero value', () => {
      // Arrange & Act
      const wrapper = mount(StatCard, {
        props: {
          title: 'Items',
          value: 100,
          trend: 0,
          trendLabel: 'No change',
        },
        global: {
          stubs: {
            Card: CardStub,
          },
        },
      })

      // Assert
      expect(wrapper.text()).toContain('No change')
      const trendIcon = wrapper.find('.pi-arrow-down')
      expect(trendIcon.exists()).toBe(true)
    })
  })

  describe('severity styling', () => {
    it('should apply info severity by default', () => {
      // Arrange & Act
      const wrapper = mount(StatCard, {
        props: {
          title: 'Default',
          value: 100,
          icon: 'pi pi-info',
        },
        global: {
          stubs: {
            Card: CardStub,
          },
        },
      })

      // Assert
      const iconContainer = wrapper.find('.rounded-lg')
      expect(iconContainer.classes()).toContain('bg-teal-100')
      expect(iconContainer.classes()).toContain('text-teal-600')
    })

    it('should apply success severity', () => {
      // Arrange & Act
      const wrapper = mount(StatCard, {
        props: {
          title: 'Success',
          value: 100,
          icon: 'pi pi-check',
          severity: 'success',
        },
        global: {
          stubs: {
            Card: CardStub,
          },
        },
      })

      // Assert
      const iconContainer = wrapper.find('.rounded-lg')
      expect(iconContainer.classes()).toContain('bg-green-100')
      expect(iconContainer.classes()).toContain('text-green-600')
    })

    it('should apply warning severity', () => {
      // Arrange & Act
      const wrapper = mount(StatCard, {
        props: {
          title: 'Warning',
          value: 50,
          icon: 'pi pi-exclamation-triangle',
          severity: 'warning',
        },
        global: {
          stubs: {
            Card: CardStub,
          },
        },
      })

      // Assert
      const iconContainer = wrapper.find('.rounded-lg')
      expect(iconContainer.classes()).toContain('bg-amber-100')
      expect(iconContainer.classes()).toContain('text-amber-600')
    })

    it('should apply danger severity', () => {
      // Arrange & Act
      const wrapper = mount(StatCard, {
        props: {
          title: 'Danger',
          value: 0,
          icon: 'pi pi-times',
          severity: 'danger',
        },
        global: {
          stubs: {
            Card: CardStub,
          },
        },
      })

      // Assert
      const iconContainer = wrapper.find('.rounded-lg')
      expect(iconContainer.classes()).toContain('bg-red-100')
      expect(iconContainer.classes()).toContain('text-red-600')
    })

    it('should apply dark mode classes for success', () => {
      // Arrange & Act
      const wrapper = mount(StatCard, {
        props: {
          title: 'Success',
          value: 100,
          icon: 'pi pi-check',
          severity: 'success',
        },
        global: {
          stubs: {
            Card: CardStub,
          },
        },
      })

      // Assert
      const iconContainer = wrapper.find('.rounded-lg')
      expect(iconContainer.classes()).toContain('dark:bg-green-900')
      expect(iconContainer.classes()).toContain('dark:text-green-400')
    })
  })

  describe('layout', () => {
    it('should have flex layout', () => {
      // Arrange & Act
      const wrapper = mount(StatCard, {
        props: {
          title: 'Test',
          value: 100,
        },
        global: {
          stubs: {
            Card: CardStub,
          },
        },
      })

      // Assert
      const container = wrapper.find('.flex.items-start.justify-between')
      expect(container.exists()).toBe(true)
    })

    it('should render full height card', () => {
      // Arrange & Act
      const wrapper = mount(StatCard, {
        props: {
          title: 'Test',
          value: 100,
        },
        global: {
          stubs: {
            Card: CardStub,
          },
        },
      })

      // Assert
      const card = wrapper.find('div')
      expect(card.classes()).toContain('h-full')
    })
  })

  describe('accessibility', () => {
    it('should have proper text hierarchy', () => {
      // Arrange & Act
      const wrapper = mount(StatCard, {
        props: {
          title: 'Total Revenue',
          value: '$1,234',
        },
        global: {
          stubs: {
            Card: CardStub,
          },
        },
      })

      // Assert
      const title = wrapper.find('.text-sm.font-medium')
      const value = wrapper.find('.text-3xl.font-bold')
      expect(title.exists()).toBe(true)
      expect(value.exists()).toBe(true)
    })

    it('should have dark mode classes for text', () => {
      // Arrange & Act
      const wrapper = mount(StatCard, {
        props: {
          title: 'Count',
          value: 42,
        },
        global: {
          stubs: {
            Card: CardStub,
          },
        },
      })

      // Assert
      const title = wrapper.find('.text-sm')
      const value = wrapper.find('.text-3xl')
      expect(title.classes()).toContain('dark:text-gray-400')
      expect(value.classes()).toContain('dark:text-gray-50')
    })
  })

  describe('combinations', () => {
    it('should render all features together', () => {
      // Arrange & Act
      const wrapper = mount(StatCard, {
        props: {
          title: 'Total Sales',
          value: '$12,345',
          icon: 'pi pi-dollar',
          trend: 15,
          trendLabel: '+15% from last month',
          severity: 'success',
        },
        global: {
          stubs: {
            Card: CardStub,
          },
        },
      })

      // Assert
      expect(wrapper.text()).toContain('Total Sales')
      expect(wrapper.text()).toContain('$12,345')
      expect(wrapper.text()).toContain('+15% from last month')
      expect(wrapper.find('.pi-dollar').exists()).toBe(true)
      expect(wrapper.find('.pi-arrow-up').exists()).toBe(true)
      expect(wrapper.find('.bg-green-100').exists()).toBe(true)
    })
  })
})
