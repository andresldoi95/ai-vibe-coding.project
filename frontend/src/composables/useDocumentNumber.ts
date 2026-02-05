/**
 * Ecuador Document Number Utilities
 * Handles sequential numbering format: 001-001-000000001
 * Format: establishment-pointOfSale-sequential
 */

export interface IDocumentNumber {
  establishment: string
  pointOfSale: string
  sequential: string
  full: string
}

export function useDocumentNumber() {
  /**
   * Format document number in Ecuador format
   * @param establishment Establishment code (001-999)
   * @param pointOfSale Point of sale code (001-999)
   * @param sequential Sequential number (1-999999999)
   */
  const formatDocumentNumber = (
    establishment: number | string,
    pointOfSale: number | string,
    sequential: number | string
  ): string => {
    const estab = String(establishment).padStart(3, '0')
    const pos = String(pointOfSale).padStart(3, '0')
    const seq = String(sequential).padStart(9, '0')
    return `${estab}-${pos}-${seq}`
  }

  /**
   * Parse document number string
   * @param documentNumber Full document number string (001-001-000000001)
   */
  const parseDocumentNumber = (documentNumber: string): IDocumentNumber | null => {
    const parts = documentNumber.split('-')
    if (parts.length !== 3) return null

    return {
      establishment: parts[0],
      pointOfSale: parts[1],
      sequential: parts[2],
      full: documentNumber
    }
  }

  /**
   * Validate document number format
   */
  const validateDocumentNumber = (documentNumber: string): boolean => {
    const regex = /^\d{3}-\d{3}-\d{9}$/
    return regex.test(documentNumber)
  }

  /**
   * Get next sequential number
   */
  const getNextSequential = (currentSequential: number): string => {
    return String(currentSequential + 1).padStart(9, '0')
  }

  /**
   * Generate access key (clave de acceso) for SRI - 48 digits
   * Format: ddmmyyyyttcccccccccrrrrrrrrrrcn
   * dd = day, mm = month, yyyy = year
   * tt = document type (01=factura, 04=nota credito, etc)
   * ccccccccc = RUC
   * rrrrrrrrr = sequential number
   * c = establishment, n = point of sale
   * Last digit = verification digit (modulo 11)
   */
  const generateAccessKey = (
    date: Date,
    documentType: string,
    ruc: string,
    establishment: string,
    pointOfSale: string,
    sequential: string,
    emissionCode: string = '1'
  ): string => {
    const day = String(date.getDate()).padStart(2, '0')
    const month = String(date.getMonth() + 1).padStart(2, '0')
    const year = String(date.getFullYear())
    
    // Build 47 digits (without verification digit)
    let accessKey = day + month + year
    accessKey += documentType.padStart(2, '0')
    accessKey += ruc.padStart(13, '0')
    accessKey += '2' // Environment (1=pruebas, 2=produccion)
    accessKey += establishment.padStart(3, '0')
    accessKey += pointOfSale.padStart(3, '0')
    accessKey += sequential.padStart(9, '0')
    accessKey += '12345678' // Random number (should be generated)
    accessKey += emissionCode // 1=normal
    
    // Calculate verification digit (modulo 11)
    const verificationDigit = calculateModulo11(accessKey)
    
    return accessKey + verificationDigit
  }

  /**
   * Calculate verification digit using modulo 11
   */
  const calculateModulo11 = (numbers: string): string => {
    let total = 0
    let multiplier = 2

    for (let i = numbers.length - 1; i >= 0; i--) {
      total += parseInt(numbers[i]) * multiplier
      multiplier = multiplier === 7 ? 2 : multiplier + 1
    }

    const remainder = total % 11
    const digit = 11 - remainder

    if (digit === 11) return '0'
    if (digit === 10) return '1'
    return String(digit)
  }

  /**
   * Validate RUC (Ecuador tax ID)
   */
  const validateRUC = (ruc: string): boolean => {
    if (!ruc || ruc.length !== 13) return false
    
    // Check if it's numeric
    if (!/^\d+$/.test(ruc)) return false
    
    // Third digit should be less than 6 (for natural persons) or 6 or 9 (for companies)
    const thirdDigit = parseInt(ruc[2])
    if (thirdDigit > 9) return false
    
    // Check verification digit
    const digits = ruc.substring(0, 10).split('').map(Number)
    const verifier = parseInt(ruc[9])
    
    let total = 0
    const coefficients = [2, 1, 2, 1, 2, 1, 2, 1, 2]
    
    for (let i = 0; i < 9; i++) {
      let value = digits[i] * coefficients[i]
      if (value >= 10) value -= 9
      total += value
    }
    
    const calculatedVerifier = (10 - (total % 10)) % 10
    
    return calculatedVerifier === verifier
  }

  /**
   * Validate cedula (Ecuador national ID)
   */
  const validateCedula = (cedula: string): boolean => {
    if (!cedula || cedula.length !== 10) return false
    
    // Check if it's numeric
    if (!/^\d+$/.test(cedula)) return false
    
    // Province code (first two digits) should be between 01 and 24
    const province = parseInt(cedula.substring(0, 2))
    if (province < 1 || province > 24) return false
    
    // Third digit should be less than 6
    const thirdDigit = parseInt(cedula[2])
    if (thirdDigit >= 6) return false
    
    // Check verification digit
    const digits = cedula.split('').map(Number)
    const verifier = digits[9]
    
    let total = 0
    for (let i = 0; i < 9; i++) {
      let value = digits[i]
      if (i % 2 === 0) {
        value *= 2
        if (value > 9) value -= 9
      }
      total += value
    }
    
    const calculatedVerifier = (10 - (total % 10)) % 10
    
    return calculatedVerifier === verifier
  }

  return {
    formatDocumentNumber,
    parseDocumentNumber,
    validateDocumentNumber,
    getNextSequential,
    generateAccessKey,
    calculateModulo11,
    validateRUC,
    validateCedula
  }
}
