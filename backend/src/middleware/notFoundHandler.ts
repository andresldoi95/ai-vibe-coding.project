import { Request, Response, NextFunction } from 'express';
import { ApiError } from './errorHandler';

export const notFoundHandler = (
  req: Request,
  _res: Response,
  next: NextFunction
) => {
  next(new ApiError(404, `Route ${req.method} ${req.url} not found`));
};
