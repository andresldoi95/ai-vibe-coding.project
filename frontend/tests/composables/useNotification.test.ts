import { beforeEach, describe, expect, it, vi } from 'vitest'
import { useNotification } from '~/composables/useNotification'

// Mock PrimeVue's useToast
const mockToastAdd = vi.fn()
vi.mock('primevue/usetoast', () => ({
  useToast: () => ({
    add: mockToastAdd,
  }),
}))

describe('useNotification', () => {
  beforeEach(() => {
    mockToastAdd.mockClear()
  })

  describe('showSuccess', () => {
    it('should show success notification with message only', () => {
      const { showSuccess } = useNotification()

      showSuccess('Operation completed successfully')

      expect(mockToastAdd).toHaveBeenCalledWith({
        severity: 'success',
        summary: 'Operation completed successfully',
        detail: undefined,
        life: 3000,
      })
      expect(mockToastAdd).toHaveBeenCalledTimes(1)
    })

    it('should show success notification with message and detail', () => {
      const { showSuccess } = useNotification()

      showSuccess('Product created', 'The product was added to the catalog')

      expect(mockToastAdd).toHaveBeenCalledWith({
        severity: 'success',
        summary: 'Product created',
        detail: 'The product was added to the catalog',
        life: 3000,
      })
    })
  })

  describe('showError', () => {
    it('should show error notification with message only', () => {
      const { showError } = useNotification()

      showError('Failed to save data')

      expect(mockToastAdd).toHaveBeenCalledWith({
        severity: 'error',
        summary: 'Failed to save data',
        detail: undefined,
        life: 5000,
      })
      expect(mockToastAdd).toHaveBeenCalledTimes(1)
    })

    it('should show error notification with message and detail', () => {
      const { showError } = useNotification()

      showError('Validation failed', 'Please check the required fields')

      expect(mockToastAdd).toHaveBeenCalledWith({
        severity: 'error',
        summary: 'Validation failed',
        detail: 'Please check the required fields',
        life: 5000,
      })
    })

    it('should display error for longer duration than success', () => {
      const { showError, showSuccess } = useNotification()

      showSuccess('Success message')
      showError('Error message')

      const successCall = mockToastAdd.mock.calls[0][0]
      const errorCall = mockToastAdd.mock.calls[1][0]

      expect(errorCall.life).toBeGreaterThan(successCall.life)
    })
  })

  describe('showWarning', () => {
    it('should show warning notification with message only', () => {
      const { showWarning } = useNotification()

      showWarning('Action required')

      expect(mockToastAdd).toHaveBeenCalledWith({
        severity: 'warn',
        summary: 'Action required',
        detail: undefined,
        life: 4000,
      })
      expect(mockToastAdd).toHaveBeenCalledTimes(1)
    })

    it('should show warning notification with message and detail', () => {
      const { showWarning } = useNotification()

      showWarning('Low stock alert', 'Product inventory is running low')

      expect(mockToastAdd).toHaveBeenCalledWith({
        severity: 'warn',
        summary: 'Low stock alert',
        detail: 'Product inventory is running low',
        life: 4000,
      })
    })
  })

  describe('showInfo', () => {
    it('should show info notification with message only', () => {
      const { showInfo } = useNotification()

      showInfo('New feature available')

      expect(mockToastAdd).toHaveBeenCalledWith({
        severity: 'info',
        summary: 'New feature available',
        detail: undefined,
        life: 3000,
      })
      expect(mockToastAdd).toHaveBeenCalledTimes(1)
    })

    it('should show info notification with message and detail', () => {
      const { showInfo } = useNotification()

      showInfo('System update', 'The system will be updated tonight')

      expect(mockToastAdd).toHaveBeenCalledWith({
        severity: 'info',
        summary: 'System update',
        detail: 'The system will be updated tonight',
        life: 3000,
      })
    })
  })

  describe('showPermissionError', () => {
    it('should show permission error with predefined message', () => {
      const { showPermissionError } = useNotification()

      showPermissionError()

      expect(mockToastAdd).toHaveBeenCalledWith({
        severity: 'error',
        summary: 'Insufficient Permissions',
        detail: 'You do not have permission to perform this action. Contact your administrator.',
        life: 5000,
      })
      expect(mockToastAdd).toHaveBeenCalledTimes(1)
    })

    it('should always use error severity for permission errors', () => {
      const { showPermissionError } = useNotification()

      showPermissionError()

      const call = mockToastAdd.mock.calls[0][0]
      expect(call.severity).toBe('error')
    })
  })

  describe('toast instance', () => {
    it('should expose the toast instance', () => {
      const { toast } = useNotification()

      expect(toast).toBeDefined()
      expect(toast.add).toBe(mockToastAdd)
    })
  })

  describe('multiple notifications', () => {
    it('should handle multiple notifications in sequence', () => {
      const { showSuccess, showInfo, showWarning } = useNotification()

      showSuccess('First notification')
      showInfo('Second notification')
      showWarning('Third notification')

      expect(mockToastAdd).toHaveBeenCalledTimes(3)
      expect(mockToastAdd.mock.calls[0][0].severity).toBe('success')
      expect(mockToastAdd.mock.calls[1][0].severity).toBe('info')
      expect(mockToastAdd.mock.calls[2][0].severity).toBe('warn')
    })
  })

  describe('notification lifetimes', () => {
    it('should use correct lifetime for each severity', () => {
      const { showSuccess, showError, showWarning, showInfo } = useNotification()

      showSuccess('Success')
      showError('Error')
      showWarning('Warning')
      showInfo('Info')

      expect(mockToastAdd.mock.calls[0][0].life).toBe(3000)
      expect(mockToastAdd.mock.calls[1][0].life).toBe(5000)
      expect(mockToastAdd.mock.calls[2][0].life).toBe(4000)
      expect(mockToastAdd.mock.calls[3][0].life).toBe(3000)
    })
  })
})
