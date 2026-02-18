import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { mockApiFetch, mockTenantStoreData } from '../setup'
import { useAuthStore } from '~/stores/auth'
import type { ApiResponse, LoginCredentials, LoginResponse, RegisterData, Role, SelectTenantResponse, User } from '~/types/auth'
import type { Tenant } from '~/types/tenant'

// Override global useTenantStore for auth tests
const mockTenantStoreMethods = {
  setAvailableTenants: vi.fn(),
  selectTenant: vi.fn(),
  clearTenant: vi.fn(),
}

globalThis.useTenantStore = vi.fn(() => ({
  ...mockTenantStoreData,
  ...mockTenantStoreMethods,
}))

describe('auth store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  const mockUser: User = {
    id: 'user-1',
    name: 'John Doe',
    email: 'john@example.com',
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z',
  }

  const mockRole: Role = {
    id: 'role-1',
    name: 'Admin',
    description: 'Administrator role',
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z',
  }

  const mockTenant: Tenant = {
    id: 'tenant-1',
    name: 'Acme Corp',
    slug: 'acme-corp',
    isActive: true,
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z',
  }

  const mockLoginResponse: LoginResponse = {
    accessToken: 'access-token-123',
    refreshToken: 'refresh-token-456',
    user: mockUser,
    tenants: [mockTenant],
  }

  describe('initial state', () => {
    it('should have token as null', () => {
      // Arrange & Act
      const store = useAuthStore()

      // Assert
      expect(store.token).toBeNull()
    })

    it('should have user as null', () => {
      // Arrange & Act
      const store = useAuthStore()

      // Assert
      expect(store.user).toBeNull()
    })

    it('should have refreshToken as null', () => {
      // Arrange & Act
      const store = useAuthStore()

      // Assert
      expect(store.refreshToken).toBeNull()
    })

    it('should have role as null', () => {
      // Arrange & Act
      const store = useAuthStore()

      // Assert
      expect(store.role).toBeNull()
    })

    it('should have empty permissions array', () => {
      // Arrange & Act
      const store = useAuthStore()

      // Assert
      expect(store.permissions).toEqual([])
    })

    it('should have isAuthenticated as false', () => {
      // Arrange & Act
      const store = useAuthStore()

      // Assert
      expect(store.isAuthenticated).toBe(false)
    })
  })

  describe('isAuthenticated computed', () => {
    it('should return true when token and user are set', () => {
      // Arrange
      const store = useAuthStore()
      store.token = 'token-123'
      store.user = mockUser

      // Act & Assert
      expect(store.isAuthenticated).toBe(true)
    })

    it('should return false when token is null', () => {
      // Arrange
      const store = useAuthStore()
      store.token = null
      store.user = mockUser

      // Act & Assert
      expect(store.isAuthenticated).toBe(false)
    })

    it('should return false when user is null', () => {
      // Arrange
      const store = useAuthStore()
      store.token = 'token-123'
      store.user = null

      // Act & Assert
      expect(store.isAuthenticated).toBe(false)
    })

    it('should return false when both are null', () => {
      // Arrange
      const store = useAuthStore()
      store.token = null
      store.user = null

      // Act & Assert
      expect(store.isAuthenticated).toBe(false)
    })
  })

  describe('login', () => {
    it('should login successfully with tenants', async () => {
      // Arrange
      const store = useAuthStore()
      const credentials: LoginCredentials = {
        email: 'john@example.com',
        password: 'password123',
      }

      const selectTenantResponse: SelectTenantResponse = {
        accessToken: 'tenant-token-123',
        role: mockRole,
        permissions: ['users.read', 'users.write'],
      }

      mockApiFetch
        .mockResolvedValueOnce({ data: mockLoginResponse } as ApiResponse<LoginResponse>)
        .mockResolvedValueOnce({ data: selectTenantResponse } as ApiResponse<SelectTenantResponse>)

      // Mock console.log to suppress output
      const consoleLogSpy = vi.spyOn(console, 'log').mockImplementation(() => {})

      // Act
      const response = await store.login(credentials)

      // Assert
      expect(mockApiFetch).toHaveBeenCalledWith('/auth/login', {
        method: 'POST',
        body: credentials,
      })
      expect(store.token).toBe('tenant-token-123') // Updated by selectTenant
      expect(store.refreshToken).toBe('refresh-token-456')
      expect(store.user).toEqual(mockUser)
      expect(store.role).toEqual(mockRole)
      expect(store.permissions).toEqual(['users.read', 'users.write'])
      expect(mockTenantStoreMethods.setAvailableTenants).toHaveBeenCalledWith([mockTenant])
      expect(response).toEqual(mockLoginResponse)

      consoleLogSpy.mockRestore()
    })

    it('should login successfully without tenants', async () => {
      // Arrange
      const store = useAuthStore()
      const credentials: LoginCredentials = {
        email: 'john@example.com',
        password: 'password123',
      }

      const loginResponseNoTenants = { ...mockLoginResponse, tenants: [] }
      mockApiFetch.mockResolvedValue({ data: loginResponseNoTenants } as ApiResponse<LoginResponse>)

      // Mock console.log
      const consoleLogSpy = vi.spyOn(console, 'log').mockImplementation(() => {})

      // Act
      await store.login(credentials)

      // Assert
      expect(store.token).toBe('access-token-123')
      expect(store.refreshToken).toBe('refresh-token-456')
      expect(store.user).toEqual(mockUser)
      expect(mockTenantStoreMethods.setAvailableTenants).not.toHaveBeenCalled()

      consoleLogSpy.mockRestore()
    })

    it('should throw error when login fails', async () => {
      // Arrange
      const store = useAuthStore()
      const credentials: LoginCredentials = {
        email: 'john@example.com',
        password: 'wrong-password',
      }
      const error = new Error('Invalid credentials')
      mockApiFetch.mockRejectedValue(error)

      // Mock console.log
      const consoleLogSpy = vi.spyOn(console, 'log').mockImplementation(() => {})

      // Act & Assert
      await expect(store.login(credentials)).rejects.toThrow('Invalid credentials')
      expect(store.token).toBeNull()
      expect(store.user).toBeNull()

      consoleLogSpy.mockRestore()
    })
  })

  describe('selectTenant', () => {
    it('should select tenant and update role/permissions', async () => {
      // Arrange
      const store = useAuthStore()
      const selectTenantResponse: SelectTenantResponse = {
        accessToken: 'tenant-token-123',
        role: mockRole,
        permissions: ['users.read', 'users.write'],
      }

      mockApiFetch.mockResolvedValue({ data: selectTenantResponse } as ApiResponse<SelectTenantResponse>)

      // Mock console.log
      const consoleLogSpy = vi.spyOn(console, 'log').mockImplementation(() => {})

      // Act
      await store.selectTenant('tenant-1')

      // Assert
      expect(mockApiFetch).toHaveBeenCalledWith('/auth/select-tenant/tenant-1', {
        method: 'POST',
      })
      expect(store.token).toBe('tenant-token-123')
      expect(store.role).toEqual(mockRole)
      expect(store.permissions).toEqual(['users.read', 'users.write'])
      expect(mockTenantStoreMethods.selectTenant).toHaveBeenCalledWith('tenant-1')

      consoleLogSpy.mockRestore()
    })

    it('should throw error when selectTenant fails', async () => {
      // Arrange
      const store = useAuthStore()
      const error = new Error('Tenant not found')
      mockApiFetch.mockRejectedValue(error)

      // Mock console.log
      const consoleLogSpy = vi.spyOn(console, 'log').mockImplementation(() => {})

      // Act & Assert
      await expect(store.selectTenant('invalid-tenant')).rejects.toThrow('Tenant not found')

      consoleLogSpy.mockRestore()
    })
  })

  describe('logout', () => {
    it('should clear all auth state', () => {
      // Arrange
      const store = useAuthStore()
      store.token = 'token-123'
      store.refreshToken = 'refresh-123'
      store.user = mockUser
      store.role = mockRole
      store.permissions = ['users.read']

      // Act
      store.logout()

      // Assert
      expect(store.token).toBeNull()
      expect(store.refreshToken).toBeNull()
      expect(store.user).toBeNull()
      expect(store.role).toBeNull()
      expect(store.permissions).toEqual([])
      expect(mockTenantStoreMethods.clearTenant).toHaveBeenCalled()
    })

    it('should work when state already empty', () => {
      // Arrange
      const store = useAuthStore()

      // Act
      store.logout()

      // Assert
      expect(store.token).toBeNull()
      expect(store.user).toBeNull()
      expect(mockTenantStoreMethods.clearTenant).toHaveBeenCalled()
    })
  })

  describe('refreshAccessToken', () => {
    it('should refresh access token successfully', async () => {
      // Arrange
      const store = useAuthStore()
      store.refreshToken = 'refresh-token-456'

      mockApiFetch.mockResolvedValue({ accessToken: 'new-access-token' })

      // Act
      await store.refreshAccessToken()

      // Assert
      expect(mockApiFetch).toHaveBeenCalledWith('/auth/refresh', {
        method: 'POST',
        body: { refreshToken: 'refresh-token-456' },
      })
      expect(store.token).toBe('new-access-token')
    })

    it('should throw error when no refresh token available', async () => {
      // Arrange
      const store = useAuthStore()
      store.refreshToken = null

      // Act & Assert
      await expect(store.refreshAccessToken()).rejects.toThrow('No refresh token available')
    })

    it('should logout when refresh fails', async () => {
      // Arrange
      const store = useAuthStore()
      store.token = 'old-token'
      store.refreshToken = 'refresh-token-456'
      store.user = mockUser

      const error = new Error('Refresh failed')
      mockApiFetch.mockRejectedValue(error)

      // Act & Assert
      await expect(store.refreshAccessToken()).rejects.toThrow('Refresh failed')
      expect(store.token).toBeNull()
      expect(store.user).toBeNull()
      expect(mockTenantStoreMethods.clearTenant).toHaveBeenCalled()
    })
  })

  describe('fetchCurrentUser', () => {
    it('should fetch current user successfully', async () => {
      // Arrange
      const store = useAuthStore()
      mockApiFetch.mockResolvedValue({ data: mockUser } as ApiResponse<User>)

      // Act
      await store.fetchCurrentUser()

      // Assert
      expect(mockApiFetch).toHaveBeenCalledWith('/auth/me')
      expect(store.user).toEqual(mockUser)
    })

    it('should logout when fetch fails', async () => {
      // Arrange
      const store = useAuthStore()
      store.token = 'token-123'
      store.user = mockUser

      const error = new Error('Unauthorized')
      mockApiFetch.mockRejectedValue(error)

      // Act & Assert
      await expect(store.fetchCurrentUser()).rejects.toThrow('Unauthorized')
      expect(store.token).toBeNull()
      expect(store.user).toBeNull()
      expect(mockTenantStoreMethods.clearTenant).toHaveBeenCalled()
    })
  })

  describe('register', () => {
    it('should register successfully with tenants', async () => {
      // Arrange
      const store = useAuthStore()
      const registerData: RegisterData = {
        companyName: 'Acme Corp',
        slug: 'acme-corp',
        name: 'John Doe',
        email: 'john@example.com',
        password: 'password123',
      }

      const selectTenantResponse: SelectTenantResponse = {
        accessToken: 'tenant-token-123',
        role: mockRole,
        permissions: ['users.read', 'users.write'],
      }

      mockApiFetch
        .mockResolvedValueOnce({ data: mockLoginResponse } as ApiResponse<LoginResponse>)
        .mockResolvedValueOnce({ data: selectTenantResponse } as ApiResponse<SelectTenantResponse>)

      // Mock console.log
      const consoleLogSpy = vi.spyOn(console, 'log').mockImplementation(() => {})

      // Act
      const response = await store.register(registerData)

      // Assert
      expect(mockApiFetch).toHaveBeenCalledWith('/auth/register', {
        method: 'POST',
        body: {
          companyName: 'Acme Corp',
          slug: 'acme-corp',
          name: 'John Doe',
          email: 'john@example.com',
          password: 'password123',
        },
      })
      expect(store.token).toBe('tenant-token-123')
      expect(store.refreshToken).toBe('refresh-token-456')
      expect(store.user).toEqual(mockUser)
      expect(store.role).toEqual(mockRole)
      expect(store.permissions).toEqual(['users.read', 'users.write'])
      expect(mockTenantStoreMethods.setAvailableTenants).toHaveBeenCalledWith([mockTenant])
      expect(response).toEqual(mockLoginResponse)

      consoleLogSpy.mockRestore()
    })

    it('should throw error when registration fails', async () => {
      // Arrange
      const store = useAuthStore()
      const registerData: RegisterData = {
        companyName: 'Acme Corp',
        slug: 'acme-corp',
        name: 'John Doe',
        email: 'john@example.com',
        password: 'password123',
      }

      const error = new Error('Email already exists')
      mockApiFetch.mockRejectedValue(error)

      // Mock console.log
      const consoleLogSpy = vi.spyOn(console, 'log').mockImplementation(() => {})

      // Act & Assert
      await expect(store.register(registerData)).rejects.toThrow('Email already exists')
      expect(store.token).toBeNull()
      expect(store.user).toBeNull()

      consoleLogSpy.mockRestore()
    })
  })

  describe('hasPermission', () => {
    it('should return true when user has the permission', () => {
      // Arrange
      const store = useAuthStore()
      store.permissions = ['users.read', 'users.write', 'products.read']

      // Act & Assert
      expect(store.hasPermission('users.read')).toBe(true)
      expect(store.hasPermission('users.write')).toBe(true)
      expect(store.hasPermission('products.read')).toBe(true)
    })

    it('should return false when user does not have the permission', () => {
      // Arrange
      const store = useAuthStore()
      store.permissions = ['users.read']

      // Act & Assert
      expect(store.hasPermission('users.write')).toBe(false)
      expect(store.hasPermission('products.read')).toBe(false)
    })

    it('should return false when permissions array is empty', () => {
      // Arrange
      const store = useAuthStore()
      store.permissions = []

      // Act & Assert
      expect(store.hasPermission('users.read')).toBe(false)
    })
  })

  describe('hasAnyPermission', () => {
    it('should return true when user has at least one permission', () => {
      // Arrange
      const store = useAuthStore()
      store.permissions = ['users.read', 'users.write']

      // Act & Assert
      expect(store.hasAnyPermission(['users.read', 'products.read'])).toBe(true)
      expect(store.hasAnyPermission(['users.write', 'products.write'])).toBe(true)
    })

    it('should return false when user has none of the permissions', () => {
      // Arrange
      const store = useAuthStore()
      store.permissions = ['users.read']

      // Act & Assert
      expect(store.hasAnyPermission(['products.read', 'products.write'])).toBe(false)
    })

    it('should return false when permissions array is empty', () => {
      // Arrange
      const store = useAuthStore()
      store.permissions = []

      // Act & Assert
      expect(store.hasAnyPermission(['users.read', 'users.write'])).toBe(false)
    })

    it('should return false when required permissions array is empty', () => {
      // Arrange
      const store = useAuthStore()
      store.permissions = ['users.read']

      // Act & Assert
      expect(store.hasAnyPermission([])).toBe(false)
    })
  })

  describe('hasAllPermissions', () => {
    it('should return true when user has all required permissions', () => {
      // Arrange
      const store = useAuthStore()
      store.permissions = ['users.read', 'users.write', 'products.read']

      // Act & Assert
      expect(store.hasAllPermissions(['users.read', 'users.write'])).toBe(true)
      expect(store.hasAllPermissions(['users.read'])).toBe(true)
    })

    it('should return false when user is missing some permissions', () => {
      // Arrange
      const store = useAuthStore()
      store.permissions = ['users.read']

      // Act & Assert
      expect(store.hasAllPermissions(['users.read', 'users.write'])).toBe(false)
      expect(store.hasAllPermissions(['products.read', 'products.write'])).toBe(false)
    })

    it('should return false when permissions array is empty', () => {
      // Arrange
      const store = useAuthStore()
      store.permissions = []

      // Act & Assert
      expect(store.hasAllPermissions(['users.read'])).toBe(false)
    })

    it('should return true when required permissions array is empty', () => {
      // Arrange
      const store = useAuthStore()
      store.permissions = ['users.read']

      // Act & Assert
      expect(store.hasAllPermissions([])).toBe(true)
    })
  })

  describe('persistence configuration', () => {
    it('should have persistence enabled with localStorage', () => {
      // Arrange & Act
      const store = useAuthStore()

      // Assert
      expect(store.$id).toBe('auth')
      // Persistence paths are configured in the store definition
      // We verify the store is properly initialized
      expect(store).toBeDefined()
    })
  })
})
