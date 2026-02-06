import pool from '../config/database';
import { IProduct, ICreateProductDTO, IUpdateProductDTO, IProductFilters, IProductResponse } from '../types/product';

export class ProductModel {
  /**
   * Get all products for a company with filters
   */
  static async findAll(companyId: string, filters: IProductFilters = {}): Promise<IProductResponse> {
    const {
      search = '',
      category,
      status,
      hasInventory,
      page = 1,
      limit = 20
    } = filters;

    const offset = (page - 1) * limit;
    const queryParams: any[] = [companyId];
    let paramCount = 1;

    let whereConditions = [`company_id = $${paramCount}`];

    if (search) {
      paramCount++;
      whereConditions.push(`(code ILIKE $${paramCount} OR name ILIKE $${paramCount} OR barcode ILIKE $${paramCount})`);
      queryParams.push(`%${search}%`);
    }

    if (category) {
      paramCount++;
      whereConditions.push(`category = $${paramCount}`);
      queryParams.push(category);
    }

    if (status) {
      paramCount++;
      whereConditions.push(`status = $${paramCount}`);
      queryParams.push(status);
    }

    if (hasInventory !== undefined) {
      paramCount++;
      whereConditions.push(`has_inventory = $${paramCount}`);
      queryParams.push(hasInventory);
    }

    const whereClause = whereConditions.join(' AND ');

    // Count total
    const countQuery = `SELECT COUNT(*) FROM products WHERE ${whereClause}`;
    const countResult = await pool.query(countQuery, queryParams);
    const total = parseInt(countResult.rows[0].count);

    // Get products
    const query = `
      SELECT 
        id, company_id as "companyId", code, barcode, name, description,
        category, unit, price, cost, tax_type_id as "taxTypeId",
        has_inventory as "hasInventory", min_stock as "minStock",
        max_stock as "maxStock", reorder_point as "reorderPoint",
        image, status, created_at as "createdAt", updated_at as "updatedAt"
      FROM products
      WHERE ${whereClause}
      ORDER BY name ASC
      LIMIT $${paramCount + 1} OFFSET $${paramCount + 2}
    `;
    
    queryParams.push(limit, offset);
    const result = await pool.query(query, queryParams);

    return {
      products: result.rows,
      total,
      page,
      totalPages: Math.ceil(total / limit)
    };
  }

  /**
   * Get product by ID
   */
  static async findById(id: string, companyId: string): Promise<IProduct | null> {
    const query = `
      SELECT 
        id, company_id as "companyId", code, barcode, name, description,
        category, unit, price, cost, tax_type_id as "taxTypeId",
        has_inventory as "hasInventory", min_stock as "minStock",
        max_stock as "maxStock", reorder_point as "reorderPoint",
        image, status, created_at as "createdAt", updated_at as "updatedAt"
      FROM products
      WHERE id = $1 AND company_id = $2
    `;
    const result = await pool.query(query, [id, companyId]);
    return result.rows[0] || null;
  }

  /**
   * Get product by code
   */
  static async findByCode(code: string, companyId: string): Promise<IProduct | null> {
    const query = `
      SELECT 
        id, company_id as "companyId", code, barcode, name, description,
        category, unit, price, cost, tax_type_id as "taxTypeId",
        has_inventory as "hasInventory", min_stock as "minStock",
        max_stock as "maxStock", reorder_point as "reorderPoint",
        image, status, created_at as "createdAt", updated_at as "updatedAt"
      FROM products
      WHERE code = $1 AND company_id = $2
    `;
    const result = await pool.query(query, [code, companyId]);
    return result.rows[0] || null;
  }

  /**
   * Create new product
   */
  static async create(companyId: string, data: ICreateProductDTO): Promise<IProduct> {
    const query = `
      INSERT INTO products (
        company_id, code, barcode, name, description, category, unit,
        price, cost, tax_type_id, has_inventory, min_stock, max_stock,
        reorder_point, image
      ) VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11, $12, $13, $14, $15)
      RETURNING 
        id, company_id as "companyId", code, barcode, name, description,
        category, unit, price, cost, tax_type_id as "taxTypeId",
        has_inventory as "hasInventory", min_stock as "minStock",
        max_stock as "maxStock", reorder_point as "reorderPoint",
        image, status, created_at as "createdAt", updated_at as "updatedAt"
    `;

    const values = [
      companyId,
      data.code,
      data.barcode || null,
      data.name,
      data.description || null,
      data.category || null,
      data.unit,
      data.price,
      data.cost,
      data.taxTypeId,
      data.hasInventory,
      data.minStock || null,
      data.maxStock || null,
      data.reorderPoint || null,
      data.image || null
    ];

    const result = await pool.query(query, values);
    return result.rows[0];
  }

  /**
   * Update product
   */
  static async update(id: string, companyId: string, data: IUpdateProductDTO): Promise<IProduct | null> {
    const fields: string[] = [];
    const values: any[] = [];
    let paramCount = 1;

    // Build dynamic update query
    Object.entries(data).forEach(([key, value]) => {
      if (value !== undefined) {
        const dbKey = key.replace(/([A-Z])/g, '_$1').toLowerCase();
        fields.push(`${dbKey} = $${paramCount}`);
        values.push(value);
        paramCount++;
      }
    });

    if (fields.length === 0) {
      throw new Error('No fields to update');
    }

    fields.push(`updated_at = NOW()`);

    const query = `
      UPDATE products
      SET ${fields.join(', ')}
      WHERE id = $${paramCount} AND company_id = $${paramCount + 1}
      RETURNING 
        id, company_id as "companyId", code, barcode, name, description,
        category, unit, price, cost, tax_type_id as "taxTypeId",
        has_inventory as "hasInventory", min_stock as "minStock",
        max_stock as "maxStock", reorder_point as "reorderPoint",
        image, status, created_at as "createdAt", updated_at as "updatedAt"
    `;

    values.push(id, companyId);
    const result = await pool.query(query, values);
    return result.rows[0] || null;
  }

  /**
   * Delete product (soft delete)
   */
  static async delete(id: string, companyId: string): Promise<boolean> {
    const query = `
      UPDATE products
      SET status = 'inactive', updated_at = NOW()
      WHERE id = $1 AND company_id = $2
    `;
    const result = await pool.query(query, [id, companyId]);
    return (result.rowCount || 0) > 0;
  }

  /**
   * Get product categories for a company
   */
  static async getCategories(companyId: string): Promise<string[]> {
    const query = `
      SELECT DISTINCT category
      FROM products
      WHERE company_id = $1 AND category IS NOT NULL
      ORDER BY category ASC
    `;
    const result = await pool.query(query, [companyId]);
    return result.rows.map(row => row.category);
  }
}
