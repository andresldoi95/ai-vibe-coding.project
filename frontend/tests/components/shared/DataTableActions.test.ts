import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import DataTableActions from '~/components/shared/DataTableActions.vue'

describe('dataTableActions component', () => {
  const ButtonStub = {
    template: '<button :data-icon="icon" :data-severity="severity"><slot /></button>',
    props: ['icon', 'severity', 'text', 'rounded', 'ariaLabel'],
  }

  describe('rendering', () => {
    it('should render all action buttons by default', () => {
      // Arrange & Act
      const wrapper = mount(DataTableActions, {
        global: {
          stubs: {
            Button: ButtonStub,
          },
        },
      })

      // Assert
      const buttons = wrapper.findAll('button')
      expect(buttons).toHaveLength(3)
    })

    it('should render view button with correct props', () => {
      // Arrange & Act
      const wrapper = mount(DataTableActions, {
        global: {
          stubs: {
            Button: ButtonStub,
          },
        },
      })

      // Assert
      const viewButton = wrapper.findAll('button')[0]
      expect(viewButton.attributes('data-icon')).toBe('pi pi-eye')
      expect(viewButton.attributes('data-severity')).toBe('info')
    })

    it('should render edit button with correct props', () => {
      // Arrange & Act
      const wrapper = mount(DataTableActions, {
        global: {
          stubs: {
            Button: ButtonStub,
          },
        },
      })

      // Assert
      const editButton = wrapper.findAll('button')[1]
      expect(editButton.attributes('data-icon')).toBe('pi pi-pencil')
      expect(editButton.attributes('data-severity')).toBe('warning')
    })

    it('should render delete button with correct props', () => {
      // Arrange & Act
      const wrapper = mount(DataTableActions, {
        global: {
          stubs: {
            Button: ButtonStub,
          },
        },
      })

      // Assert
      const deleteButton = wrapper.findAll('button')[2]
      expect(deleteButton.attributes('data-icon')).toBe('pi pi-trash')
      expect(deleteButton.attributes('data-severity')).toBe('danger')
    })
  })

  describe('conditional rendering', () => {
    it('should hide view button when showView is false', () => {
      // Arrange & Act
      const wrapper = mount(DataTableActions, {
        props: {
          showView: false,
        },
        global: {
          stubs: {
            Button: ButtonStub,
          },
        },
      })

      // Assert
      const buttons = wrapper.findAll('button')
      expect(buttons).toHaveLength(2)
      expect(buttons[0].attributes('data-icon')).toBe('pi pi-pencil')
    })

    it('should hide edit button when showEdit is false', () => {
      // Arrange & Act
      const wrapper = mount(DataTableActions, {
        props: {
          showEdit: false,
        },
        global: {
          stubs: {
            Button: ButtonStub,
          },
        },
      })

      // Assert
      const buttons = wrapper.findAll('button')
      expect(buttons).toHaveLength(2)
      expect(buttons[0].attributes('data-icon')).toBe('pi pi-eye')
      expect(buttons[1].attributes('data-icon')).toBe('pi pi-trash')
    })

    it('should hide delete button when showDelete is false', () => {
      // Arrange & Act
      const wrapper = mount(DataTableActions, {
        props: {
          showDelete: false,
        },
        global: {
          stubs: {
            Button: ButtonStub,
          },
        },
      })

      // Assert
      const buttons = wrapper.findAll('button')
      expect(buttons).toHaveLength(2)
      expect(buttons[0].attributes('data-icon')).toBe('pi pi-eye')
      expect(buttons[1].attributes('data-icon')).toBe('pi pi-pencil')
    })

    it('should hide multiple buttons', () => {
      // Arrange & Act
      const wrapper = mount(DataTableActions, {
        props: {
          showView: false,
          showDelete: false,
        },
        global: {
          stubs: {
            Button: ButtonStub,
          },
        },
      })

      // Assert
      const buttons = wrapper.findAll('button')
      expect(buttons).toHaveLength(1)
      expect(buttons[0].attributes('data-icon')).toBe('pi pi-pencil')
    })

    it('should render empty when all buttons hidden', () => {
      // Arrange & Act
      const wrapper = mount(DataTableActions, {
        props: {
          showView: false,
          showEdit: false,
          showDelete: false,
        },
        global: {
          stubs: {
            Button: ButtonStub,
          },
        },
      })

      // Assert
      const buttons = wrapper.findAll('button')
      expect(buttons).toHaveLength(0)
    })
  })

  describe('events', () => {
    it('should emit view event when view button clicked', async () => {
      // Arrange
      const wrapper = mount(DataTableActions, {
        global: {
          stubs: {
            Button: ButtonStub,
          },
        },
      })

      // Act
      await wrapper.findAll('button')[0].trigger('click')

      // Assert
      expect(wrapper.emitted('view')).toBeTruthy()
      expect(wrapper.emitted('view')).toHaveLength(1)
    })

    it('should emit edit event when edit button clicked', async () => {
      // Arrange
      const wrapper = mount(DataTableActions, {
        global: {
          stubs: {
            Button: ButtonStub,
          },
        },
      })

      // Act
      await wrapper.findAll('button')[1].trigger('click')

      // Assert
      expect(wrapper.emitted('edit')).toBeTruthy()
      expect(wrapper.emitted('edit')).toHaveLength(1)
    })

    it('should emit delete event when delete button clicked', async () => {
      // Arrange
      const wrapper = mount(DataTableActions, {
        global: {
          stubs: {
            Button: ButtonStub,
          },
        },
      })

      // Act
      await wrapper.findAll('button')[2].trigger('click')

      // Assert
      expect(wrapper.emitted('delete')).toBeTruthy()
      expect(wrapper.emitted('delete')).toHaveLength(1)
    })

    it('should emit multiple events independently', async () => {
      // Arrange
      const wrapper = mount(DataTableActions, {
        global: {
          stubs: {
            Button: ButtonStub,
          },
        },
      })

      // Act
      await wrapper.findAll('button')[0].trigger('click')
      await wrapper.findAll('button')[1].trigger('click')
      await wrapper.findAll('button')[2].trigger('click')

      // Assert
      expect(wrapper.emitted('view')).toHaveLength(1)
      expect(wrapper.emitted('edit')).toHaveLength(1)
      expect(wrapper.emitted('delete')).toHaveLength(1)
    })
  })

  describe('styling', () => {
    it('should apply gap between buttons', () => {
      // Arrange & Act
      const wrapper = mount(DataTableActions, {
        global: {
          stubs: {
            Button: ButtonStub,
          },
        },
      })

      // Assert
      const container = wrapper.find('.flex.gap-2')
      expect(container.exists()).toBe(true)
    })

    it('should use flex layout', () => {
      // Arrange & Act
      const wrapper = mount(DataTableActions, {
        global: {
          stubs: {
            Button: ButtonStub,
          },
        },
      })

      // Assert
      const container = wrapper.find('div')
      expect(container.classes()).toContain('flex')
      expect(container.classes()).toContain('gap-2')
    })
  })

  describe('accessibility', () => {
    it('should have aria-label for view button', () => {
      // Arrange & Act
      const wrapper = mount(DataTableActions, {
        global: {
          stubs: {
            Button: {
              template: '<button :aria-label="ariaLabel"><slot /></button>',
              props: ['ariaLabel', 'icon', 'severity', 'text', 'rounded'],
            },
          },
        },
      })

      // Assert
      const viewButton = wrapper.findAll('button')[0]
      expect(viewButton.attributes('aria-label')).toBe('View')
    })

    it('should have aria-label for edit button', () => {
      // Arrange & Act
      const wrapper = mount(DataTableActions, {
        global: {
          stubs: {
            Button: {
              template: '<button :aria-label="ariaLabel"><slot /></button>',
              props: ['ariaLabel', 'icon', 'severity', 'text', 'rounded'],
            },
          },
        },
      })

      // Assert
      const editButton = wrapper.findAll('button')[1]
      expect(editButton.attributes('aria-label')).toBe('Edit')
    })

    it('should have aria-label for delete button', () => {
      // Arrange & Act
      const wrapper = mount(DataTableActions, {
        global: {
          stubs: {
            Button: {
              template: '<button :aria-label="ariaLabel"><slot /></button>',
              props: ['ariaLabel', 'icon', 'severity', 'text', 'rounded'],
            },
          },
        },
      })

      // Assert
      const deleteButton = wrapper.findAll('button')[2]
      expect(deleteButton.attributes('aria-label')).toBe('Delete')
    })
  })

  describe('combinations', () => {
    it('should work with only view and edit', async () => {
      // Arrange & Act
      const wrapper = mount(DataTableActions, {
        props: {
          showView: true,
          showEdit: true,
          showDelete: false,
        },
        global: {
          stubs: {
            Button: ButtonStub,
          },
        },
      })

      // Assert
      const buttons = wrapper.findAll('button')
      expect(buttons).toHaveLength(2)

      // Act
      await buttons[0].trigger('click')
      await buttons[1].trigger('click')

      // Assert
      expect(wrapper.emitted('view')).toBeTruthy()
      expect(wrapper.emitted('edit')).toBeTruthy()
      expect(wrapper.emitted('delete')).toBeFalsy()
    })

    it('should work with only delete', () => {
      // Arrange & Act
      const wrapper = mount(DataTableActions, {
        props: {
          showView: false,
          showEdit: false,
          showDelete: true,
        },
        global: {
          stubs: {
            Button: ButtonStub,
          },
        },
      })

      // Assert
      const buttons = wrapper.findAll('button')
      expect(buttons).toHaveLength(1)
      expect(buttons[0].attributes('data-icon')).toBe('pi pi-trash')
    })
  })
})
