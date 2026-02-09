import { useToast } from 'primevue/usetoast'

export function useNotification() {
  const toast = useToast()

  const showSuccess = (message: string, detail?: string) => {
    toast.add({
      severity: 'success',
      summary: message,
      detail,
      life: 3000,
    })
  }

  const showError = (message: string, detail?: string) => {
    toast.add({
      severity: 'error',
      summary: message,
      detail,
      life: 5000,
    })
  }

  const showWarning = (message: string, detail?: string) => {
    toast.add({
      severity: 'warn',
      summary: message,
      detail,
      life: 4000,
    })
  }

  const showInfo = (message: string, detail?: string) => {
    toast.add({
      severity: 'info',
      summary: message,
      detail,
      life: 3000,
    })
  }

  const showPermissionError = () => {
    toast.add({
      severity: 'error',
      summary: 'Insufficient Permissions',
      detail: 'You do not have permission to perform this action. Contact your administrator.',
      life: 5000,
    })
  }

  return {
    toast,
    showSuccess,
    showError,
    showWarning,
    showInfo,
    showPermissionError,
  }
}
