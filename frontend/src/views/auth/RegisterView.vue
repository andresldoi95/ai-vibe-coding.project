<template>
  <div class="min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-50 to-indigo-100 py-12 px-4 sm:px-6 lg:px-8">
    <div class="max-w-4xl w-full bg- white rounded-lg shadow-xl p-8">
      <!-- Header -->
      <div class="text-center mb-8">
        <h2 class="text-3xl font-bold text-gray-900">Registro de Empresa</h2>
        <p class="mt-2 text-sm text-gray-600">
          Crea tu cuenta y comienza a facturar electrónicamente
        </p>
      </div>

      <!-- Stepper -->
      <Stepper :active-step="currentStep" class="mb-8">
        <StepperPanel header="Datos de la Empresa">
          <template #content="{ nextCallback }">
            <div class="space-y-6">
              <!-- RUC -->
              <div>
                <label for="ruc" class="block text-sm font-medium text-gray-700 mb-1">
                  RUC <span class="text-red-500">*</span>
                </label>
                <InputText
                  id="ruc"
                  v-model="companyForm.ruc"
                  :class="{ 'p-invalid': rucError }"
                  class="w-full"
                  placeholder="1234567890001"
                  maxlength="13"
                  @blur="validateRucField"
                />
                <small v-if="rucError" class="text-red-500">{{ rucError }}</small>
                <small v-else class="text-gray-500">13 dígitos</small>
              </div>

              <!-- Business Name -->
              <div>
                <label for="businessName" class="block text-sm font-medium text-gray-700 mb-1">
                  Razón Social <span class="text-red-500">*</span>
                </label>
                <InputText
                  id="businessName"
                  v-model="companyForm.businessName"
                  :class="{ 'p-invalid': errors.businessName }"
                  class="w-full"
                  placeholder="Mi Empresa S.A."
                />
                <small v-if="errors.businessName" class="text-red-500">{{ errors.businessName }}</small>
              </div>

              <!-- Trade Name -->
              <div>
                <label for="tradeName" class="block text-sm font-medium text-gray-700 mb-1">
                  Nombre Comercial
                </label>
                <InputText
                  id="tradeName"
                  v-model="companyForm.tradeName"
                  class="w-full"
                  placeholder="Mi Negocio"
                />
                <small class="text-gray-500">Opcional</small>
              </div>

              <!-- Email -->
              <div>
                <label for="companyEmail" class="block text-sm font-medium text-gray-700 mb-1">
                  Correo Electrónico <span class="text-red-500">*</span>
                </label>
                <InputText
                  id="companyEmail"
                  v-model="companyForm.email"
                  :class="{ 'p-invalid': errors.companyEmail }"
                  class="w-full"
                  type="email"
                  placeholder="empresa@ejemplo.com"
                />
                <small v-if="errors.companyEmail" class="text-red-500">{{ errors.companyEmail }}</small>
              </div>

              <!-- Phone -->
              <div>
                <label for="companyPhone" class="block text-sm font-medium text-gray-700 mb-1">
                  Teléfono <span class="text-red-500">*</span>
                </label>
                <InputText
                  id="companyPhone"
                  v-model="companyForm.phone"
                  :class="{ 'p-invalid': errors.companyPhone }"
                  class="w-full"
                  placeholder="02-2345678 o 0991234567"
                />
                <small v-if="errors.companyPhone" class="text-red-500">{{ errors.companyPhone }}</small>
              </div>

              <!-- Address -->
              <div>
                <label for="address" class="block text-sm font-medium text-gray-700 mb-1">
                  Dirección <span class="text-red-500">*</span>
                </label>
                <Textarea
                  id="address"
                  v-model="companyForm.address"
                  :class="{ 'p-invalid': errors.address }"
                  class="w-full"
                  rows="3"
                  placeholder="Calle principal, sector, ciudad"
                />
                <small v-if="errors.address" class="text-red-500">{{ errors.address }}</small>
              </div>

              <!-- Navigation -->
              <div class="flex justify-end gap-2 pt-4">
                <Button
                  label="Siguiente"
                  icon="pi pi-arrow-right"
                  icon-pos="right"
                  @click="() => validateStep1(nextCallback)"
                  :disabled="isValidatingRuc"
                />
              </div>
            </div>
          </template>
        </StepperPanel>

        <StepperPanel header="Datos del Administrador">
          <template #content="{ prevCallback, nextCallback }">
            <div class="space-y-6">
              <!-- Full Name -->
              <div>
                <label for="fullName" class="block text-sm font-medium text-gray-700 mb-1">
                  Nombre Completo <span class="text-red-500">*</span>
                </label>
                <InputText
                  id="fullName"
                  v-model="adminForm.fullName"
                  :class="{ 'p-invalid': errors.fullName }"
                  class="w-full"
                  placeholder="Juan Pérez"
                />
                <small v-if="errors.fullName" class="text-red-500">{{ errors.fullName }}</small>
              </div>

              <!-- Email -->
              <div>
                <label for="adminEmail" class="block text-sm font-medium text-gray-700 mb-1">
                  Correo Electrónico <span class="text-red-500">*</span>
                </label>
                <InputText
                  id="adminEmail"
                  v-model="adminForm.email"
                  :class="{ 'p-invalid': errors.adminEmail }"
                  class="w-full"
                  type="email"
                  placeholder="admin@ejemplo.com"
                />
                <small v-if="errors.adminEmail" class="text-red-500">{{ errors.adminEmail }}</small>
              </div>

              <!-- Password -->
              <div>
                <label for="password" class="block text-sm font-medium text-gray-700 mb-1">
                  Contraseña <span class="text-red-500">*</span>
                </label>
                <Password
                  id="password"
                  v-model="adminForm.password"
                  :class="{ 'p-invalid': errors.password }"
                  class="w-full"
                  toggle-mask
                  :feedback="false"
                  placeholder="Mínimo 6 caracteres"
                />
                <small v-if="errors.password" class="text-red-500">{{ errors.password }}</small>
              </div>

              <!-- Confirm Password -->
              <div>
                <label for="confirmPassword" class="block text-sm font-medium text-gray-700 mb-1">
                  Confirmar Contraseña <span class="text-red-500">*</span>
                </label>
                <Password
                  id="confirmPassword"
                  v-model="adminForm.confirmPassword"
                  :class="{ 'p-invalid': errors.confirmPassword }"
                  class="w-full"
                  toggle-mask
                  :feedback="false"
                  placeholder="Repita su contraseña"
                />
                <small v-if="errors.confirmPassword" class="text-red-500">{{ errors.confirmPassword }}</small>
              </div>

              <!-- Phone (Optional) -->
              <div>
                <label for="adminPhone" class="block text-sm font-medium text-gray-700 mb-1">
                  Teléfono
                </label>
                <InputText
                  id="adminPhone"
                  v-model="adminForm.phone"
                  class="w-full"
                  placeholder="0991234567"
                />
                <small class="text-gray-500">Opcional</small>
              </div>

              <!-- Navigation -->
              <div class="flex justify-between gap-2 pt-4">
                <Button
                  label="Anterior"
                  severity="secondary"
                  icon="pi pi-arrow-left"
                  @click="() => prevCallback()"
                />
                <Button
                  label="Siguiente"
                  icon="pi pi-arrow-right"
                  icon-pos="right"
                  @click="() => validateStep2(nextCallback)"
                />
              </div>
            </div>
          </template>
        </StepperPanel>

        <StepperPanel header="Confirmar y Registrar">
          <template #content="{ prevCallback }">
            <div class="space-y-6">
              <!-- Summary -->
              <div class="bg-gray-50 rounded-lg p-6 space-y-4">
                <div>
                  <h3 class="text-lg font-semibold text-gray-900 mb-4">Resumen de Registro</h3>
                </div>

                <div class="border-t border-gray-200 pt-4">
                  <h4 class="font-medium text-gray-700 mb-2">Datos de la Empresa</h4>
                  <dl class="grid grid-cols-1 gap-2 text-sm">
                    <div class="flex justify-between">
                      <dt class="text-gray-600">RUC:</dt>
                      <dd class="font-medium text-gray-900">{{ companyForm.ruc }}</dd>
                    </div>
                    <div class="flex justify-between">
                      <dt class="text-gray-600">Razón Social:</dt>
                      <dd class="font-medium text-gray-900">{{ companyForm.businessName }}</dd>
                    </div>
                    <div v-if="companyForm.tradeName" class="flex justify-between">
                      <dt class="text-gray-600">Nombre Comercial:</dt>
                      <dd class="font-medium text-gray-900">{{ companyForm.tradeName }}</dd>
                    </div>
                    <div class="flex justify-between">
                      <dt class="text-gray-600">Email:</dt>
                      <dd class="font-medium text-gray-900">{{ companyForm.email }}</dd>
                    </div>
                    <div class="flex justify-between">
                      <dt class="text-gray-600">Teléfono:</dt>
                      <dd class="font-medium text-gray-900">{{ companyForm.phone }}</dd>
                    </div>
                    <div class="flex justify-between">
                      <dt class="text-gray-600">Dirección:</dt>
                      <dd class="font-medium text-gray-900">{{ companyForm.address }}</dd>
                    </div>
                  </dl>
                </div>

                <div class="border-t border-gray-200 pt-4">
                  <h4 class="font-medium text-gray-700 mb-2">Administrador</h4>
                  <dl class="grid grid-cols-1 gap-2 text-sm">
                    <div class="flex justify-between">
                      <dt class="text-gray-600">Nombre:</dt>
                      <dd class="font-medium text-gray-900">{{ adminForm.fullName }}</dd>
                    </div>
                    <div class="flex justify-between">
                      <dt class="text-gray-600">Email:</dt>
                      <dd class="font-medium text-gray-900">{{ adminForm.email }}</dd>
                    </div>
                    <div v-if="adminForm.phone" class="flex justify-between">
                      <dt class="text-gray-600">Teléfono:</dt>
                      <dd class="font-medium text-gray-900">{{ adminForm.phone }}</dd>
                    </div>
                  </dl>
                </div>
              </div>

              <!-- Terms Agreement -->
              <div class="flex items-start gap-3">
                <Checkbox
                  v-model="acceptTerms"
                  input-id="acceptTerms"
                  :binary="true"
                  :class="{ 'p-invalid': showTermsError }"
                />
                <label for="acceptTerms" class="text-sm text-gray-700 cursor-pointer">
                  Acepto los términos y condiciones de uso del sistema de facturación electrónica
                  <span class="text-red-500">*</span>
                </label>
              </div>
              <small v-if="showTermsError" class="text-red-500">Debe aceptar los términos y condiciones</small>

              <!-- Error Message -->
              <Message v-if="registrationError" severity="error" :closable="false">
                {{ registrationError }}
              </Message>

              <!-- Navigation -->
              <div class="flex justify-between gap-2 pt-4">
                <Button
                  label="Anterior"
                  severity="secondary"
                  icon="pi pi-arrow-left"
                  @click="() => prevCallback()"
                  :disabled="isRegistering"
                />
                <Button
                  label="Registrar Empresa"
                  icon="pi pi-check"
                  icon-pos="right"
                  @click="handleRegister"
                  :loading="isRegistering"
                  :disabled="isRegistering"
                />
              </div>
            </div>
          </template>
        </StepperPanel>
      </Stepper>

      <!-- Login Link -->
      <div class ="text-center mt-6">
        <p class="text-sm text-gray-600">
          ¿Ya tienes una cuenta?
          <router-link to="/login" class="font-medium text-blue-600 hover:text-blue-500">
            Inicia sesión aquí
          </router-link>
        </p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useDocumentNumber } from '@/composables/useDocumentNumber'
