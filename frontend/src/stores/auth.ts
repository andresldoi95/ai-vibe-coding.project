import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { IUser, IUserCompany, ISelectedCompany } from '@/types'
import { authAPI } from '@/api/auth'

export const useAuthStore = defineStore('auth', () => {
  // Initialize from localStorage
  const storedToken = localStorage.getItem('token')
  const storedUser = localStorage.getItem('user')
  const storedCompany = localStorage.getItem('company')
  
  const user = ref<IUser | null>(storedUser ? JSON.parse(storedUser) : null)
  const token = ref<string | null>(storedToken)
  const currentCompany = ref<ISelectedCompany | null>(storedCompany ? JSON.parse(storedCompany) : null)
  const availableCompanies = ref<IUserCompany[]>([])
  const isAuthenticated = computed(() => !!token.value)

  function setAuth(newToken: string, newUser: IUser, company: ISelectedCompany) {
    token.value = newToken
    user.value = newUser
    currentCompany.value = company
    localStorage.setItem('token', newToken)
    localStorage.setItem('user', JSON.stringify(newUser))
    localStorage.setItem('company', JSON.stringify(company))
  }

  function clearAuth() {
    token.value = null
    user.value = null
    currentCompany.value = null
    availableCompanies.value = []
    localStorage.removeItem('token')
    localStorage.removeItem('user')
    localStorage.removeItem('company')
  }

  async function login(credentials: { email: string; password: string }) {
    try {
      const response = await authAPI.login(credentials.email, credentials.password)
      
      // Store user and available companies
      user.value = response.user
      availableCompanies.value = response.companies
      
      // If user has only one company, auto-select it
      if (!response.requiresCompanySelection && response.companies.length === 1) {
        const company = response.companies[0]
        const selectResponse = await authAPI.selectCompany(credentials.email, company.companyId)
        setAuth(selectResponse.token, selectResponse.user, selectResponse.company)
      }
      
      return response
    } catch (error) {
      clearAuth()
      throw error
    }
  }

  async function selectCompany(email: string, companyId: string) {
    try {
      const response = await authAPI.selectCompany(email, companyId)
      setAuth(response.token, response.user, response.company)
      return response
    } catch (error) {
      clearAuth()
      throw error
    }
  }

  async function logout() {
    try {
      await authAPI.logout()
    } catch (error) {
      // Continue with logout even if API call fails
      console.error('Logout API error:', error)
    } finally {
      clearAuth()
    }
  }

  async function fetchCurrentUser() {
    try {
      if (!token.value) {
        return null
      }
      const response = await authAPI.getCurrentUser()
      user.value = response.user
      return response.user
    } catch (error) {
      clearAuth()
      throw error
    }
  }

  async function registerCompany(data: {
    company: {
      ruc: string
      businessName: string
      tradeName?: string
      email: string
      phone: string
      address: string
    }
    admin: {
      email: string
      password: string
      fullName: string
      phone?: string
    }
  }) {
    try {
      const response = await authAPI.registerCompany(data)
      
      // Auto-login after successful registration
      setAuth(
        response.data.token,
        {
          id: response.data.user.id,
          email: response.data.user.email,
          fullName: response.data.user.fullName,
          phone: response.data.user.phone,
          status: response.data.user.status
        },
        response.data.currentCompany
      )
      
      availableCompanies.value = response.data.user.companies
      
      return response
    } catch (error) {
      clearAuth()
      throw error
    }
  }

  return {
    user,
    token,
    currentCompany,
    availableCompanies,
    isAuthenticated,
    setAuth,
    clearAuth,
    login,
    selectCompany,
    logout,
    fetchCurrentUser,
    registerCompany
  }
})
