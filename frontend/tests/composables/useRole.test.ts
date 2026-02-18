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
          id: 'role-1',
          name: 'Admin',
          description: 'Administrator role with full access',
          priority: 1,
          isSystemRole: true,
          isActive: true,
          userCount: 5,
        },
        {
          id: 'role-2',
          name: 'User',
          description: 'Standard user role',
          priority: 2,
          isSystemRole: false,
          isActive: true,
          userCount: 20,
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockRoles, success: true })

      const { getAllRoles } = useRole()
      const result = await getAllRoles()

      expect(mockApiFetch).toHaveBeenCalledWith('/roles')
      expect(result).toEqual(mockRoles)
      expect(result).toHaveLength(2)
    })

    it('should handle empty roles list', async () => {
      mockApiFetch.mockResolvedValue({ data: [], success: true })

      const { getAllRoles } = useRole()
      const result = await getAllRoles()

      expect(result).toEqual([])
      expect(result).toHaveLength(0)
    })

    it('should handle API errors when fetching roles', async () => {
      mockApiFetch.mockRejectedValue(new Error('Network error'))

      const { getAllRoles } = useRole()

      await expect(getAllRoles()).rejects.toThrow('Network error')
    })
  })

  describe('getRoleById', () => {
    it('should fetch a role by id successfully', async () => {
      const mockRole: Role = {
        id: 'role-1',
        name: 'Admin',
        description: 'Administrator role with full access',
        priority: 1,
        isSystemRole: true,
        isActive: true,
        userCount: 5,
        permissions: [
          {
            id: 'perm-1',
            name: 'users.read',
            description: 'Read users',
            resource: 'users',
            action: 'read',
          },
          {
            id: 'perm-2',
            name: 'users.write',
            description: 'Write users',
            resource: 'users',
            action: 'write',
          },
        ],
      }

      mockApiFetch.mockResolvedValue({ data: mockRole, success: true })

      const { getRoleById } = useRole()
      const result = await getRoleById('role-1')

      expect(mockApiFetch).toHaveBeenCalledWith('/roles/role-1')
      expect(result).toEqual(mockRole)
      expect(result.id).toBe('role-1')
      expect(result.name).toBe('Admin')
      expect(result.permissions).toHaveLength(2)
    })

    it('should handle API errors when fetching role by id', async () => {
      mockApiFetch.mockRejectedValue(new Error('Role not found'))

      const { getRoleById } = useRole()

      await expect(getRoleById('invalid-role')).rejects.toThrow('Role not found')
    })
  })

  describe('getAllPermissions', () => {
    it('should fetch all permissions successfully', async () => {
      const mockPermissions: Permission[] = [
        {
          id: 'perm-1',
          name: 'users.read',
          description: 'Read users',
          resource: 'users',
          action: 'read',
        },
        {
          id: 'perm-2',
          name: 'users.write',
          description: 'Write users',
          resource: 'users',
          action: 'write',
        },
        {
          id: 'perm-3',
          name: 'products.read',
          description: 'Read products',
          resource: 'products',
          action: 'read',
        },
        {
          id: 'perm-4',
          name: 'products.write',
          description: 'Write products',
          resource: 'products',
          action: 'write',
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockPermissions, success: true })

      const { getAllPermissions } = useRole()
      const result = await getAllPermissions()

      expect(mockApiFetch).toHaveBeenCalledWith('/permissions')
      expect(result).toEqual(mockPermissions)
      expect(result).toHaveLength(4)
    })

    it('should handle empty permissions list', async () => {
      mockApiFetch.mockResolvedValue({ data: [], success: true })

      const { getAllPermissions } = useRole()
      const result = await getAllPermissions()

      expect(result).toEqual([])
      expect(result).toHaveLength(0)
    })

    it('should handle API errors when fetching permissions', async () => {
      mockApiFetch.mockRejectedValue(new Error('Network error'))

      const { getAllPermissions } = useRole()

      await expect(getAllPermissions()).rejects.toThrow('Network error')
    })
  })

  describe('createRole', () => {
    it('should create a role successfully', async () => {
      const roleData: RoleFormData = {
        name: 'Manager',
        description: 'Manager role with limited access',
        priority: 3,
        permissionIds: ['perm-1', 'perm-3'],
      }

      const mockCreatedRole: Role = {
        id: 'role-3',
        name: 'Manager',
        description: 'Manager role with limited access',
        priority: 3,
        isSystemRole: false,
        isActive: true,
        userCount: 0,
      }

      mockApiFetch.mockResolvedValue({ data: mockCreatedRole, success: true })

      const { createRole } = useRole()
      const result = await createRole(roleData)

      expect(mockApiFetch).toHaveBeenCalledWith('/roles', {
        method: 'POST',
        body: roleData,
      })
      expect(result).toEqual(mockCreatedRole)
      expect(result.id).toBe('role-3')
      expect(result.name).toBe('Manager')
    })

    it('should create a role with no permissions', async () => {
      const roleData: RoleFormData = {
        name: 'Guest',
        description: 'Guest role with no permissions',
        priority: 10,
        permissionIds: [],
      }

      const mockCreatedRole: Role = {
        id: 'role-4',
        name: 'Guest',
        description: 'Guest role with no permissions',
        priority: 10,
        isSystemRole: false,
        isActive: true,
        userCount: 0,
      }

      mockApiFetch.mockResolvedValue({ data: mockCreatedRole, success: true })

      const { createRole } = useRole()
      const result = await createRole(roleData)

      expect(result).toEqual(mockCreatedRole)
      expect(result.name).toBe('Guest')
    })

    it('should handle API errors when creating role', async () => {
      const roleData: RoleFormData = {
        name: 'Admin',
        description: 'Duplicate admin role',
        priority: 1,
        permissionIds: [],
      }

      mockApiFetch.mockRejectedValue(new Error('Role name already exists'))

      const { createRole } = useRole()

      await expect(createRole(roleData)).rejects.toThrow('Role name already exists')
    })
  })

  describe('updateRole', () => {
    it('should update a role successfully', async () => {
      const roleData: RoleFormData = {
        name: 'Updated Manager',
        description: 'Updated manager role with expanded access',
        priority: 2,
        permissionIds: ['perm-1', 'perm-2', 'perm-3'],
      }

      const mockUpdatedRole: Role = {
        id: 'role-3',
        name: 'Updated Manager',
        description: 'Updated manager role with expanded access',
        priority: 2,
        isSystemRole: false,
        isActive: true,
        userCount: 5,
      }

      mockApiFetch.mockResolvedValue({ data: mockUpdatedRole, success: true })

      const { updateRole } = useRole()
      const result = await updateRole('role-3', roleData)

      expect(mockApiFetch).toHaveBeenCalledWith('/roles/role-3', {
        method: 'PUT',
        body: roleData,
      })
      expect(result).toEqual(mockUpdatedRole)
      expect(result.name).toBe('Updated Manager')
      expect(result.priority).toBe(2)
    })

    it('should handle API errors when updating role', async () => {
      const roleData: RoleFormData = {
        name: 'Admin',
        description: 'Cannot rename to existing role',
        priority: 1,
        permissionIds: [],
      }

      mockApiFetch.mockRejectedValue(new Error('Role name already exists'))

      const { updateRole } = useRole()

      await expect(updateRole('role-3', roleData)).rejects.toThrow('Role name already exists')
    })

    it('should handle updating system role error', async () => {
      const roleData: RoleFormData = {
        name: 'System Admin',
        description: 'Cannot update system role',
        priority: 1,
        permissionIds: [],
      }

      mockApiFetch.mockRejectedValue(new Error('Cannot modify system role'))

      const { updateRole } = useRole()

      await expect(updateRole('role-1', roleData)).rejects.toThrow('Cannot modify system role')
    })
  })

  describe('deleteRole', () => {
    it('should delete a role successfully', async () => {
      mockApiFetch.mockResolvedValue({ data: true, success: true })

      const { deleteRole } = useRole()
      await deleteRole('role-3')

      expect(mockApiFetch).toHaveBeenCalledWith('/roles/role-3', {
        method: 'DELETE',
      })
    })

    it('should handle API errors when deleting role', async () => {
      mockApiFetch.mockRejectedValue(new Error('Cannot delete role with assigned users'))

      const { deleteRole } = useRole()

      await expect(deleteRole('role-2')).rejects.toThrow('Cannot delete role with assigned users')
    })

    it('should handle deleting system role error', async () => {
      mockApiFetch.mockRejectedValue(new Error('Cannot delete system role'))

      const { deleteRole } = useRole()

      await expect(deleteRole('role-1')).rejects.toThrow('Cannot delete system role')
    })
  })

  describe('integration scenarios', () => {
    it('should handle complete role management workflow', async () => {
      const { getAllRoles, getAllPermissions, createRole, updateRole, deleteRole } = useRole()

      // Get initial roles
      const mockRoles: Role[] = [
        {
          id: 'role-1',
          name: 'Admin',
          description: 'Admin role',
          priority: 1,
          isSystemRole: true,
          isActive: true,
        },
      ]
      mockApiFetch.mockResolvedValue({ data: mockRoles, success: true })
      const roles = await getAllRoles()
      expect(roles).toHaveLength(1)

      // Get available permissions
      const mockPermissions: Permission[] = [
        {
          id: 'perm-1',
          name: 'users.read',
          description: 'Read users',
          resource: 'users',
          action: 'read',
        },
        {
          id: 'perm-2',
          name: 'users.write',
          description: 'Write users',
          resource: 'users',
          action: 'write',
        },
      ]
      mockApiFetch.mockResolvedValue({ data: mockPermissions, success: true })
      const permissions = await getAllPermissions()
      expect(permissions).toHaveLength(2)

      // Create new role
      const newRoleData: RoleFormData = {
        name: 'Manager',
        description: 'Manager role',
        priority: 2,
        permissionIds: ['perm-1'],
      }
      const mockCreatedRole: Role = {
        id: 'role-2',
        ...newRoleData,
        isSystemRole: false,
        isActive: true,
        userCount: 0,
      }
      mockApiFetch.mockResolvedValue({ data: mockCreatedRole, success: true })
      const createdRole = await createRole(newRoleData)
      expect(createdRole.name).toBe('Manager')

      // Update role
      const updateData: RoleFormData = {
        name: 'Senior Manager',
        description: 'Senior manager role',
        priority: 2,
        permissionIds: ['perm-1', 'perm-2'],
      }
      const mockUpdatedRole: Role = {
        id: 'role-2',
        ...updateData,
        isSystemRole: false,
        isActive: true,
        userCount: 0,
      }
      mockApiFetch.mockResolvedValue({ data: mockUpdatedRole, success: true })
      const updatedRole = await updateRole('role-2', updateData)
      expect(updatedRole.name).toBe('Senior Manager')

      // Delete role
      mockApiFetch.mockResolvedValue({ data: true, success: true })
      await deleteRole('role-2')
      expect(mockApiFetch).toHaveBeenCalledWith('/roles/role-2', {
        method: 'DELETE',
      })
    })

    it('should handle role with permissions retrieval', async () => {
      const { getRoleById, getAllPermissions } = useRole()

      // Get all permissions first
      const mockPermissions: Permission[] = [
        {
          id: 'perm-1',
          name: 'users.read',
          description: 'Read users',
          resource: 'users',
          action: 'read',
        },
        {
          id: 'perm-2',
          name: 'users.write',
          description: 'Write users',
          resource: 'users',
          action: 'write',
        },
      ]
      mockApiFetch.mockResolvedValue({ data: mockPermissions, success: true })
      const permissions = await getAllPermissions()
      expect(permissions).toHaveLength(2)

      // Get role with permissions
      const mockRole: Role = {
        id: 'role-1',
        name: 'Admin',
        description: 'Administrator role',
        priority: 1,
        isSystemRole: true,
        isActive: true,
        permissions: mockPermissions,
      }
      mockApiFetch.mockResolvedValue({ data: mockRole, success: true })
      const role = await getRoleById('role-1')
      expect(role.permissions).toHaveLength(2)
      expect(role.permissions?.[0].resource).toBe('users')
    })

    it('should handle priority-based role ordering', async () => {
      const mockRoles: Role[] = [
        {
          id: 'role-1',
          name: 'Admin',
          description: 'Admin',
          priority: 1,
          isSystemRole: true,
          isActive: true,
        },
        {
          id: 'role-2',
          name: 'Manager',
          description: 'Manager',
          priority: 2,
          isSystemRole: false,
          isActive: true,
        },
        {
          id: 'role-3',
          name: 'User',
          description: 'User',
          priority: 3,
          isSystemRole: false,
          isActive: true,
        },
      ]

      mockApiFetch.mockResolvedValue({ data: mockRoles, success: true })

      const { getAllRoles } = useRole()
      const roles = await getAllRoles()

      expect(roles).toHaveLength(3)
      expect(roles[0].priority).toBeLessThan(roles[1].priority)
      expect(roles[1].priority).toBeLessThan(roles[2].priority)
    })
  })
})
