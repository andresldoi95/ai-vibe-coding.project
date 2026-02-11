import type { SriEnvironment } from './sri-enums'

export interface SriConfiguration {
  id: string
  companyRuc: string
  legalName: string
  tradeName: string
  mainAddress: string
  accountingRequired: boolean
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
  accountingRequired: boolean
  environment: SriEnvironment
}

export interface UploadCertificateDto {
  certificateData: string // Base64 encoded
  password: string
}

export interface CertificateInfo {
  subject: string
  issuer: string
  validFrom: string
  validTo: string
  isValid: boolean
}