import Stepper from 'primevue/stepper'
import StepperPanel from 'primevue/stepperpanel'
import InputText from 'primevue/inputtext'
import Textarea from 'primevue/textarea'
import Password from 'primevue/password'
import Button from 'primevue/button'
import Checkbox from 'primevue/checkbox'
import Message from 'primevue/message'

const router = useRouter()
const authStore = useAuthStore()
const { validateRUC } = useDocumentNumber()

// Form data
const companyForm = reactive({
  ruc: '',
  businessName: '',
  tradeName: '',
  email: '',
  phone: '',
  address: ''
})

const adminForm = reactive({
  fullName: '',
  email: '',
  password: '',
  confirmPassword: '',
  phone: ''
})

// State
const currentStep = ref(0)
const acceptTerms = ref(false)
const showTermsError = ref(false)
const isRegistering = ref(false)
const isValidatingRuc = ref(false)
const registrationError = ref('')
const rucError = ref('')

// Validation errors
const errors = reactive({
  businessName: '',
  companyEmail: '',
  companyPhone: '',
  address: '',
  fullName: '',
  adminEmail: '',
  password: '',
  confirmPassword: ''
})

// Validate RUC field
const validateRucField = async () => {
  rucError.value = ''
  
  if (!companyForm.ruc) {
    rucError.value = 'RUC es requerido'
    return false
  }

  const isValid = validateRUC(companyForm.ruc)
  if (!isValid) {
    rucError.value = 'RUC inválido - verifique el formato y dígito verificador'
    return false
  }

  return true
}

