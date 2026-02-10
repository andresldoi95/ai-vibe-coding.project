<script setup lang="ts">
import type { Role } from '~/types/auth'

definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { can } = usePermissions()
const { t } = useI18n()

// Load roles function
async function loadRoles() {
  const roleService = useRole()
  return await roleService.getAllRoles()
}

// Delete role function
async function deleteRoleById(id: string) {
  const roleService = useRole()
  await roleService.deleteRole(id)
}

// ✅ Using useCrudPage composable
const {
  items: roles,
  loading: rolesLoading,
  deleteDialog: deleteDialogVisible,
  selectedItem: roleToDelete,
  handleView: viewRole,
  handleEdit: editRole,
  confirmDelete,
  handleDelete: deleteRole,
} = useCrudPage({
  resourceName: 'roles',
  parentRoute: 'settings',
  basePath: '/settings/roles',
  loadItems: loadRoles,
  deleteItem: deleteRoleById,
  getItemName: (role) => role.name,
})

// Breadcrumbs
const breadcrumbs = [
  { label: t('nav.settings'), url: '/settings' },
  { label: t('roles.title'), url: '/settings/roles' },
]
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

    <!-- Roles Table -->
    <Card>
      <template #content>
        <LoadingState v-if="rolesLoading" :message="$t('common.loading')" />

        <DataTable
          v-else
          :value="roles"
          paginator
          :rows="10"
          :rows-per-page-options="[10, 25, 50]"
          striped-rows
          responsive-layout="scroll"
        >
          <template #empty>
            <EmptyState
              icon="pi pi-shield"
              :title="$t('common.no_data')"
              :description="$t('roles.empty_description')"
              :action-label="$t('roles.create_role')"
              action-icon="pi pi-plus"
              @action="navigateTo('/settings/roles/create')"
            />
          </template>

          <Column
            field="name"
            :header="$t('roles.name')"
            sortable
          >
            <template #body="{ data }">
              <div class="flex items-center gap-2">
                <i class="pi pi-shield text-teal-600" />
                <span class="font-semibold">{{ data.name }}</span>
              </div>
            </template>
          </Column>

          <Column
            field="description"
            :header="$t('roles.description')"
          >
            <template #body="{ data }">
              <span class="text-slate-600 dark:text-slate-400">
                {{ data.description || '—' }}
              </span>
            </template>
          </Column>

          <Column
            field="permissionsCount"
            :header="$t('roles.permissions')"
            sortable
          >
            <template #body="{ data }">
              <Badge
                :value="data.permissions?.length || 0"
                severity="info"
              />
            </template>
          </Column>

          <Column
            field="usersCount"
            :header="$t('roles.users')"
            sortable
          >
            <template #body="{ data }">
              <Badge
                :value="data.usersCount || 0"
                severity="secondary"
              />
            </template>
          </Column>

          <Column :header="$t('common.actions')">
            <template #body="{ data }">
              <div class="flex gap-2">
                <Button
                  icon="pi pi-eye"
                  text
                  rounded
                  severity="info"
                  :aria-label="$t('common.view')"
                  @click="viewRole(data.id)"
                />
                <Button
                  v-if="can.manageRoles()"
                  icon="pi pi-pencil"
                  text
                  rounded
                  severity="warning"
                  :aria-label="$t('common.edit')"
                  @click="editRole(data.id)"
                />
                <Button
                  v-if="can.manageRoles() && !data.isSystem"
                  icon="pi pi-trash"
                  text
                  rounded
                  severity="danger"
                  :aria-label="$t('common.delete')"
                  @click="confirmDelete(data)"
                />
              </div>
            </template>
          </Column>
        </DataTable>
      </template>
    </Card>

    <!-- ✅ Using DeleteConfirmDialog component -->
    <DeleteConfirmDialog
      v-model:visible="deleteDialogVisible"
      :item-name="roleToDelete?.name"
      :title="$t('roles.delete_title')"
      :message="$t('roles.delete_confirm')"
      @confirm="deleteRole"
    />
  </div>
</template>
