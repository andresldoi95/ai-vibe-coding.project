import bcrypt from 'bcryptjs';
import { pool } from '../config/database';
import { logger } from '../config/logger';
import { validateRUC } from '../utils/validators';

export interface ICompanyRegistrationData {
  ruc: string;
  businessName: string;
  tradeName?: string;
  email: string;
  phone: string;
  address: string;
}

export interface IAdminUserData {
  email: string;
  password: string;
  fullName: string;
  phone?: string;
}

export interface ICompanyRegistrationResult {
  company: {
    id: string;
    ruc: string;
    businessName: string;
    tradeName: string | null;
    email: string;
    phone: string;
    address: string;
    status: string;
  };
  user: {
    id: string;
    email: string;
    fullName: string;
    phone: string | null;
    status: string;
  };
  role: {
    id: string;
    name: string;
  };
}

/**
 * Company Service
 * Handles company-related business logic
 */
export class CompanyService {
  /**
   * Register a new company with its first admin user
   * This is an atomic operation - either both are created or neither
   */
  static async registerCompanyWithAdmin(
    companyData: ICompanyRegistrationData,
    adminData: IAdminUserData
  ): Promise<ICompanyRegistrationResult> {
    const client = await pool.connect();

    try {
      await client.query('BEGIN');

      // 1. Validate RUC format and check digit
      const rucValidation = validateRUC(companyData.ruc);
      if (!rucValidation.isValid) {
        throw new Error(rucValidation.error || 'RUC inválido');
      }

      // 2. Check if RUC already exists
      const rucCheck = await client.query(
        'SELECT id FROM companies WHERE ruc = $1',
        [companyData.ruc]
      );
      if (rucCheck.rows.length > 0) {
        throw new Error('RUC ya está registrado en el sistema');
      }

      // 3. Check if admin email already exists
      const emailCheck = await client.query(
        'SELECT id FROM users WHERE email = $1',
        [adminData.email]
      );
      if (emailCheck.rows.length > 0) {
        throw new Error('El correo electrónico ya está registrado');
      }

      // 4. Create company
      const companyResult = await client.query(
        `INSERT INTO companies (
          ruc, business_name, trade_name, email, phone, address, 
          status, sri_environment
        )
        VALUES ($1, $2, $3, $4, $5, $6, 'active', 1)
        RETURNING 
          id, ruc, business_name as "businessName", 
          trade_name as "tradeName", email, phone, address, status`,
        [
          companyData.ruc,
          companyData.businessName,
          companyData.tradeName || null,
          companyData.email,
          companyData.phone,
          companyData.address
        ]
      );

      const company = companyResult.rows[0];
      logger.info(`Company created: ${company.businessName} (RUC: ${company.ruc})`);

      // 5. Hash admin password
      const salt = await bcrypt.genSalt(10);
      const passwordHash = await bcrypt.hash(adminData.password, salt);

      // 6. Create admin user
      const userResult = await client.query(
        `INSERT INTO users (email, password_hash, full_name, phone, status)
         VALUES ($1, $2, $3, $4, 'active')
         RETURNING 
           id, email, full_name as "fullName", phone, status`,
        [
          adminData.email,
          passwordHash,
          adminData.fullName,
          adminData.phone || null
        ]
      );

      const user = userResult.rows[0];
      logger.info(`Admin user created: ${user.email}`);

      // 7. Get admin role (using name to avoid hardcoding UUID)
      const roleResult = await client.query(
        `SELECT id, name FROM roles WHERE name = 'admin' LIMIT 1`
      );

      if (roleResult.rows.length === 0) {
        throw new Error('Admin role not found in system. Database may not be properly seeded.');
      }

      const adminRole = roleResult.rows[0];

      // 8. Assign admin role to user for this company
      await client.query(
        `INSERT INTO user_company_roles (user_id, company_id, role_id)
         VALUES ($1, $2, $3)`,
        [user.id, company.id, adminRole.id]
      );

      logger.info(`Admin role assigned to user ${user.email} for company ${company.businessName}`);

      await client.query('COMMIT');

      return {
        company,
        user,
        role: adminRole
      };
    } catch (error) {
      await client.query('ROLLBACK');
      logger.error('Error registering company with admin:', error);
      throw error;
    } finally {
      client.release();
    }
  }

  /**
   * Check if a RUC exists
   */
  static async rucExists(ruc: string): Promise<boolean> {
    try {
      const result = await pool.query(
        'SELECT id FROM companies WHERE ruc = $1',
        [ruc]
      );
      return result.rows.length > 0;
    } catch (error) {
      logger.error('Error checking RUC existence:', error);
      throw error;
    }
  }

  /**
   * Get company by ID
   */
  static async findById(id: string): Promise<any | null> {
    try {
      const result = await pool.query(
        `SELECT 
          id, ruc, business_name as "businessName", 
          trade_name as "tradeName", email, phone, address,
          accounting_required as "accountingRequired",
          special_taxpayer_number as "specialTaxpayerNumber",
          sri_environment as "sriEnvironment",
          status,
          created_at as "createdAt",
          updated_at as "updatedAt"
        FROM companies WHERE id = $1`,
        [id]
      );
      
      return result.rows[0] || null;
    } catch (error) {
      logger.error('Error finding company by ID:', error);
      throw error;
    }
  }

  /**
   * Get company by RUC
   */
  static async findByRuc(ruc: string): Promise<any | null> {
    try {
      const result = await pool.query(
        `SELECT 
          id, ruc, business_name as "businessName", 
          trade_name as "tradeName", email, phone, address,
          accounting_required as "accountingRequired",
          special_taxpayer_number as "specialTaxpayerNumber",
          sri_environment as "sriEnvironment",
          status,
          created_at as "createdAt",
          updated_at as "updatedAt"
        FROM companies WHERE ruc = $1`,
        [ruc]
      );
      
      return result.rows[0] || null;
    } catch (error) {
      logger.error('Error finding company by RUC:', error);
      throw error;
    }
  }
}
