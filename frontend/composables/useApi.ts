export function useApi() {
  const { $apiFetch } = useNuxtApp()

  return {
    apiFetch: $apiFetch,
  }
}
