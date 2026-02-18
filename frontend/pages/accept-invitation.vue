<script setup lang="ts">
import { useVuelidate } from '@vuelidate/core'
import { helpers, minLength, required, sameAs } from '@vuelidate/validators'
import type { AcceptInvitationData } from '~/types/user'

definePageMeta({
  layout: false, // Public page without layout
})

const { t } = useI18n()
const route = useRoute()
const router = useRouter()
const toast = useToast()
const { acceptInvitation } = useUser()
const authStore = useAuthStore()

const token = computed(() => route.query.token as string)

const loading = ref(true)
const submitting = ref(false)
const tokenError = ref('')
const isNewUser = ref(true)
const invitationData = ref({
  companyName: '',
  email: '',
})

const formData = ref({
  firstName: '',
  lastName: '',
  password: '',
  confirmPassword: '',
})

// Rules only apply for new users
const rules = computed(() => {
  if (!isNewUser.value)
    return {}

  return {
    firstName: {
      required: helpers.withMessage(t('users.validation.first_name_required'), required),
    },
    lastName: {
      required: helpers.withMessage(t('users.validation.last_name_required'), required),
    },
    password: {
      required: helpers.withMessage(t('users.validation.password_required'), required),
      minLength: helpers.withMessage(t('users.validation.password_min_length'), minLength(8)),
    },
    confirmPassword: {
      required: helpers.withMessage(t('users.validation.password_required'), required),
      sameAs: helpers.withMessage(t('users.validation.passwords_must_match'), sameAs(formData.value.password)),
    },
  }
})

const v$ = useVuelidate(rules, formData)

async function validateToken() {
  if (!token.value) {
    tokenError.value = t('users.accept.token_invalid')
    loading.value = false
    return
  }

  try {
    // Here you could make an API call to validate the token and get invitation details
    // For now, we'll assume the token is valid and proceed
    // In a real implementation, you'd fetch:
    // - Is this a new user or existing user?
    // - Company name
    // - User email

    // Simulating token validation
    await new Promise(resolve => setTimeout(resolve, 500))

    // For demo purposes, set isNewUser based on some logic
    // In production, this would come from the API
    isNewUser.value = true
    invitationData.value = {
      companyName: 'Demo Company',
      email: 'user@example.com',
    }

    loading.value = false
  }
  catch (error: unknown) {
    tokenError.value = (error as Error).message || t('users.accept.token_invalid')
    loading.value = false
  }
}

async function handleAccept() {
  // Validate only if new user
  if (isNewUser.value) {
    const isValid = await v$.value.$validate()
    if (!isValid)
      return
  }

  try {
    submitting.value = true

    const acceptData: AcceptInvitationData = {
      invitationToken: token.value,
    }

    if (isNewUser.value) {
      acceptData.name = `${formData.value.firstName} ${formData.value.lastName}`.trim()
      acceptData.password = formData.value.password
    }

    const response = await acceptInvitation(acceptData)

    // Handle response like login flow
    authStore.$patch({
      token: response.accessToken,
      refreshToken: response.refreshToken,
      user: response.user,
    })

    // Set available tenants and select the first one
    const tenantStore = useTenantStore()
    if (response.tenants && response.tenants.length > 0) {
      tenantStore.setAvailableTenants(response.tenants)
      // Auto-select first tenant (this will fetch role and permissions)
      await authStore.selectTenant(response.tenants[0].id)
    }

    toast.add({
      severity: 'success',
      summary: t('common.success'),
      detail: t('users.messages.invitation_accepted'),
      life: 3000,
    })

    // Redirect to home page (dashboard)
    await router.push('/')
  }
  catch (error: unknown) {
    toast.add({
      severity: 'error',
      summary: t('common.error'),
      detail: (error as Error).message || t('users.messages.error_accepting_invitation'),
      life: 5000,
    })
  }
  finally {
    submitting.value = false
  }
}

onMounted(() => {
  validateToken()
})
</script>

