import { defineStore } from 'pinia'
import type { ApiResponse, LoginCredentials, LoginResponse, RegisterData, Role, SelectTenantResponse, User } from '~/types/auth'

export const useAuthStore = defineStore(
  'auth',
  () => {
    const token = ref<string | null>(null)
    const user = ref<User | null>(null)
    const refreshToken = ref<string | null>(null)
    const role = ref<Role | null>(null)
    const permissions = ref<string[]>([])

    const isAuthenticated = computed(() => !!token.value && !!user.value)

    const selectTenant = async (tenantId: string): Promise<void> => {
      const { apiFetch } = useApi()

      // eslint-disable-next-line no-console
      console.log('[AuthStore] Selecting tenant:', tenantId)

      const apiResponse = await apiFetch<ApiResponse<SelectTenantResponse>>(
        `/auth/select-tenant/${tenantId}`,
        { method: 'POST' },
      )

      const response = apiResponse.data

      // Update token with tenant-scoped JWT that includes role and permissions
      token.value = response.accessToken
      role.value = response.role
      permissions.value = response.permissions

      // Also update tenant store
      const tenantStore = useTenantStore()
      tenantStore.selectTenant(tenantId)

      // eslint-disable-next-line no-console
      console.log('[AuthStore] Tenant selected. Role:', response.role.name, 'Permissions:', response.permissions.length)
    }

    const login = async (credentials: LoginCredentials): Promise<LoginResponse> => {
      const { apiFetch } = useApi()

      // eslint-disable-next-line no-console
      console.log('[AuthStore] Logging in...', credentials.email)

      // Backend wraps response in { data, message, success }
      const apiResponse = await apiFetch<ApiResponse<LoginResponse>>('/auth/login', {
        method: 'POST',
        body: credentials,
      })

      // eslint-disable-next-line no-console
      console.log('[AuthStore] API Response:', apiResponse)

      const response = apiResponse.data

      token.value = response.accessToken
      refreshToken.value = response.refreshToken
      user.value = response.user

      // Set available tenants and select the first one
      const tenantStore = useTenantStore()
      if (response.tenants && response.tenants.length > 0) {
        // eslint-disable-next-line no-console
        console.log('[AuthStore] Setting tenants:', response.tenants)
        tenantStore.setAvailableTenants(response.tenants)
        // Auto-select first tenant (this will fetch role and permissions)
        await selectTenant(response.tenants[0].id)
      }

      // eslint-disable-next-line no-console
      console.log('[AuthStore] Login complete')
      return response
    }

    const logout = () => {
      token.value = null
      role.value = null
      permissions.value = []
      user.value = null
      refreshToken.value = null

      // Clear tenant store on logout
      const tenantStore = useTenantStore()
      tenantStore.clearTenant()
    }

    const refreshAccessToken = async (): Promise<void> => {
      if (!refreshToken.value) {
        throw new Error('No refresh token available')
      }

      const { apiFetch } = useApi()

      try {
        const response = await apiFetch<{ accessToken: string }>(
          '/auth/refresh',
          {
            method: 'POST',
            body: { refreshToken: refreshToken.value },
          },
        )

        token.value = response.accessToken
      }
      catch (error) {
        logout()
        throw error
      }
    }

    const fetchCurrentUser = async (): Promise<void> => {
      const { apiFetch } = useApi()

      try {
        const apiResponse = await apiFetch<ApiResponse<User>>('/auth/me')
        user.value = apiResponse.data
      }
      catch (error) {
        logout()
        throw error
      }
    }

    const register = async (data: RegisterData): Promise<LoginResponse> => {
      const { apiFetch } = useApi()

      // eslint-disable-next-line no-console
      console.log('[AuthStore] Registering new tenant...', data.email)

      // Transform frontend data to match backend RegisterCommand
      const registerCommand = {
        companyName: data.companyName,
        slug: data.slug,
        name: data.name,
        email: data.email,
        password: data.password,
      }

      // Backend wraps response in { data, message, success }
      const apiResponse = await apiFetch<ApiResponse<LoginResponse>>('/auth/register', {
        method: 'POST',
        body: registerCommand,
      })

      // eslint-disable-next-line no-console
      console.log('[AuthStore] Registration successful:', apiResponse)

      const response = apiResponse.data

      // Auto-login after registration
      token.value = response.accessToken
      refreshToken.value = response.refreshToken
      user.value = response.user

      // Set available tenants and select the first one
      const tenantStore = useTenantStore()
      if (response.tenants && response.tenants.length > 0) {
        // eslint-disable-next-line no-console
        console.log('[AuthStore] Setting tenants:', response.tenants)
        tenantStore.setAvailableTenants(response.tenants)
        // Auto-select first tenant (this will fetch role and permissions)
        await selectTenant(response.tenants[0].id)
      }

      // eslint-disable-next-line no-console
      console.log('[AuthStore] Registration complete')
      return response
    }

    const hasPermission = (permission: string): boolean => {
      return permissions.value.includes(permission)
    }

    const hasAnyPermission = (requiredPermissions: string[]): boolean => {
      return requiredPermissions.some(p => permissions.value.includes(p))
    }

    const hasAllPermissions = (requiredPermissions: string[]): boolean => {
      return requiredPermissions.every(p => permissions.value.includes(p))
    }

    return {
      token,
      user,
      refreshToken,
      role,
      permissions,
      isAuthenticated,
      login,
      logout,
      selectTenant,
      refreshAccessToken,
      fetchCurrentUser,
      register,
      hasPermission,
      hasAnyPermission,
      hasAllPermissions,
    }
  },
  {
    persist: {
      storage: persistedState.localStorage,
      paths: ['token', 'refreshToken', 'user', 'role', 'permissions'],
    },
  },
)
