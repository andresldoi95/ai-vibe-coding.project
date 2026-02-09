<script setup lang="ts">
import type { Role } from '~/types/auth'

definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { can } = usePermissions()
const { showSuccess, showError } = useNotification()
const { t } = useI18n()

// Breadcrumbs
const breadcrumbs = [
  { label: t('nav.settings'), url: '/settings' },
  { label: t('roles.title'), url: '/settings/roles' },
]

// State
const roles = ref<Role[]>([])
const rolesLoading = ref(false)
const deleteDialogVisible = ref(false)
const roleToDelete = ref<Role | null>(null)
const deleteLoading = ref(false)

// Fetch roles
async function fetchRoles() {
  rolesLoading.value = true
  try {
    const roleService = useRole()
    roles.value = await roleService.getAllRoles()
  }
  catch {
    showError(t('messages.error_load'))
  }
  finally {
    rolesLoading.value = false
  }
}

// View role
function viewRole(id: string) {
  navigateTo(`/settings/roles/${id}`)
}

// Edit role
function editRole(id: string) {
  navigateTo(`/settings/roles/${id}/edit`)
}

// Confirm delete
function confirmDelete(role: Role) {
  roleToDelete.value = role
  deleteDialogVisible.value = true
}

// Delete role
async function deleteRole() {
  if (!roleToDelete.value)
    return

  deleteLoading.value = true
  try {
    const roleService = useRole()
    await roleService.deleteRole(roleToDelete.value.id)
    showSuccess(t('messages.success_delete'))
    deleteDialogVisible.value = false
    await fetchRoles()
  }
  catch {
    showError(t('messages.error_delete'))
  }
  finally {
    deleteLoading.value = false
  }
}

// Load roles on mount
onMounted(() => {
  fetchRoles()
})
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <PageHeader
      :title="$t('roles.title')"
      :subtitle="$t('roles.subtitle')"
      :breadcrumbs="breadcrumbs"
    >
      <template #actions>
        <Button
          v-if="can.manageRoles()"
          :label="$t('roles.create_role')"
          icon="pi pi-plus"
          @click="navigateTo('/settings/roles/create')"
        />
      </template>
    </PageHeader>

    <!-- Loading State -->
    <LoadingState v-if="rolesLoading" />

    <!-- Empty State -->
    <EmptyState
      v-else-if="!roles.length"
      :title="$t('roles.no_roles')"
      :description="$t('roles.get_started_roles')"
      icon="pi pi-shield"
    >
      <Button
        v-if="can.manageRoles()"
        :label="$t('roles.create_role')"
        icon="pi pi-plus"
        @click="navigateTo('/settings/roles/create')"
      />
    </EmptyState>

    <!-- Data Table -->
    <Card v-else>
      <template #content>
        <DataTable
          :value="roles"
          :rows="10"
          :paginator="roles.length > 10"
          :loading="rolesLoading"
          striped-rows
          responsive-layout="scroll"
        >
          <Column field="name" :header="$t('roles.role_name')" sortable>
            <template #body="{ data }">
              <div class="flex items-center gap-2">
                <span class="font-medium">{{ data.name }}</span>
                <Tag v-if="data.isSystemRole" severity="info" :value="$t('roles.system_role')" />
              </div>
            </template>
          </Column>

          <Column field="description" :header="$t('common.description')" />

          <Column field="priority" :header="$t('roles.priority')" sortable>
            <template #body="{ data }">
              <span class="font-mono">{{ data.priority }}</span>
            </template>
          </Column>

          <Column field="userCount" :header="$t('roles.user_count')" sortable>
            <template #body="{ data }">
              <span class="font-medium">{{ data.userCount }}</span>
            </template>
          </Column>

          <Column field="permissions" :header="$t('roles.permissions')">
            <template #body="{ data }">
              <span class="text-gray-600">{{ data.permissions?.length || 0 }} permissions</span>
            </template>
          </Column>

          <Column :header="$t('common.actions')" style="width: 12rem">
            <template #body="{ data }">
              <DataTableActions
                :show-view="can.viewRoles()"
                :show-edit="can.manageRoles() && !data.isSystemRole"
                :show-delete="can.manageRoles() && !data.isSystemRole"
                @view="viewRole(data.id)"
                @edit="editRole(data.id)"
                @delete="confirmDelete(data)"
              />
            </template>
          </Column>
        </DataTable>
      </template>
    </Card>

    <!-- Delete Confirm Dialog -->
    <Dialog
      v-model:visible="deleteDialogVisible"
      :header="$t('roles.delete_role')"
      :modal="true"
      :style="{ width: '450px' }"
    >
      <div class="space-y-4">
        <p class="text-gray-700">
          {{ $t('roles.confirm_delete_role', { name: roleToDelete?.name }) }}
        </p>
        <Message
          v-if="roleToDelete?.userCount && roleToDelete.userCount > 0"
          severity="warn"
          :closable="false"
        >
          {{ $t('roles.cannot_delete_assigned', { count: roleToDelete.userCount }) }}
        </Message>
      </div>
      <template #footer>
        <Button
          :label="$t('common.cancel')"
          icon="pi pi-times"
          text
          @click="deleteDialogVisible = false"
        />
        <Button
          :label="$t('common.delete')"
          icon="pi pi-trash"
          severity="danger"
          :loading="deleteLoading"
          :disabled="roleToDelete?.userCount && roleToDelete.userCount > 0"
          @click="deleteRole"
        />
      </template>
    </Dialog>
  </div>
</template>
