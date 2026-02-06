import { Request, Response, NextFunction } from 'express';
import { body, validationResult } from 'express-validator';

export const validateProduct = [
  body('code')
    .trim()
    .notEmpty().withMessage('Product code is required')
    .isLength({ max: 50 }).withMessage('Code must be at most 50 characters'),
  
  body('name')
    .trim()
    .notEmpty().withMessage('Product name is required')
    .isLength({ max: 255 }).withMessage('Name must be at most 255 characters'),
  
  body('unit')
    .trim()
    .notEmpty().withMessage('Unit is required')
    .isIn(['UND', 'KG', 'LT', 'MT', 'M2', 'M3', 'PKG', 'BOX', 'DOZ'])
    .withMessage('Invalid unit type'),
  
  body('price')
    .isFloat({ min: 0 }).withMessage('Price must be a positive number'),
  
  body('cost')
    .isFloat({ min: 0 }).withMessage('Cost must be a positive number'),
  
  body('taxTypeId')
    .notEmpty().withMessage('Tax type is required')
    .isUUID().withMessage('Invalid tax type ID'),
  
  body('hasInventory')
    .isBoolean().withMessage('Has inventory must be true or false'),
  
  body('barcode')
    .optional()
    .trim()
    .isLength({ max: 100 }).withMessage('Barcode must be at most 100 characters'),
  
  body('description')
    .optional()
    .trim()
    .isLength({ max: 1000 }).withMessage('Description must be at most 1000 characters'),
  
  body('category')
    .optional()
    .trim()
    .isLength({ max: 100 }).withMessage('Category must be at most 100 characters'),
  
  body('minStock')
    .optional()
    .isInt({ min: 0 }).withMessage('Min stock must be a positive integer'),
  
  body('maxStock')
    .optional()
    .isInt({ min: 0 }).withMessage('Max stock must be a positive integer'),
  
  body('reorderPoint')
    .optional()
    .isInt({ min: 0 }).withMessage('Reorder point must be a positive integer'),

  (req: Request, res: Response, next: NextFunction) => {
    const errors = validationResult(req);
    if (!errors.isEmpty()) {
      res.status(400).json({ errors: errors.array() });
      return;
    }
    next();
  }
];
