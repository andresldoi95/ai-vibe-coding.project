import { Request, Response, NextFunction } from 'express';
import jwt, { SignOptions } from 'jsonwebtoken';

declare global {
  namespace Express {
    interface Request {
      user?: {
        userId: string;
        email: string;
        companyId: string;
        role: string;
      };
    }
  }
}

export interface IJwtPayload {
  userId: string;
  email: string;
  companyId: string;
  role: string;
}

export const authenticateToken = (req: Request, res: Response, next: NextFunction): void => {
  const authHeader = req.headers['authorization'];
  const token = authHeader && authHeader.split(' ')[1]; // Bearer TOKEN

  if (!token) {
    res.status(401).json({ error: 'Access token required' });
    return;
  }

  try {
    const jwtSecret = process.env.JWT_SECRET || 'your-super-secret-jwt-key-change-this';
    const payload = jwt.verify(token, jwtSecret) as IJwtPayload;
    
    req.user = {
      userId: payload.userId,
      email: payload.email,
      companyId: payload.companyId,
      role: payload.role
    };
    
    next();
  } catch (error) {
    res.status(403).json({ error: 'Invalid or expired token' });
    return;
  }
};

export const generateToken = (payload: IJwtPayload): string => {
  const jwtSecret = process.env.JWT_SECRET || 'your-super-secret-jwt-key-change-this';
  
  const options: SignOptions = {
    expiresIn: '7d'
  };
  
  return jwt.sign(payload, jwtSecret, options);
};
