import type { Permission, Role, RoleFormData } from '~/types/auth'
import type { ApiResponse } from '~/types/api'

export function useRole() {
  const { apiFetch } = useApi()

  const getAllRoles = async (): Promise<Role[]> => {
    const response = await apiFetch<ApiResponse<Role[]>>('/roles')
    return response.data
  }

  const getRoleById = async (id: string): Promise<Role> => {
    const response = await apiFetch<ApiResponse<Role>>(`/roles/${id}`)
    return response.data
  }

  const getAllPermissions = async (): Promise<Permission[]> => {
    const response = await apiFetch<ApiResponse<Permission[]>>('/permissions')
    return response.data
  }

  const createRole = async (data: RoleFormData): Promise<Role> => {
    const response = await apiFetch<ApiResponse<Role>>('/roles', {
      method: 'POST',
      body: data,
    })
    return response.data
  }

  const updateRole = async (id: string, data: RoleFormData): Promise<Role> => {
    const response = await apiFetch<ApiResponse<Role>>(`/roles/${id}`, {
      method: 'PUT',
      body: data,
    })
    return response.data
  }

  const deleteRole = async (id: string): Promise<void> => {
    await apiFetch<ApiResponse<boolean>>(`/roles/${id}`, {
      method: 'DELETE',
    })
  }

  return {
    getAllRoles,
    getRoleById,
    getAllPermissions,
    createRole,
    updateRole,
    deleteRole,
  }
}
