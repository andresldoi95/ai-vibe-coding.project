import axios, { type AxiosInstance, type AxiosRequestConfig } from 'axios'
import { useAuthStore } from '@/stores/auth'
import router from '@/router'

const apiClient: AxiosInstance = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:3000/api/v1',
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json'
  }
})

// Request interceptor - Add auth token
apiClient.interceptors.request.use(
  (config) => {
    const authStore = useAuthStore()
    if (authStore.token) {
      config.headers.Authorization = `Bearer ${authStore.token}`
    }
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// Response interceptor - Handle errors
apiClient.interceptors.response.use(
  (response) => {
    return response
  },
  (error) => {
    if (error.response?.status === 401) {
      // Unauthorized - clear auth and redirect to login
      const authStore = useAuthStore()
      authStore.clearAuth()
      router.push({ name: 'login' })
    }
    return Promise.reject(error)
  }
)

export default apiClient

// API helper functions
export const api = {
  get: <T>(url: string, config?: AxiosRequestConfig) =>
    apiClient.get<T>(url, config).then(res => res.data),
  
  post: <T>(url: string, data?: any, config?: AxiosRequestConfig) =>
    apiClient.post<T>(url, data, config).then(res => res.data),
  
  put: <T>(url: string, data?: any, config?: AxiosRequestConfig) =>
    apiClient.put<T>(url, data, config).then(res => res.data),
  
  patch: <T>(url: string, data?: any, config?: AxiosRequestConfig) =>
    apiClient.patch<T>(url, data, config).then(res => res.data),
  
  delete: <T>(url: string, config?: AxiosRequestConfig) =>
    apiClient.delete<T>(url, config).then(res => res.data)
}