<template>
  <div class="min-h-screen flex items-center justify-center bg-surface-50 dark:bg-surface-900 px-4">
    <Card class="w-full max-w-md">
      <template #title>
        <div class="text-center">
          <h1 class="text-2xl font-bold text-dark dark:text-light">
            {{ t('users.accept.title') }}
          </h1>
        </div>
      </template>

      <template #content>
        <!-- Loading State -->
        <div v-if="loading" class="flex flex-col items-center gap-4 py-8">
          <ProgressSpinner style="width: 50px; height: 50px" />
          <p class="text-surface-600 dark:text-surface-400">
            {{ t('common.loading') }}
          </p>
        </div>

        <!-- Invalid/Expired Token -->
        <div v-else-if="tokenError" class="text-center py-8">
          <i class="pi pi-times-circle text-6xl text-red-500 mb-4" />
          <p class="text-lg font-medium text-dark dark:text-light mb-2">
            {{ t('users.accept.token_invalid') }}
          </p>
          <p class="text-surface-600 dark:text-surface-400">
            {{ tokenError }}
          </p>
        </div>

        <!-- Acceptance Form -->
        <div v-else class="space-y-6">
          <!-- Welcome Message -->
          <div class="text-center mb-6">
            <p class="text-lg text-dark dark:text-light">
              {{ t('users.accept.welcome', { companyName: invitationData.companyName || 'the company' }) }}
            </p>
            <p class="text-surface-600 dark:text-surface-400 mt-2">
              {{ isNewUser ? t('users.accept.new_user') : t('users.accept.existing_user') }}
            </p>
          </div>

          <form class="space-y-4" @submit.prevent="handleAccept">
            <!-- New User Fields -->
            <template v-if="isNewUser">
              <!-- First Name -->
              <div class="field">
                <label for="firstName" class="block text-base font-medium mb-2">
                  {{ t('users.accept.first_name') }} <span class="text-red-500">*</span>
                </label>
                <InputText
                  id="firstName"
                  v-model="formData.firstName"
                  class="w-full"
                  :class="{ 'p-invalid': v$.firstName.$error }"
                  @blur="v$.firstName.$touch()"
                />
                <small v-if="v$.firstName.$error" class="p-error">
                  {{ v$.firstName.$errors[0].$message }}
                </small>
              </div>

              <!-- Last Name -->
              <div class="field">
                <label for="lastName" class="block text-base font-medium mb-2">
                  {{ t('users.accept.last_name') }} <span class="text-red-500">*</span>
                </label>
                <InputText
                  id="lastName"
                  v-model="formData.lastName"
                  class="w-full"
                  :class="{ 'p-invalid': v$.lastName.$error }"
                  @blur="v$.lastName.$touch()"
                />
                <small v-if="v$.lastName.$error" class="p-error">
                  {{ v$.lastName.$errors[0].$message }}
                </small>
              </div>

              <!-- Password -->
              <div class="field">
                <label for="password" class="block text-base font-medium mb-2">
                  {{ t('users.accept.password') }} <span class="text-red-500">*</span>
                </label>
                <Password
                  id="password"
                  v-model="formData.password"
                  toggleMask
                  :feedback="true"
                  class="w-full"
                  inputClass="w-full"
                  :class="{ 'p-invalid': v$.password.$error }"
                  @blur="v$.password.$touch()"
                />
                <small v-if="v$.password.$error" class="p-error">
                  {{ v$.password.$errors[0].$message }}
                </small>
              </div>

              <!-- Confirm Password -->
              <div class="field">
                <label for="confirmPassword" class="block text-base font-medium mb-2">
                  {{ t('users.accept.confirm_password') }} <span class="text-red-500">*</span>
                </label>
                <Password
                  id="confirmPassword"
                  v-model="formData.confirmPassword"
                  toggleMask
                  :feedback="false"
                  class="w-full"
                  inputClass="w-full"
                  :class="{ 'p-invalid': v$.confirmPassword.$error }"
                  @blur="v$.confirmPassword.$touch()"
                />
                <small v-if="v$.confirmPassword.$error" class="p-error">
                  {{ v$.confirmPassword.$errors[0].$message }}
                </small>
              </div>
            </template>

            <!-- Submit Button -->
            <Button
              type="submit"
              :label="t('users.accept.accept_invitation')"
              icon="pi pi-check"
              class="w-full"
              :loading="submitting"
            />
          </form>
        </div>
      </template>
    </Card>
  </div>
</template>

<style scoped>
:deep(.p-password) {
  width: 100%;
}

:deep(.p-password input) {
  width: 100%;
}
</style>
