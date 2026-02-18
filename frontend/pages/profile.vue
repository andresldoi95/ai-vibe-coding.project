<script setup lang="ts">
import { useVuelidate } from '@vuelidate/core'
import { helpers, maxLength, minLength, required, sameAs } from '@vuelidate/validators'
import type { ChangePasswordData, UpdateProfileData } from '~/types/auth'

definePageMeta({
  middleware: ['auth'],
  layout: 'default',
})

const { t } = useI18n()
const uiStore = useUiStore()
const toast = useNotification()
const authStore = useAuthStore()
const { updateCurrentUser, changePassword } = useUser()

const loadingProfile = ref(false)
const loadingPassword = ref(false)

// Profile form data
const profileForm = reactive({
  name: '',
})

// Password form data
const passwordForm = reactive({
  currentPassword: '',
  newPassword: '',
  confirmPassword: '',
})

// Profile validation rules
const profileRules = computed(() => ({
  name: {
    required: helpers.withMessage(t('validation.required'), required),
    maxLength: helpers.withMessage(t('validation.max_length', { max: 256 }), maxLength(256)),
  },
}))

// Password validation rules
const passwordRules = computed(() => ({
  currentPassword: {
    required: helpers.withMessage(t('validation.required'), required),
  },
  newPassword: {
    required: helpers.withMessage(t('validation.required'), required),
    minLength: helpers.withMessage(t('validation.min_length', { min: 8 }), minLength(8)),
    maxLength: helpers.withMessage(t('validation.max_length', { max: 100 }), maxLength(100)),
  },
  confirmPassword: {
    required: helpers.withMessage(t('validation.required'), required),
    sameAsPassword: helpers.withMessage(
      t('profile.passwords_must_match'),
      sameAs(passwordForm.newPassword),
    ),
  },
}))

const v$Profile = useVuelidate(profileRules, profileForm)
const v$Password = useVuelidate(passwordRules, passwordForm)

// Load current user data
function loadUserData() {
  if (authStore.user) {
    profileForm.name = authStore.user.name
  }
}

// Handle profile update
async function handleProfileSubmit() {
  const isValid = await v$Profile.value.$validate()
  if (!isValid) {
    toast.showWarning(t('validation.invalid_form'), t('validation.fix_errors'))
    return
  }

  loadingProfile.value = true
  try {
    const data: UpdateProfileData = {
      name: profileForm.name,
    }
    await updateCurrentUser(data)
    toast.showSuccess(t('messages.success_update'), t('profile.profile_updated'))
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('profile.error_update_profile'), errMessage)
  }
  finally {
    loadingProfile.value = false
  }
}

// Handle password change
async function handlePasswordSubmit() {
  const isValid = await v$Password.value.$validate()
  if (!isValid) {
    toast.showWarning(t('validation.invalid_form'), t('validation.fix_errors'))
    return
  }

  loadingPassword.value = true
  try {
    const data: ChangePasswordData = {
      currentPassword: passwordForm.currentPassword,
      newPassword: passwordForm.newPassword,
      confirmPassword: passwordForm.confirmPassword,
    }
    await changePassword(data)
    toast.showSuccess(t('messages.success_update'), t('profile.password_changed'))

    // Reset password form
    passwordForm.currentPassword = ''
    passwordForm.newPassword = ''
    passwordForm.confirmPassword = ''
    v$Password.value.$reset()
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    
    // Show specific error for invalid current password
    if (errMessage.includes('incorrect') || errMessage.includes('Current password')) {
      toast.showError(t('profile.error_change_password'), t('profile.invalid_current_password'))
    }
    else {
      toast.showError(t('profile.error_change_password'), errMessage)
    }
  }
  finally {
    loadingPassword.value = false
  }
}

onMounted(() => {
  loadUserData()
  uiStore.setBreadcrumbs([
    { label: t('profile.title') },
  ])
})
</script>

