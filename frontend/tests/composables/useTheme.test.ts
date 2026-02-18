import { beforeEach, describe, expect, it, vi } from 'vitest'
import { reactive, ref } from '../setup'
import { useTheme } from '~/composables/useTheme'

// Mock useColorMode
const mockColorModeValue = ref('light')
const mockColorModePreference = ref('system')

const mockUseColorMode = vi.fn(() => ({
  value: mockColorModeValue.value,
  preference: mockColorModePreference.value,
}))

// Make useColorMode available globally
globalThis.useColorMode = mockUseColorMode

describe('useTheme', () => {
  beforeEach(() => {
    mockColorModeValue.value = 'light'
    mockColorModePreference.value = 'system'
    mockUseColorMode.mockClear()
  })

  describe('isDark', () => {
    it('should return false when theme is light', () => {
      mockColorModeValue.value = 'light'

      const { isDark } = useTheme()

      expect(isDark.value).toBe(false)
    })

    it('should return true when theme is dark', () => {
      mockColorModeValue.value = 'dark'

      const { isDark } = useTheme()

      expect(isDark.value).toBe(true)
    })

    it('should be a computed property', () => {
      const { isDark } = useTheme()

      expect(isDark).toHaveProperty('value')
    })
  })

  describe('preference', () => {
    it('should return current theme preference', () => {
      mockColorModePreference.value = 'dark'

      const { preference } = useTheme()

      expect(preference.value).toBe('dark')
    })

    it('should return system preference when set', () => {
      mockColorModePreference.value = 'system'

      const { preference } = useTheme()

      expect(preference.value).toBe('system')
    })

    it('should be a computed property', () => {
      const { preference } = useTheme()

      expect(preference).toHaveProperty('value')
    })
  })

  describe('toggleTheme', () => {
    it('should toggle from light to dark', () => {
      const mockColorMode = {
        value: 'light',
        preference: 'light',
      }

      globalThis.useColorMode = vi.fn(() => mockColorMode)

      const { toggleTheme } = useTheme()

      toggleTheme()

      expect(mockColorMode.preference).toBe('dark')
    })

    it('should toggle from dark to light', () => {
      const mockColorMode = {
        value: 'dark',
        preference: 'dark',
      }

      globalThis.useColorMode = vi.fn(() => mockColorMode)

      const { toggleTheme } = useTheme()

      toggleTheme()

      expect(mockColorMode.preference).toBe('light')
    })

    it('should toggle multiple times correctly', () => {
      const mockColorMode = {
        value: 'light',
        preference: 'light',
      }

      globalThis.useColorMode = vi.fn(() => mockColorMode)

      const { toggleTheme } = useTheme()

      toggleTheme()
      expect(mockColorMode.preference).toBe('dark')

      mockColorMode.value = 'dark'
      toggleTheme()
      expect(mockColorMode.preference).toBe('light')

      mockColorMode.value = 'light'
      toggleTheme()
      expect(mockColorMode.preference).toBe('dark')
    })
  })

  describe('setTheme', () => {
    it('should set theme to light', () => {
      const mockColorMode = {
        value: 'dark',
        preference: 'dark',
      }

      globalThis.useColorMode = vi.fn(() => mockColorMode)

      const { setTheme } = useTheme()

      setTheme('light')

      expect(mockColorMode.preference).toBe('light')
    })

    it('should set theme to dark', () => {
      const mockColorMode = {
        value: 'light',
        preference: 'light',
      }

      globalThis.useColorMode = vi.fn(() => mockColorMode)

      const { setTheme } = useTheme()

      setTheme('dark')

      expect(mockColorMode.preference).toBe('dark')
    })

    it('should set theme to system', () => {
      const mockColorMode = {
        value: 'light',
        preference: 'light',
      }

      globalThis.useColorMode = vi.fn(() => mockColorMode)

      const { setTheme } = useTheme()

      setTheme('system')

      expect(mockColorMode.preference).toBe('system')
    })

    it('should override current preference', () => {
      const mockColorMode = {
        value: 'dark',
        preference: 'dark',
      }

      globalThis.useColorMode = vi.fn(() => mockColorMode)

      const { setTheme } = useTheme()

      setTheme('light')
      expect(mockColorMode.preference).toBe('light')

      setTheme('system')
      expect(mockColorMode.preference).toBe('system')
    })
  })

  describe('colorMode', () => {
    it('should expose the colorMode object', () => {
      const mockColorMode = {
        value: 'light',
        preference: 'light',
      }

      globalThis.useColorMode = vi.fn(() => mockColorMode)

      const { colorMode } = useTheme()

      expect(colorMode).toBe(mockColorMode)
    })
  })

  describe('integration', () => {
    it('should work with all methods together', () => {
      const mockColorMode = reactive({
        value: 'light',
        preference: 'system',
      })

      globalThis.useColorMode = vi.fn(() => mockColorMode)

      const { isDark, preference, toggleTheme, setTheme } = useTheme()

      expect(isDark.value).toBe(false)
      expect(preference.value).toBe('system')

      setTheme('dark')
      mockColorMode.value = 'dark' // Simulate color mode value change

      expect(preference.value).toBe('dark')

      toggleTheme()
      mockColorMode.value = 'light' // Simulate color mode value change after toggle

      expect(preference.value).toBe('light')
    })
  })
})
