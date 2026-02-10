/**
 * Composable for loading data with built-in error handling
 * Eliminates repetitive try/catch/finally blocks
 */

import type { Ref } from 'vue'

export interface DataLoaderOptions {
  /**
   * Error message to display on failure
   */
  errorMessage?: string

  /**
   * Whether to show toast notifications
   */
  showToast?: boolean

  /**
   * Callback on success
   */
  onSuccess?: () => void

  /**
   * Callback on error
   */
  onError?: (error: Error) => void
}

export interface DataLoaderState<T> {
  data: Ref<T | null>
  loading: Ref<boolean>
  error: Ref<Error | null>
  load: (loader: () => Promise<T>, options?: DataLoaderOptions) => Promise<void>
  reload: () => Promise<void>
}

export function useDataLoader<T>(): DataLoaderState<T> {
  const { t } = useI18n()
  const toast = useNotification()

  const data = ref<T | null>(null) as Ref<T | null>
  const loading = ref(false)
  const error = ref<Error | null>(null) as Ref<Error | null>

  let lastLoader: (() => Promise<T>) | null = null
  let lastOptions: DataLoaderOptions | undefined

  async function load(
    loader: () => Promise<T>,
    options: DataLoaderOptions = {},
  ): Promise<void> {
    lastLoader = loader
    lastOptions = options

    loading.value = true
    error.value = null

    try {
      data.value = await loader()

      if (options.onSuccess) {
        options.onSuccess()
      }
    }
    catch (err) {
      const errorObj = err instanceof Error ? err : new Error(String(err))
      error.value = errorObj

      if (options.showToast !== false) {
        const message = options.errorMessage || t('messages.error_load')
        toast.showError(message, errorObj.message)
      }

      if (options.onError) {
        options.onError(errorObj)
      }
    }
    finally {
      loading.value = false
    }
  }

  async function reload(): Promise<void> {
    if (lastLoader) {
      await load(lastLoader, lastOptions)
    }
  }

  return {
    data,
    loading,
    error,
    load,
    reload,
  }
}
