<template>
  <div class="flex items-center justify-center min-h-screen bg-gradient-primary p-4">
    <div class="w-full max-w-md">
      <Card class="shadow-2xl">
        <template #header>
          <div class="text-center py-8 bg-gradient-primary text-white rounded-t-lg">
            <i class="pi pi-receipt text-6xl mb-4 block"></i>
            <h1 class="text-3xl font-bold mb-2">Sistema de Facturación</h1>
            <p class="opacity-90 text-base">Ecuador - Cumplimiento SRI</p>
          </div>
        </template>

        <template #content>
          <!-- Login Form -->
          <form v-if="!showCompanySelection" @submit.prevent="handleLogin" class="flex flex-col gap-5">
            <div class="flex flex-col gap-2">
              <label for="email" class="font-semibold text-gray-700">Correo Electrónico</label>
              <InputText
                id="email"
                v-model="email"
                type="email"
                placeholder="correo@ejemplo.com"
                :class="{ 'p-invalid': errors.email }"
                autocomplete="email"
              />
              <small v-if="errors.email" class="p-error">{{ errors.email }}</small>
            </div>

            <div class="flex flex-col gap-2">
              <label for="password" class="font-semibold text-gray-700">Contraseña</label>
              <Password
                id="password"
                v-model="password"
                placeholder="Ingrese su contraseña"
                :class="{ 'p-invalid': errors.password }"
                :feedback="false"
                toggleMask
                autocomplete="current-password"
              />
              <small v-if="errors.password" class="p-error">{{ errors.password }}</small>
            </div>

            <div class="flex items-center gap-2">
              <Checkbox
                id="remember"
                v-model="rememberMe"
                :binary="true"
              />
              <label for="remember" class="cursor-pointer">Recordarme</label>
            </div>

            <Button
              type="submit"
              label="Iniciar Sesión"
              :loading="loading"
              :disabled="loading"
              icon="pi pi-sign-in"
              class="w-full py-3 text-base font-semibold"
            />

            <div v-if="errorMessage" class="mt-2">
              <Message severity="error" :closable="false">{{ errorMessage }}</Message>
            </div>
          </form>

          <!-- Company Selection -->
          <div v-else class="flex flex-col gap-5">
            <div class="text-center mb-4">
              <h2 class="text-xl font-semibold text-gray-800 mb-2">Seleccione una Empresa</h2>
              <p class="text-gray-600 text-sm">Tiene acceso a múltiples empresas. Seleccione una para continuar.</p>
            </div>

            <div class="flex flex-col gap-3">
              <div
                v-for="company in authStore.availableCompanies"
                :key="company.companyId"
                @click="handleCompanySelect(company.companyId)"
                class="p-4 border-2 rounded-lg cursor-pointer transition-all duration-200 hover:border-primary hover:bg-gray-50"
                :class="{ 'border-primary bg-blue-50': selectedCompanyId === company.companyId }"
              >
                <div class="flex items-start justify-between">
                  <div class="flex-1">
                    <h3 class="font-semibold text-gray-800">{{ company.companyName }}</h3>
                    <p class="text-sm text-gray-600">RUC: {{ company.companyRuc }}</p>
                    <p class="text-xs text-gray-500 mt-1">Rol: {{ company.roleName }}</p>
                  </div>
                  <i 
                    class="pi pi-check-circle text-2xl transition-all duration-200"
                    :class="selectedCompanyId === company.companyId ? 'text-primary' : 'text-gray-300'"
                  ></i>
                </div>
              </div>
            </div>

            <Button
              label="Continuar"
              :loading="loading"
              :disabled="!selectedCompanyId || loading"
              icon="pi pi-arrow-right"
              @click="confirmCompanySelection"
              class="w-full py-3 text-base font-semibold mt-2"
            />

            <Button
              label="Volver"
              outlined
              icon="pi pi-arrow-left"
              @click="backToLogin"
              class="w-full"
            />
          </div>
        </template>

        <template #footer v-if="!showCompanySelection">
          <div class="text-center space-y-3">
            <a href="#" class="text-primary no-underline hover:underline">¿Olvidó su contraseña?</a>
            <div class="border-t pt-3">
              <p class="text-gray-600 text-sm mb-2">¿No tiene una cuenta?</p>
              <router-link 
                to="/register" 
                class="text-primary font-semibold no-underline hover:underline"
              >
                Registre su empresa aquí
              </router-link>
            </div>
          </div>
        </template>
      </Card>

      <div class="mt-6" v-if="!showCompanySelection">
        <Message severity="info" :closable="false" class="shadow-md">
          <div class="leading-relaxed">
            <strong class="block mb-2">Credenciales de prueba:</strong>
            <div class="text-sm font-semibold">Usuario single-company:</div>
            <div class="text-sm ml-4">Email: admin@demo.com</div>
            <div class="text-sm ml-4">Contraseña: password123</div>
            <div class="text-sm font-semibold mt-2">Usuario multi-company:</div>
            <div class="text-sm ml-4">Email: multi@demo.com</div>
            <div class="text-sm ml-4">Contraseña: password123</div>
          </div>
        </Message>
      </div>
    </div>

    <Toast />
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useToast } from 'primevue/usetoast'
import Card from 'primevue/card'
import InputText from 'primevue/inputtext'
import Password from 'primevue/password'
import Button from 'primevue/button'
import Checkbox from 'primevue/checkbox'
import Message from 'primevue/message'
import Toast from 'primevue/toast'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()
const toast = useToast()

