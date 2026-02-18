<script setup lang="ts">
import { useVuelidate } from '@vuelidate/core'
import { email, helpers, maxLength, required } from '@vuelidate/validators'
import type { InviteUserData } from '~/types/user'
import type { Role } from '~/types/role'

definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const router = useRouter()
const toast = useToast()
const { inviteUser } = useUser()
const { getAllRoles } = useRole()

const home = { icon: 'pi pi-home', to: '/' }
const breadcrumbItems = [
  { label: t('nav.settings'), to: '/settings' },
  { label: t('users.title'), to: '/settings/users' },
  { label: t('users.invite.title') },
]

const formData = ref<InviteUserData>({
  email: '',
  roleId: '',
  personalMessage: '',
})

const roles = ref<Role[]>([])
const loadingRoles = ref(false)
const submitting = ref(false)

const rules = computed(() => ({
  email: {
    required: helpers.withMessage(t('users.validation.email_required'), required),
    email: helpers.withMessage(t('users.validation.email_invalid'), email),
  },
  roleId: {
    required: helpers.withMessage(t('users.validation.role_required'), required),
  },
  personalMessage: {
    maxLength: helpers.withMessage(t('users.validation.personal_message_max_length'), maxLength(500)),
  },
}))

const v$ = useVuelidate(rules, formData)

async function loadRoles() {
  try {
    loadingRoles.value = true
    roles.value = await getAllRoles()
  }
  catch (error: unknown) {
    toast.add({
      severity: 'error',
      summary: t('common.error'),
      detail: (error as Error).message || t('roles.messages.load_error'),
      life: 5000,
    })
  }
  finally {
    loadingRoles.value = false
  }
}

async function handleSubmit() {
  const isValid = await v$.value.$validate()
  if (!isValid)
    return

  try {
    submitting.value = true

    const dataToSubmit: InviteUserData = {
      email: formData.value.email,
      roleId: formData.value.roleId,
    }

    if (formData.value.personalMessage?.trim()) {
      dataToSubmit.personalMessage = formData.value.personalMessage
    }

    await inviteUser(dataToSubmit)

    toast.add({
      severity: 'success',
      summary: t('common.success'),
      detail: t('users.messages.invitation_sent'),
      life: 3000,
    })

    router.push('/settings/users')
  }
  catch (error: unknown) {
    toast.add({
      severity: 'error',
      summary: t('common.error'),
      detail: (error as Error).message || t('users.messages.error_sending_invitation'),
      life: 5000,
    })
  }
  finally {
    submitting.value = false
  }
}

function handleCancel() {
  router.push('/settings/users')
}

onMounted(() => {
  loadRoles()
})
</script>

<template>
  <div class="max-w-3xl mx-auto">
    <!-- Header -->
    <div class="mb-6">
      <h1 class="text-dark dark:text-light text-2xl font-semibold mb-1">
        {{ t('users.invite.title') }}
      </h1>
      <Breadcrumb :home="home" :model="breadcrumbItems" />
    </div>

    <!-- Invite Form -->
    <Card>
      <template #content>
        <form class="space-y-6" @submit.prevent="handleSubmit">
          <!-- Email -->
          <div class="field">
            <label for="email" class="block text-base font-medium mb-2">
              {{ t('users.invite.email') }} <span class="text-red-500">*</span>
            </label>
            <InputText
              id="email"
              v-model="formData.email"
              type="email"
              :placeholder="t('users.invite.email_placeholder')"
              class="w-full"
              :class="{ 'p-invalid': v$.email.$error }"
              @blur="v$.email.$touch()"
            />
            <small v-if="v$.email.$error" class="p-error">
              {{ v$.email.$errors[0].$message }}
            </small>
          </div>

          <!-- Role -->
          <div class="field">
            <label for="roleId" class="block text-base font-medium mb-2">
              {{ t('users.invite.role') }} <span class="text-red-500">*</span>
            </label>
            <Dropdown
              id="roleId"
              v-model="formData.roleId"
              :options="roles"
              optionLabel="name"
              optionValue="id"
              :placeholder="t('users.invite.role_placeholder')"
              class="w-full"
              :class="{ 'p-invalid': v$.roleId.$error }"
              :loading="loadingRoles"
              @blur="v$.roleId.$touch()"
            />
            <small v-if="v$.roleId.$error" class="p-error">
              {{ v$.roleId.$errors[0].$message }}
            </small>
          </div>

          <!-- Personal Message -->
          <div class="field">
            <label for="personalMessage" class="block text-base font-medium mb-2">
              {{ t('users.invite.personal_message') }}
            </label>
            <Textarea
              id="personalMessage"
              v-model="formData.personalMessage"
              :placeholder="t('users.invite.personal_message_placeholder')"
              rows="4"
              class="w-full"
              :class="{ 'p-invalid': v$.personalMessage.$error }"
              @blur="v$.personalMessage.$touch()"
            />
            <small v-if="v$.personalMessage.$error" class="p-error">
              {{ v$.personalMessage.$errors[0].$message }}
            </small>
          </div>

          <!-- Actions -->
          <div class="flex justify-end gap-3 pt-4">
            <Button
              :label="t('users.invite.cancel')"
              icon="pi pi-times"
              severity="secondary"
              outlined
              :disabled="submitting"
              @click="handleCancel"
            />
            <Button
              type="submit"
              :label="t('users.invite.send_invitation')"
              icon="pi pi-send"
              :loading="submitting"
            />
          </div>
        </form>
      </template>
    </Card>
  </div>
</template>
