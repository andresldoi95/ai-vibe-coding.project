import express, { Application } from 'express';
import cors from 'cors';
import helmet from 'helmet';
import compression from 'compression';
import morgan from 'morgan';
import dotenv from 'dotenv';
import { logger } from './config/logger';
import { errorHandler } from './middleware/errorHandler';
import { notFoundHandler } from './middleware/notFoundHandler';

// Load environment variables
dotenv.config();

const app: Application = express();

// Security middleware
app.use(helmet());
app.use(cors({
  origin: process.env.CORS_ORIGIN || 'http://localhost:5173',
  credentials: true
}));

// Request parsing
app.use(express.json({ limit: '10mb' }));
app.use(express.urlencoded({ extended: true, limit: '10mb' }));

// Compression
app.use(compression());

// Logging
if (process.env.NODE_ENV !== 'test') {
  app.use(morgan('combined', {
    stream: {
      write: (message: string) => logger.info(message.trim())
    }
  }));
}

// Health check
app.get('/health', (_req, res) => {
  res.json({
    status: 'ok',
    timestamp: new Date().toISOString(),
    environment: process.env.NODE_ENV,
    sriEnvironment: process.env.SRI_ENVIRONMENT === '2' ? 'producción' : 'pruebas'
  });
});

// API Routes
const API_PREFIX = process.env.API_PREFIX || '/api/v1';

// Import routes
import authRoutes from './routes/auth';
import productsRoutes from './routes/products';
import companiesRoutes from './routes/companies';

// Mount routes
app.use(`${API_PREFIX}/auth`, authRoutes);
app.use(`${API_PREFIX}/products`, productsRoutes);
app.use(`${API_PREFIX}/companies`, companiesRoutes);

// TODO: Add more routes
// app.use(`${API_PREFIX}/customers`, customersRoutes);
// app.use(`${API_PREFIX}/invoices`, invoicesRoutes);
// app.use(`${API_PREFIX}/inventory`, inventoryRoutes);

// 404 handler
app.use(notFoundHandler);

// Error handling middleware (must be last)
app.use(errorHandler);

export default app;