// Form data
const email = ref('')
const password = ref('')
const rememberMe = ref(false)

// State
const loading = ref(false)
const errorMessage = ref('')
const errors = ref<{ email?: string; password?: string }>({})
const showCompanySelection = ref(false)
const selectedCompanyId = ref<string | null>(null)

// Handle login
const handleLogin = async () => {
  // Reset errors
  errors.value = {}
  errorMessage.value = ''

  // Validate
  if (!email.value) {
    errors.value.email = 'El correo electrónico es requerido'
    return
  }

  if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.value)) {
    errors.value.email = 'Ingrese un correo electrónico válido'
    return
  }

  if (!password.value) {
    errors.value.password = 'La contraseña es requerida'
    return
  }

  if (password.value.length < 6) {
    errors.value.password = 'La contraseña debe tener al menos 6 caracteres'
    return
  }

  loading.value = true

  try {
    const response = await authStore.login({
      email: email.value,
      password: password.value
    })

    // If user has multiple companies, show selection
    if (response.requiresCompanySelection) {
      showCompanySelection.value = true
      loading.value = false
      return
    }

    // If single company or already has token, redirect
    toast.add({
      severity: 'success',
      summary: 'Inicio de sesión exitoso',
      detail: `Bienvenido ${authStore.user?.fullName || ''}`,
      life: 3000
    })

    // Redirect to intended page or dashboard
    const redirect = route.query.redirect as string || '/dashboard'
    router.push(redirect)
  } catch (error: any) {
    const errorMsg = error.response?.data?.error || error.message || 'Error al iniciar sesión'
    errorMessage.value = errorMsg
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: errorMsg,
      life: 5000
    })
  } finally {
    loading.value = false
  }
}

// Handle company selection
const handleCompanySelect = (companyId: string) => {
  selectedCompanyId.value = companyId
}

// Confirm company selection
const confirmCompanySelection = async () => {
  if (!selectedCompanyId.value) return

  loading.value = true

  try {
    await authStore.selectCompany(email.value, selectedCompanyId.value)

    toast.add({
      severity: 'success',
      summary: 'Empresa seleccionada',
      detail: `Bienvenido a ${authStore.currentCompany?.name || ''}`,
      life: 3000
    })

    // Redirect to intended page or dashboard
    const redirect = route.query.redirect as string || '/dashboard'
    router.push(redirect)
  } catch (error: any) {
    const errorMsg = error.response?.data?.error || error.message || 'Error al seleccionar empresa'
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: errorMsg,
      life: 5000
    })
  } finally {
    loading.value = false
  }
}

// Back to login
const backToLogin = () => {
  showCompanySelection.value = false
  selectedCompanyId.value = null
  password.value = ''
  errorMessage.value = ''
}
</script>