<template>
  <div>
    <!-- Page Header -->
    <PageHeader
      :title="t('profile.title')"
      :description="t('profile.description')"
    />

    <div class="flex flex-col gap-6">
      <!-- Personal Information Card -->
      <Card>
        <template #content>
          <form @submit.prevent="handleProfileSubmit">
            <div class="mb-6">
              <h3 class="mb-2 text-lg font-semibold text-slate-900 dark:text-white">
                {{ t('profile.personal_info') }}
              </h3>
              <p class="text-sm text-slate-600 dark:text-slate-400">
                {{ t('profile.personal_info_description') }}
              </p>
            </div>

            <div class="grid grid-cols-1 gap-6 md:grid-cols-2">
              <!-- Name -->
              <div class="flex flex-col gap-2">
                <label for="name" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('profile.name') }} *
                </label>
                <InputText
                  id="name"
                  v-model="profileForm.name"
                  :invalid="v$Profile.name.$error"
                  :placeholder="t('profile.name_placeholder')"
                  @blur="v$Profile.name.$touch()"
                />
                <small v-if="v$Profile.name.$error" class="text-red-600 dark:text-red-400">
                  {{ v$Profile.name.$errors[0].$message }}
                </small>
              </div>

              <!-- Email (read-only) -->
              <div class="flex flex-col gap-2">
                <label for="email" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('profile.email') }}
                </label>
                <InputText
                  id="email"
                  :model-value="authStore.user?.email"
                  disabled
                  :placeholder="t('profile.email_placeholder')"
                />
                <small class="text-slate-500 dark:text-slate-400">
                  {{ t('profile.email_helper') }}
                </small>
              </div>
            </div>

            <!-- Submit Button -->
            <div class="mt-6 flex justify-end gap-3">
              <Button
                type="submit"
                :label="t('profile.save_profile')"
                :loading="loadingProfile"
                :disabled="loadingProfile"
                icon="pi pi-save"
              />
            </div>
          </form>
        </template>
      </Card>

      <!-- Password & Security Card -->
      <Card>
        <template #content>
          <form @submit.prevent="handlePasswordSubmit">
            <div class="mb-6">
              <h3 class="mb-2 text-lg font-semibold text-slate-900 dark:text-white">
                {{ t('profile.security') }}
              </h3>
              <p class="text-sm text-slate-600 dark:text-slate-400">
                {{ t('profile.security_description') }}
              </p>
            </div>

            <div class="grid grid-cols-1 gap-6">
              <!-- Current Password -->
              <div class="flex flex-col gap-2">
                <label for="currentPassword" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('profile.current_password') }} *
                </label>
                <Password
                  id="currentPassword"
                  v-model="passwordForm.currentPassword"
                  :invalid="v$Password.currentPassword.$error"
                  :placeholder="t('profile.current_password_placeholder')"
                  :feedback="false"
                  toggle-mask
                  @blur="v$Password.currentPassword.$touch()"
                />
                <small v-if="v$Password.currentPassword.$error" class="text-red-600 dark:text-red-400">
                  {{ v$Password.currentPassword.$errors[0].$message }}
                </small>
              </div>

              <!-- New Password -->
              <div class="flex flex-col gap-2">
                <label for="newPassword" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('profile.new_password') }} *
                </label>
                <Password
                  id="newPassword"
                  v-model="passwordForm.newPassword"
                  :invalid="v$Password.newPassword.$error"
                  :placeholder="t('profile.new_password_placeholder')"
                  toggle-mask
                  @blur="v$Password.newPassword.$touch()"
                />
                <small v-if="v$Password.newPassword.$error" class="text-red-600 dark:text-red-400">
                  {{ v$Password.newPassword.$errors[0].$message }}
                </small>
              </div>

              <!-- Confirm Password -->
              <div class="flex flex-col gap-2">
                <label for="confirmPassword" class="font-semibold text-slate-700 dark:text-slate-200">
                  {{ t('profile.confirm_password') }} *
                </label>
                <Password
                  id="confirmPassword"
                  v-model="passwordForm.confirmPassword"
                  :invalid="v$Password.confirmPassword.$error"
                  :placeholder="t('profile.confirm_password_placeholder')"
                  :feedback="false"
                  toggle-mask
                  @blur="v$Password.confirmPassword.$touch()"
                />
                <small v-if="v$Password.confirmPassword.$error" class="text-red-600 dark:text-red-400">
                  {{ v$Password.confirmPassword.$errors[0].$message }}
                </small>
              </div>
            </div>

            <!-- Submit Button -->
            <div class="mt-6 flex justify-end gap-3">
              <Button
                type="submit"
                :label="t('profile.change_password')"
                :loading="loadingPassword"
                :disabled="loadingPassword"
                icon="pi pi-lock"
              />
            </div>
          </form>
        </template>
      </Card>
    </div>
  </div>
</template>
