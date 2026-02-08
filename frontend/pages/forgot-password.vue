<script setup lang="ts">
definePageMeta({
  layout: 'auth',
  middleware: 'auth',
})

const { apiFetch } = useApi()
const { t } = useI18n()
const { showSuccess, showError } = useNotification()

const email = ref('')
const loading = ref(false)
const emailSent = ref(false)
const errors = ref<{ email?: string }>({})

function validateEmail() {
  errors.value = {}

  if (!email.value) {
    errors.value.email = t('validation.required', { field: t('auth.email') })
    return false
  }

  const emailRegex = /^[^\s@]+@[^\s@][^\s.@]*\.[^\s@]+$/
  if (!emailRegex.test(email.value)) {
    errors.value.email = t('validation.invalidEmail')
    return false
  }

  return true
}

async function handleSubmit() {
  if (!validateEmail()) {
    return
  }

  loading.value = true

  try {
    await apiFetch('/auth/forgot-password', {
      method: 'POST',
      body: {
        email: email.value,
      },
    })

    emailSent.value = true
    showSuccess(t('auth.forgotPassword.successMessage'))
  }
  catch (error: unknown) {
    const message = error instanceof Error ? error.message : t('common.error.generic')
    showError(message)
  }
  finally {
    loading.value = false
  }
}

// SEO
useHead({
  title: t('auth.forgotPassword.title'),
})
</script>

<template>
  <div class="min-h-screen flex items-center justify-center bg-gray-50 dark:bg-gray-900 py-12 px-4 sm:px-6 lg:px-8">
    <div class="max-w-md w-full space-y-8">
      <!-- Header -->
      <div class="text-center">
        <h2 class="text-3xl font-bold text-gray-900 dark:text-white">
          {{ $t('auth.forgotPassword.title') }}
        </h2>
        <p class="mt-2 text-sm text-gray-600 dark:text-gray-400">
          {{ $t('auth.forgotPassword.subtitle') }}
        </p>
      </div>

      <!-- Success Message -->
      <div v-if="emailSent" class="rounded-md bg-teal-50 dark:bg-teal-900/20 p-4">
        <div class="flex">
          <div class="flex-shrink-0">
            <i class="pi pi-check-circle text-teal-400 text-xl" />
          </div>
          <div class="ml-3">
            <h3 class="text-sm font-medium text-teal-800 dark:text-teal-200">
              {{ $t('auth.forgotPassword.successTitle') }}
            </h3>
            <div class="mt-2 text-sm text-teal-700 dark:text-teal-300">
              <p>{{ $t('auth.forgotPassword.successMessage') }}</p>
            </div>
            <div class="mt-4">
              <NuxtLink
                to="/login"
                class="text-sm font-medium text-teal-600 dark:text-teal-400 hover:text-teal-500"
              >
                {{ $t('auth.forgotPassword.backToLogin') }}
              </NuxtLink>
            </div>
          </div>
        </div>
      </div>

      <!-- Form -->
      <form v-else class="mt-8 space-y-6" @submit.prevent="handleSubmit">
        <div class="rounded-md shadow-sm -space-y-px">
          <div>
            <label for="email" class="sr-only">{{ $t('auth.email') }}</label>
            <InputText
              id="email"
              v-model="email"
              type="email"
              :placeholder="$t('auth.email')"
              class="w-full"
              :class="{ 'p-invalid': errors.email }"
              :disabled="loading"
              autocomplete="email"
            />
            <small v-if="errors.email" class="p-error">{{ errors.email }}</small>
          </div>
        </div>

        <div>
          <Button
            type="submit"
            :label="loading ? $t('common.sending') : $t('auth.forgotPassword.sendResetLink')"
            class="w-full"
            :loading="loading"
            :disabled="loading"
          />
        </div>

        <div class="flex items-center justify-between">
          <div class="text-sm">
            <NuxtLink
              to="/login"
              class="font-medium text-teal-600 dark:text-teal-400 hover:text-teal-500"
            >
              {{ $t('auth.forgotPassword.backToLogin') }}
            </NuxtLink>
          </div>
          <div class="text-sm">
            <NuxtLink
              to="/register"
              class="font-medium text-teal-600 dark:text-teal-400 hover:text-teal-500"
            >
              {{ $t('auth.register.createAccount') }}
            </NuxtLink>
          </div>
        </div>
      </form>
    </div>
  </div>
</template>
