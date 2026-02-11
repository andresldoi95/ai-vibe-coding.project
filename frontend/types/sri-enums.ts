// SRI Ecuador Document Types
export enum DocumentType {
  Invoice = 1,
  PurchaseLiquidation = 3,
  CreditNote = 4,
  DebitNote = 5,
  Retention = 7,
}

// SRI Ecuador Payment Methods
export enum SriPaymentMethod {
  Cash = 1,
  Check = 2,
  BankTransfer = 3,
  AccountDeposit = 4,
  DebitCard = 16,
  ElectronicMoney = 17,
  PrepaidCard = 18,
  CreditCard = 19,
  Other = 20,
  TitleEndorsement = 21,
}

// SRI Ecuador Identification Types
export enum IdentificationType {
  Ruc = 4,
  Cedula = 5,
  Passport = 6,
  ConsumerFinal = 7,
  ForeignId = 8,
}

// SRI Environment
export enum SriEnvironment {
  Test = 1,
  Production = 2,
}

// Emission Type
export enum EmissionType {
  Normal = 1,
  Contingency = 2,
}

// Label mappings for UI display
export const documentTypeLabels: Record<DocumentType, string> = {
  [DocumentType.Invoice]: 'Invoice',
  [DocumentType.PurchaseLiquidation]: 'Purchase Liquidation',
  [DocumentType.CreditNote]: 'Credit Note',
  [DocumentType.DebitNote]: 'Debit Note',
  [DocumentType.Retention]: 'Retention Receipt',
}

export const sriPaymentMethodLabels: Record<SriPaymentMethod, string> = {
  [SriPaymentMethod.Cash]: 'Cash',
  [SriPaymentMethod.Check]: 'Check',
  [SriPaymentMethod.BankTransfer]: 'Bank Transfer',
  [SriPaymentMethod.AccountDeposit]: 'Account Deposit',
  [SriPaymentMethod.DebitCard]: 'Debit Card',
  [SriPaymentMethod.ElectronicMoney]: 'Electronic Money',
  [SriPaymentMethod.PrepaidCard]: 'Prepaid Card',
  [SriPaymentMethod.CreditCard]: 'Credit Card',
  [SriPaymentMethod.Other]: 'Other',
  [SriPaymentMethod.TitleEndorsement]: 'Title Endorsement',
}

export const identificationTypeLabels: Record<IdentificationType, string> = {
  [IdentificationType.Ruc]: 'RUC',
  [IdentificationType.Cedula]: 'Cédula',
  [IdentificationType.Passport]: 'Passport',
  [IdentificationType.ConsumerFinal]: 'Consumer Final',
  [IdentificationType.ForeignId]: 'Foreign ID',
}

export const sriEnvironmentLabels: Record<SriEnvironment, string> = {
  [SriEnvironment.Test]: 'Test',
  [SriEnvironment.Production]: 'Production',
}
