export function useTheme() {
  const colorMode = useColorMode()

  const isDark = computed(() => colorMode.value === 'dark')
  const preference = computed(() => colorMode.preference)

  const toggleTheme = () => {
    colorMode.preference = colorMode.value === 'dark' ? 'light' : 'dark'
  }

  const setTheme = (theme: 'light' | 'dark' | 'system') => {
    colorMode.preference = theme
  }

  return { isDark, preference, toggleTheme, setTheme, colorMode }
}
