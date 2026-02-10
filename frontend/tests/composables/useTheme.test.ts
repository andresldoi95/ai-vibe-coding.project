import { beforeEach, describe, expect, it, vi } from 'vitest'
import { ref, computed } from 'vue'

// Create mock colorMode object that simulates @vueuse/core useColorMode
const mockColorMode = {
  value: 'light',
  preference: 'light',
}

// Create the global mock for useColorMode that returns the mock object
globalThis.useColorMode = vi.fn(() => mockColorMode)

// Make computed available globally for the composable
globalThis.computed = computed

// Import the composable after setting up the mock
import { useTheme } from '~/composables/useTheme'

describe('useTheme', () => {
  beforeEach(() => {
    // Reset color mode to light before each test
    mockColorMode.value = 'light'
    mockColorMode.preference = 'light'
  })

  describe('isDark computed property', () => {
    it('should return true when color mode value is dark', () => {
      const { isDark } = useTheme()
      
      mockColorMode.value = 'dark'
      
      expect(isDark.value).toBe(true)
    })

    it('should return false when color mode value is light', () => {
      const { isDark } = useTheme()
      
      mockColorMode.value = 'light'
      
      expect(isDark.value).toBe(false)
    })
  })

  describe('preference computed property', () => {
    it('should return the current preference value', () => {
      const { preference } = useTheme()
      
      mockColorMode.preference = 'dark'
      
      expect(preference.value).toBe('dark')
    })

    it('should return system when preference is set to system', () => {
      const { preference } = useTheme()
      
      mockColorMode.preference = 'system'
      
      expect(preference.value).toBe('system')
    })
  })

  describe('toggleTheme function', () => {
    it('should switch from light to dark when called', () => {
      mockColorMode.value = 'light'
      mockColorMode.preference = 'light'
      
      const { toggleTheme } = useTheme()
      toggleTheme()
      
      expect(mockColorMode.preference).toBe('dark')
    })

    it('should switch from dark to light when called', () => {
      mockColorMode.value = 'dark'
      mockColorMode.preference = 'dark'
      
      const { toggleTheme } = useTheme()
      toggleTheme()
      
      expect(mockColorMode.preference).toBe('light')
    })
  })

  describe('setTheme function', () => {
    it('should set theme to light', () => {
      const { setTheme } = useTheme()
      setTheme('light')
      
      expect(mockColorMode.preference).toBe('light')
    })

    it('should set theme to dark', () => {
      const { setTheme } = useTheme()
      setTheme('dark')
      
      expect(mockColorMode.preference).toBe('dark')
    })

    it('should set theme to system', () => {
      const { setTheme } = useTheme()
      setTheme('system')
      
      expect(mockColorMode.preference).toBe('system')
    })
  })
})
