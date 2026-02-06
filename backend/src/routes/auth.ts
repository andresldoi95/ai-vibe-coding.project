import { Router } from 'express';
import { AuthController } from '../controllers/AuthController';
import { authenticateToken } from '../middleware/auth';
import { validateLogin, validateRegister, validateCompanySelection } from '../middleware/validators/authValidator';

const router = Router();

/**
 * Auth Routes
 * 
 * POST /login - Login with email and password, returns user with available companies
 * POST /select-company - Select company and get JWT token
 * POST /register - Register new user
 * GET /me - Get current authenticated user
 * POST /logout - Logout (optional, JWT is stateless)
 */

// Public routes
router.post('/login', validateLogin, AuthController.login);
router.post('/select-company', validateCompanySelection, AuthController.selectCompany);
router.post('/register', validateRegister, AuthController.register);

// Protected routes (require authentication)
router.get('/me', authenticateToken, AuthController.getCurrentUser);
router.post('/logout', authenticateToken, AuthController.logout);

export default router;
