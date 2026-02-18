import { beforeEach, describe, expect, it } from 'vitest'
import { mockApiFetch } from '../setup'
import { useSriConfiguration } from '~/composables/useSriConfiguration'
import { SriEnvironment } from '~/types/sri-enums'
import type { CertificateInfo, SriConfiguration, UpdateSriConfigurationDto, UploadCertificateDto } from '~/types/sri-configuration'

describe('useSriConfiguration', () => {
  beforeEach(() => {
    mockApiFetch.mockReset()
  })

  describe('getSriConfiguration', () => {
    it('should fetch SRI configuration successfully', async () => {
      const mockConfig: SriConfiguration = {
        id: '1',
        companyRuc: '1234567890001',
        legalName: 'Company Legal Name S.A.',
        tradeName: 'Company Trade Name',
        mainAddress: 'Main St 123, Quito, Ecuador',
        phone: '+593-2-123-4567',
        email: 'info@company.com',
        accountingRequired: true,
        specialTaxpayerNumber: '12345',
        isRiseRegime: false,
        environment: SriEnvironment.Test,
        isCertificateConfigured: true,
        certificateExpiryDate: '2025-12-31T23:59:59Z',
        isCertificateValid: true,
        tenantId: 'tenant-1',
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockConfig, success: true })

      const { getSriConfiguration } = useSriConfiguration()
      const result = await getSriConfiguration()

      expect(mockApiFetch).toHaveBeenCalledWith('/sri-configuration', {
        method: 'GET',
      })
      expect(result).toEqual(mockConfig)
      expect(result.companyRuc).toBe('1234567890001')
      expect(result.environment).toBe(SriEnvironment.Test)
    })

    it('should fetch SRI configuration for production environment', async () => {
      const mockConfig: SriConfiguration = {
        id: '1',
        companyRuc: '9876543210001',
        legalName: 'Production Company S.A.',
        tradeName: 'Prod Co',
        mainAddress: 'Production Ave 456, Guayaquil, Ecuador',
        phone: '+593-4-987-6543',
        email: 'prod@company.com',
        accountingRequired: true,
        isRiseRegime: false,
        environment: SriEnvironment.Production,
        isCertificateConfigured: true,
        certificateExpiryDate: '2026-06-30T23:59:59Z',
        isCertificateValid: true,
        tenantId: 'tenant-1',
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-15T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockConfig, success: true })

      const { getSriConfiguration } = useSriConfiguration()
      const result = await getSriConfiguration()

      expect(result.environment).toBe(SriEnvironment.Production)
    })

    it('should fetch SRI configuration for RISE regime company', async () => {
      const mockConfig: SriConfiguration = {
        id: '1',
        companyRuc: '1111111111001',
        legalName: 'RISE Company',
        tradeName: 'RISE Trade',
        mainAddress: 'Small Business St, Cuenca, Ecuador',
        phone: '+593-7-111-1111',
        email: 'rise@company.com',
        accountingRequired: false,
        isRiseRegime: true,
        environment: SriEnvironment.Production,
        isCertificateConfigured: false,
        isCertificateValid: false,
        tenantId: 'tenant-1',
        createdAt: '2024-02-01T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockConfig, success: true })

      const { getSriConfiguration } = useSriConfiguration()
      const result = await getSriConfiguration()

      expect(result.isRiseRegime).toBe(true)
      expect(result.accountingRequired).toBe(false)
    })

    it('should handle API errors when fetching SRI configuration', async () => {
      mockApiFetch.mockRejectedValue(new Error('Configuration not found'))

      const { getSriConfiguration } = useSriConfiguration()

      await expect(getSriConfiguration()).rejects.toThrow('Configuration not found')
    })
  })

  describe('updateSriConfiguration', () => {
    it('should update SRI configuration successfully', async () => {
      const updateData: UpdateSriConfigurationDto = {
        companyRuc: '1234567890001',
        legalName: 'Updated Legal Name S.A.',
        tradeName: 'Updated Trade Name',
        mainAddress: 'Updated Address 789, Quito, Ecuador',
        phone: '+593-2-999-9999',
        email: 'updated@company.com',
        accountingRequired: true,
        specialTaxpayerNumber: '54321',
        isRiseRegime: false,
        environment: SriEnvironment.Production,
      }

      const mockUpdatedConfig: SriConfiguration = {
        id: '1',
        ...updateData,
        isCertificateConfigured: true,
        certificateExpiryDate: '2025-12-31T23:59:59Z',
        isCertificateValid: true,
        tenantId: 'tenant-1',
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-02-01T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockUpdatedConfig, success: true })

      const { updateSriConfiguration } = useSriConfiguration()
      const result = await updateSriConfiguration(updateData)

      expect(mockApiFetch).toHaveBeenCalledWith('/sri-configuration', {
        method: 'PUT',
        body: updateData,
      })
      expect(result).toEqual(mockUpdatedConfig)
      expect(result.legalName).toBe('Updated Legal Name S.A.')
      expect(result.environment).toBe(SriEnvironment.Production)
    })

    it('should update configuration to test environment', async () => {
      const updateData: UpdateSriConfigurationDto = {
        companyRuc: '1234567890001',
        legalName: 'Test Company S.A.',
        tradeName: 'Test Co',
        mainAddress: 'Test St 123',
        phone: '+593-2-000-0000',
        email: 'test@company.com',
        accountingRequired: true,
        isRiseRegime: false,
        environment: SriEnvironment.Test,
      }

      const mockUpdatedConfig: SriConfiguration = {
        id: '1',
        ...updateData,
        isCertificateConfigured: false,
        isCertificateValid: false,
        tenantId: 'tenant-1',
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-02-02T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockUpdatedConfig, success: true })

      const { updateSriConfiguration } = useSriConfiguration()
      const result = await updateSriConfiguration(updateData)

      expect(result.environment).toBe(SriEnvironment.Test)
    })

    it('should update configuration without optional fields', async () => {
      const updateData: UpdateSriConfigurationDto = {
        companyRuc: '9999999999001',
        legalName: 'Minimal Config Company',
        tradeName: 'Min Config',
        mainAddress: 'Min Address',
        phone: '+593-9-000-0000',
        email: 'min@company.com',
        accountingRequired: false,
        isRiseRegime: true,
        environment: SriEnvironment.Test,
      }

      const mockUpdatedConfig: SriConfiguration = {
        id: '1',
        ...updateData,
        isCertificateConfigured: false,
        isCertificateValid: false,
        tenantId: 'tenant-1',
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-02-03T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockUpdatedConfig, success: true })

      const { updateSriConfiguration } = useSriConfiguration()
      const result = await updateSriConfiguration(updateData)

      expect(result.specialTaxpayerNumber).toBeUndefined()
      expect(result.isRiseRegime).toBe(true)
    })

    it('should handle API errors when updating SRI configuration', async () => {
      const updateData: UpdateSriConfigurationDto = {
        companyRuc: 'invalid',
        legalName: 'Test',
        tradeName: 'Test',
        mainAddress: 'Test',
        phone: 'Test',
        email: 'invalid-email',
        accountingRequired: true,
        isRiseRegime: false,
        environment: SriEnvironment.Test,
      }

      mockApiFetch.mockRejectedValue(new Error('Invalid RUC format'))

      const { updateSriConfiguration } = useSriConfiguration()

      await expect(updateSriConfiguration(updateData)).rejects.toThrow('Invalid RUC format')
    })
  })

  describe('uploadCertificate', () => {
    it('should upload certificate successfully', async () => {
      const mockFile = new File(['certificate content'], 'certificate.p12', { type: 'application/x-pkcs12' })
      const uploadData: UploadCertificateDto = {
        certificateFile: mockFile,
        certificatePassword: 'secure-password',
      }

      const mockUpdatedConfig: SriConfiguration = {
        id: '1',
        companyRuc: '1234567890001',
        legalName: 'Company S.A.',
        tradeName: 'Company',
        mainAddress: 'Address 123',
        phone: '+593-2-123-4567',
        email: 'info@company.com',
        accountingRequired: true,
        isRiseRegime: false,
        environment: SriEnvironment.Test,
        isCertificateConfigured: true,
        certificateExpiryDate: '2025-12-31T23:59:59Z',
        isCertificateValid: true,
        tenantId: 'tenant-1',
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-02-04T00:00:00Z',
      }

      mockApiFetch.mockResolvedValue({ data: mockUpdatedConfig, success: true })

      const { uploadCertificate } = useSriConfiguration()
      const result = await uploadCertificate(uploadData)

      expect(mockApiFetch).toHaveBeenCalledWith('/sri-configuration/certificate', {
        method: 'POST',
        body: expect.any(FormData),
      })
      expect(result).toEqual(mockUpdatedConfig)
      expect(result.isCertificateConfigured).toBe(true)
      expect(result.isCertificateValid).toBe(true)
    })

    it('should handle API errors when uploading invalid certificate', async () => {
      const mockFile = new File(['invalid content'], 'invalid.txt', { type: 'text/plain' })
      const uploadData: UploadCertificateDto = {
        certificateFile: mockFile,
        certificatePassword: 'wrong-password',
      }

      mockApiFetch.mockRejectedValue(new Error('Invalid certificate format'))

      const { uploadCertificate } = useSriConfiguration()

      await expect(uploadCertificate(uploadData)).rejects.toThrow('Invalid certificate format')
    })

    it('should handle API errors for incorrect certificate password', async () => {
      const mockFile = new File(['certificate content'], 'certificate.p12', { type: 'application/x-pkcs12' })
      const uploadData: UploadCertificateDto = {
        certificateFile: mockFile,
        certificatePassword: 'wrong-password',
      }

      mockApiFetch.mockRejectedValue(new Error('Incorrect certificate password'))

      const { uploadCertificate } = useSriConfiguration()

      await expect(uploadCertificate(uploadData)).rejects.toThrow('Incorrect certificate password')
    })
  })

  describe('getCertificateInfo', () => {
    it('should get certificate info successfully', async () => {
      const mockCertInfo: CertificateInfo = {
        subject: 'CN=Company S.A., O=Company, C=EC',
        issuer: 'CN=Security Data Signing CA, O=Security Data, C=EC',
        validFrom: '2024-01-01T00:00:00Z',
        validTo: '2025-12-31T23:59:59Z',
        isValid: true,
      }

      mockApiFetch.mockResolvedValue({ data: mockCertInfo, success: true })

      const { getCertificateInfo } = useSriConfiguration()
      const result = await getCertificateInfo()

      expect(mockApiFetch).toHaveBeenCalledWith('/sri-configuration/certificate/info', {
        method: 'GET',
      })
      expect(result).toEqual(mockCertInfo)
      expect(result?.isValid).toBe(true)
    })

    it('should return null when certificate is not configured', async () => {
      mockApiFetch.mockRejectedValue(new Error('Certificate not found'))

      const { getCertificateInfo } = useSriConfiguration()
      const result = await getCertificateInfo()

      expect(result).toBeNull()
    })

    it('should get info for expired certificate', async () => {
      const mockCertInfo: CertificateInfo = {
        subject: 'CN=Old Company S.A., O=Old Company, C=EC',
        issuer: 'CN=Security Data Signing CA, O=Security Data, C=EC',
        validFrom: '2022-01-01T00:00:00Z',
        validTo: '2023-12-31T23:59:59Z',
        isValid: false,
      }

      mockApiFetch.mockResolvedValue({ data: mockCertInfo, success: true })

      const { getCertificateInfo } = useSriConfiguration()
      const result = await getCertificateInfo()

      expect(result?.isValid).toBe(false)
    })
  })

  describe('removeCertificate', () => {
    it('should remove certificate successfully', async () => {
      mockApiFetch.mockResolvedValue({ success: true })

      const { removeCertificate } = useSriConfiguration()
      await removeCertificate()

      expect(mockApiFetch).toHaveBeenCalledWith('/sri-configuration/certificate', {
        method: 'DELETE',
      })
    })

    it('should handle API errors when removing certificate', async () => {
      mockApiFetch.mockRejectedValue(new Error('Certificate not found'))

      const { removeCertificate } = useSriConfiguration()

      await expect(removeCertificate()).rejects.toThrow('Certificate not found')
    })
  })
})
