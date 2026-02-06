import { Request, Response, NextFunction } from 'express';
import { ProductModel } from '../models/Product';
import { ICreateProductDTO, IUpdateProductDTO, IProductFilters } from '../types/product';

export class ProductController {
  /**
   * Get all products
   */
  static async getProducts(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const companyId = req.user?.companyId; // Will be set by auth middleware
      
      if (!companyId) {
        res.status(400).json({ error: 'Company ID required' });
        return;
      }

      const filters: IProductFilters = {
        search: req.query.search as string,
        category: req.query.category as string,
        status: req.query.status as 'active' | 'inactive',
        hasInventory: req.query.hasInventory === 'true' ? true : req.query.hasInventory === 'false' ? false : undefined,
        page: parseInt(req.query.page as string) || 1,
        limit: parseInt(req.query.limit as string) || 20
      };

      const result = await ProductModel.findAll(companyId, filters);
      res.json(result);
    } catch (error) {
      next(error);
    }
  }

  /**
   * Get product by ID
   */
  static async getProductById(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { id } = req.params;
      const companyId = req.user?.companyId;

      if (!companyId) {
        res.status(400).json({ error: 'Company ID required' });
        return;
      }

      const product = await ProductModel.findById(id, companyId);

      if (!product) {
        res.status(404).json({ error: 'Product not found' });
        return;
      }

      res.json(product);
    } catch (error) {
      next(error);
    }
  }

  /**
   * Create new product
   */
  static async createProduct(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const companyId = req.user?.companyId;

      if (!companyId) {
        res.status(400).json({ error: 'Company ID required' });
        return;
      }

      const data: ICreateProductDTO = req.body;

      // Check if code already exists
      const existing = await ProductModel.findByCode(data.code, companyId);
      if (existing) {
        res.status(409).json({ error: 'Product code already exists' });
        return;
      }

      const product = await ProductModel.create(companyId, data);
      res.status(201).json(product);
    } catch (error) {
      next(error);
    }
  }

  /**
   * Update product
   */
  static async updateProduct(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { id } = req.params;
      const companyId = req.user?.companyId;

      if (!companyId) {
        res.status(400).json({ error: 'Company ID required' });
        return;
      }

      const data: IUpdateProductDTO = req.body;

      // If updating code, check uniqueness
      if (data.code) {
        const existing = await ProductModel.findByCode(data.code, companyId);
        if (existing && existing.id !== id) {
          res.status(409).json({ error: 'Product code already exists' });
          return;
        }
      }

      const product = await ProductModel.update(id, companyId, data);

      if (!product) {
        res.status(404).json({ error: 'Product not found' });
        return;
      }

      res.json(product);
    } catch (error) {
      next(error);
    }
  }

  /**
   * Delete product
   */
  static async deleteProduct(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { id } = req.params;
      const companyId = req.user?.companyId;

      if (!companyId) {
        res.status(400).json({ error: 'Company ID required' });
        return;
      }

      const success = await ProductModel.delete(id, companyId);

      if (!success) {
        res.status(404).json({ error: 'Product not found' });
        return;
      }

      res.status(204).send();
    } catch (error) {
      next(error);
    }
  }

  /**
   * Get product categories
   */
  static async getCategories(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const companyId = req.user?.companyId;

      if (!companyId) {
        res.status(400).json({ error: 'Company ID required' });
        return;
      }

      const categories = await ProductModel.getCategories(companyId);
      res.json({ categories });
    } catch (error) {
      next(error);
    }
  }
}
