<script setup lang="ts">
import { useVuelidate } from '@vuelidate/core'
import { email, helpers, maxLength, minLength, required, sameAs } from '@vuelidate/validators'
import type { RegisterData } from '~/types/auth'

definePageMeta({
  layout: 'auth',
  middleware: 'auth',
})

const { t } = useI18n()
const authStore = useAuthStore()
const toast = useNotification()
const router = useRouter()

const formData = reactive<RegisterData>({
  companyName: '',
  slug: '',
  name: '',
  email: '',
  password: '',
  confirmPassword: '',
})

// Custom validator for slug format
const slugFormat = helpers.regex(/^[a-z0-9-]+$/)

// Custom validator for password strength
const hasUpperCase = helpers.regex(/[A-Z]/)
const hasLowerCase = helpers.regex(/[a-z]/)
const hasNumber = helpers.regex(/\d/)

const rules = computed(() => ({
  companyName: {
    required,
    maxLength: maxLength(256),
  },
  slug: {
    required,
    maxLength: maxLength(100),
    slugFormat: helpers.withMessage(t('validation.slug_invalid'), slugFormat),
  },
  name: {
    required,
    maxLength: maxLength(256),
  },
  email: {
    required,
    email,
    maxLength: maxLength(256),
  },
  password: {
    required,
    minLength: minLength(8),
    hasUpperCase: helpers.withMessage(t('validation.password_uppercase'), hasUpperCase),
    hasLowerCase: helpers.withMessage(t('validation.password_lowercase'), hasLowerCase),
    hasNumber: helpers.withMessage(t('validation.password_number'), hasNumber),
  },
  confirmPassword: {
    required,
    sameAsPassword: helpers.withMessage(t('validation.password_match'), sameAs(computed(() => formData.password))),
  },
}))

const v$ = useVuelidate(rules, formData)
const loading = ref(false)

// Auto-generate slug from company name
watch(() => formData.companyName, (newValue) => {
  if (!formData.slug || formData.slug === generateSlug(formData.companyName)) {
    formData.slug = generateSlug(newValue)
  }
})

function generateSlug(text: string): string {
  return text
    .toLowerCase()
    .replace(/[^a-z0-9\s-]/g, '')
    .replace(/\s+/g, '-')
    .replace(/-+/g, '-')
    .trim()
}

function companyNameErrorMessage() {
  if (!v$.value.companyName.$error)
    return ''
  if (!v$.value.companyName.required)
    return t('validation.company_name_required')
  if (!v$.value.companyName.maxLength)
    return t('validation.company_name_max')
  return t('validation.invalid_field')
}

function slugErrorMessage() {
  if (!v$.value.slug.$error)
    return ''
  if (!v$.value.slug.required)
    return t('validation.slug_required')
  if (!v$.value.slug.slugFormat)
    return t('validation.slug_invalid')
  if (!v$.value.slug.maxLength)
    return t('validation.slug_max')
  return t('validation.invalid_field')
}

function nameErrorMessage() {
  if (!v$.value.name.$error)
    return ''
  if (!v$.value.name.required)
    return t('validation.name_required')
  if (!v$.value.name.maxLength)
    return t('validation.name_max')
  return t('validation.invalid_field')
}

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
  if (!v$.value.password.hasUpperCase)
    return t('validation.password_uppercase')
  if (!v$.value.password.hasLowerCase)
    return t('validation.password_lowercase')
  if (!v$.value.password.hasNumber)
    return t('validation.password_number')
  return t('validation.invalid_field')
}

function confirmPasswordErrorMessage() {
  if (!v$.value.confirmPassword.$error)
    return ''
  if (!v$.value.confirmPassword.required)
    return t('validation.password_required')
  if (!v$.value.confirmPassword.sameAsPassword)
    return t('validation.password_match')
  return t('validation.invalid_field')
}

