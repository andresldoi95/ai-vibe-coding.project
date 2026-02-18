import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import { mockI18nLocale, mockI18nLocales, mockSetLocale } from '../../setup'
import LanguageSwitcher from '~/components/shared/LanguageSwitcher.vue'

// Reusable Select stub
const SelectStub = {
  template: `
    <div class="select-stub" :class="$attrs.class">
      <div class="select-value">
        <slot name="value" :value="modelValue" />
      </div>
      <div class="select-options">
        <div
          v-for="(option, index) in options"
          :key="index"
          class="select-option"
          :data-value="option.value"
          @click="$emit('update:modelValue', option.value)"
        >
          {{ option.label }}
        </div>
      </div>
    </div>
  `,
  props: ['modelValue', 'options', 'optionLabel', 'optionValue', 'placeholder', 'appendTo'],
}

describe('languageSwitcher component', () => {
  describe('rendering', () => {
    it('should render Select component', () => {
      // Arrange & Act
      const wrapper = mount(LanguageSwitcher, {
        global: {
          stubs: {
            Select: SelectStub,
          },
        },
      })

      // Assert
      expect(wrapper.find('.select-stub').exists()).toBe(true)
    })

    it('should render available locale options', () => {
      // Arrange & Act
      const wrapper = mount(LanguageSwitcher, {
        global: {
          stubs: {
            Select: SelectStub,
          },
        },
      })

      // Assert
      const options = wrapper.findAll('.select-option')
      expect(options).toHaveLength(2)
      expect(options[0].text()).toBe('English (US)')
      expect(options[1].text()).toBe('Español')
    })

    it('should display current locale in value slot', () => {
      // Arrange
      mockI18nLocale.value = 'es-ES'

      // Act
      const wrapper = mount(LanguageSwitcher, {
        global: {
          stubs: {
            Select: SelectStub,
          },
        },
      })

      // Assert
      const value = wrapper.find('.select-value')
      expect(value.text()).toContain('Español')
    })

    it('should render globe icon in value slot', () => {
      // Arrange & Act
      const wrapper = mount(LanguageSwitcher, {
        global: {
          stubs: {
            Select: SelectStub,
          },
        },
      })

      // Assert
      const icon = wrapper.find('.select-value i.pi-globe')
      expect(icon.exists()).toBe(true)
    })

    it('should hide locale name on small screens', () => {
      // Arrange & Act
      const wrapper = mount(LanguageSwitcher, {
        global: {
          stubs: {
            Select: SelectStub,
          },
        },
      })

      // Assert
      const label = wrapper.find('.select-value span.hidden.sm\\:inline')
      expect(label.exists()).toBe(true)
    })
  })

  describe('locale selection', () => {
    it('should initialize with current locale', () => {
      // Arrange
      mockI18nLocale.value = 'en-US'

      // Act
      const wrapper = mount(LanguageSwitcher, {
        global: {
          stubs: {
            Select: SelectStub,
          },
        },
      })

      // Assert
      const value = wrapper.find('.select-value')
      expect(value.text()).toContain('English (US)')
    })

    it('should call setLocale when locale changes', async () => {
      // Arrange
      mockSetLocale.mockClear()
      mockI18nLocale.value = 'en-US'

      const wrapper = mount(LanguageSwitcher, {
        global: {
          stubs: {
            Select: SelectStub,
          },
        },
      })

      // Act
      const spanishOption = wrapper.findAll('.select-option')[1]
      await spanishOption.trigger('click')

      // Assert
      expect(mockSetLocale).toHaveBeenCalledWith('es-ES')
    })

    it('should update locale when Spanish selected', async () => {
      // Arrange
      mockI18nLocale.value = 'en-US'

      const wrapper = mount(LanguageSwitcher, {
        global: {
          stubs: {
            Select: SelectStub,
          },
        },
      })

      // Act
      const spanishOption = wrapper.findAll('.select-option')[1]
      await spanishOption.trigger('click')

      // Assert
      expect(mockSetLocale).toHaveBeenCalledWith('es-ES')
    })
  })

  describe('available locales', () => {
    it('should map locales to label/value pairs', () => {
      // Arrange & Act
      const wrapper = mount(LanguageSwitcher, {
        global: {
          stubs: {
            Select: SelectStub,
          },
        },
      })

      // Assert
      const options = wrapper.findAll('.select-option')
      expect(options[0].attributes('data-value')).toBe('en-US')
      expect(options[1].attributes('data-value')).toBe('es-ES')
    })

    it('should handle dynamic locale list', async () => {
      // Arrange
      const wrapper = mount(LanguageSwitcher, {
        global: {
          stubs: {
            Select: SelectStub,
          },
        },
      })

      // Act - Add a new locale
      mockI18nLocales.value = [
        ...mockI18nLocales.value,
        { code: 'fr-FR', name: 'Français' },
      ]

      await wrapper.vm.$nextTick()

      // Assert
      const options = wrapper.findAll('.select-option')
      expect(options).toHaveLength(3)
      expect(options[2].text()).toBe('Français')
    })

    it('should display correct label for current locale', () => {
      // Arrange
      mockI18nLocale.value = 'es-ES'

      // Act
      const wrapper = mount(LanguageSwitcher, {
        global: {
          stubs: {
            Select: SelectStub,
          },
        },
      })

      // Assert
      const value = wrapper.find('.select-value')
      expect(value.text()).toContain('Español')
    })
  })

  describe('styling', () => {
    it('should have responsive width classes', () => {
      // Arrange & Act
      const wrapper = mount(LanguageSwitcher, {
        global: {
          stubs: {
            Select: SelectStub,
          },
        },
      })

      // Assert
      const select = wrapper.find('.select-stub')
      expect(select.attributes('class')).toContain('w-full')
      expect(select.attributes('class')).toContain('sm:w-40')
    })

    it('should use flex layout for value display', () => {
      // Arrange & Act
      const wrapper = mount(LanguageSwitcher, {
        global: {
          stubs: {
            Select: SelectStub,
          },
        },
      })

      // Assert
      const valueContainer = wrapper.find('.select-value .flex.items-center.gap-2')
      expect(valueContainer.exists()).toBe(true)
    })
  })

  describe('accessibility', () => {
    it('should have descriptive option labels', () => {
      // Arrange & Act
      const wrapper = mount(LanguageSwitcher, {
        global: {
          stubs: {
            Select: SelectStub,
          },
        },
      })

      // Assert
      const options = wrapper.findAll('.select-option')
      expect(options[0].text()).toBe('English (US)')
      expect(options[1].text()).toBe('Español')
    })

    it('should have globe icon for visual indication', () => {
      // Arrange & Act
      const wrapper = mount(LanguageSwitcher, {
        global: {
          stubs: {
            Select: SelectStub,
          },
        },
      })

      // Assert
      const icon = wrapper.find('i.pi-globe')
      expect(icon.exists()).toBe(true)
    })
  })

  describe('integration', () => {
    it('should reflect locale changes in display', async () => {
      // Arrange
      mockI18nLocale.value = 'en-US'

      const wrapper = mount(LanguageSwitcher, {
        global: {
          stubs: {
            Select: SelectStub,
          },
        },
      })

      // Act - Change locale manually
      mockI18nLocale.value = 'es-ES'
      await wrapper.vm.$nextTick()

      // Assert
      const value = wrapper.find('.select-value')
      expect(value.text()).toContain('Español')
    })

    it('should handle missing locale gracefully', () => {
      // Arrange
      mockI18nLocale.value = 'unknown-LOCALE'

      // Act
      const wrapper = mount(LanguageSwitcher, {
        global: {
          stubs: {
            Select: SelectStub,
          },
        },
      })

      // Assert - Should still render without crashing
      expect(wrapper.find('.select-stub').exists()).toBe(true)
    })
  })
})
