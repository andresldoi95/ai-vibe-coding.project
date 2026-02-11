export interface EmissionPoint {
  id: string
  emissionPointCode: string
  name: string
  isActive: boolean
  invoiceSequence: number
  creditNoteSequence: number
  debitNoteSequence: number
  retentionSequence: number
  establishmentId: string
  establishmentCode?: string
  establishmentName?: string
  createdAt: string
  updatedAt?: string
}

export interface CreateEmissionPointDto {
  emissionPointCode: string
  name: string
  isActive: boolean
  establishmentId: string
}

export interface UpdateEmissionPointDto {
  name: string
  isActive: boolean
}
