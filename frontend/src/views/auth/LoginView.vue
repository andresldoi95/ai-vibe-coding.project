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
          <form @submit.prevent="handleLogin" class="flex flex-col gap-5">
            <div class="flex flex-col gap-2">
              <label for="email" class="font-medium text-gray-700">Correo Electrónico</label>
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
              <label for="password" class="font-medium text-gray-700">Contraseña</label>
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

            <div v-if="errorMessage" class="mt-4">
              <Message severity="error" :closable="false">{{ errorMessage }}</Message>
            </div>
          </form>
        </template>

        <template #footer>
          <div class="text-center">
            <a href="#" class="text-primary no-underline hover:underline">¿Olvidó su contraseña?</a>
          </div>
        </template>
      </Card>

      <div class="mt-6">
        <Message severity="info" :closable="false" class="shadow-md">
          <div class="leading-relaxed">
            <strong class="block mb-2">Credenciales de prueba:</strong>
            <div class="text-sm">Email: admin@example.com</div>
            <div class="text-sm">Contraseña: admin123</div>
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
    await authStore.login({
      email: email.value,
      password: password.value,
      rememberMe: rememberMe.value
    })

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
    errorMessage.value = error.message || 'Error al iniciar sesión. Verifique sus credenciales.'
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: errorMessage.value,
      life: 5000
    })
  } finally {
    loading.value = false
  }
}
</script>
