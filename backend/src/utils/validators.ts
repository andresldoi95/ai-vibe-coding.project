/**
 * Validation utilities for Ecuador-specific identifications and documents
 */

/**
 * Validates Ecuador RUC (Registro Único de Contribuyentes)
 * RUC format: 13 digits total
 * - First 10 digits: Natural person cedula or juridical person number
 * - Digits 11-12: Establishment code (001-999)
 * - Digit 13: Verification digit
 * 
 * @param ruc - RUC string to validate
 * @returns Object with isValid flag and error message if invalid
 */
export function validateRUC(ruc: string): { isValid: boolean; error?: string } {
  // Remove any whitespace
  const cleanRuc = ruc.trim();

  // Check length
  if (cleanRuc.length !== 13) {
    return {
      isValid: false,
      error: 'RUC debe tener exactamente 13 dígitos'
    };
  }

  // Check if all characters are digits
  if (!/^\d{13}$/.test(cleanRuc)) {
    return {
      isValid: false,
      error: 'RUC debe contener solo números'
    };
  }

  // Get the third digit to determine RUC type
  const thirdDigit = parseInt(cleanRuc[2]);

  // Third digit determines the type:
  // 0-5: Natural person (cedula-based)
  // 6: Public sector
  // 9: Juridical person (company)
  if (thirdDigit === 6) {
    return validatePublicSectorRUC(cleanRuc);
  } else if (thirdDigit === 9) {
    return validateJuridicalRUC(cleanRuc);
  } else if (thirdDigit >= 0 && thirdDigit <= 5) {
    return validateNaturalPersonRUC(cleanRuc);
  } else {
    return {
      isValid: false,
      error: 'Tipo de RUC no válido (tercer dígito debe ser 0-6 o 9)'
    };
  }
}

/**
 * Validates RUC for natural persons (third digit 0-5)
 * Uses modulo 10 algorithm
 */
function validateNaturalPersonRUC(ruc: string): { isValid: boolean; error?: string } {
  // Extract first 9 digits for validation
  const digits = ruc.substring(0, 9).split('').map(Number);
  const verificationDigit = parseInt(ruc[9]);

  // Coefficients for natural person: 2,1,2,1,2,1,2,1,2
  const coefficients = [2, 1, 2, 1, 2, 1, 2, 1, 2];
  
  let sum = 0;
  for (let i = 0; i < 9; i++) {
    let product = digits[i] * coefficients[i];
    // If product >= 10, subtract 9
    if (product >= 10) {
      product -= 9;
    }
    sum += product;
  }

  // Calculate verification digit
  const modulo = sum % 10;
  const calculatedDigit = modulo === 0 ? 0 : 10 - modulo;

  if (calculatedDigit !== verificationDigit) {
    return {
      isValid: false,
      error: 'Dígito verificador de RUC inválido'
    };
  }

  // Check establishment code (positions 10-11) is between 001-999
  const establishmentCode = ruc.substring(10, 13);
  if (establishmentCode === '000') {
    return {
      isValid: false,
      error: 'Código de establecimiento debe ser 001 o mayor'
    };
  }

  return { isValid: true };
}

/**
 * Validates RUC for juridical persons (companies) - third digit is 9
 * Uses modulo 11 algorithm
 */
function validateJuridicalRUC(ruc: string): { isValid: boolean; error?: string } {
  // Extract first 9 digits for validation
  const digits = ruc.substring(0, 9).split('').map(Number);
  const verificationDigit = parseInt(ruc[9]);

  // Coefficients for juridical person: 4,3,2,7,6,5,4,3,2
  const coefficients = [4, 3, 2, 7, 6, 5, 4, 3, 2];
  
  let sum = 0;
  for (let i = 0; i < 9; i++) {
    sum += digits[i] * coefficients[i];
  }

  // Calculate verification digit
  const modulo = sum % 11;
  const calculatedDigit = modulo === 0 ? 0 : 11 - modulo;

  if (calculatedDigit !== verificationDigit) {
    return {
      isValid: false,
      error: 'Dígito verificador de RUC inválido'
    };
  }

  // Check establishment code (positions 10-11) is between 001-999
  const establishmentCode = ruc.substring(10, 13);
  if (establishmentCode === '000') {
    return {
      isValid: false,
      error: 'Código de establecimiento debe ser 001 o mayor'
    };
  }

  return { isValid: true };
}

/**
 * Validates RUC for public sector - third digit is 6
 * Uses modulo 11 algorithm (similar to juridical)
 */
function validatePublicSectorRUC(ruc: string): { isValid: boolean; error?: string } {
  // Extract first 8 digits for validation (public sector uses 8 digits)
  const digits = ruc.substring(0, 8).split('').map(Number);
  const verificationDigit = parseInt(ruc[8]);

  // Coefficients for public sector: 3,2,7,6,5,4,3,2
  const coefficients = [3, 2, 7, 6, 5, 4, 3, 2];
  
  let sum = 0;
  for (let i = 0; i < 8; i++) {
    sum += digits[i] * coefficients[i];
  }

  // Calculate verification digit
  const modulo = sum % 11;
  const calculatedDigit = modulo === 0 ? 0 : 11 - modulo;

  if (calculatedDigit !== verificationDigit) {
    return {
      isValid: false,
      error: 'Dígito verificador de RUC inválido'
    };
  }

  // Check establishment code
  const establishmentCode = ruc.substring(9, 13);
  if (establishmentCode === '0000') {
    return {
      isValid: false,
      error: 'Código de establecimiento debe ser 0001 o mayor'
    };
  }

  return { isValid: true };
}

/**
 * Validates Ecuador Cedula (ID card)
 * 10 digits total with modulo 10 verification
 */
export function validateCedula(cedula: string): { isValid: boolean; error?: string } {
  // Remove any whitespace
  const cleanCedula = cedula.trim();

  // Check length
  if (cleanCedula.length !== 10) {
    return {
      isValid: false,
      error: 'Cédula debe tener exactamente 10 dígitos'
    };
  }

  // Check if all characters are digits
  if (!/^\d{10}$/.test(cleanCedula)) {
    return {
      isValid: false,
      error: 'Cédula debe contener solo números'
    };
  }

  // First two digits must be between 01 and 24 (province code)
  const provinceCode = parseInt(cleanCedula.substring(0, 2));
  if (provinceCode < 1 || provinceCode > 24) {
    return {
      isValid: false,
      error: 'Código de provincia inválido (primeros dos dígitos deben ser 01-24)'
    };
  }

  // Third digit must be less than 6 for natural persons
  const thirdDigit = parseInt(cleanCedula[2]);
  if (thirdDigit >= 6) {
    return {
      isValid: false,
      error: 'Tercer dígito debe ser menor a 6 para cédulas de personas naturales'
    };
  }

  // Validate check digit using modulo 10
  const digits = cleanCedula.substring(0, 9).split('').map(Number);
  const verificationDigit = parseInt(cleanCedula[9]);

  const coefficients = [2, 1, 2, 1, 2, 1, 2, 1, 2];
  
  let sum = 0;
  for (let i = 0; i < 9; i++) {
    let product = digits[i] * coefficients[i];
    if (product >= 10) {
      product -= 9;
    }
    sum += product;
  }

  const modulo = sum % 10;
  const calculatedDigit = modulo === 0 ? 0 : 10 - modulo;

  if (calculatedDigit !== verificationDigit) {
    return {
      isValid: false,
      error: 'Dígito verificador de cédula inválido'
    };
  }

  return { isValid: true };
}
