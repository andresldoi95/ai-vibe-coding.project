import { Request, Response, NextFunction } from 'express';

/**
 * Validation middleware for authentication endpoints
 */

export const validateLogin = (req: Request, res: Response, next: NextFunction): void => {
  const { email, password } = req.body;

  const errors: string[] = [];

  if (!email || typeof email !== 'string' || !email.trim()) {
    errors.push('Email is required');
  } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
    errors.push('Invalid email format');
  }

  if (!password || typeof password !== 'string' || !password.trim()) {
    errors.push('Password is required');
  } else if (password.length < 6) {
    errors.push('Password must be at least 6 characters');
  }

  if (errors.length > 0) {
    res.status(400).json({ error: errors.join(', ') });
    return;
  }

  next();
};

export const validateRegister = (req: Request, res: Response, next: NextFunction): void => {
  const { email, password, fullName, companyId, roleId } = req.body;

  const errors: string[] = [];

  if (!email || typeof email !== 'string' || !email.trim()) {
    errors.push('Email is required');
  } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
    errors.push('Invalid email format');
  }

  if (!password || typeof password !== 'string' || !password.trim()) {
    errors.push('Password is required');
  } else if (password.length < 6) {
    errors.push('Password must be at least 6 characters');
  }

  if (!fullName || typeof fullName !== 'string' || !fullName.trim()) {
    errors.push('Full name is required');
  }

  if (!companyId || typeof companyId !== 'string' || !companyId.trim()) {
    errors.push('Company ID is required');
  }

  if (!roleId || typeof roleId !== 'string' || !roleId.trim()) {
    errors.push('Role ID is required');
  }

  if (errors.length > 0) {
    res.status(400).json({ error: errors.join(', ') });
    return;
  }

  next();
};

export const validateCompanySelection = (req: Request, res: Response, next: NextFunction): void => {
  const { companyId } = req.body;

  if (!companyId || typeof companyId !== 'string' || !companyId.trim()) {
    res.status(400).json({ error: 'Company ID is required' });
    return;
  }

  next();
};
