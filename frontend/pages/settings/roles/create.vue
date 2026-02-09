<script setup lang="ts">
import type { Permission, RoleFormData } from '~/types/auth'

definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const route = useRoute()
const { t } = useI18n()
const { showSuccess, showError } = useNotification()

// State
const isEditMode = computed(() => !!route.params.id)
const roleId = computed(() => route.params.id as string)
const isSystemRole = ref(false)

const breadcrumbs = computed(() => [
  { label: t('nav.settings'), url: '/settings' },
  { label: t('roles.title'), url: '/settings/roles' },
  { label: isEditMode.value ? t('roles.edit_role') : t('roles.create_role'), url: route.path },
])

// Form data
const formData = ref<RoleFormData>({
  name: '',
  description: '',
  priority: 15,
  permissionIds: [],
})

const errors = ref<Partial<Record<keyof RoleFormData, string>>>({})
const loading = ref(false)
const saving = ref(false)
const permissionsLoading = ref(false)
const allPermissions = ref<Permission[]>([])

// Grouped permissions by resource
const groupedPermissions = computed(() => {
  return allPermissions.value.reduce((groups, permission) => {
    const resource = permission.resource
    if (!groups[resource]) {
      groups[resource] = []
    }
    groups[resource].push(permission)
    return groups
  }, {} as Record<string, Permission[]>)
})

// Fetch role data (edit mode)
async function fetchRole() {
  if (!isEditMode.value)
    return

  loading.value = true
  try {
    const roleService = useRole()
    const role = await roleService.getRoleById(roleId.value)
    formData.value = {
      name: role.name,
      description: role.description || '',
      priority: role.priority,
      permissionIds: role.permissions?.map(p => p.id) || [],
    }
    isSystemRole.value = role.isSystemRole || false
  }
  catch {
    showError(t('messages.error_load'))
    navigateTo('/settings/roles')
  }
  finally {
    loading.value = false
  }
}

// Fetch all permissions
async function fetchPermissions() {
  permissionsLoading.value = true
  try {
    const roleService = useRole()
    allPermissions.value = await roleService.getAllPermissions()
  }
  catch {
    showError(t('messages.error_load'))
  }
  finally {
    permissionsLoading.value = false
  }
}

// Select all permissions
function selectAllPermissions() {
  formData.value.permissionIds = allPermissions.value.map(p => p.id)
}

// Deselect all permissions
function deselectAllPermissions() {
  formData.value.permissionIds = []
}

// Get resource label for translation
function getResourceLabel(resource: string): string {
  const key = `roles.${resource}_permissions`
  const translated = t(key)
  return translated !== key ? translated : resource
}

// Get action label for translation
function getActionLabel(action: string): string {
  const key = `roles.${action}`
  const translated = t(key)
  return translated !== key ? translated : action
}

// Validate form
function validateForm(): boolean {
  errors.value = {}

  if (!formData.value.name.trim()) {
    errors.value.name = 'Role name is required'
  }

  if (formData.value.priority < 1 || formData.value.priority > 100) {
    errors.value.priority = 'Priority must be between 1 and 100'
  }

  if (formData.value.permissionIds.length === 0) {
    errors.value.permissionIds = 'Select at least one permission'
  }

  return Object.keys(errors.value).length === 0
}

// Handle submit
async function handleSubmit() {
  if (!validateForm())
    return

  saving.value = true
  try {
    const roleService = useRole()

    if (isEditMode.value) {
      await roleService.updateRole(roleId.value, formData.value)
      showSuccess(t('messages.success_update'))
    }
    else {
      await roleService.createRole(formData.value)
      showSuccess(t('messages.success_create'))
    }

    navigateTo('/settings/roles')
  }
  catch (error) {
    const message = error?.data?.message || t('messages.error_save')
    showError(message)
  }
  finally {
    saving.value = false
  }
}