async function handleRegister() {
  const isValid = await v$.value.$validate()
  if (!isValid) {
    toast.showWarning(t('validation.invalid_form'), t('validation.fix_errors'))
    return
  }

  loading.value = true
  try {
    // eslint-disable-next-line no-console
    console.log('[Register] Attempting registration with:', formData.email)
    const response = await authStore.register(formData)
    // eslint-disable-next-line no-console
    console.log('[Register] Registration successful, response:', response)
    toast.showSuccess(
      'Registration successful',
      `Welcome, ${response.user.name}!`,
    )
    await router.push('/')
  }
  catch (error) {
    console.error('[Register] Registration error:', error)
    const errMessage = error instanceof Error ? error.message : 'Registration failed'
    toast.showError('Registration failed', errMessage)
  }
  finally {
    loading.value = false
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

    <div class="w-full max-w-4xl">
      <!-- Welcome Text Above Card -->
      <div class="mb-8 text-center animate-fade-in">
        <h1 class="text-3xl font-bold text-slate-900 dark:text-white">
          {{ t('auth.get_started') }}
        </h1>
        <p class="mt-2 text-slate-600 dark:text-slate-400">
          {{ t('auth.register_subtitle') }}
        </p>
      </div>

      <!-- Main Card with shadow and animation -->
      <Card class="overflow-hidden shadow-2xl shadow-slate-200/50 ring-1 ring-slate-900/5 transition-all duration-300 hover:shadow-2xl hover:shadow-teal-100/50 dark:shadow-none dark:ring-slate-800/50 dark:hover:shadow-teal-950/20">
        <template #header>
          <div class="bg-gradient-to-br from-teal-600 to-cyan-600 px-8 py-10 text-center">
            <!-- Animated Icon -->
            <div class="mx-auto mb-4 flex h-16 w-16 animate-bounce-slow items-center justify-center rounded-2xl bg-white/20 shadow-lg backdrop-blur-sm">
              <i class="pi pi-building text-3xl text-white" />
            </div>

            <h2 class="text-xl font-bold text-white drop-shadow-sm">
              {{ t('app.title') }}
            </h2>
          </div>
        </template>

        <template #content>
          <div class="px-8 py-10">
            <form class="flex flex-col gap-6" @submit.prevent="handleRegister">
              <!-- Full Width Fields -->
              <div class="grid grid-cols-1 gap-6">
                <!-- Company Name Field -->
                <div class="flex flex-col gap-2 transition-all duration-200">
                  <label for="companyName" class="text-sm font-semibold text-slate-700 dark:text-slate-200">
                    {{ t('auth.company_name') }}
                  </label>

                  <IconField>
                    <InputIcon class="pi pi-building" />
                    <InputText
                      id="companyName"
                      v-model="formData.companyName"
                      type="text"
                      :placeholder="t('auth.company_name_placeholder')"
                      :invalid="v$.companyName.$error"
                      autocomplete="organization"
                      class="w-full transition-all duration-200 focus:shadow-md"
                      @blur="v$.companyName.$touch()"
                    />
                  </IconField>

                  <Transition name="slide-fade">
                    <small v-if="v$.companyName.$error" class="text-red-600 dark:text-red-400">
                      {{ companyNameErrorMessage() }}
                    </small>
                  </Transition>
                </div>

                <!-- Company Slug Field -->
                <div class="flex flex-col gap-2 transition-all duration-200">
                  <label for="slug" class="text-sm font-semibold text-slate-700 dark:text-slate-200">
                    {{ t('auth.company_slug') }}
                  </label>

                  <IconField>
                    <InputIcon class="pi pi-link" />
                    <InputText
                      id="slug"
                      v-model="formData.slug"
                      type="text"
                      :placeholder="t('auth.company_slug_placeholder')"
                      :invalid="v$.slug.$error"
                      class="w-full transition-all duration-200 focus:shadow-md"
                      @blur="v$.slug.$touch()"
                    />
                  </IconField>

                  <Transition name="slide-fade">
                    <small v-if="v$.slug.$error" class="text-red-600 dark:text-red-400">
                      {{ slugErrorMessage() }}
                    </small>
                    <small v-else class="text-slate-500 dark:text-slate-400">
                      {{ t('auth.slug_helper') }}
                    </small>
                  </Transition>
                </div>
              </div>

              <!-- Full Width Name and Email -->
              <div class="grid grid-cols-1 gap-6">
                <!-- Full Name Field -->
                <div class="flex flex-col gap-2 transition-all duration-200">
                  <label for="name" class="text-sm font-semibold text-slate-700 dark:text-slate-200">
                    {{ t('auth.full_name') }}
                  </label>

                  <IconField>
                    <InputIcon class="pi pi-user" />
                    <InputText
                      id="name"
                      v-model="formData.name"
                      type="text"
                      :placeholder="t('auth.full_name_placeholder')"
                      :invalid="v$.name.$error"
                      autocomplete="name"
                      class="w-full transition-all duration-200 focus:shadow-md"
                      @blur="v$.name.$touch()"
                    />
                  </IconField>

                  <Transition name="slide-fade">
                    <small v-if="v$.name.$error" class="text-red-600 dark:text-red-400">
                      {{ nameErrorMessage() }}
                    </small>
                  </Transition>
                </div>

                <!-- Email Field -->
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
                    <small v-if="v$.email.$error" class="text-red-600 dark:text-red-400">
                      {{ emailErrorMessage() }}
                    </small>
                  </Transition>
                </div>
              </div>

              <!-- Password Fields -->
              <div class="grid grid-cols-1 gap-6">
                <!-- Password Field -->
                <div class="flex flex-col gap-2 transition-all duration-200">
                  <label for="password" class="text-sm font-semibold text-slate-700 dark:text-slate-200">
                    {{ t('auth.password') }}
                  </label>

                  <IconField>
                    <InputIcon class="pi pi-lock" />
                    <Password
                      id="password"
                      v-model="formData.password"
                      :placeholder="t('auth.password_placeholder')"
                      :invalid="v$.password.$error"
                      toggle-mask
                      :feedback="false"
                      autocomplete="new-password"
                      input-class="w-full"
                      class="w-full"
                      @blur="v$.password.$touch()"
                    />
                  </IconField>

                  <Transition name="slide-fade">
                    <small v-if="v$.password.$error" class="text-red-600 dark:text-red-400">
                      {{ passwordErrorMessage() }}
                    </small>
                  </Transition>
                </div>

                <!-- Confirm Password Field -->
                <div class="flex flex-col gap-2 transition-all duration-200">
                  <label for="confirmPassword" class="text-sm font-semibold text-slate-700 dark:text-slate-200">
                    {{ t('auth.confirm_password') }}
                  </label>

                  <IconField>
                    <InputIcon class="pi pi-lock" />
                    <Password
                      id="confirmPassword"
                      v-model="formData.confirmPassword"
                      :placeholder="t('auth.confirm_password_placeholder')"
                      :invalid="v$.confirmPassword.$error"
                      toggle-mask
                      :feedback="false"
                      autocomplete="new-password"
                      input-class="w-full"
                      class="w-full"
                      @blur="v$.confirmPassword.$touch()"
                    />
                  </IconField>

                  <Transition name="slide-fade">
                    <small v-if="v$.confirmPassword.$error" class="text-red-600 dark:text-red-400">
                      {{ confirmPasswordErrorMessage() }}
                    </small>
                  </Transition>
                </div>
              </div>

              <!-- Register Button -->
              <Button
                type="submit"
                :label="t('auth.register')"
                :loading="loading"
                icon="pi pi-user-plus"
                :disabled="loading"
                class="bg-gradient-to-r from-teal-600 to-cyan-600 font-semibold shadow-lg transition-all duration-200 hover:from-teal-700 hover:to-cyan-700 hover:shadow-xl"
                size="large"
              />
            </form>
          </div>
        </template>

        <template #footer>
          <div class="border-t border-slate-200 bg-slate-50 px-8 py-6 text-center dark:border-slate-700 dark:bg-slate-800/50">
            <!-- Security Note -->
            <div class="mb-4 flex items-center justify-center gap-2 text-sm text-slate-600 dark:text-slate-400">
              <i class="pi pi-shield text-teal-600 dark:text-teal-400" />
              <span>{{ t('auth.security_note') }}</span>
            </div>

            <!-- Sign In Link -->
            <div class="text-sm text-slate-600 dark:text-slate-400">
              {{ t('auth.already_have_account') }}
              <NuxtLink to="/login" class="font-semibold text-teal-600 transition-colors hover:text-teal-700 dark:text-teal-400 dark:hover:text-teal-300">
                {{ t('auth.sign_in_here') }}
              </NuxtLink>
            </div>
          </div>
        </template>
      </Card>
    </div>
  </div>
</template>

<style scoped>
/* Slide fade transition for error messages */
.slide-fade-enter-active {
  transition: all 0.2s ease-out;
}

.slide-fade-leave-active {
  transition: all 0.15s ease-in;
}

.slide-fade-enter-from {
  transform: translateY(-4px);
  opacity: 0;
}

.slide-fade-leave-to {
  transform: translateY(-4px);
  opacity: 0;
}

/* Animate fade in */
@keyframes fade-in {
  from {
    opacity: 0;
    transform: translateY(-10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.animate-fade-in {
  animation: fade-in 0.6s ease-out;
}

/* Slow bounce for icon */
@keyframes bounce-slow {
  0%, 100% {
    transform: translateY(0);
  }
  50% {
    transform: translateY(-10px);
  }
}

.animate-bounce-slow {
  animation: bounce-slow 3s ease-in-out infinite;
}
</style>
