// Common types shared across modules

export interface Country {
  id: string
  code: string // ISO 3166-1 alpha-2 (e.g., "US", "EC")
  name: string // English name (e.g., "United States", "Ecuador")
  alpha3Code?: string // ISO 3166-1 alpha-3 (e.g., "USA", "ECU")
  numericCode?: string // ISO 3166-1 numeric (e.g., "840")
  isActive: boolean
}
