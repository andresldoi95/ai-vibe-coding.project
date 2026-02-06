import { Request, Response } from 'express';
import jwt from 'jsonwebtoken';
import { logger } from '../config/logger';
import { User } from '../models/User';

/**
 * Auth Controller
 * Handles multi-company authentication
 */

export class AuthController {
  /**
   * Login - Validate credentials and return user with available companies
   */
  static async login(req: Request, res: Response): Promise<void> {
    try {
      const { email, password } = req.body;

      // Verify credentials
      const user = await User.verifyPassword(email, password);
      
      if (!user) {
        res.status(401).json({
          error: 'Invalid email or password'
        });
        return;
      }

      // Check if user is active
      if (user.status !== 'active') {
        res.status(403).json({
          error: 'User account is not active'
        });
        return;
      }

      // Get user's companies
      const companies = await User.getUserCompanies(user.id);

      if (companies.length === 0) {
        res.status(403).json({
          error: 'User is not assigned to any company'
        });
        return;
      }

      // Update last login
      await User.updateLastLogin(user.id);

      logger.info(`User logged in: ${email}`);

      // Return user info with companies (no token yet - user needs to select company)
      res.json({
        success: true,
        requiresCompanySelection: companies.length > 1,
        user: {
          id: user.id,
          email: user.email,
          fullName: user.fullName,
          phone: user.phone,
          status: user.status
        },
        companies
      });
    } catch (error) {
      logger.error('Login error:', error);
      res.status(500).json({
        error: 'Internal server error during login'
      });
    }
  }

  /**
   * Select company and generate JWT token
   */
  static async selectCompany(req: Request, res: Response): Promise<void> {
    try {
      const { email, companyId } = req.body;

      // Find user
      const user = await User.findByEmail(email);
      if (!user) {
        res.status(401).json({ error: 'User not found' });
        return;
      }

      // Verify user has access to the company
      const hasAccess = await User.hasAccessToCompany(user.id, companyId);
      if (!hasAccess) {
        res.status(403).json({
          error: 'You do not have access to this company'
        });
        return;
      }

      // Get company role
      const companies = await User.getUserCompanies(user.id);
      const selectedCompany = companies.find(c => c.companyId === companyId);

      if (!selectedCompany) {
        res.status(404).json({ error: 'Company not found' });
        return;
      }

      // Generate JWT token with company context
      const jwtSecret = process.env.JWT_SECRET || 'your-super-secret-jwt-key-change-this';
      const token = jwt.sign(
        {
          userId: user.id,
          email: user.email,
          companyId: selectedCompany.companyId,
          role: selectedCompany.roleName
        },
        jwtSecret,
        { expiresIn: '7d' }
      );

      logger.info(`User ${email} selected company: ${selectedCompany.companyName}`);

      res.json({
        success: true,
        token,
        user: {
          id: user.id,
          email: user.email,
          fullName: user.fullName,
          phone: user.phone,
          status: user.status
        },
        company: {
          id: selectedCompany.companyId,
          name: selectedCompany.companyName,
          ruc: selectedCompany.companyRuc,
          role: selectedCompany.roleName
        }
      });
    } catch (error) {
      logger.error('Company selection error:', error);
      res.status(500).json({
        error: 'Internal server error'
      });
    }
  }

  /**
   * Register new user
   */
  static async register(req: Request, res: Response): Promise<void> {
    try {
      const { email, password, fullName, phone, companyId, roleId } = req.body;

      // Check if email already exists
      const exists = await User.emailExists(email);
      if (exists) {
        res.status(409).json({
          error: 'Email already registered'
        });
        return;
      }

      // Create user
      const user = await User.create({
        email,
        password,
        fullName,
        phone,
        companyId,
        roleId
      });

      logger.info(`User registered: ${email}`);

      res.status(201).json({
        success: true,
        message: 'User registered successfully',
        user: {
          id: user.id,
          email: user.email,
          fullName: user.fullName
        }
      });
    } catch (error) {
      logger.error('Registration error:', error);
      res.status(500).json({
        error: 'Internal server error during registration'
      });
    }
  }

  /**
   * Get current user info from token
   */
  static async getCurrentUser(req: Request, res: Response): Promise<void> {
    try {
      // User info is already attached to req.user by auth middleware
      if (!req.user) {
        res.status(401).json({ error: 'Not authenticated' });
        return;
      }

      res.json({
        success: true,
        user: {
          id: req.user.userId,
          email: req.user.email,
          companyId: req.user.companyId,
          role: req.user.role
        }
      });
    } catch (error) {
      logger.error('Get current user error:', error);
      res.status(500).json({
        error: 'Internal server error'
      });
    }
  }

  /**
   * Logout (client-side token removal)
   */
  static async logout(_req: Request, res: Response): Promise<void> {
    // In JWT auth, logout is typically handled client-side
    // Server can optionally maintain a token blacklist
    res.json({
      success: true,
      message: 'Logged out successfully'
    });
  }
}
