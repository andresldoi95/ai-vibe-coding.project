import type { AcceptInvitationData, AcceptInvitationResponse, CompanyUser, InviteUserData } from '~/types/user'
import type { ApiResponse } from '~/types/api'
import type { ChangePasswordData, UpdateProfileData, User } from '~/types/auth'

export function useUser() {
  const { apiFetch } = useApi()

  const getAllUsers = async (): Promise<CompanyUser[]> => {
    const response = await apiFetch<ApiResponse<CompanyUser[]>>('/users')
    return response.data
  }

  const getUserById = async (id: string): Promise<CompanyUser> => {
    const response = await apiFetch<ApiResponse<CompanyUser>>(`/users/${id}`)
    return response.data
  }

  const inviteUser = async (data: InviteUserData): Promise<void> => {
    await apiFetch<ApiResponse<void>>('/users/invite', {
      method: 'POST',
      body: data,
    })
  }

  const updateUserRole = async (userId: string, roleId: string): Promise<void> => {
    await apiFetch<ApiResponse<void>>(`/users/${userId}/role`, {
      method: 'PUT',
      body: { roleId },
    })
  }

  const removeUser = async (userId: string): Promise<void> => {
    await apiFetch<ApiResponse<void>>(`/users/${userId}`, {
      method: 'DELETE',
    })
  }

  const acceptInvitation = async (data: AcceptInvitationData): Promise<AcceptInvitationResponse> => {
    const response = await apiFetch<ApiResponse<AcceptInvitationResponse>>('/auth/accept-invitation', {
      method: 'POST',
      body: data,
    })
    return response.data
  }

  const updateCurrentUser = async (data: UpdateProfileData): Promise<User> => {
    const authStore = useAuthStore()
    const response = await apiFetch<ApiResponse<User>>('/auth/profile', {
      method: 'PUT',
      body: data,
    })
    // Update auth store with new user data
    authStore.user = response.data
    return response.data
  }

  const changePassword = async (data: ChangePasswordData): Promise<void> => {
    await apiFetch<ApiResponse<void>>('/auth/change-password', {
      method: 'PUT',
      body: data,
    })
  }

  return {
    getAllUsers,
    getUserById,
    inviteUser,
    updateUserRole,
    removeUser,
    acceptInvitation,
    updateCurrentUser,
    changePassword,
  }
}