// Validate Step 1 - Company Info
const validateStep1 = async (nextCallback: () => void) => {
  // Clear errors
  errors.businessName = ''
  errors.companyEmail = ''
  errors.companyPhone = ''
  errors.address = ''
  
  let isValid = true

  // Validate RUC
  const rucValid = await validateRucField()
  if (!rucValid) {
    isValid = false
  }

  // Validate business name
  if (!companyForm.businessName.trim()) {
    errors.businessName = 'Razón social es requerida'
    isValid = false
  } else if (companyForm.businessName.trim().length < 3) {
    errors.businessName = 'Razón social debe tener al menos 3 caracteres'
    isValid = false
  }

  // Validate email
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
  if (!companyForm.email.trim()) {
    errors.companyEmail = 'Correo electrónico es requerido'
    isValid = false
  } else if (!emailRegex.test(companyForm.email)) {
    errors.companyEmail = 'Formato de correo inválido'
    isValid = false
  }

  // Validate phone
  if (!companyForm.phone.trim()) {
    errors.companyPhone = 'Teléfono es requerido'
    isValid = false
  } else if (companyForm.phone.trim().length < 7) {
    errors.companyPhone = 'Teléfono debe tener al menos 7 caracteres'
    isValid = false
  }

  // Validate address
  if (!companyForm.address.trim()) {
    errors.address = 'Dirección es requerida'
    isValid = false
  } else if (companyForm.address.trim().length < 5) {
    errors.address = 'Dirección debe tener al menos 5 caracteres'
    isValid = false
  }

  if (isValid) {
    nextCallback()
  }
}

