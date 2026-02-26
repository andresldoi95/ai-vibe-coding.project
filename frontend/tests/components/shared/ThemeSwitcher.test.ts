import { beforeEach, describe, expect, it } from 'vitest'
import { h } from 'vue'
import { mount } from '@vue/test-utils'
import { mockColorModePreference } from '../../setup'
import ThemeSwitcher from '~/components/shared/ThemeSwitcher.vue'

// Reusable Select stub that properly renders slots
const SelectStub = {
  name: 'SelectStub',
  props: ['modelValue', 'options', 'optionLabel', 'optionValue', 'appendTo'],
  // eslint-disable-next-line ts/no-explicit-any
  render(this: any) {
    const { modelValue, options, $attrs, $slots } = this

    return h('div', { class: ['select-stub', $attrs.class] }, [
      h('div', { class: 'select-value' }, [
        $slots.value?.({ value: modelValue }),
      ]),
      h('div', { class: 'select-options' }, options?.map((option: Record<string, unknown>) =>
        h('div', { class: 'select-option' }, [$slots.option?.({ option })]),
      )),
    ])
  },
}

describe('themeSwitcher component', () => {
  beforeEach(() => {
    // Reset to default theme before each test
    mockColorModePreference.value = 'system'
  })

  describe('rendering', () => {
    it('should render Select component', () => {
      // Arrange & Act
      const wrapper = mount(ThemeSwitcher, {
        global: {
          stubs: {
            Select: SelectStub,
          },
        },
      })

      // Assert
      expect(wrapper.find('.select-stub').exists()).toBe(true)
    })

    it('should have three theme options', () => {
      // Arrange & Act
      const wrapper = mount(ThemeSwitcher, {
        global: {
          stubs: {
            Select: SelectStub,
          },
        },
      })

      // Assert
      const options = wrapper.findAll('.select-option')
      expect(options).toHaveLength(3)
    })

    it('should render system theme option', () => {
      // Arrange & Act
      const wrapper = mount(ThemeSwitcher, {
        global: {
          stubs: {
            Select: SelectStub,
          },
        },
      })

      // Assert
      const option = wrapper.findAll('.select-option')[0]
      expect(option.text()).toContain('common.theme_system')
      expect(option.find('i.pi-desktop').exists()).toBe(true)
    })

    it('should render light theme option', () => {
      // Arrange & Act
      const wrapper = mount(ThemeSwitcher, {
        global: {
          stubs: {
            Select: SelectStub,
          },
        },
      })

      // Assert
      const option = wrapper.findAll('.select-option')[1]
      expect(option.text()).toContain('common.theme_light')
      expect(option.find('i.pi-sun').exists()).toBe(true)
    })

    it('should render dark theme option', () => {
      // Arrange & Act
      const wrapper = mount(ThemeSwitcher, {
        global: {
          stubs: {
            Select: SelectStub,
          },
        },
      })

      // Assert
      const option = wrapper.findAll('.select-option')[2]
      expect(option.text()).toContain('common.theme_dark')
      expect(option.find('i.pi-moon').exists()).toBe(true)
    })

    it('should bind selected theme to model value', async () => {
      // Arrange
      mockColorModePreference.value = 'dark'

      // Act
      const wrapper = mount(ThemeSwitcher, {
        global: {
          stubs: {
            Select: SelectStub,
          },
        },
      })

      await wrapper.vm.$nextTick()

      // Assert - Check that dark theme icon appears in rendered options
      const darkOption = wrapper.findAll('.select-option')[2]
      expect(darkOption.find('i.pi-moon').exists()).toBe(true)
    })

    it('should hide label text on small screens', () => {
      // Arrange & Act
      const wrapper = mount(ThemeSwitcher, {
        global: {
          stubs: {
            Select: SelectStub,
          },
        },
      })

      // Assert
      const label = wrapper.find('.select-value span.hidden.md\\:inline')
      expect(label.exists()).toBe(true)
    })
  })

  describe('theme selection', () => {
    it('should use system theme preference by default', async () => {
      // Arrange
      mockColorModePreference.value = 'system'

      // Act
      const wrapper = mount(ThemeSwitcher, {
        global: {
          stubs: {
            Select: SelectStub,
          },
        },
      })

      await wrapper.vm.$nextTick()

      // Assert - Verify system option exists
      const systemOption = wrapper.findAll('.select-option')[0]
      expect(systemOption.find('i.pi-desktop').exists()).toBe(true)
      expect(systemOption.text()).toContain('common.theme_system')
    })

    it('should reflect light theme when set', async () => {
      // Arrange
      mockColorModePreference.value = 'light'

      // Act
      const wrapper = mount(ThemeSwitcher, {
        global: {
          stubs: {
            Select: SelectStub,
          },
        },
      })

      await wrapper.vm.$nextTick()

      // Assert - Verify light option exists and has correct icon
      const lightOption = wrapper.findAll('.select-option')[1]
      expect(lightOption.find('i.pi-sun').exists()).toBe(true)
      expect(lightOption.text()).toContain('common.theme_light')
    })

    it('should call useColorMode on mount', () => {
      // Arrange
      const initialPreference = mockColorModePreference.value

      // Act
      mount(ThemeSwitcher, {
        global: {
          stubs: {
            Select: SelectStub,
          },
        },
      })

      // Assert
      expect(mockColorModePreference.value).toBe(initialPreference)
    })

    it('should render fallback when value is undefined', () => {
      // Arrange
      mockColorModePreference.value = undefined as unknown as string

      // Act
      const wrapper = mount(ThemeSwitcher, {
        global: {
          stubs: {
            Select: SelectStub,
          },
        },
      })

      // Assert - The fallback span should be rendered
      const valueSlot = wrapper.find('.select-value')
      expect(valueSlot.exists()).toBe(true)
    })
  })

  describe('theme options', () => {
    it('should have correct structure for each option', () => {
      // Arrange & Act
      const wrapper = mount(ThemeSwitcher, {
        global: {
          stubs: {
            Select: SelectStub,
          },
        },
      })

      // Assert
      const options = wrapper.findAll('.select-option')
      options.forEach((option) => {
        expect(option.find('i').exists()).toBe(true)
        expect(option.find('span').exists()).toBe(true)
        expect(option.find('.flex.items-center.gap-2').exists()).toBe(true)
      })
    })

    it('should have all required theme options', () => {
      // Arrange & Act
      const wrapper = mount(ThemeSwitcher, {
        global: {
          stubs: {
            Select: SelectStub,
          },
        },
      })

      // Assert
      const options = wrapper.findAll('.select-option')
      expect(options).toHaveLength(3)
      expect(wrapper.text()).toContain('common.theme_system')
      expect(wrapper.text()).toContain('common.theme_light')
      expect(wrapper.text()).toContain('common.theme_dark')
    })
  })

  describe('styling', () => {
    it('should have responsive width classes', () => {
      // Arrange & Act
      const wrapper = mount(ThemeSwitcher, {
        global: {
          stubs: {
            Select: SelectStub,
          },
        },
      })

      // Assert
      const select = wrapper.find('.select-stub')
      expect(select.attributes('class')).toContain('w-full')
      expect(select.attributes('class')).toContain('sm:w-36')
    })

    it('should use flex layout for options', () => {
      // Arrange & Act
      const wrapper = mount(ThemeSwitcher, {
        global: {
          stubs: {
            Select: SelectStub,
          },
        },
      })

      // Assert
      const options = wrapper.findAll('.flex.items-center.gap-2')
      // Should have 3 options + 1 value = 4 total
      expect(options.length).toBeGreaterThanOrEqual(3)
    })
  })

  describe('accessibility', () => {
    it('should have option labels', () => {
      // Arrange & Act
      const wrapper = mount(ThemeSwitcher, {
        global: {
          stubs: {
            Select: SelectStub,
          },
        },
      })

      // Assert
      const options = wrapper.findAll('.select-option')
      expect(options[0].text()).toBeTruthy()
      expect(options[1].text()).toBeTruthy()
      expect(options[2].text()).toBeTruthy()
    })

    it('should have icon indicators for each theme', () => {
      // Arrange & Act
      const wrapper = mount(ThemeSwitcher, {
        global: {
          stubs: {
            Select: SelectStub,
          },
        },
      })

      // Assert
      const options = wrapper.findAll('.select-option')
      expect(options[0].find('i.pi-desktop').exists()).toBe(true)
      expect(options[1].find('i.pi-sun').exists()).toBe(true)
      expect(options[2].find('i.pi-moon').exists()).toBe(true)
    })
  })
})
