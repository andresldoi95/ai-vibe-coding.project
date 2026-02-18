import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import LoadingState from '~/components/shared/LoadingState.vue'

describe('loadingState component', () => {
  describe('rendering', () => {
    it('should render ProgressSpinner', () => {
      // Arrange & Act
      const wrapper = mount(LoadingState, {
        global: {
          stubs: {
            ProgressSpinner: {
              template: '<div class="progress-spinner"></div>',
            },
          },
        },
      })

      // Assert
      expect(wrapper.find('.progress-spinner').exists()).toBe(true)
    })

    it('should render without message by default', () => {
      // Arrange & Act
      const wrapper = mount(LoadingState, {
        global: {
          stubs: {
            ProgressSpinner: true,
          },
        },
      })

      // Assert
      expect(wrapper.find('p').exists()).toBe(false)
    })

    it('should render message when provided', () => {
      // Arrange & Act
      const wrapper = mount(LoadingState, {
        props: {
          message: 'Loading data...',
        },
        global: {
          stubs: {
            ProgressSpinner: true,
          },
        },
      })

      // Assert
      const message = wrapper.find('p')
      expect(message.exists()).toBe(true)
      expect(message.text()).toBe('Loading data...')
    })

    it('should not render message when not provided', () => {
      // Arrange & Act
      const wrapper = mount(LoadingState, {
        global: {
          stubs: {
            ProgressSpinner: true,
          },
        },
      })

      // Assert
      expect(wrapper.find('p').exists()).toBe(false)
    })
  })

  describe('overlay mode', () => {
    it('should not apply overlay classes by default', () => {
      // Arrange & Act
      const wrapper = mount(LoadingState, {
        global: {
          stubs: {
            ProgressSpinner: true,
          },
        },
      })

      // Assert
      const container = wrapper.find('div')
      expect(container.classes()).not.toContain('absolute')
      expect(container.classes()).not.toContain('inset-0')
      expect(container.classes()).not.toContain('z-10')
    })

    it('should apply overlay classes when overlay is true', () => {
      // Arrange & Act
      const wrapper = mount(LoadingState, {
        props: {
          overlay: true,
        },
        global: {
          stubs: {
            ProgressSpinner: true,
          },
        },
      })

      // Assert
      const container = wrapper.find('div')
      expect(container.classes()).toContain('absolute')
      expect(container.classes()).toContain('inset-0')
      expect(container.classes()).toContain('z-10')
    })

    it('should apply overlay background classes', () => {
      // Arrange & Act
      const wrapper = mount(LoadingState, {
        props: {
          overlay: true,
        },
        global: {
          stubs: {
            ProgressSpinner: true,
          },
        },
      })

      // Assert
      const container = wrapper.find('div')
      const classes = container.classes().join(' ')
      expect(classes).toContain('bg-white')
      expect(classes).toContain('dark:bg-gray-900')
    })
  })

  describe('styling', () => {
    it('should apply correct layout classes', () => {
      // Arrange & Act
      const wrapper = mount(LoadingState, {
        global: {
          stubs: {
            ProgressSpinner: true,
          },
        },
      })

      // Assert
      const container = wrapper.find('div')
      expect(container.classes()).toContain('flex')
      expect(container.classes()).toContain('flex-col')
      expect(container.classes()).toContain('items-center')
      expect(container.classes()).toContain('justify-center')
      expect(container.classes()).toContain('py-12')
    })

    it('should apply dark mode classes to message', () => {
      // Arrange & Act
      const wrapper = mount(LoadingState, {
        props: {
          message: 'Loading...',
        },
        global: {
          stubs: {
            ProgressSpinner: true,
          },
        },
      })

      // Assert
      const message = wrapper.find('p')
      expect(message.classes()).toContain('dark:text-gray-400')
    })

    it('should apply spacing classes to message', () => {
      // Arrange & Act
      const wrapper = mount(LoadingState, {
        props: {
          message: 'Please wait...',
        },
        global: {
          stubs: {
            ProgressSpinner: true,
          },
        },
      })

      // Assert
      const message = wrapper.find('p')
      expect(message.classes()).toContain('mt-4')
      expect(message.classes()).toContain('text-sm')
    })
  })

  describe('combinations', () => {
    it('should render overlay with message', () => {
      // Arrange & Act
      const wrapper = mount(LoadingState, {
        props: {
          overlay: true,
          message: 'Processing...',
        },
        global: {
          stubs: {
            ProgressSpinner: true,
          },
        },
      })

      // Assert
      const container = wrapper.find('div')
      const message = wrapper.find('p')
      expect(container.classes()).toContain('absolute')
      expect(message.exists()).toBe(true)
      expect(message.text()).toBe('Processing...')
    })

    it('should render non-overlay with message', () => {
      // Arrange & Act
      const wrapper = mount(LoadingState, {
        props: {
          overlay: false,
          message: 'Loading data...',
        },
        global: {
          stubs: {
            ProgressSpinner: true,
          },
        },
      })

      // Assert
      const container = wrapper.find('div')
      const message = wrapper.find('p')
      expect(container.classes()).not.toContain('absolute')
      expect(message.exists()).toBe(true)
      expect(message.text()).toBe('Loading data...')
    })
  })

  describe('accessibility', () => {
    it('should have loading indicator', () => {
      // Arrange & Act
      const wrapper = mount(LoadingState, {
        global: {
          stubs: {
            ProgressSpinner: {
              template: '<div role="status" aria-live="polite"></div>',
            },
          },
        },
      })

      // Assert
      expect(wrapper.html()).toContain('role="status"')
    })

    it('should render message as text content', () => {
      // Arrange & Act
      const wrapper = mount(LoadingState, {
        props: {
          message: 'Loading content',
        },
        global: {
          stubs: {
            ProgressSpinner: true,
          },
        },
      })

      // Assert
      expect(wrapper.text()).toContain('Loading content')
    })
  })
})
