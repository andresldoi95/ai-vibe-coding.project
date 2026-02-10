import { beforeEach, describe, expect, it } from 'vitest'
import { mockApiFetch } from '../setup'
import { useRole } from '~/composables/useRole'
import type { Permission, Role, RoleFormData } from '~/types/auth'

describe('useRole', () => {
  beforeEach(() => {
    // Reset all mocks before each test
    mockApiFetch.mockReset()
  })

  describe('getAllRoles', () => {
    it('should fetch all roles successfully', async () => {
      const mockRoles: Role[] = [
        {
          id: '1',
          name: 'Admin',
          description: 'Administrator role',
          priority: 1,
          isSystemRole: true,
          isActive: true,
          userCount: 5,
        },
        {
          id: '2',
          name: 'Manager',
          description: 'Manager role',
          priority: 2,
          isSystemRole: false,
          isActive: true,
          userCount: 10,
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockRoles, success: true })

      const { getAllRoles } = useRole()
      const result = await getAllRoles()

      expect(mockApiFetch).toHaveBeenCalledWith('/roles')
      expect(result).toEqual(mockRoles)
      expect(result).toHaveLength(2)
    })

    it('should handle empty role list', async () => {
      mockApiFetch.mockResolvedValue({ data: [], success: true })

      const { getAllRoles } = useRole()
      const result = await getAllRoles()

      expect(result).toEqual([])
      expect(result).toHaveLength(0)
    })
  })

  describe('getRoleById', () => {
    it('should fetch a role by id successfully', async () => {
      const mockRole: Role = {
        id: '1',
        name: 'Admin',
        description: 'Administrator role with full access',
        priority: 1,
        isSystemRole: true,
        isActive: true,
        userCount: 5,
        permissions: [
          {
            id: 'p1',
            name: 'Create Users',
            description: 'Can create new users',
            resource: 'users',
            action: 'create',
          },
        ],
      }

      mockApiFetch.mockResolvedValue({ data: mockRole, success: true })

      const { getRoleById } = useRole()
      const result = await getRoleById('1')

      expect(mockApiFetch).toHaveBeenCalledWith('/roles/1')
      expect(result).toEqual(mockRole)
      expect(result.id).toBe('1')
      expect(result.name).toBe('Admin')
      expect(result.permissions).toHaveLength(1)
    })
  })

  describe('getAllPermissions', () => {
    it('should fetch all permissions successfully', async () => {
      const mockPermissions: Permission[] = [
        {
          id: 'p1',
          name: 'Create Users',
          description: 'Can create new users',
          resource: 'users',
          action: 'create',
        },
        {
          id: 'p2',
          name: 'Edit Users',
          description: 'Can edit existing users',
          resource: 'users',
          action: 'update',
        },
        {
          id: 'p3',
          name: 'Delete Users',
          description: 'Can delete users',
          resource: 'users',
          action: 'delete',
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockPermissions, success: true })

      const { getAllPermissions } = useRole()
      const result = await getAllPermissions()

      expect(mockApiFetch).toHaveBeenCalledWith('/permissions')
      expect(result).toEqual(mockPermissions)
      expect(result).toHaveLength(3)
    })

    it('should handle empty permissions list', async () => {
      mockApiFetch.mockResolvedValue({ data: [], success: true })

      const { getAllPermissions } = useRole()
      const result = await getAllPermissions()

      expect(result).toEqual([])
      expect(result).toHaveLength(0)
    })
  })

  describe('createRole', () => {
    it('should create a new role successfully', async () => {
      const newRoleData: RoleFormData = {
        name: 'Supervisor',
        description: 'Supervisor role',
        priority: 3,
        permissionIds: ['p1', 'p2'],
      }

      const mockCreatedRole: Role = {
        id: '3',
        name: 'Supervisor',
        description: 'Supervisor role',
        priority: 3,
        isSystemRole: false,
        isActive: true,
        userCount: 0,
      }

      mockApiFetch.mockResolvedValue({ data: mockCreatedRole, success: true })

      const { createRole } = useRole()
      const result = await createRole(newRoleData)

      expect(mockApiFetch).toHaveBeenCalledWith('/roles', {
        method: 'POST',
        body: newRoleData,
      })
      expect(result).toEqual(mockCreatedRole)
      expect(result.id).toBe('3')
      expect(result.name).toBe('Supervisor')
    })
  })

  describe('updateRole', () => {
    it('should update an existing role successfully', async () => {
      const updateData: RoleFormData = {
        name: 'Updated Manager',
        description: 'Updated manager role description',
        priority: 2,
        permissionIds: ['p1', 'p2', 'p3'],
      }

      const mockUpdatedRole: Role = {
        id: '2',
        name: 'Updated Manager',
        description: 'Updated manager role description',
        priority: 2,
        isSystemRole: false,
        isActive: true,
        userCount: 10,
      }

      mockApiFetch.mockResolvedValue({ data: mockUpdatedRole, success: true })

      const { updateRole } = useRole()
      const result = await updateRole('2', updateData)

      expect(mockApiFetch).toHaveBeenCalledWith('/roles/2', {
        method: 'PUT',
        body: updateData,
      })
      expect(result).toEqual(mockUpdatedRole)
      expect(result.name).toBe('Updated Manager')
      expect(result.description).toBe('Updated manager role description')
    })
  })

  describe('deleteRole', () => {
    it('should delete a role successfully', async () => {
      mockApiFetch.mockResolvedValue({ data: true, success: true })

      const { deleteRole } = useRole()
      await deleteRole('3')

      expect(mockApiFetch).toHaveBeenCalledWith('/roles/3', {
        method: 'DELETE',
      })
    })
  })

  describe('error handling', () => {
    it('should handle API errors when fetching roles', async () => {
      const mockError = new Error('Network error')
      mockApiFetch.mockRejectedValue(mockError)

      const { getAllRoles } = useRole()

      await expect(getAllRoles()).rejects.toThrow('Network error')
    })

    it('should handle API errors when creating role', async () => {
      const mockError = new Error('Validation error: Name is required')
      mockApiFetch.mockRejectedValue(mockError)

      const newRoleData: RoleFormData = {
        name: '',
        description: 'Test role',
        priority: 1,
        permissionIds: [],
      }

      const { createRole } = useRole()

      await expect(createRole(newRoleData)).rejects.toThrow('Validation error: Name is required')
    })
  })
})
