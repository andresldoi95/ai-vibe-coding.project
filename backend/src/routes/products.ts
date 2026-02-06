import { Router } from 'express';
import { ProductController } from '../controllers/ProductController';
import { authenticateToken } from '../middleware/auth';
import { validateProduct } from '../middleware/validators/productValidator';

const router = Router();

// All routes require authentication
router.use(authenticateToken);

// GET /api/v1/products - Get all products
router.get('/', ProductController.getProducts);

// GET /api/v1/products/categories - Get product categories
router.get('/categories', ProductController.getCategories);

// GET /api/v1/products/:id - Get product by ID
router.get('/:id', ProductController.getProductById);

// POST /api/v1/products - Create new product
router.post('/', validateProduct, ProductController.createProduct);

// PUT /api/v1/products/:id - Update product
router.put('/:id', validateProduct, ProductController.updateProduct);

// DELETE /api/v1/products/:id - Delete product
router.delete('/:id', ProductController.deleteProduct);

export default router;
