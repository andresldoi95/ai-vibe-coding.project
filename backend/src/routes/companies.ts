import { Router } from 'express';
import { CompanyController } from '../controllers/CompanyController';
import { validateCompanyRegistration } from '../middleware/validators/companyValidator';
import { authenticateToken } from '../middleware/auth';

const router = Router();

/**
 * Public routes
 */

// Register new company with admin user
router.post('/register', validateCompanyRegistration, CompanyController.register);

// Check if RUC exists (useful for real-time validation)
router.get('/check-ruc/:ruc', CompanyController.checkRuc);

/**
 * Protected routes (require authentication)
 */

// Get company by ID
router.get('/:id', authenticateToken, CompanyController.getById);

export default router;