// Load data on mount
onMounted(async () => {
  await fetchPermissions()
  if (isEditMode.value) {
    await fetchRole()
  }
})
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <PageHeader
      :title="isEditMode ? $t('roles.edit_role') : $t('roles.create_role')"
      :subtitle="$t('roles.subtitle')"
      :breadcrumbs="breadcrumbs"
    />

    <!-- Loading State -->
    <LoadingState v-if="loading" />

    <!-- Form -->
    <Card v-else>
      <template #content>
        <form class="space-y-6" @submit.prevent="handleSubmit">
          <!-- Role Name -->
          <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div class="space-y-2">
              <label for="name" class="font-medium">
                {{ $t('roles.role_name') }} <span class="text-red-500">*</span>
              </label>
              <InputText
                id="name"
                v-model="formData.name"
                :placeholder="$t('roles.role_name')"
                :invalid="!!errors.name"
                :disabled="isSystemRole"
                class="w-full"
                required
              />
              <small v-if="errors.name" class="text-red-500">{{ errors.name }}</small>
            </div>

            <!-- Priority -->
            <div class="space-y-2">
              <label for="priority" class="font-medium">
                {{ $t('roles.priority') }} <span class="text-red-500">*</span>
              </label>
              <InputNumber
                id="priority"
                v-model="formData.priority"
                :placeholder="$t('roles.priority')"
                :invalid="!!errors.priority"
                :disabled="isSystemRole"
                :min="1"
                :max="100"
                class="w-full"
                required
              />
              <small class="text-gray-600">{{ $t('roles.priority_hint') }}</small>
              <small v-if="errors.priority" class="text-red-500 block">{{ errors.priority }}</small>
            </div>
          </div>

          <!-- Description -->
          <div class="space-y-2">
            <label for="description" class="font-medium">
              {{ $t('common.description') }}
            </label>
            <Textarea
              id="description"
              v-model="formData.description"
              :placeholder="$t('common.description')"
              rows="3"
              class="w-full"
            />
          </div>

          <!-- Permissions -->
          <div class="space-y-4">
            <div class="flex items-center justify-between">
              <h3 class="text-lg font-medium">
                {{ $t('roles.permissions') }}
              </h3>
              <div class="flex gap-2">
                <Button
                  type="button"
                  :label="$t('common.select_all')"
                  size="small"
                  text
                  @click="selectAllPermissions"
                />
                <Button
                  type="button"
                  :label="$t('common.deselect_all')"
                  size="small"
                  text
                  @click="deselectAllPermissions"
                />
              </div>
            </div>

            <LoadingState v-if="permissionsLoading" message="Loading permissions..." />

            <div v-else class="space-y-4">
              <div
                v-for="(group, resource) in groupedPermissions"
                :key="resource"
                class="border rounded-lg p-4"
              >
                <h4 class="font-medium mb-3 capitalize">
                  {{ getResourceLabel(resource) }}
                </h4>
                <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-3">
                  <div
                    v-for="permission in group"
                    :key="permission.id"
                    class="flex items-center"
                  >
                    <Checkbox
                      :id="permission.id"
                      v-model="formData.permissionIds"
                      :value="permission.id"
                      :binary="false"
                    />
                    <label :for="permission.id" class="ml-2 cursor-pointer">
                      {{ getActionLabel(permission.action) }}
                    </label>
                  </div>
                </div>
              </div>
            </div>

            <small v-if="errors.permissionIds" class="text-red-500">{{ errors.permissionIds }}</small>
          </div>

          <!-- Actions -->
          <div class="flex gap-3 pt-4 border-t">
            <Button
              type="submit"
              :label="$t('common.save')"
              icon="pi pi-check"
              :loading="saving"
            />
            <Button
              type="button"
              :label="$t('common.cancel')"
              icon="pi pi-times"
              severity="secondary"
              outlined
              @click="navigateTo('/settings/roles')"
            />
          </div>
        </form>
      </template>
    </Card>
  </div>
</template>
