import { beforeEach, describe, expect, it, vi } from 'vitest'
import { useNotification } from '~/composables/useNotification'

// Mock PrimeVue's useToast
const mockAdd = vi.fn()
vi.mock('primevue/usetoast', () => ({
  useToast: () => ({
    add: mockAdd,
  }),
}))

describe('useNotification', () => {
  beforeEach(() => {
    // Reset all mocks before each test
    vi.clearAllMocks()
  })

  describe('showSuccess', () => {
    it('should call toast.add with success severity and message', () => {
      const { showSuccess } = useNotification()
      showSuccess('Operation successful')

      expect(mockAdd).toHaveBeenCalledTimes(1)
      expect(mockAdd).toHaveBeenCalledWith({
        severity: 'success',
        summary: 'Operation successful',
        detail: undefined,
        life: 3000,
      })
    })

    it('should call toast.add with success severity, message, and detail', () => {
      const { showSuccess } = useNotification()
      showSuccess('Operation successful', 'The item was created successfully')

      expect(mockAdd).toHaveBeenCalledTimes(1)
      expect(mockAdd).toHaveBeenCalledWith({
        severity: 'success',
        summary: 'Operation successful',
        detail: 'The item was created successfully',
        life: 3000,
      })
    })
  })

  describe('showError', () => {
    it('should call toast.add with error severity and message', () => {
      const { showError } = useNotification()
      showError('Operation failed')

      expect(mockAdd).toHaveBeenCalledTimes(1)
      expect(mockAdd).toHaveBeenCalledWith({
        severity: 'error',
        summary: 'Operation failed',
        detail: undefined,
        life: 5000,
      })
    })

    it('should call toast.add with error severity, message, and detail', () => {
      const { showError } = useNotification()
      showError('Operation failed', 'Unable to connect to server')

      expect(mockAdd).toHaveBeenCalledTimes(1)
      expect(mockAdd).toHaveBeenCalledWith({
        severity: 'error',
        summary: 'Operation failed',
        detail: 'Unable to connect to server',
        life: 5000,
      })
    })
  })

  describe('showWarning', () => {
    it('should call toast.add with warn severity and message', () => {
      const { showWarning } = useNotification()
      showWarning('Warning message')

      expect(mockAdd).toHaveBeenCalledTimes(1)
      expect(mockAdd).toHaveBeenCalledWith({
        severity: 'warn',
        summary: 'Warning message',
        detail: undefined,
        life: 4000,
      })
    })

    it('should call toast.add with warn severity, message, and detail', () => {
      const { showWarning } = useNotification()
      showWarning('Warning message', 'This action may have consequences')

      expect(mockAdd).toHaveBeenCalledTimes(1)
      expect(mockAdd).toHaveBeenCalledWith({
        severity: 'warn',
        summary: 'Warning message',
        detail: 'This action may have consequences',
        life: 4000,
      })
    })
  })

  describe('showInfo', () => {
    it('should call toast.add with info severity and message', () => {
      const { showInfo } = useNotification()
      showInfo('Information message')

      expect(mockAdd).toHaveBeenCalledTimes(1)
      expect(mockAdd).toHaveBeenCalledWith({
        severity: 'info',
        summary: 'Information message',
        detail: undefined,
        life: 3000,
      })
    })

    it('should call toast.add with info severity, message, and detail', () => {
      const { showInfo } = useNotification()
      showInfo('Information message', 'Here is some additional information')

      expect(mockAdd).toHaveBeenCalledTimes(1)
      expect(mockAdd).toHaveBeenCalledWith({
        severity: 'info',
        summary: 'Information message',
        detail: 'Here is some additional information',
        life: 3000,
      })
    })
  })

  describe('showPermissionError', () => {
    it('should call toast.add with fixed permission error message', () => {
      const { showPermissionError } = useNotification()
      showPermissionError()

      expect(mockAdd).toHaveBeenCalledTimes(1)
      expect(mockAdd).toHaveBeenCalledWith({
        severity: 'error',
        summary: 'Insufficient Permissions',
        detail: 'You do not have permission to perform this action. Contact your administrator.',
        life: 5000,
      })
    })
  })

  describe('toast instance', () => {
    it('should return toast instance', () => {
      const { toast } = useNotification()
      expect(toast).toBeDefined()
      expect(toast.add).toBe(mockAdd)
    })
  })
})
