import { Request, Response, NextFunction } from 'express';
import { validateRUC } from '../../utils/validators';

/**
 * Validation middleware for company operations
 */

/**
 * Validate company registration request
 * Checks both company and admin user data
 */
export const validateCompanyRegistration = (
  req: Request,
  res: Response,
  next: NextFunction
): void => {
  const { company, admin } = req.body;

  // Check top-level structure
  if (!company || typeof company !== 'object') {
    res.status(400).json({
      success: false,
      message: 'Datos de empresa requeridos',
      field: 'company'
    });
    return;
  }

  if (!admin || typeof admin !== 'object') {
    res.status(400).json({
      success: false,
      message: 'Datos de administrador requeridos',
      field: 'admin'
    });
    return;
  }

  // Validate company fields
  const companyErrors = validateCompanyData(company);
  if (companyErrors.length > 0) {
    res.status(400).json({
      success: false,
      message: companyErrors[0].message,
      field: companyErrors[0].field,
      errors: companyErrors
    });
    return;
  }

  // Validate admin user fields
  const adminErrors = validateAdminData(admin);
  if (adminErrors.length > 0) {
    res.status(400).json({
      success: false,
      message: adminErrors[0].message,
      field: adminErrors[0].field,
      errors: adminErrors
    });
    return;
  }

  next();
};

/**
 * Validate company data fields
 */
function validateCompanyData(company: any): Array<{ field: string; message: string }> {
  const errors: Array<{ field: string; message: string }> = [];

  // RUC - required
  if (!company.ruc || typeof company.ruc !== 'string') {
    errors.push({
      field: 'company.ruc',
      message: 'RUC es requerido'
    });
  } else {
    const ruc = company.ruc.trim();
    if (ruc.length === 0) {
      errors.push({
        field: 'company.ruc',
        message: 'RUC no puede estar vacío'
      });
    } else {
      const rucValidation = validateRUC(ruc);
      if (!rucValidation.isValid) {
        errors.push({
          field: 'company.ruc',
          message: rucValidation.error || 'RUC inválido'
        });
      }
    }
  }

  // Business name - required
  if (!company.businessName || typeof company.businessName !== 'string') {
    errors.push({
      field: 'company.businessName',
      message: 'Razón social es requerida'
    });
  } else if (company.businessName.trim().length < 3) {
    errors.push({
      field: 'company.businessName',
      message: 'Razón social debe tener al menos 3 caracteres'
    });
  } else if (company.businessName.trim().length > 255) {
    errors.push({
      field: 'company.businessName',
      message: 'Razón social debe tener máximo 255 caracteres'
    });
  }

  // Trade name - optional but validate if provided
  if (company.tradeName !== undefined && company.tradeName !== null) {
    if (typeof company.tradeName !== 'string') {
      errors.push({
        field: 'company.tradeName',
        message: 'Nombre comercial debe ser texto'
      });
    } else if (company.tradeName.trim().length > 255) {
      errors.push({
        field: 'company.tradeName',
        message: 'Nombre comercial debe tener máximo 255 caracteres'
      });
    }
  }

  // Email - required
  if (!company.email || typeof company.email !== 'string') {
    errors.push({
      field: 'company.email',
      message: 'Correo electrónico de la empresa es requerido'
    });
  } else {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(company.email.trim())) {
      errors.push({
        field: 'company.email',
        message: 'Formato de correo electrónico inválido'
      });
    }
  }

  // Phone - required
  if (!company.phone || typeof company.phone !== 'string') {
    errors.push({
      field: 'company.phone',
      message: 'Teléfono de la empresa es requerido'
    });
  } else if (company.phone.trim().length < 7) {
    errors.push({
      field: 'company.phone',
      message: 'Teléfono debe tener al menos 7 caracteres'
    });
  } else if (company.phone.trim().length > 20) {
    errors.push({
      field: 'company.phone',
      message: 'Teléfono debe tener máximo 20 caracteres'
    });
  }

  // Address - required
  if (!company.address || typeof company.address !== 'string') {
    errors.push({
      field: 'company.address',
      message: 'Dirección de la empresa es requerida'
    });
  } else if (company.address.trim().length < 5) {
    errors.push({
      field: 'company.address',
      message: 'Dirección debe tener al menos 5 caracteres'
    });
  }

  return errors;
}

/**
 * Validate admin user data fields
 */
function validateAdminData(admin: any): Array<{ field: string; message: string }> {
  const errors: Array<{ field: string; message: string }> = [];

  // Email - required
  if (!admin.email || typeof admin.email !== 'string') {
    errors.push({
      field: 'admin.email',
      message: 'Correo electrónico del administrador es requerido'
    });
  } else {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(admin.email.trim())) {
      errors.push({
        field: 'admin.email',
        message: 'Formato de correo electrónico inválido'
      });
    }
  }

  // Password - required
  if (!admin.password || typeof admin.password !== 'string') {
    errors.push({
      field: 'admin.password',
      message: 'Contraseña es requerida'
    });
  } else if (admin.password.length < 6) {
    errors.push({
      field: 'admin.password',
      message: 'Contraseña debe tener al menos 6 caracteres'
    });
  } else if (admin.password.length > 100) {
    errors.push({
      field: 'admin.password',
      message: 'Contraseña debe tener máximo 100 caracteres'
    });
  }

  // Full name - required
  if (!admin.fullName || typeof admin.fullName !== 'string') {
    errors.push({
      field: 'admin.fullName',
      message: 'Nombre completo del administrador es requerido'
    });
  } else if (admin.fullName.trim().length < 3) {
    errors.push({
      field: 'admin.fullName',
      message: 'Nombre completo debe tener al menos 3 caracteres'
    });
  } else if (admin.fullName.trim().length > 255) {
    errors.push({
      field: 'admin.fullName',
      message: 'Nombre completo debe tener máximo 255 caracteres'
    });
  }

  // Phone - optional but validate if provided
  if (admin.phone !== undefined && admin.phone !== null && admin.phone !== '') {
    if (typeof admin.phone !== 'string') {
      errors.push({
        field: 'admin.phone',
        message: 'Teléfono debe ser texto'
      });
    } else if (admin.phone.trim().length < 7) {
      errors.push({
        field: 'admin.phone',
        message: 'Teléfono debe tener al menos 7 caracteres'
      });
    } else if (admin.phone.trim().length > 20) {
      errors.push({
        field: 'admin.phone',
        message: 'Teléfono debe tener máximo 20 caracteres'
      });
    }
  }

  return errors;
}