// Validate Step 2 - Admin Info
const validateStep2 = (nextCallback: () => void) => {
  // Clear errors
  errors.fullName = ''
  errors.adminEmail = ''
  errors.password = ''
  errors.confirmPassword = ''
  
  let isValid = true

  // Validate full name
  if (!adminForm.fullName.trim()) {
    errors.fullName = 'Nombre completo es requerido'
    isValid = false
  } else if (adminForm.fullName.trim().length < 3) {
    errors.fullName = 'Nombre debe tener al menos 3 caracteres'
    isValid = false
  }

  // Validate email
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
  if (!adminForm.email.trim()) {
    errors.adminEmail = 'Correo electrónico es requerido'
    isValid = false
  } else if (!emailRegex.test(adminForm.email)) {
    errors.adminEmail = 'Formato de correo inválido'
    isValid = false
  }

  // Validate password
  if (!adminForm.password) {
    errors.password = 'Contraseña es requerida'
    isValid = false
  } else if (adminForm.password.length < 6) {
    errors.password = 'Contraseña debe tener al menos 6 caracteres'
    isValid = false
  }

  // Validate confirm password
  if (!adminForm.confirmPassword) {
    errors.confirmPassword = 'Debe confirmar su contraseña'
    isValid = false
  } else if (adminForm.password !== adminForm.confirmPassword) {
    errors.confirmPassword = 'Las contraseñas no coinciden'
    isValid = false
  }

  if (isValid) {
    nextCallback()
  }
}

// Handle registration
const handleRegister = async () => {
  showTermsError.value = false
  registrationError.value = ''

  // Validate terms acceptance
  if (!acceptTerms.value) {
    showTermsError.value = true
    return
  }

  isRegistering.value = true

  try {
    await authStore.registerCompany({
      company: {
        ruc: companyForm.ruc,
        businessName: companyForm.businessName,
        tradeName: companyForm.tradeName || undefined,
        email: companyForm.email,
        phone: companyForm.phone,
        address: companyForm.address
      },
      admin: {
        email: adminForm.email,
        password: adminForm.password,
        fullName: adminForm.fullName,
        phone: adminForm.phone || undefined
      }
    })

    // Registration successful, redirect to dashboard
    router.push('/')
  } catch (error: any) {
    registrationError.value = error.response?.data?.message || 'Error al registrar la empresa. Intente nuevamente.'
  } finally {
    isRegistering.value = false
  }
}
</script>
