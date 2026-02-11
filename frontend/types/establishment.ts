export interface Establishment {
  id: string
  establishmentCode: string
  name: string
  address: string
  phone?: string
  email?: string
  isActive: boolean
  tenantId: string
  createdAt: string
  updatedAt?: string
}

export interface CreateEstablishmentDto {
  establishmentCode: string
  name: string
  address: string
  phone?: string
  email?: string
  isActive: boolean
}

export interface UpdateEstablishmentDto {
  establishmentCode: string
  name: string
  address: string
  phone?: string
  email?: string
  isActive: boolean
}
