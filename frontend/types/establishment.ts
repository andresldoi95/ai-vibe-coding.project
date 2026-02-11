export interface Establishment {
  id: string
  establishmentCode: string
  name: string
  address: string
  phone?: string
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
  isActive: boolean
}

export interface UpdateEstablishmentDto {
  name: string
  address: string
  phone?: string
  isActive: boolean
}
