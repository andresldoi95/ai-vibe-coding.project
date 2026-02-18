import { beforeEach, describe, expect, it } from 'vitest'
import { mockApiFetch, mockAuthStoreData } from '../setup'
import { useUser } from '~/composables/useUser'
import type { AcceptInvitationData, AcceptInvitationResponse, CompanyUser, InviteUserData } from '~/types/user'
import type { ChangePasswordData, UpdateProfileData, User } from '~/types/auth'

describe('useUser', () => {
  beforeEach(() => {
    // Reset all mocks before each test
    mockApiFetch.mockReset()
    // Reset auth store data
    mockAuthStoreData.user = {
      id: 'user-1',
      email: 'test@example.com',
      name: 'Test User',
      isActive: true,
      emailConfirmed: true,
    }
  })

  describe('getAllUsers', () => {
    it('should fetch all users successfully', async () => {
      const mockUsers: CompanyUser[] = [
        {
          id: 'user-1',
          name: 'John Doe',
          email: 'john@example.com',
          role: {
            id: 'role-1',
            name: 'Admin',
          },
          joinedAt: '2024-01-01T00:00:00Z',
          isActive: true,
        },
        {
          id: 'user-2',
          name: 'Jane Smith',
          email: 'jane@example.com',
          role: {
            id: 'role-2',
            name: 'User',
          },
          joinedAt: '2024-01-10T00:00:00Z',
          isActive: true,
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockUsers, success: true })

      const { getAllUsers } = useUser()
      const result = await getAllUsers()

      expect(mockApiFetch).toHaveBeenCalledWith('/users')
      expect(result).toEqual(mockUsers)
      expect(result).toHaveLength(2)
    })

    it('should handle empty user list', async () => {
      mockApiFetch.mockResolvedValue({ data: [], success: true })

      const { getAllUsers } = useUser()
      const result = await getAllUsers()

      expect(result).toEqual([])
      expect(result).toHaveLength(0)
    })

    it('should handle API errors when fetching users', async () => {
      mockApiFetch.mockRejectedValue(new Error('Network error'))

      const { getAllUsers } = useUser()

      await expect(getAllUsers()).rejects.toThrow('Network error')
    })
  })

  describe('getUserById', () => {
    it('should fetch a user by id successfully', async () => {
      const mockUser: CompanyUser = {
        id: 'user-1',
        name: 'John Doe',
        email: 'john@example.com',
        role: {
          id: 'role-1',
          name: 'Admin',
        },
        joinedAt: '2024-01-01T00:00:00Z',
        isActive: true,
      }

      mockApiFetch.mockResolvedValue({ data: mockUser, success: true })

      const { getUserById } = useUser()
      const result = await getUserById('user-1')

      expect(mockApiFetch).toHaveBeenCalledWith('/users/user-1')
      expect(result).toEqual(mockUser)
      expect(result.id).toBe('user-1')
      expect(result.name).toBe('John Doe')
    })

    it('should handle API errors when fetching user by id', async () => {
      mockApiFetch.mockRejectedValue(new Error('User not found'))

      const { getUserById } = useUser()

      await expect(getUserById('invalid-user')).rejects.toThrow('User not found')
    })
  })

  describe('inviteUser', () => {
    it('should invite a user successfully', async () => {
      const inviteData: InviteUserData = {
        email: 'newuser@example.com',
        roleId: 'role-2',
        personalMessage: 'Welcome to our team!',
      }

      mockApiFetch.mockResolvedValue({ success: true })

      const { inviteUser } = useUser()
      await inviteUser(inviteData)

      expect(mockApiFetch).toHaveBeenCalledWith('/users/invite', {
        method: 'POST',
        body: inviteData,
      })
    })

    it('should invite a user without personal message', async () => {
      const inviteData: InviteUserData = {
        email: 'newuser@example.com',
        roleId: 'role-2',
      }

      mockApiFetch.mockResolvedValue({ success: true })

      const { inviteUser } = useUser()
      await inviteUser(inviteData)

      expect(mockApiFetch).toHaveBeenCalledWith('/users/invite', {
        method: 'POST',
        body: inviteData,
      })
    })

    it('should handle API errors when inviting user', async () => {
      const inviteData: InviteUserData = {
        email: 'existing@example.com',
        roleId: 'role-2',
      }

      mockApiFetch.mockRejectedValue(new Error('User already exists'))

      const { inviteUser } = useUser()

      await expect(inviteUser(inviteData)).rejects.toThrow('User already exists')
    })
  })

  describe('updateUserRole', () => {
    it('should update user role successfully', async () => {
      mockApiFetch.mockResolvedValue({ success: true })

      const { updateUserRole } = useUser()
      await updateUserRole('user-1', 'role-3')

      expect(mockApiFetch).toHaveBeenCalledWith('/users/user-1/role', {
        method: 'PUT',
        body: { roleId: 'role-3' },
      })
    })

    it('should handle API errors when updating user role', async () => {
      mockApiFetch.mockRejectedValue(new Error('Role not found'))

      const { updateUserRole } = useUser()

      await expect(updateUserRole('user-1', 'invalid-role')).rejects.toThrow('Role not found')
    })
  })

  describe('removeUser', () => {
    it('should remove a user successfully', async () => {
      mockApiFetch.mockResolvedValue({ success: true })

      const { removeUser } = useUser()
      await removeUser('user-1')

      expect(mockApiFetch).toHaveBeenCalledWith('/users/user-1', {
        method: 'DELETE',
      })
    })

    it('should handle API errors when removing user', async () => {
      mockApiFetch.mockRejectedValue(new Error('Cannot remove user'))

      const { removeUser } = useUser()

      await expect(removeUser('user-1')).rejects.toThrow('Cannot remove user')
    })
  })

  describe('acceptInvitation', () => {
    it('should accept invitation successfully', async () => {
      const acceptData: AcceptInvitationData = {
        invitationToken: 'token-123',
        name: 'New User',
        password: 'SecurePass123!',
      }

      const mockResponse: AcceptInvitationResponse = {
        accessToken: 'access-token-xyz',
        refreshToken: 'refresh-token-abc',
        user: {
          id: 'user-new',
          name: 'New User',
          email: 'newuser@example.com',
          isActive: true,
          emailConfirmed: true,
        },
        tenants: [
          {
            id: 'tenant-1',
            name: 'Test Company',
            slug: 'test-company',
            status: 'Active',
          },
        ],
      }

      mockApiFetch.mockResolvedValue({ data: mockResponse, success: true })

      const { acceptInvitation } = useUser()
      const result = await acceptInvitation(acceptData)

      expect(mockApiFetch).toHaveBeenCalledWith('/auth/accept-invitation', {
        method: 'POST',
        body: acceptData,
      })
      expect(result).toEqual(mockResponse)
      expect(result.user.name).toBe('New User')
      expect(result.tenants).toHaveLength(1)
    })

    it('should accept invitation with minimal data', async () => {
      const acceptData: AcceptInvitationData = {
        invitationToken: 'token-456',
      }

      const mockResponse: AcceptInvitationResponse = {
        accessToken: 'access-token-xyz',
        refreshToken: 'refresh-token-abc',
        user: {
          id: 'user-new',
          name: 'User',
          email: 'user@example.com',
          isActive: true,
          emailConfirmed: true,
        },
        tenants: [],
      }

      mockApiFetch.mockResolvedValue({ data: mockResponse, success: true })

      const { acceptInvitation } = useUser()
      const result = await acceptInvitation(acceptData)

      expect(result).toEqual(mockResponse)
    })

    it('should handle API errors when accepting invitation', async () => {
      const acceptData: AcceptInvitationData = {
        invitationToken: 'invalid-token',
      }

      mockApiFetch.mockRejectedValue(new Error('Invalid invitation token'))

      const { acceptInvitation } = useUser()

      await expect(acceptInvitation(acceptData)).rejects.toThrow('Invalid invitation token')
    })
  })

  describe('updateCurrentUser', () => {
    it('should update current user profile successfully', async () => {
      const updateData: UpdateProfileData = {
        name: 'Updated Name',
      }

      const mockUpdatedUser: User = {
        id: 'user-1',
        name: 'Updated Name',
        email: 'test@example.com',
        isActive: true,
        emailConfirmed: true,
      }

      mockApiFetch.mockResolvedValue({ data: mockUpdatedUser, success: true })

      const { updateCurrentUser } = useUser()
      const result = await updateCurrentUser(updateData)

      expect(mockApiFetch).toHaveBeenCalledWith('/auth/profile', {
        method: 'PUT',
        body: updateData,
      })
      expect(result).toEqual(mockUpdatedUser)
      expect(result.name).toBe('Updated Name')
      // Verify auth store was updated
      expect(mockAuthStoreData.user).toEqual(mockUpdatedUser)
    })

    it('should handle API errors when updating profile', async () => {
      const updateData: UpdateProfileData = {
        name: '',
      }

      mockApiFetch.mockRejectedValue(new Error('Name is required'))

      const { updateCurrentUser } = useUser()

      await expect(updateCurrentUser(updateData)).rejects.toThrow('Name is required')
    })
  })

  describe('changePassword', () => {
    it('should change password successfully', async () => {
      const passwordData: ChangePasswordData = {
        currentPassword: 'OldPass123!',
        newPassword: 'NewPass456!',
        confirmPassword: 'NewPass456!',
      }

      mockApiFetch.mockResolvedValue({ success: true })

      const { changePassword } = useUser()
      await changePassword(passwordData)

      expect(mockApiFetch).toHaveBeenCalledWith('/auth/change-password', {
        method: 'PUT',
        body: passwordData,
      })
    })

    it('should handle API errors when changing password', async () => {
      const passwordData: ChangePasswordData = {
        currentPassword: 'WrongPass123!',
        newPassword: 'NewPass456!',
        confirmPassword: 'NewPass456!',
      }

      mockApiFetch.mockRejectedValue(new Error('Current password is incorrect'))

      const { changePassword } = useUser()

      await expect(changePassword(passwordData)).rejects.toThrow('Current password is incorrect')
    })

    it('should handle password mismatch error', async () => {
      const passwordData: ChangePasswordData = {
        currentPassword: 'OldPass123!',
        newPassword: 'NewPass456!',
        confirmPassword: 'DifferentPass789!',
      }

      mockApiFetch.mockRejectedValue(new Error('Passwords do not match'))

      const { changePassword } = useUser()

      await expect(changePassword(passwordData)).rejects.toThrow('Passwords do not match')
    })
  })

  describe('integration scenarios', () => {
    it('should handle complete user management workflow', async () => {
      const { getAllUsers, inviteUser, updateUserRole } = useUser()

      // Get initial users
      const mockUsers: CompanyUser[] = [
        {
          id: 'user-1',
          name: 'John Doe',
          email: 'john@example.com',
          role: { id: 'role-1', name: 'Admin' },
          joinedAt: '2024-01-01T00:00:00Z',
          isActive: true,
        },
      ]
      mockApiFetch.mockResolvedValue({ data: mockUsers, success: true })
      const users = await getAllUsers()
      expect(users).toHaveLength(1)

      // Invite new user
      mockApiFetch.mockResolvedValue({ success: true })
      await inviteUser({ email: 'newuser@example.com', roleId: 'role-2' })
      expect(mockApiFetch).toHaveBeenCalledWith('/users/invite', {
        method: 'POST',
        body: { email: 'newuser@example.com', roleId: 'role-2' },
      })

      // Update user role
      mockApiFetch.mockResolvedValue({ success: true })
      await updateUserRole('user-1', 'role-3')
      expect(mockApiFetch).toHaveBeenCalledWith('/users/user-1/role', {
        method: 'PUT',
        body: { roleId: 'role-3' },
      })
    })

    it('should handle profile update and password change workflow', async () => {
      const { updateCurrentUser, changePassword } = useUser()

      // Update profile
      const mockUpdatedUser: User = {
        id: 'user-1',
        name: 'Updated Name',
        email: 'test@example.com',
        isActive: true,
        emailConfirmed: true,
      }
      mockApiFetch.mockResolvedValue({ data: mockUpdatedUser, success: true })
      const updatedUser = await updateCurrentUser({ name: 'Updated Name' })
      expect(updatedUser.name).toBe('Updated Name')
      expect(mockAuthStoreData.user.name).toBe('Updated Name')

      // Change password
      mockApiFetch.mockResolvedValue({ success: true })
      await changePassword({
        currentPassword: 'OldPass123!',
        newPassword: 'NewPass456!',
        confirmPassword: 'NewPass456!',
      })
      expect(mockApiFetch).toHaveBeenCalledWith('/auth/change-password', {
        method: 'PUT',
        body: {
          currentPassword: 'OldPass123!',
          newPassword: 'NewPass456!',
          confirmPassword: 'NewPass456!',
        },
      })
    })
  })
})
