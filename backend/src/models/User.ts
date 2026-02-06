import bcrypt from 'bcryptjs';
import { pool } from '../config/database';
import type { IUser, IUserCompany, ICreateUser, IUserWithCompanies } from '../types/user';
import { logger } from '../config/logger';

/**
 * User Model
 * Handles user authentication and multi-company relationships
 */

export class User {
  /**
   * Find user by email
   */
  static async findByEmail(email: string): Promise<IUser | null> {
    try {
      const result = await pool.query(
        `SELECT 
          id, email, password_hash as "passwordHash", 
          full_name as "fullName", phone, status, 
          last_login_at as "lastLoginAt", 
          created_at as "createdAt", 
          updated_at as "updatedAt"
        FROM users WHERE email = $1`,
        [email]
      );
      
      return result.rows[0] || null;
    } catch (error) {
      logger.error('Error finding user by email:', error);
      throw error;
    }
  }

  /**
   * Find user by ID
   */
  static async findById(id: string): Promise<IUser | null> {
    try {
      const result = await pool.query(
        `SELECT 
          id, email, password_hash as "passwordHash", 
          full_name as "fullName", phone, status, 
          last_login_at as "lastLoginAt", 
          created_at as "createdAt", 
          updated_at as "updatedAt"
        FROM users WHERE id = $1`,
        [id]
      );
      
      return result.rows[0] || null;
    } catch (error) {
      logger.error('Error finding user by ID:', error);
      throw error;
    }
  }

  /**
   * Get user's companies and roles
   */
  static async getUserCompanies(userId: string): Promise<IUserCompany[]> {
    try {
      const result = await pool.query<IUserCompany>(
        `SELECT 
          c.id as "companyId",
          c.business_name as "companyName",
          c.ruc as "companyRuc",
          r.name as "roleName",
          r.id as "roleId"
        FROM user_company_roles ucr
        JOIN companies c ON c.id = ucr.company_id
        JOIN roles r ON r.id = ucr.role_id
        WHERE ucr.user_id = $1
        AND c.status = 'active'
        ORDER BY c.business_name`,
        [userId]
      );
      
      return result.rows;
    } catch (error) {
      logger.error('Error getting user companies:', error);
      throw error;
    }
  }

  /**
   * Get user with companies
   */
  static async findByIdWithCompanies(userId: string): Promise<IUserWithCompanies | null> {
    try {
      const user = await this.findById(userId);
      if (!user) return null;

      const companies = await this.getUserCompanies(userId);

      const { passwordHash, ...userWithoutPassword } = user;

      return {
        ...userWithoutPassword,
        companies
      };
    } catch (error) {
      logger.error('Error finding user with companies:', error);
      throw error;
    }
  }

  /**
   * Verify password
   */
  static async verifyPassword(email: string, password: string): Promise<IUser | null> {
    try {
      const user = await this.findByEmail(email);
      if (!user) return null;

      const isValid = await bcrypt.compare(password, user.passwordHash);
      if (!isValid) return null;

      return user;
    } catch (error) {
      logger.error('Error verifying password:', error);
      throw error;
    }
  }

  /**
   * Create new user
   */
  static async create(userData: ICreateUser): Promise<IUser> {
    const client = await pool.connect();
    
    try {
      await client.query('BEGIN');

      // Hash password
      const salt = await bcrypt.genSalt(10);
      const passwordHash = await bcrypt.hash(userData.password, salt);

      // Create user
      const userResult = await client.query<IUser>(
        `INSERT INTO users (email, password_hash, full_name, phone, status)
         VALUES ($1, $2, $3, $4, 'active')
         RETURNING *`,
        [userData.email, passwordHash, userData.fullName, userData.phone || null]
      );

      const user = userResult.rows[0];

      // Assign user to company with role
      await client.query(
        `INSERT INTO user_company_roles (user_id, company_id, role_id)
         VALUES ($1, $2, $3)`,
        [user.id, userData.companyId, userData.roleId]
      );

      await client.query('COMMIT');

      logger.info(`User created: ${user.email}`);
      return user;
    } catch (error) {
      await client.query('ROLLBACK');
      logger.error('Error creating user:', error);
      throw error;
    } finally {
      client.release();
    }
  }

  /**
   * Update last login timestamp
   */
  static async updateLastLogin(userId: string): Promise<void> {
    try {
      await pool.query(
        'UPDATE users SET last_login_at = NOW() WHERE id = $1',
        [userId]
      );
    } catch (error) {
      logger.error('Error updating last login:', error);
      // Don't throw - this is not critical
    }
  }

  /**
   * Check if email exists
   */
  static async emailExists(email: string): Promise<boolean> {
    try {
      const result = await pool.query(
        'SELECT 1 FROM users WHERE email = $1',
        [email]
      );
      return result.rows.length > 0;
    } catch (error) {
      logger.error('Error checking email existence:', error);
      throw error;
    }
  }

  /**
   * Verify user has access to company
   */
  static async hasAccessToCompany(userId: string, companyId: string): Promise<boolean> {
    try {
      const result = await pool.query(
        `SELECT 1 FROM user_company_roles ucr
         JOIN companies c ON c.id = ucr.company_id
         WHERE ucr.user_id = $1 
         AND ucr.company_id = $2 
         AND c.status = 'active'`,
        [userId, companyId]
      );
      return result.rows.length > 0;
    } catch (error) {
      logger.error('Error checking company access:', error);
      throw error;
    }
  }
}
