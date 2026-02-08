<script setup lang="ts">
definePageMeta({
  layout: 'auth',
  middleware: 'auth',
})

const route = useRoute()
const { apiFetch } = useApi()
const { t } = useI18n()
const { showSuccess, showError } = useNotification()

const token = ref(route.query.token as string || '')
const password = ref('')
const confirmPassword = ref('')
const loading = ref(false)
const passwordReset = ref(false)
const invalidToken = ref(false)
const errors = ref<{ password?: string, confirmPassword?: string }>({})

// Check if token is present
onMounted(() => {
  if (!token.value) {
    invalidToken.value = true
  }
})

function validateForm() {
  errors.value = {}

  if (!password.value) {
    errors.value.password = t('validation.required', { field: t('auth.password') })
    return false
  }

  if (password.value.length < 8) {
    errors.value.password = t('validation.minLength', { field: t('auth.password'), min: 8 })
    return false
  }

  // Check password complexity
  const hasUpperCase = /[A-Z]/.test(password.value)
  const hasLowerCase = /[a-z]/.test(password.value)
  const hasNumber = /\d/.test(password.value)
  const hasSpecialChar = /[@$!%*?&#]/.test(password.value)

  if (!hasUpperCase || !hasLowerCase || !hasNumber || !hasSpecialChar) {
    errors.value.password = t('validation.passwordComplexity')
    return false
  }

  if (!confirmPassword.value) {
    errors.value.confirmPassword = t('validation.required', { field: t('auth.confirmPassword') })
    return false
  }

  if (password.value !== confirmPassword.value) {
    errors.value.confirmPassword = t('validation.passwordMismatch')
    return false
  }

  return true
}

async function handleSubmit() {
  if (!validateForm()) {
    return
  }

  loading.value = true

  try {
    await apiFetch('/auth/reset-password', {
      method: 'POST',
      body: {
        token: token.value,
        newPassword: password.value,
        confirmPassword: confirmPassword.value,
      },
    })

    passwordReset.value = true
    showSuccess(t('auth.resetPassword.successMessage'))
  }
  catch (error: unknown) {
    const message = error instanceof Error ? error.message : t('common.error.generic')

    // Check if it's an invalid token error
    if (message.toLowerCase().includes('invalid') || message.toLowerCase().includes('expired')) {
      invalidToken.value = true
    }

    showError(message)
  }
  finally {
    loading.value = false
  }
}

// SEO
useHead({
  title: t('auth.resetPassword.title'),
})
</script>

<template>
  <div class="min-h-screen flex items-center justify-center bg-gray-50 dark:bg-gray-900 py-12 px-4 sm:px-6 lg:px-8">
    <div class="max-w-md w-full space-y-8">
      <!-- Header -->
      <div class="text-center">
        <h2 class="text-3xl font-bold text-gray-900 dark:text-white">
          {{ $t('auth.resetPassword.title') }}
        </h2>
        <p class="mt-2 text-sm text-gray-600 dark:text-gray-400">
          {{ $t('auth.resetPassword.subtitle') }}
        </p>
      </div>

      <!-- Success Message -->
      <div v-if="passwordReset" class="rounded-md bg-teal-50 dark:bg-teal-900/20 p-4">
        <div class="flex">
          <div class="flex-shrink-0">
            <i class="pi pi-check-circle text-teal-400 text-xl" />
          </div>
          <div class="ml-3">
            <h3 class="text-sm font-medium text-teal-800 dark:text-teal-200">
              {{ $t('auth.resetPassword.successTitle') }}
            </h3>
            <div class="mt-2 text-sm text-teal-700 dark:text-teal-300">
              <p>{{ $t('auth.resetPassword.successMessage') }}</p>
            </div>
            <div class="mt-4">
              <NuxtLink
                to="/login"
                class="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-teal-600 hover:bg-teal-700"
              >
                {{ $t('auth.resetPassword.goToLogin') }}
              </NuxtLink>
            </div>
          </div>
        </div>
      </div>

      <!-- Invalid Token Message -->
      <div v-else-if="invalidToken" class="rounded-md bg-red-50 dark:bg-red-900/20 p-4">
        <div class="flex">
          <div class="flex-shrink-0">
            <i class="pi pi-times-circle text-red-400 text-xl" />
          </div>
          <div class="ml-3">
            <h3 class="text-sm font-medium text-red-800 dark:text-red-200">
              {{ $t('auth.resetPassword.invalidTokenTitle') }}
            </h3>
            <div class="mt-2 text-sm text-red-700 dark:text-red-300">
              <p>{{ $t('auth.resetPassword.invalidTokenMessage') }}</p>
            </div>
            <div class="mt-4 space-x-2">
              <NuxtLink
                to="/forgot-password"
                class="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-red-600 hover:bg-red-700"
              >
                {{ $t('auth.resetPassword.requestNewLink') }}
              </NuxtLink>
              <NuxtLink
                to="/login"
                class="inline-flex items-center px-4 py-2 border border-gray-300 dark:border-gray-600 text-sm font-medium rounded-md text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-800 hover:bg-gray-50 dark:hover:bg-gray-700"
              >
                {{ $t('auth.resetPassword.backToLogin') }}
              </NuxtLink>
            </div>
          </div>
        </div>
      </div>

      <!-- Form -->
      <form v-else class="mt-8 space-y-6" @submit.prevent="handleSubmit">
        <div class="space-y-4">
          <div>
            <label for="password" class="block text-sm font-medium text-gray-700 dark:text-gray-300">
              {{ $t('auth.newPassword') }}
            </label>
            <Password
              id="password"
              v-model="password"
              :placeholder="$t('auth.newPassword')"
              class="w-full mt-1"
              :class="{ 'p-invalid': errors.password }"
              :disabled="loading"
              toggle-mask
              :feedback="true"
            >
              <template #footer>
                <p class="mt-2 text-xs text-gray-500 dark:text-gray-400">
                  {{ $t('auth.passwordRequirements') }}
                </p>
              </template>
            </Password>
            <small v-if="errors.password" class="p-error">{{ errors.password }}</small>
          </div>

          <div>
            <label for="confirmPassword" class="block text-sm font-medium text-gray-700 dark:text-gray-300">
              {{ $t('auth.confirmPassword') }}
            </label>
            <Password
              id="confirmPassword"
              v-model="confirmPassword"
              :placeholder="$t('auth.confirmPassword')"
              class="w-full mt-1"
              :class="{ 'p-invalid': errors.confirmPassword }"
              :disabled="loading"
              toggle-mask
              :feedback="false"
            />
            <small v-if="errors.confirmPassword" class="p-error">{{ errors.confirmPassword }}</small>
          </div>
        </div>

        <div>
          <Button
            type="submit"
            :label="loading ? $t('common.processing') : $t('auth.resetPassword.resetButton')"
            class="w-full"
            :loading="loading"
            :disabled="loading"
          />
        </div>

        <div class="text-center">
          <NuxtLink
            to="/login"
            class="font-medium text-sm text-teal-600 dark:text-teal-400 hover:text-teal-500"
          >
            {{ $t('auth.resetPassword.backToLogin') }}
          </NuxtLink>
        </div>
      </form>
    </div>
  </div>
</template>
