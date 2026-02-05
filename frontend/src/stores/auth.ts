import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { IUser } from '@/types'

export const useAuthStore = defineStore('auth', () => {
  // Initialize from localStorage
  const storedToken = localStorage.getItem('token')
  const storedUser = localStorage.getItem('user')
  
  const user = ref<IUser | null>(storedUser ? JSON.parse(storedUser) : null)
  const token = ref<string | null>(storedToken)
  const isAuthenticated = computed(() => !!token.value)

  function setAuth(newToken: string, newUser: IUser) {
    token.value = newToken
    user.value = newUser
    localStorage.setItem('token', newToken)
    localStorage.setItem('user', JSON.stringify(newUser))
  }

  function clearAuth() {
    token.value = null
    user.value = null
    localStorage.removeItem('token')
    localStorage.removeItem('user')
  }

  async function login(credentials: { email: string; password: string; rememberMe?: boolean }) {
    // TODO: Implement login API call
    // For now, mock successful login
    const mockUser: IUser = {
      id: '1',
      email: credentials.email,
      fullName: 'Usuario Demo',
      phone: '',
      status: 'active'
    }
    setAuth('mock-jwt-token', mockUser)
  }

  async function logout() {
    clearAuth()
    // TODO: Implement logout API call
  }

  async function fetchCurrentUser() {
    // TODO: Implement fetch current user API call
  }

  return {
    user,
    token,
    isAuthenticated,
    setAuth,
    clearAuth,
    login,
    logout,
    fetchCurrentUser
  }
})
