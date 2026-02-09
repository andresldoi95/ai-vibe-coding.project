<script setup lang="ts">
import type { Permission, Role } from '~/types/auth'

definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const uiStore = useUiStore()
const toast = useNotification()
const route = useRoute()
const router = useRouter()
const { can } = usePermissions()
const { getRoleById, deleteRole } = useRole()

const role = ref<Role | null>(null)
const loading = ref(false)
const deleteDialog = ref(false)

async function loadRole() {
  loading.value = true
  try {
    const id = route.params.id as string
    role.value = await getRoleById(id)
    uiStore.setBreadcrumbs([
      { label: t('nav.settings'), to: '/settings' },
      { label: t('roles.title'), to: '/settings/roles' },
      { label: role.value.name },
    ])
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('messages.error_load'), errMessage)
    router.push('/settings/roles')
  }
  finally {
    loading.value = false
  }
}

function editRole() {
  router.push(`/settings/roles/${route.params.id}/edit`)
}

async function handleDelete() {
  if (!role.value)
    return

  // Prevent deleting system roles
  if (role.value.isSystemRole) {
    toast.showError(t('roles.cannot_delete_system'))
    deleteDialog.value = false
    return
  }

  // Prevent deleting roles with assigned users
  if (role.value.userCount && role.value.userCount > 0) {
    toast.showError(t('roles.cannot_delete_assigned', { count: role.value.userCount }))
    deleteDialog.value = false
    return
  }

  try {
    await deleteRole(role.value.id)
    toast.showSuccess(t('messages.success_delete'), t('roles.delete_role'))
    router.push('/settings/roles')
  }
  catch (error) {
    const errMessage = error instanceof Error ? error.message : 'Unknown error'
    toast.showError(t('messages.error_delete'), errMessage)
  }
  finally {
    deleteDialog.value = false
  }
}

// Group permissions by resource
const groupedPermissions = computed(() => {
  if (!role.value?.permissions)
    return {}

  const grouped: Record<string, Permission[]> = {}
  role.value.permissions.forEach((permission) => {
    if (!grouped[permission.resource]) {
      grouped[permission.resource] = []
    }
    grouped[permission.resource].push(permission)
  })
  return grouped
})

// Get resource display name
function getResourceName(resource: string): string {
  const key = `roles.${resource.toLowerCase()}_permissions`
  const translated = t(key)
  return translated !== key ? translated : resource
}

// Get action display name
function getActionName(action: string): string {
  const key = `roles.${action.toLowerCase()}`
  const translated = t(key)
  return translated !== key ? translated : action
}

onMounted(() => {
  loadRole()
})
</script>

<template>
  <div>
    <LoadingState v-if="loading" :message="t('common.loading')" />

    <div v-else-if="role">
      <!-- Page Header -->
      <PageHeader
        :title="role.name"
        :description="role.description"
      >
        <template #actions>
          <Button
            v-if="can.manageRoles() && !role.isSystemRole"
            :label="t('common.edit')"
            icon="pi pi-pencil"
            @click="editRole"
          />
          <Button
            v-if="can.manageRoles() && !role.isSystemRole"
            :label="t('common.delete')"
            icon="pi pi-trash"
            severity="danger"
            outlined
            @click="deleteDialog = true"
          />
        </template>
      </PageHeader>

      <!-- Details Card -->
      <Card>
        <template #content>
          <div class="grid grid-cols-1 gap-8 lg:grid-cols-2">
            <!-- Basic Information -->
            <div>
              <h3 class="mb-4 text-lg font-semibold text-slate-900 dark:text-white">
                {{ t('common.basic_info') }}
              </h3>
              <div class="space-y-4">
                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('roles.role_name') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ role.name }}
                  </p>
                </div>

                <div v-if="role.description">
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('roles.role_description') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ role.description }}
                  </p>
                </div>

                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('roles.priority') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ role.priority }}
                  </p>
                </div>

                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('common.type') }}
                  </label>
                  <div class="mt-1">
                    <Tag
                      :value="role.isSystemRole ? t('roles.system_role') : t('roles.custom_role')"
                      :severity="role.isSystemRole ? 'info' : 'success'"
                    />
                  </div>
                </div>

                <div>
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('common.status') }}
                  </label>
                  <div class="mt-1">
                    <Tag
                      :value="role.isActive ? t('common.active') : t('common.inactive')"
                      :severity="role.isActive ? 'success' : 'danger'"
                    />
                  </div>
                </div>

                <div v-if="role.userCount !== undefined">
                  <label class="text-sm font-medium text-slate-600 dark:text-slate-400">
                    {{ t('roles.user_count') }}
                  </label>
                  <p class="text-slate-900 dark:text-white">
                    {{ role.userCount }}
                  </p>
                </div>
              </div>
            </div>

            <!-- Permissions -->
            <div class="lg:col-span-2">
              <h3 class="mb-4 text-lg font-semibold text-slate-900 dark:text-white">
                {{ t('roles.permissions') }}
              </h3>

              <div v-if="Object.keys(groupedPermissions).length === 0" class="text-slate-600 dark:text-slate-400">
                {{ t('roles.select_permissions') }}
              </div>

              <div v-else class="space-y-6">
                <div
                  v-for="(permissions, resource) in groupedPermissions"
                  :key="resource"
                >
                  <h4 class="mb-3 text-base font-medium text-slate-700 dark:text-slate-300">
                    {{ getResourceName(resource) }}
                  </h4>
                  <div class="flex flex-wrap gap-2">
                    <Tag
                      v-for="permission in permissions"
                      :key="permission.id"
                      :value="getActionName(permission.action)"
                      severity="secondary"
                      class="px-3 py-1"
                    />
                  </div>
                </div>
              </div>
            </div>
          </div>
        </template>
      </Card>
    </div>

    <!-- Delete Confirmation Dialog -->
    <Dialog
      v-model:visible="deleteDialog"
      :header="t('common.confirm')"
      :modal="true"
      :style="{ width: '450px' }"
    >
      <div class="flex items-center gap-4">
        <i class="pi pi-exclamation-triangle text-3xl text-orange-500" />
        <span v-if="role">
          {{ t('roles.confirm_delete_role', { name: role.name }) }}
        </span>
      </div>
      <template #footer>
        <Button
          :label="t('common.cancel')"
          icon="pi pi-times"
          text
          @click="deleteDialog = false"
        />
        <Button
          :label="t('common.delete')"
          icon="pi pi-trash"
          severity="danger"
          @click="handleDelete"
        />
      </template>
    </Dialog>
  </div>
</template>
