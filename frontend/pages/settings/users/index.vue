<script setup lang="ts">
import type { CompanyUser } from '~/types/user'
import type { Role } from '~/types/auth'

definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const router = useRouter()
const toast = useToast()
const { hasPermission } = usePermissions()
const { getAllUsers, updateUserRole, removeUser } = useUser()
const { getAllRoles } = useRole()
const authStore = useAuthStore()

const currentUserId = computed(() => authStore.user?.id)

const home = { icon: 'pi pi-home', to: '/' }
const breadcrumbItems = [
  { label: t('nav.settings'), to: '/settings' },
  { label: t('users.title') },
]

const users = ref<CompanyUser[]>([])
const roles = ref<Role[]>([])
const loading = ref(false)
const removing = ref(false)
const removeDialogVisible = ref(false)
const selectedUser = ref<CompanyUser | null>(null)

function getInitials(name: string) {
  const parts = name.split(' ')
  if (parts.length >= 2) {
    return `${parts[0].charAt(0)}${parts[parts.length - 1].charAt(0)}`.toUpperCase()
  }
  return name.charAt(0).toUpperCase()
}

function formatDate(date: string) {
  return new Date(date).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  })
}

async function loadUsers() {
  try {
    loading.value = true
    users.value = await getAllUsers()

    // Attach roleId to each user for the dropdown
    users.value = users.value.map(user => ({
      ...user,
      roleId: user.role?.id || '',
    }))
  }
  catch (error: unknown) {
    toast.add({
      severity: 'error',
      summary: t('common.error'),
      detail: (error as Error).message || t('users.messages.error_loading_users'),
      life: 5000,
    })
  }
  finally {
    loading.value = false
  }
}

async function loadRoles() {
  try {
    roles.value = await getAllRoles()
  }
  catch (error: unknown) {
    console.error('Error loading roles:', error)
  }
}

async function handleRoleChange(user: CompanyUser & { roleId: string }) {
  try {
    await updateUserRole(user.id, user.roleId)

    // Update the role in the UI
    const role = roles.value.find(r => r.id === user.roleId)
    if (role) {
      user.role = { id: role.id, name: role.name }
    }

    toast.add({
      severity: 'success',
      summary: t('common.success'),
      detail: t('users.messages.role_updated'),
      life: 3000,
    })
  }
  catch (error: unknown) {
    // Revert the change in the UI
    user.roleId = user.role.id

    toast.add({
      severity: 'error',
      summary: t('common.error'),
      detail: (error as Error).message || t('users.messages.error_updating_role'),
      life: 5000,
    })
  }
}

function confirmRemoveUser(user: CompanyUser) {
  selectedUser.value = user
  removeDialogVisible.value = true
}

async function handleRemoveUser() {
  if (!selectedUser.value)
    return

  try {
    removing.value = true
    await removeUser(selectedUser.value.id)

    // Remove from UI
    users.value = users.value.filter(u => u.id !== selectedUser.value!.id)

    toast.add({
      severity: 'success',
      summary: t('common.success'),
      detail: t('users.messages.user_removed'),
      life: 3000,
    })

    removeDialogVisible.value = false
    selectedUser.value = null
  }
  catch (error: unknown) {
    toast.add({
      severity: 'error',
      summary: t('common.error'),
      detail: (error as Error).message || t('users.messages.error_removing_user'),
      life: 5000,
    })
  }
  finally {
    removing.value = false
  }
}

function navigateToInvite() {
  router.push('/settings/users/invite')
}

onMounted(async () => {
  await loadRoles()
  await loadUsers()
})
</script>

<template>
  <div class="max-w-7xl mx-auto">
    <!-- Header -->
    <div class="flex justify-between items-center mb-6">
      <div>
        <h1 class="text-dark dark:text-light text-2xl font-semibold mb-1">
          {{ t('users.list.title') }}
        </h1>
        <Breadcrumb :home="home" :model="breadcrumbItems" />
      </div>
      <Button
        v-if="hasPermission('users.invite')"
        :label="t('users.invite_user')"
        icon="pi pi-plus"
        @click="navigateToInvite"
      />
    </div>

    <!-- Users DataTable -->
    <Card>
      <template #content>
        <DataTable
          :value="users"
          :loading="loading"
          stripedRows
          responsiveLayout="scroll"
          :emptyMessage="t('users.list.no_users')"
          class="p-datatable-sm"
        >
          <Column field="name" :header="t('users.list.name')" sortable>
            <template #body="{ data }">
              <div class="flex items-center gap-2">
                <Avatar
                  :label="getInitials(data.name)"
                  size="normal"
                  shape="circle"
                  class="bg-primary text-white"
                />
                <span class="font-medium">{{ data.name }}</span>
              </div>
            </template>
          </Column>

          <Column field="email" :header="t('users.list.email')" sortable />

          <Column field="role.name" :header="t('users.list.role')" sortable>
            <template #body="{ data }">
              <Dropdown
                v-model="data.roleId"
                :options="roles"
                optionLabel="name"
                optionValue="id"
                :disabled="!hasPermission('users.update')"
                class="w-full"
                @change="() => handleRoleChange(data)"
              />
            </template>
          </Column>

          <Column field="joinedAt" :header="t('users.list.joined_at')" sortable>
            <template #body="{ data }">
              {{ formatDate(data.joinedAt) }}
            </template>
          </Column>

          <Column :header="t('users.list.actions')" style="width: 8rem">
            <template #body="{ data }">
              <Button
                v-if="hasPermission('users.remove')"
                icon="pi pi-trash"
                severity="danger"
                text
                rounded
                :disabled="data.id === currentUserId"
                @click="confirmRemoveUser(data)"
              />
            </template>
          </Column>
        </DataTable>
      </template>
    </Card>

    <!-- Remove User Confirmation Dialog -->
    <Dialog
      v-model:visible="removeDialogVisible"
      :header="t('users.remove.confirm_title')"
      :modal="true"
      :style="{ width: '450px' }"
    >
      <div class="flex items-center gap-4">
        <i class="pi pi-exclamation-triangle text-4xl text-orange-500" />
        <span>{{ t('users.remove.confirm_message', { userName: selectedUser?.name || '' }) }}</span>
      </div>
      <template #footer>
        <Button
          :label="t('users.remove.cancel')"
          icon="pi pi-times"
          text
          @click="removeDialogVisible = false"
        />
        <Button
          :label="t('users.remove.confirm')"
          icon="pi pi-check"
          severity="danger"
          :loading="removing"
          @click="handleRemoveUser"
        />
      </template>
    </Dialog>
  </div>
</template>
