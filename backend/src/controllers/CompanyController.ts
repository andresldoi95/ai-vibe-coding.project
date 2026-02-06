import { Request, Response } from 'express';
import jwt from 'jsonwebtoken';
import { CompanyService } from '../services/companyService';
import { User } from '../models/User';
import { logger } from '../config/logger';

const JWT_SECRET = process.env.JWT_SECRET || 'your-secret-key-change-in-production';
const JWT_EXPIRES_IN = process.env.JWT_EXPIRES_IN || '24h';

/**
 * Company Controller
 * Handles company-related HTTP requests
 */
export class CompanyController {
  /**
   * Register a new company with admin user
   * POST /api/v1/companies/register
   */
  static async register(req: Request, res: Response): Promise<void> {
    try {
      const { company, admin } = req.body;

      // Validate required fields
      if (!company || !admin) {
        res.status(400).json({
          success: false,
          message: 'Se requieren datos de empresa y administrador'
        });
        return;
      }

      // Register company with admin user
      const result = await CompanyService.registerCompanyWithAdmin(company, admin);

      // Get user's companies for the response
      const userCompanies = await User.getUserCompanies(result.user.id);

      // Generate JWT token (auto-login after registration)
      const token = jwt.sign(
        {
          userId: result.user.id,
          email: result.user.email,
          companyId: result.company.id,
          roleId: result.role.id
        },
        JWT_SECRET,
        { expiresIn: JWT_EXPIRES_IN } as jwt.SignOptions
      );

      logger.info(`Company registered successfully: ${result.company.businessName}`);

      res.status(201).json({
        success: true,
        message: 'Empresa registrada exitosamente',
        data: {
          token,
          user: {
            id: result.user.id,
            email: result.user.email,
            fullName: result.user.fullName,
            phone: result.user.phone,
            status: result.user.status,
            companies: userCompanies
          },
          currentCompany: {
            id: result.company.id,
            name: result.company.businessName,
            ruc: result.company.ruc
          },
          currentRole: result.role.name
        }
      });
    } catch (error: any) {
      logger.error('Error in company registration:', error);

      // Handle specific errors with appropriate status codes
      if (error.message.includes('RUC')) {
        res.status(400).json({
          success: false,
          message: error.message
        });
        return;
      }

      if (error.message.includes('correo electrónico')) {
        res.status(409).json({
          success: false,
          message: error.message
        });
        return;
      }

      if (error.message.includes('Admin role not found')) {
        res.status(500).json({
          success: false,
          message: 'Error de configuración del sistema. Contacte al administrador.'
        });
        return;
      }

      // Generic error
      res.status(500).json({
        success: false,
        message: 'Error al registrar la empresa. Intente nuevamente.'
      });
    }
  }

  /**
   * Get company by ID
   * GET /api/v1/companies/:id
   */
  static async getById(req: Request, res: Response): Promise<void> {
    try {
      const { id } = req.params;

      const company = await CompanyService.findById(id);

      if (!company) {
        res.status(404).json({
          success: false,
          message: 'Empresa no encontrada'
        });
        return;
      }

      // Remove sensitive data
      const { digitalCertificatePath, certificatePassword, ...safeCompany } = company;

      res.json({
        success: true,
        data: safeCompany
      });
    } catch (error) {
      logger.error('Error getting company:', error);
      res.status(500).json({
        success: false,
        message: 'Error al obtener datos de la empresa'
      });
    }
  }

  /**
   * Check if RUC exists
   * GET /api/v1/companies/check-ruc/:ruc
   */
  static async checkRuc(req: Request, res: Response): Promise<void> {
    try {
      const { ruc } = req.params;

      const exists = await CompanyService.rucExists(ruc);

      res.json({
        success: true,
        data: {
          exists,
          available: !exists
        }
      });
    } catch (error) {
      logger.error('Error checking RUC:', error);
      res.status(500).json({
        success: false,
        message: 'Error al verificar RUC'
      });
    }
  }
}
