<script setup lang="ts">
import { useVuelidate } from '@vuelidate/core'
import { email, minLength, required } from '@vuelidate/validators'
import type { LoginCredentials } from '~/types/auth'

definePageMeta({
  layout: 'auth',
  middleware: 'auth',
})

const { t } = useI18n()
const authStore = useAuthStore()
const toast = useNotification()
const router = useRouter()

const formData = reactive<LoginCredentials>({
  email: '',
  password: '',
})

const rules = {
  email: { required, email },
  password: { required, minLength: minLength(6) },
}

const v$ = useVuelidate(rules, formData)
const loading = ref(false)

function emailErrorMessage() {
  if (!v$.value.email.$error)
    return ''
  if (!v$.value.email.required)
    return t('validation.email_required')
  if (!v$.value.email.email)
    return t('validation.email_invalid')
  return t('validation.invalid_field')
}

function passwordErrorMessage() {
  if (!v$.value.password.$error)
    return ''
  if (!v$.value.password.required)
    return t('validation.password_required')
  if (!v$.value.password.minLength)
    return t('validation.password_min_length')
  return t('validation.invalid_field')
}

async function handleLogin() {
  const isValid = await v$.value.$validate()
  if (!isValid) {
    toast.showWarning(t('validation.invalid_form'), t('validation.fix_errors'))
    return
  }

  loading.value = true
  try {
    // eslint-disable-next-line no-console
    console.log('[Login] Attempting login with:', formData.email)
    const response = await authStore.login(formData)
    // eslint-disable-next-line no-console
    console.log('[Login] Login successful, response:', response)
    toast.showSuccess(
      'Login successful',
      `Welcome back, ${response.user.name}!`,
    )
    await router.push('/')
  }
  catch (error) {
    console.error('[Login] Login error:', error)
    const errMessage = error instanceof Error ? error.message : 'Invalid credentials'
    toast.showError('Login failed', errMessage)
  }
  finally {
    loading.value = false
  }
}

async function copyToClipboard(text: string) {
  try {
    await navigator.clipboard.writeText(text)
    toast.showSuccess('Copied!', `${text} copied to clipboard`)
  }
  catch {
    toast.showError('Failed to copy', 'Please copy manually')
  }
}
</script>

