import { Request, Response, NextFunction } from 'express';
import { logger } from '../config/logger';

export interface AppError extends Error {
  statusCode?: number;
  isOperational?: boolean;
}

export const errorHandler = (
  err: AppError,
  req: Request,
  res: Response,
  _next: NextFunction
) => {
  const statusCode = err.statusCode || 500;
  const message = err.message || 'Internal Server Error';

  // Log error
  logger.error({
    message: err.message,
    statusCode,
    stack: err.stack,
    url: req.url,
    method: req.method,
    ip: req.ip,
    userId: (req as any).user?.id
  });

  // Don't leak error details in production
  const response: any = {
    success: false,
    message: statusCode === 500 && process.env.NODE_ENV === 'production' 
      ? 'Internal Server Error' 
      : message
  };

  // Include stack trace in development
  if (process.env.NODE_ENV === 'development') {
    response.stack = err.stack;
  }

  res.status(statusCode).json(response);
};

// Helper function to create operational errors
export class ApiError extends Error {
  statusCode: number;
  isOperational: boolean;

  constructor(statusCode: number, message: string, isOperational = true) {
    super(message);
    this.statusCode = statusCode;
    this.isOperational = isOperational;
    Error.captureStackTrace(this, this.constructor);
  }
}

// Common error creators
export const BadRequestError = (message: string) => new ApiError(400, message);
export const UnauthorizedError = (message: string = 'Unauthorized') => new ApiError(401, message);
export const ForbiddenError = (message: string = 'Forbidden') => new ApiError(403, message);
export const NotFoundError = (message: string) => new ApiError(404, message);
export const ConflictError = (message: string) => new ApiError(409, message);
export const ValidationError = (message: string) => new ApiError(422, message);
export const InternalError = (message: string = 'Internal Server Error') => new ApiError(500, message);
