import type { SriEnvironment } from './sri-enums'

export interface SriConfiguration {
  id: string
  companyRuc: string
  legalName: string
  tradeName: string
  mainAddress: string
  phone: string
  email: string
  accountingRequired: boolean
  specialTaxpayerNumber?: string
  isRiseRegime: boolean
  environment: SriEnvironment
  isCertificateConfigured: boolean
  certificateExpiryDate?: string
  isCertificateValid: boolean
  tenantId: string
  createdAt: string
  updatedAt?: string
}

export interface UpdateSriConfigurationDto {
  companyRuc: string
  legalName: string
  tradeName: string
  mainAddress: string
  phone: string
  email: string
  accountingRequired: boolean
  specialTaxpayerNumber?: string
  isRiseRegime: boolean
  environment: SriEnvironment
}

export interface UploadCertificateDto {
  certificateFile: File
  certificatePassword: string
}

export interface CertificateInfo {
  subject: string
  issuer: string
  validFrom: string
  validTo: string
  isValid: boolean
}