<template>
  <div class="relative min-h-screen flex items-center justify-center px-4 py-12">
    <!-- Enhanced Animated Background -->
    <div class="absolute inset-0 -z-10 overflow-hidden">
      <div class="absolute inset-0 bg-gradient-to-br from-white via-slate-50 to-teal-50/30 dark:from-slate-950 dark:via-slate-900 dark:to-slate-900" />

      <!-- Animated gradient orbs -->
      <div class="absolute left-1/2 top-1/4 h-96 w-96 -translate-x-1/2 animate-pulse rounded-full bg-gradient-to-r from-teal-400/20 to-cyan-400/20 blur-3xl dark:from-teal-500/10 dark:to-cyan-500/10" />
      <div class="absolute right-1/4 bottom-1/4 h-64 w-64 animate-pulse rounded-full bg-gradient-to-r from-blue-400/15 to-teal-400/15 blur-3xl delay-1000 dark:from-blue-500/8 dark:to-teal-500/8" />
    </div>

    <div class="w-full max-w-md">
      <!-- Welcome Text Above Card -->
      <div class="mb-8 text-center animate-fade-in">
        <h1 class="text-3xl font-bold text-slate-900 dark:text-white">
          {{ t('auth.welcome_back') }}
        </h1>
        <p class="mt-2 text-slate-600 dark:text-slate-400">
          {{ t('auth.login_subtitle') }}
        </p>
      </div>

      <!-- Main Card with shadow and animation -->
      <Card class="overflow-hidden shadow-2xl shadow-slate-200/50 ring-1 ring-slate-900/5 transition-all duration-300 hover:shadow-2xl hover:shadow-teal-100/50 dark:shadow-none dark:ring-slate-800/50 dark:hover:shadow-teal-950/20">
        <template #header>
          <div class="bg-gradient-to-br from-teal-600 to-cyan-600 px-8 py-10 text-center">
            <!-- Animated Icon -->
            <div class="mx-auto mb-4 flex h-16 w-16 animate-bounce-slow items-center justify-center rounded-2xl bg-white/20 shadow-lg backdrop-blur-sm">
              <i class="pi pi-chart-line text-3xl text-white" />
            </div>

            <h2 class="text-xl font-bold text-white drop-shadow-sm">
              {{ t('app.title') }}
            </h2>
          </div>
        </template>

        <template #content>
          <div class="px-8 py-8">
            <form class="flex flex-col gap-6" @submit.prevent="handleLogin">
              <!-- Email Field with enhanced styling -->
              <div class="flex flex-col gap-2 transition-all duration-200">
                <label for="email" class="text-sm font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('auth.email') }}
                </label>

                <IconField>
                  <InputIcon class="pi pi-envelope" />
                  <InputText
                    id="email"
                    v-model="formData.email"
                    type="email"
                    :placeholder="t('auth.email_placeholder')"
                    :invalid="v$.email.$error"
                    autocomplete="email"
                    class="w-full transition-all duration-200 focus:shadow-md"
                    @blur="v$.email.$touch()"
                  />
                </IconField>

                <Transition name="slide-fade">
                  <small v-if="v$.email.$error" class="flex items-center gap-1.5 text-red-600 dark:text-red-400">
                    <i class="pi pi-exclamation-circle text-xs" />
                    {{ emailErrorMessage() }}
                  </small>
                </Transition>
              </div>

              <!-- Password Field with enhanced styling -->
              <div class="flex flex-col gap-2 transition-all duration-200">
                <div class="flex items-center justify-between">
                  <label for="password" class="text-sm font-semibold text-slate-700 dark:text-slate-200">
                    {{ t('auth.password') }}
                  </label>

                  <NuxtLink
                    to="/forgot-password"
                    class="text-xs font-medium text-teal-600 transition-colors hover:text-teal-700 hover:underline dark:text-teal-400 dark:hover:text-teal-300"
                  >
                    {{ t('auth.forgot_password') }}
                  </NuxtLink>
                </div>

                <IconField class="w-full">
                  <InputIcon class="pi pi-lock" />
                  <Password
                    id="password"
                    v-model="formData.password"
                    :placeholder="t('auth.password_placeholder')"
                    :invalid="v$.password.$error"
                    :feedback="false"
                    autocomplete="current-password"
                    toggleMask
                    class="w-full"
                    inputClass="w-full transition-all duration-200 focus:shadow-md"
                    @blur="v$.password.$touch()"
                  />
                </IconField>

                <Transition name="slide-fade">
                  <small v-if="v$.password.$error" class="flex items-center gap-1.5 text-red-600 dark:text-red-400">
                    <i class="pi pi-exclamation-circle text-xs" />
                    {{ passwordErrorMessage() }}
                  </small>
                </Transition>
              </div>

              <!-- Submit Button with enhanced styling -->
              <Button
                type="submit"
                :label="t('auth.login')"
                :loading="loading"
                icon="pi pi-sign-in"
                iconPos="right"
                class="mt-2 w-full !bg-gradient-to-r !from-teal-600 !to-cyan-600 !text-white shadow-lg transition-all duration-200 hover:!from-teal-700 hover:!to-cyan-700 hover:shadow-xl"
                size="large"
              />

              <!-- Enhanced Demo Credentials Block -->
              <div class="mt-3 overflow-hidden rounded-xl border border-teal-200/60 bg-gradient-to-br from-teal-50 to-cyan-50/50 shadow-sm dark:border-teal-900/30 dark:from-teal-950/20 dark:to-cyan-950/10">
                <div class="border-b border-teal-200/60 bg-teal-100/50 px-4 py-2.5 dark:border-teal-900/30 dark:bg-teal-900/20">
                  <div class="flex items-center gap-2 text-sm font-semibold text-teal-900 dark:text-teal-100">
                    <i class="pi pi-key text-teal-600 dark:text-teal-400" />
                    {{ t('auth.demo_credentials') }}
                  </div>
                </div>

                <div class="space-y-3 p-4">
                  <div class="group flex items-center justify-between rounded-lg bg-white/80 px-4 py-3 shadow-sm ring-1 ring-slate-200/50 transition-all hover:shadow dark:bg-slate-900/40 dark:ring-slate-700/50">
                    <span class="text-xs font-medium uppercase tracking-wide text-slate-500 dark:text-slate-400">{{ t('auth.email') }}</span>
                    <div class="flex items-center gap-2">
                      <code class="rounded bg-slate-100 px-2 py-1 text-sm font-mono text-slate-700 dark:bg-slate-800 dark:text-slate-300">admin@example.com</code>
                      <Button
                        icon="pi pi-copy"
                        text
                        rounded
                        size="small"
                        severity="secondary"
                        class="!h-7 !w-7 opacity-0 transition-opacity group-hover:opacity-100"
                        @click="copyToClipboard('admin@example.com')"
                      />
                    </div>
                  </div>

                  <div class="group flex items-center justify-between rounded-lg bg-white/80 px-4 py-3 shadow-sm ring-1 ring-slate-200/50 transition-all hover:shadow dark:bg-slate-900/40 dark:ring-slate-700/50">
                    <span class="text-xs font-medium uppercase tracking-wide text-slate-500 dark:text-slate-400">{{ t('auth.password') }}</span>
                    <div class="flex items-center gap-2">
                      <code class="rounded bg-slate-100 px-2 py-1 text-sm font-mono text-slate-700 dark:bg-slate-800 dark:text-slate-300">password</code>
                      <Button
                        icon="pi pi-copy"
                        text
                        rounded
                        size="small"
                        severity="secondary"
                        class="!h-7 !w-7 opacity-0 transition-opacity group-hover:opacity-100"
                        @click="copyToClipboard('password')"
                      />
                    </div>
                  </div>
                </div>
              </div>
            </form>
          </div>
        </template>

        <template #footer>
          <div class="bg-slate-50/50 px-8 py-6 dark:bg-slate-900/30">
            <div class="flex items-center justify-center gap-2 text-xs text-slate-500 dark:text-slate-400">
              <i class="pi pi-lock text-teal-600 dark:text-teal-400" />
              <span>{{ t('auth.security_note') }}</span>
            </div>
          </div>
        </template>
      </Card>
    </div>
  </div>
</template>
